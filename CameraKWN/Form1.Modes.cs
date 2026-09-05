//----------------------------------------------------------------------------
// Form1 (partial) — Application.Idle mode handlers
// Each mode is registered in CheckMode() (Form1.Camera.cs).
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ZXing;

namespace camera_show {
    public partial class Form1 {

        // ── Helper: handle a null / dead capture inside a mode handler ────────
        private bool HandleNullCapture() {
            ReopenCamera();
            if (IsCaptureValid()) {
                this.ClientSize  = new Size(setCamera.Capture.Width, setCamera.Capture.Height);
                pictureBox1.Size = new Size(setCamera.Capture.Width, setCamera.Capture.Height);
                pictureBox1.Location = new Point(0, 0);
                ReadConfigCamera();
            }
            return true; // caller should return after this
        }

        // ── Frame display helper ──────────────────────────────────────────────

        /// <summary>
        /// Routes the PictureBox to the correct frame.
        /// While snapshot is active the frozen copy is shown; otherwise the live frame is shown.
        /// </summary>
        private void ShowFrame() {
            pictureBox1.Image = flag.Snapshot ? _snapshotImage?.Bitmap : global.Image?.Bitmap;
        }

        // ── Normal mode (display only, no recognition) ────────────────────────

        private void NormalMode(object sender, EventArgs e) {
            if (_isMouseDown) return;
            if (!IsCaptureValid()) { HandleNullCapture(); return; }

            try {
                using (Mat frame = setCamera.Capture.QueryFrame()) {
                    if (IsBlackFrame(frame)) { HandleBlackFrame(); return; }
                    _consecutiveBlackFrames = 0;
                    var old = global.UpdateImage(frame.ToImage<Bgr, byte>());
                    ShowFrame();
                    old?.Dispose();
                }
            } catch {
                MessageBox.Show(AppText.CannotOpenCamera);
                flag.CloseForm = true;
                Application.Exit();
            }

            Thread.Sleep(100);
        }

        // ── Barcode / QR reader mode ──────────────────────────────────────────

        private void Read2dMode(object sender, EventArgs e) {
            if (flag.ClearStopWatch) {
                flag.ClearStopWatch = false;
                global.StopWatch.Restart();
            }

            if (global.StopWatch.ElapsedMilliseconds >= global.TimeOut) {
                StampFailRead2d();
                if (!flag.Debug) return;
            }

            if (_isMouseDown) return;

            if (!IsCaptureValid()) {
                HandleNullCapture();
                flag.ClearStopWatch = true;
                return;
            }

            // ── Grab frame ───────────────────────────────────────────────────
            try {
                // Flush frames that accumulated in the camera buffer during the
                // previous decode pass so we always process the latest image.
                for (int flush = 0; flush < 3; flush++) {
                    using (var dummy = setCamera.Capture.QueryFrame()) {
                        if (dummy == null || dummy.IsEmpty) break;
                    }
                }
                using (Mat frame = setCamera.Capture.QueryFrame()) {
                    if (IsBlackFrame(frame)) { HandleBlackFrame(); return; }
                    _consecutiveBlackFrames = 0;
                    var raw = frame.ToImage<Bgr, byte>();
                    var old = global.UpdateImage(raw.Rotate(global.AdjustDegree, new Bgr()));
                    raw.Dispose();
                    ShowFrame(); // updated later after overlay
                    old?.Dispose();
                }
            } catch {
                MessageBox.Show(AppText.CannotOpenCamera);
                Application.Exit();
                return;
            }

            // ── Draw ROI ─────────────────────────────────────────────────────
            using (Graphics g = Graphics.FromImage(global.Image.Bitmap))
                g.DrawRectangle(Pens.Red, Rect);

            // ── Crop to ROI ──────────────────────────────────────────────────
            var imageCut = global.Image.Copy();
            var r = Rect.Width == 0 ? new Rectangle(Rect.X, Rect.Y, 100, 100) : Rect;
            try { imageCut.ROI = r; } catch { }
            var temp = imageCut.Copy();
            imageCut.Dispose();

            // ── Canny-based rectangle detection ──────────────────────────────
            var boxList   = new List<RotatedRect>();
            var imgGray   = temp.Convert<Gray, byte>().ThresholdBinary(new Gray(150), new Gray(255));
            using (var edges = new UMat()) {
                CvInvoke.Canny(imgGray, edges, 180.0, 120.0);
                using (var contours = new VectorOfVectorOfPoint()) {
                    CvInvoke.FindContours(edges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                    for (int i = 0; i < contours.Size; i++) {
                        using (var contour = contours[i])
                        using (var approx  = new VectorOfPoint()) {
                            CvInvoke.ApproxPolyDP(contour, approx,
                                CvInvoke.ArcLength(contour, true) * 0.1, true);
                            if (CvInvoke.ContourArea(approx, false) > 50 && approx.Size == 4) {
                                bool isRect = true;
                                var pts = approx.ToArray();
                                var edges2 = PointCollection.PolyLine(pts, true);
                                for (int j = 0; j < edges2.Length; j++) {
                                    double angle = Math.Abs(edges2[(j + 1) % edges2.Length]
                                                   .GetExteriorAngleDegree(edges2[j]));
                                    if (angle < 70 || angle > 100) { isRect = false; break; }
                                }
                                if (isRect) boxList.Add(CvInvoke.MinAreaRect(approx));
                            }
                        }
                    }
                }
            }
            imgGray.Dispose();

            // ── Barcode decode (rotating) ─────────────────────────────────────
            var    reader = new BarcodeReader();
            Result result = null;
            var    rot    = new Bgr();
            for (int i = 1; i <= 17; i++) {
                result = reader.Decode(temp.Bitmap);
                if (result != null) break;
                var prevTemp = temp;
                temp = temp.Rotate(5, rot);
                prevTemp.Dispose();
            }

            if (result == null) {
                foreach (RotatedRect box in boxList) {
                    var brect = CvInvoke.BoundingRectangle(new VectorOfPointF(box.GetVertices()));
                    var crop2 = temp.Copy();
                    crop2.ROI = brect;
                    var tempBox = crop2.Copy();
                    crop2.Dispose();
                    for (int i = 1; i <= 17; i++) {
                        result = reader.Decode(tempBox.Bitmap);
                        if (result != null) break;
                        var prevBox = tempBox;
                        tempBox = tempBox.Rotate(i * 5, rot);
                        prevBox.Dispose();
                        result  = reader.Decode(tempBox.Bitmap);
                    }
                    tempBox.Dispose();
                    if (result != null) break;
                }
            }
            temp.Dispose();

            // ── Overlay result ────────────────────────────────────────────────
            if (result != null) {
                CvInvoke.PutText(global.Image, result.Text,
                    new Point(20, 30), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);

                if (result.Text.Length == global.DigitSn) {
                    if (!setCamera.FlagOpen && !flag.Debug) {
                        File.WriteAllText(setPath.ResultTxt, result + "\r\nPASS");
                        this.Close();
                    }
                    flag.ResultPass = true;
                    global.ResultBackup = result.ToString();
                } else {
                    if (!setCamera.FlagOpen) autoScale.Run(this);
                    flag.ResultPass = false;
                }
            } else {
                CvInvoke.PutText(global.Image, "not read",
                    new Point(20, 30), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                if (!setCamera.FlagOpen) autoScale.Run(this);
                flag.ResultPass = false;
            }

            CvInvoke.PutText(global.Image, boxList.Count.ToString(),
                new Point(20, 60), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(global.Image, "Port = " + global.Port,
                new Point(20, 90), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            ShowFrame();
            Thread.Sleep(100);
        }

        // ── Image comparison mode ─────────────────────────────────────────────

        private void CompearImageMode(object sender, EventArgs e) {
            if (flag.ClearStopWatch) {
                flag.ClearStopWatch = false;
                global.StopWatch.Restart();
            }

            if (global.StopWatch.ElapsedMilliseconds >= global.TimeOut) {
                StampFailCompare();
                if (!flag.Debug) return;
            }

            if (_isMouseDown) return;

            if (!IsCaptureValid()) {
                HandleNullCapture();
                flag.ClearStopWatch = true;
                return;
            }

            try {
                using (Mat frame = setCamera.Capture.QueryFrame()) {
                    var old = global.UpdateImage(frame.ToImage<Bgr, byte>());
                    ShowFrame();
                    old?.Dispose();
                }
            } catch {
                MessageBox.Show(AppText.CannotOpenCamera);
                Application.Exit();
                return;
            }

            using (Graphics g = Graphics.FromImage(global.Image.Bitmap))
                g.DrawRectangle(Pens.Red, Rect);

            var imageCut = global.Image.Copy();
            imageCut.ROI = Rect;
            var imageSup = imageCut.Copy();

            long matchTime;
            try {
                string modelPath = setPath.FolderCompare + global.Head + "Image" + global.NumberCompare + ".png";
                using (Mat modelImage    = CvInvoke.Imread(modelPath, ImreadModes.Grayscale))
                using (Mat observedImage = imageSup.Mat) {
                    // BUG FIX: updated to new DrawMatches.Draw() signature with out bool objectFound
                    bool objectFound;
                    using (Mat drawResult = DrawMatches.Draw(modelImage, observedImage,
                                                             out matchTime, out objectFound)) {
                        CvInvoke.PutText(global.Image, objectFound.ToString(),
                            new Point(20, 30), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                        CvInvoke.PutText(global.Image, "Port = " + global.Port,
                            new Point(20, 60), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                        ShowFrame();
                    }

                    if (!objectFound) {
                        reAdjust.Run(this);
                        flag.ResultPass = false;
                        editValueCamDiv3.Process();
                    } else {
                        if (!setCamera.FlagOpen && !flag.Debug) {
                            if (Convert.ToBoolean(File.ReadAllText(setPath.HeadTxt + AppFilePath.Debug)))
                                flag.Debug = true;
                            else {
                                if (!global.StopWatchShow.IsRunning)
                                    global.StopWatchShow.Restart();
                                int checkTime = 1000;
                                try { checkTime = Convert.ToInt32(File.ReadAllText("../../config/time_checkcompare.txt")); } catch { }
                                if (global.StopWatchShow.ElapsedMilliseconds < checkTime) return;
                            }

                            global.StopWatchShow.Stop();
                            global.NumberCompare++;
                            ctms_compareNumber.Text = global.NumberCompare.ToString();

                            if (File.Exists(setPath.FolderCompare + global.Head + "Image" + global.NumberCompare + ".png")) {
                                var rx = File.ReadAllLines(setPath.FolderCompare + global.Head + AppFilePath.RectX).ToList();
                                var ry = File.ReadAllLines(setPath.FolderCompare + global.Head + AppFilePath.RectY).ToList();
                                var rw = File.ReadAllLines(setPath.FolderCompare + global.Head + AppFilePath.RectWidth).ToList();
                                var rh = File.ReadAllLines(setPath.FolderCompare + global.Head + AppFilePath.RectHeight).ToList();
                                try {
                                    var nr = Rect;
                                    nr.X      = Convert.ToInt32(rx[global.NumberCompare - 1]);
                                    nr.Y      = Convert.ToInt32(ry[global.NumberCompare - 1]);
                                    nr.Width  = Convert.ToInt32(rw[global.NumberCompare - 1]);
                                    nr.Height = Convert.ToInt32(rh[global.NumberCompare - 1]);
                                    Form1.Rect = nr;
                                } catch { }
                                global.NumStep++;
                                return;
                            }

                            File.WriteAllText(setPath.ResultTxt, "Image Detected\r\nPASS");
                            this.Close();
                        }

                        flag.ResultPass    = true;
                        global.ResultBackup = "Image Detected";
                    }
                }
            } catch {
                CvInvoke.PutText(global.Image, "Port = " + global.Port,
                    new Point(20, 60), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                ShowFrame();
            }

            Thread.Sleep(250);
        }

        // ── LED colour-check mode ─────────────────────────────────────────────

        /// <summary>
        /// Called from CheckMode before the idle handler is registered.
        /// Counts LED images and pre-loads all ROIs.
        /// In debug mode all LEDs are checked simultaneously; otherwise they are checked one by one.
        /// </summary>
        private void InitCheckLedMode() {
            _ledTotal = 0;
            global.NumberCheckLed = 1;
            _sequentialDebugMode = flag.Debug;
            while (File.Exists(setPath.FolderCheckLed + global.Head + "Image" + (_ledTotal + 1) + ".png"))
                _ledTotal++;
            if (_ledTotal == 0) _ledTotal = 1;

            _ledRects    = new List<Rectangle>(_ledTotal);
            _ledSustainSw = new Stopwatch[_ledTotal];
            _ledStatuses  = new string[_ledTotal];
            _ledPrevOn    = new bool[_ledTotal];

            // Load all ROI rectangles from the per-head text files (one line per LED).
            bool loaded = false;
            try {
                var rx = File.ReadAllLines(setPath.FolderCheckLed + global.Head + AppFilePath.RectX);
                var ry = File.ReadAllLines(setPath.FolderCheckLed + global.Head + AppFilePath.RectY);
                var rw = File.ReadAllLines(setPath.FolderCheckLed + global.Head + AppFilePath.RectWidth);
                var rh = File.ReadAllLines(setPath.FolderCheckLed + global.Head + AppFilePath.RectHeight);
                for (int i = 0; i < _ledTotal; i++)
                    _ledRects.Add(new Rectangle(
                        Convert.ToInt32(rx[i]), Convert.ToInt32(ry[i]),
                        Convert.ToInt32(rw[i]), Convert.ToInt32(rh[i])));
                loaded = true;
            } catch { }

            if (!loaded)
                for (int i = 0; i < _ledTotal; i++)
                    _ledRects.Add(Rectangle.Empty); // fallback: filled from Form1.Rect on first call

            for (int i = 0; i < _ledTotal; i++)
                _ledSustainSw[i] = new Stopwatch();
        }

        private void CheckLedMode(object sender, EventArgs e) {
            // First call: ReadConfigFile has run, so Form1.Rect is valid as a fallback.
            if (!_ledModeStarted) {
                if (_ledRects.Count > 0 && _ledRects[0] == Rectangle.Empty)
                    _ledRects[0] = Rect;
                _ledModeStarted = true;
            }

            if (flag.ClearStopWatch) {
                flag.ClearStopWatch = false;
                global.StopWatch.Restart();
                if (!_sequentialDebugMode) {
                    for (int i = 0; i < _ledTotal; i++) {
                        if (_ledStatuses[i] != null) continue;
                        _ledSustainSw[i].Reset();
                        _ledPrevOn[i] = false;
                    }
                }
            }

            // ── Timeout ───────────────────────────────────────────────────────
            if (global.StopWatch.ElapsedMilliseconds >= global.TimeOut) {
                if (!_sequentialDebugMode) { StampFailCheckLed(); return; }
                global.StopWatch.Restart(); // sequential debug: no auto-close, keep waiting
                return;
            }

            if (_isMouseDown) return;

            if (!IsCaptureValid()) { HandleNullCapture(); flag.ClearStopWatch = true; return; }

            // ── Grab frame ────────────────────────────────────────────────────
            try {
                using (Mat frame = setCamera.HsvTestFlag
                    ? new Mat("../../config/hsv_test.png")
                    : setCamera.Capture.QueryFrame()) {
                    var oldBgr = global.UpdateImage(frame.ToImage<Bgr, byte>());
                    var oldHsv = global.UpdateImageHsv(frame.ToImage<Hsv, byte>());
                    oldBgr?.Dispose();
                    oldHsv?.Dispose();
                }
            } catch {
                MessageBox.Show(AppText.CannotOpenCamera);
                Application.Exit();
                return;
            }

            if (!_sequentialDebugMode) {
                // ── Simultaneous (file debug=False): evaluate all LEDs every frame ──
                for (int i = 0; i < _ledTotal; i++) {
                    if (_ledStatuses[i] != null) {
                        using (Graphics g = Graphics.FromImage(global.Image.Bitmap))
                            g.DrawRectangle(_ledStatuses[i] == "PASS" ? Pens.Green : Pens.Yellow, _ledRects[i]);
                        CvInvoke.PutText(global.Image,
                            "LED" + (i + 1) + ": " + _ledStatuses[i],
                            new Point(20, 30 + i * 30),
                            FontFace.HersheySimplex, 0.5, new MCvScalar(0, 255, 0), 2);
                        continue;
                    }

                    int pixels = CountLedPixels(i);
                    bool on = pixels >= setCamera.HsvMask;
                    if (on) {
                        if (!_ledPrevOn[i]) { _ledSustainSw[i].Restart(); _ledPrevOn[i] = true; }
                        if (!setCamera.FlagOpen &&
                            _ledSustainSw[i].ElapsedMilliseconds >= setCamera.HsvTimeout)
                            _ledStatuses[i] = "PASS";
                    } else {
                        _ledSustainSw[i].Reset();
                        _ledPrevOn[i] = false;
                    }

                    using (Graphics g = Graphics.FromImage(global.Image.Bitmap))
                        g.DrawRectangle(Pens.Red, _ledRects[i]);
                    CvInvoke.PutText(global.Image,
                        "LED" + (i + 1) + ": " + pixels + "px  " + _ledSustainSw[i].ElapsedMilliseconds + "ms",
                        new Point(20, 30 + i * 30),
                        FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                }

                CvInvoke.PutText(global.Image, "Port = " + global.Port,
                    new Point(20, 30 + _ledTotal * 30),
                    FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                if (!setCamera.FlagOpen && _ledStatuses.All(s => s != null)) {
                    FinishLedMode();
                    return;
                }
            } else {
                // ── Sequential debug (file debug=True): stay on current LED until GUI "Set Debug" click ──
                int ci = global.NumberCheckLed - 1;
                if (ci >= _ledTotal) { FinishLedMode(); return; }

                int px = CountLedPixels(ci);

                // GUI "Set Debug" button sets flag.Debug = false — that click is the advance signal.
                if (!flag.Debug) {
                    _ledStatuses[ci] = (px >= setCamera.HsvMask) ? "PASS" : "FAIL";
                    global.NumberCheckLed++;
                    if (global.NumberCheckLed - 1 >= _ledTotal) {
                        FinishLedMode();
                        return;
                    }
                    // More LEDs remain: load next LED's stored ROI, restore debug flag, wait.
                    int nextIdx = global.NumberCheckLed - 1;
                    if (_ledRects != null && nextIdx < _ledRects.Count)
                        Form1.Rect = _ledRects[nextIdx];
                    flag.Debug = true;
                    global.StopWatch.Restart();
                    ShowFrame();
                    return;
                }

                // Show live detection for the current LED so the operator can inspect it.
                bool ledOn = px >= setCamera.HsvMask;
                if (ledOn) {
                    if (!_ledPrevOn[ci]) { _ledSustainSw[ci].Restart(); _ledPrevOn[ci] = true; }
                } else {
                    _ledSustainSw[ci].Reset();
                    _ledPrevOn[ci] = false;
                }

                using (Graphics g = Graphics.FromImage(global.Image.Bitmap))
                    g.DrawRectangle(Pens.Red, _ledRects[ci]);
                CvInvoke.PutText(global.Image,
                    "LED" + (ci + 1) + ": " + px + "px  " + _ledSustainSw[ci].ElapsedMilliseconds + "ms",
                    new Point(20, 30), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                CvInvoke.PutText(global.Image, "Port = " + global.Port,
                    new Point(20, 60), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);

                flag.ResultPass    = _ledStatuses.All(s => s == "PASS");
                global.ResultBackup = "Color Detected";
            }

            ShowFrame();
            Thread.Sleep(100);
        }

        private void FinishLedMode() {
            var lines = new List<string>();
            for (int i = 0; i < _ledTotal; i++)
                lines.Add("LED" + (i + 1) + " " + (_ledStatuses[i] ?? "FAIL"));
            Thread.Sleep(50);
            File.WriteAllText(AppFilePath.LedResultTxt, string.Join("\r\n", lines));
            this.Close();
        }

        private int CountLedPixels(int ledIndex) {
            if (!setCamera.HsvFlag) {
                var cut = global.Image.Copy();
                try { cut.ROI = _ledRects[ledIndex]; } catch { }
                var sup = cut.Copy();
                try { return sup.InRange(setCamera.BgrLow, setCamera.BgrHigh).CountNonzero()[0]; } catch { }
            } else {
                var cutHsv = global.ImageHsv.Copy();
                try { cutHsv.ROI = _ledRects[ledIndex]; } catch { }
                var supHsv = cutHsv.Copy();
                try { return supHsv.InRange(setCamera.HsvLow, setCamera.HsvHigh).CountNonzero()[0]; } catch { }
            }
            return 0;
        }

        // ── Blink-LED frequency-measurement mode ──────────────────────────────

        private void BlinkLedMode(object sender, EventArgs e) {
            if (flag.ClearStopWatch) {
                flag.ClearStopWatch = false;
                global.StopWatch.Restart();
            }

            if (global.StopWatch.ElapsedMilliseconds >= global.TimeOut) {
                StampFailBlinkLed();
                if (!flag.Debug) return;
            }

            if (_isMouseDown) return;

            if (!IsCaptureValid()) {
                HandleNullCapture();
                flag.ClearStopWatch = true;
                return;
            }

            try {
                using (Mat frame = setCamera.HsvTestFlag
                    ? new Mat("../../config/hsv_test.png")
                    : setCamera.Capture.QueryFrame()) {
                    var oldBgr = global.UpdateImage(frame.ToImage<Bgr, byte>());
                    var oldHsv = global.UpdateImageHsv(frame.ToImage<Hsv, byte>());
                    oldBgr?.Dispose();
                    oldHsv?.Dispose();
                }
            } catch {
                MessageBox.Show(AppText.CannotOpenCamera);
                Application.Exit();
                return;
            }

            using (Graphics g = Graphics.FromImage(global.Image.Bitmap))
                g.DrawRectangle(Pens.Red, Rect);

            int redpixels = 0;
            if (!setCamera.HsvFlag) {
                var cut = global.Image.Copy();
                try { cut.ROI = Rect; } catch { }
                var sup = cut.Copy();
                try { redpixels = sup.InRange(setCamera.BgrLow, setCamera.BgrHigh).CountNonzero()[0]; } catch { }
            } else {
                var cutHsv = global.ImageHsv.Copy();
                try { cutHsv.ROI = Rect; } catch { }
                var supHsv = cutHsv.Copy();
                try { redpixels = supHsv.InRange(setCamera.HsvLow, setCamera.HsvHigh).CountNonzero()[0]; } catch { }
            }

            bool mask = redpixels >= setCamera.HsvMask;
            var  bl   = global.BlinkLed;

            if (mask) {
                if (bl.TricCal) {
                    int countSup    = (int)(2000.0 / (bl.TimeLow + bl.TimeHigh));
                    bl.CounterMax   = (int)Map(countSup, 0, 2000, 1, 1200);
                    bl.TricCal      = false;
                    bl.FrequencySup += 1.0 / ((bl.TimeLow + bl.TimeHigh) / 1000.0);
                    bl.DutySup      += bl.TimeHigh * 100.0 / (bl.TimeLow + bl.TimeHigh);
                    bl.Counter++;

                    if (bl.Counter >= bl.CounterMax) {
                        bl.Frequency    = bl.FrequencySup / bl.Counter;
                        bl.Duty         = bl.DutySup      / bl.Counter;
                        bl.FindResult();
                        bl.Counter      = 0;
                        bl.FrequencySup = 0;
                        bl.DutySup      = 0;
                    }
                }
                bl.TimeHigh = bl.StopWatchHigh.ElapsedMilliseconds;
                bl.StopWatchLow.Restart();
            } else {
                bl.TimeLow  = bl.StopWatchLow.ElapsedMilliseconds;
                bl.StopWatchHigh.Restart();
                bl.TricCal  = true;
            }

            CvInvoke.PutText(global.Image, redpixels.ToString(),
                new Point(20, 30), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(global.Image, mask.ToString(),
                new Point(20, 60), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(global.Image, "Frequency=" + bl.Frequency.ToString("0.0000"),
                new Point(pictureBox1.Width - 200, 30), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(global.Image, "Duty=" + bl.Duty.ToString("0.0000"),
                new Point(pictureBox1.Width - 200, 60), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            CvInvoke.PutText(global.Image, "Port = " + global.Port,
                new Point(20, 90), FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
            ShowFrame();

            if (!setCamera.FlagOpen && !flag.Debug) {
                if (bl.FlagResult) {
                    Thread.Sleep(50);
                    File.WriteAllText(setPath.ResultTxt, bl.Result.ToString("0.0000") + "\r\nPASS");
                    this.Close();
                }
            }
        }

        // ── Timeout stamp helpers ─────────────────────────────────────────────

        private void StampFailRead2d() {
            if (flag.Debug) { global.StopWatch.Reset(); return; }
            File.WriteAllText(setPath.ResultTxt, "Unreadable\r\nFAIL");
            this.Close();
        }

        private void StampFailCompare() {
            if (flag.Debug) { global.StopWatch.Reset(); return; }
            File.WriteAllText(setPath.ResultTxt, "(" + global.NumStep + ")time over\r\nFAIL");
            this.Close();
        }

        private void StampFailCheckLed() {
            if (flag.Debug) { global.StopWatch.Reset(); return; }
            // Use whatever each LED has resolved to so far; any still pending → FAIL.
            var lines = new List<string>();
            for (int i = 0; i < _ledTotal; i++)
                lines.Add("LED" + (i + 1) + " " + (_ledStatuses[i] ?? "FAIL"));
            File.WriteAllText(AppFilePath.LedResultTxt, string.Join("\r\n", lines));
            this.Close();
        }

        private void StampFailBlinkLed() {
            if (flag.Debug) { global.StopWatch.Reset(); return; }
            File.WriteAllText(setPath.ResultTxt, "(" + global.NumStep + ")time over\r\nFAIL");
            this.Close();
        }
    }
}
