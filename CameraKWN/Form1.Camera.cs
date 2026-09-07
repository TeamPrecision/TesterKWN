//----------------------------------------------------------------------------
// Form1 (partial) — camera lifecycle: open, reopen, config, address scan
//----------------------------------------------------------------------------
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace camera_show {
    public partial class Form1 {

        // ── Port-selection dialog (set-port mode) ─────────────────────────────

        private void SetPortCamera() {
            var formSetPort = new Form {
                Text = AppText.SetPort,
                Size = new Size(100, 100)
            };
            var comboBox = new ComboBox { Size = new Size(60, 7) };

            // Probe ports without keeping VideoCapture references alive
            for (int loop = 0; loop < 9; loop++) {
                try {
                    using (var testCap = new VideoCapture(loop, _captureApi)) {
                        if (testCap.Width != 0)
                            comboBox.Items.Add(loop);
                    }
                } catch { }
            }

            formSetPort.Controls.Add(comboBox);
            formSetPort.ShowDialog();

            // BUG FIX: original did Convert.ToInt32(comboBox.Text) which throws when empty
            int portToOpen = 0;
            if (!string.IsNullOrEmpty(comboBox.Text) &&
                int.TryParse(comboBox.Text, out int parsed))
                portToOpen = parsed;

            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.Port, portToOpen.ToString(), setPath.StepCsv);
            setCamera.Capture = new VideoCapture(portToOpen, _captureApi);
        }

        // ── Normal startup (wait for fMain to assign work) ────────────────────

        private void OpenCameraList() {
            formCancel.Show(this);

            WaitFileList();
            this.Text = "[" + global.Head + "]." + global.StepTest;
            WaitOpenCamera();

            formCancel.Form.Close();
        }

        private void WaitFileList() {
            while (true) {
                if (formCancel.Form.IsDisposed) {
                    Application.Exit();
                    return;
                }

                try {
                    string fileList = File.ReadAllText(AppFilePath.List);
                    if (fileList == string.Empty) break;
                    if (fileList.Substring(0, 1) != global.Head) {
                        DelaymS(50);
                        Thread.Sleep(50);
                        continue;
                    }
                } catch {
                    DelaymS(50);
                    Thread.Sleep(50);
                    continue;
                }
                break;
            }
        }

        private void WaitOpenCamera() {
            while (true) {
                if (formCancel.Form.IsDisposed) {
                    Application.Exit();
                    return;
                }

                try {
                    try { setCamera.Capture?.Dispose(); } catch { }
                    setCamera.Capture = new VideoCapture(global.Port, _captureApi);

                    if (_address != CameraAddress.Port) {
                        int gammaCsv = Convert.ToInt32(
                            setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Gamma, setPath.StepCsv));
                        if (!CheckAddressCamera(gammaCsv)) {
                            if (!ScanAddressCamera(gammaCsv)) {
                                if (!flag.Debug) {
                                    File.WriteAllText(setPath.ResultTxt, "Can not find port\r\nFAIL");
                                    this.Close();
                                    Application.Exit();
                                    Application.ExitThread();
                                    Environment.Exit(0);
                                }
                                return;
                            }
                        }
                    }

                    // BUG FIX: guard against null / zero-width before using capture
                    if (!IsCaptureValid()) {
                        DelaymS(50);
                        Thread.Sleep(200);
                        continue;
                    }

                    Thread.Sleep(200); // let DirectShow pipeline initialize
                    // Actively drain the frame buffer so stale frames from the
                    // previous job are discarded before recognition begins.
                    var flushSw = System.Diagnostics.Stopwatch.StartNew();
                    while (flushSw.ElapsedMilliseconds < 800) {
                        try { using (var f = setCamera.Capture.QueryFrame()) { } } catch { }
                    }
                    break;
                } catch { Thread.Sleep(50); }

                DelaymS(50);
                Thread.Sleep(200);
            }
        }

        // ── Camera property loading ───────────────────────────────────────────

        private void ReadConfigCamera() {
            // Issue #3: load skip list before applying any camera properties
            setCamera.SkipSettings.Load(setPath.HeadTxt);

            Frame_height.Text = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.FrameHeight, setPath.StepCsv);
            Frame_width.Text  = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.FrameWidth,  setPath.StepCsv);

            try {
                int zoom       = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Zoom,       setPath.StepCsv));
                int pan        = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Pan,        setPath.StepCsv));
                int tilt       = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Tilt,       setPath.StepCsv));
                int contrast   = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Contrast,   setPath.StepCsv));
                int brightness = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Brightness, setPath.StepCsv));
                int saturation = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Saturation, setPath.StepCsv));
                int sharpness  = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Sharpness,  setPath.StepCsv));
                int gain       = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Gain,       setPath.StepCsv));

                setCamera.SetCapture(CapProp.FrameHeight, Convert.ToInt32(Frame_height.Text));
                setCamera.SetCapture(CapProp.FrameWidth,  Convert.ToInt32(Frame_width.Text));
                setCamera.SetCapture(CapProp.Zoom,        zoom);
                setCamera.SetCapture(CapProp.Pan,         pan);
                setCamera.SetCapture(CapProp.Tilt,        tilt);
                setCamera.SetCapture(CapProp.Contrast,    contrast);
                setCamera.SetCapture(CapProp.Brightness,  brightness);
                setCamera.SetCapture(CapProp.Saturation,  saturation);
                setCamera.SetCapture(CapProp.Sharpness,   sharpness);
                setCamera.SetCapture(CapProp.Gain,        gain);
                // Focus and Exposure intentionally left to skip-settings management
            } catch { }

            global.StopWatchHsv.Restart();
            global.StopWatch.Restart();
        }

        /// <summary>Read current camera property values back into the CSV (save config).</summary>
        private void ReadConfigCameraToCsvFile() {
            string headSup = HeadConfigKey.Head + global.Head;

            double zoom       = setCamera.Capture.GetCaptureProperty(CapProp.Zoom);
            double pan        = setCamera.Capture.GetCaptureProperty(CapProp.Pan);
            double tilt       = setCamera.Capture.GetCaptureProperty(CapProp.Tilt);
            double contrast   = setCamera.Capture.GetCaptureProperty(CapProp.Contrast);
            double brightness = setCamera.Capture.GetCaptureProperty(CapProp.Brightness);
            double saturation = setCamera.Capture.GetCaptureProperty(CapProp.Saturation);
            double sharpness  = setCamera.Capture.GetCaptureProperty(CapProp.Sharpness);
            double gain       = setCamera.Capture.GetCaptureProperty(CapProp.Gain);
            double gamma      = setCamera.Capture.GetCaptureProperty(CapProp.Gamma);

            setupPay.write_text(headSup + HeadConfigKey.Zoom,       zoom.ToString(),       setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Pan,        pan.ToString(),        setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Tilt,       tilt.ToString(),       setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Contrast,   contrast.ToString(),   setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Brightness, brightness.ToString(), setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Saturation, saturation.ToString(), setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Sharpness,  sharpness.ToString(),  setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Gain,       gain.ToString(),       setPath.StepCsv);
            setupPay.write_text(headSup + HeadConfigKey.Gamma,      gamma.ToString(),      setPath.StepCsv);
        }

        // ── Camera address management ─────────────────────────────────────────

        private void ReadFileAddress() {
            _address = CameraAddress.Gamma;
            string headSup = HeadConfigKey.Head + global.Head;
            try {
                _address = setupPay.read_text(headSup + HeadConfigKey.SetAddress, setPath.StepCsv);
            } catch { }

            ctms_setGammaAddress.Checked = (_address == CameraAddress.Gamma);
            ctms_usePort.Checked         = (_address == CameraAddress.Port);
        }

        private bool CheckAddressCamera(int addressCsv) {
            try {
                double gamma = setCamera.Capture.GetCaptureProperty(CapProp.Gamma);
                return (int)gamma == addressCsv;
            } catch { return false; }
        }

        /// <summary>
        /// Scan up to 100 ports to find the camera whose Gamma matches <paramref name="addressCsv"/>.
        /// BUG FIX: original always returned true; now returns false when no match is found.
        /// </summary>
        private bool ScanAddressCamera(int addressCsv) {
            bool found = false;
            for (int loop = 0; loop < 100; loop++) {
                try { setCamera.Capture?.Dispose(); } catch { }
                setCamera.Capture = new VideoCapture(loop, _captureApi);

                if (setCamera.Capture.Width == 0) {
                    if (flag.Debug)
                        MessageBox.Show("Cannot find camera address [" + addressCsv + "]");
                    try { setCamera.Capture.Dispose(); } catch { }
                    setCamera.Capture = new VideoCapture(0, _captureApi);
                    return false;       // BUG FIX: return false, not true
                }

                if (!CheckAddressCamera(addressCsv))
                    continue;

                SetPortCameraFollowAddress(loop);
                found = true;
                break;
            }
            return found;              // BUG FIX: was always true
        }

        private void SetPortCameraFollowAddress(int portNew) {
            global.Port = portNew;
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.Port, portNew.ToString(), setPath.StepCsv);
        }

        private bool CheckPasswordSetAddress() {
            while (true) {
                KeyPassword form = new KeyPassword();
                if (form.ShowDialog() != DialogResult.OK) return false;
                if (form.inputValue != "camera") {
                    MessageBox.Show("Password Error!!");
                    continue;
                }
                break;
            }
            return true;
        }

        // ── Black-frame detection ─────────────────────────────────────────────

        private bool IsCaptureValid() =>
            setCamera.Capture != null &&
            setCamera.Capture.Ptr != IntPtr.Zero &&
            setCamera.Capture.Width > 0;

        private bool IsBlackFrame(Emgu.CV.Mat frame) {
            if (frame == null || frame.IsEmpty) return true;
            var mean = Emgu.CV.CvInvoke.Mean(frame);
            return mean.V0 + mean.V1 + mean.V2 < BlackFrameMeanThreshold;
        }

        private void HandleBlackFrame() {
            _consecutiveBlackFrames++;
            if (_consecutiveBlackFrames >= BlackFrameThreshold) {
                _consecutiveBlackFrames = 0;
                ReopenCamera();
            }
        }

        /// <summary>
        /// Dispose and recreate VideoCapture.
        /// Issue #1: enforces a minimum cooldown between attempts so rapid cycling
        /// cannot damage camera hardware.
        /// </summary>
        private void ReopenCamera() {
            // Cooldown guard — don't thrash the hardware
            if ((DateTime.UtcNow - _lastReopenAttempt).TotalMilliseconds < ReopenCooldownMs) {
                return;
            }
            _lastReopenAttempt = DateTime.UtcNow;

            try { setCamera.Capture?.Dispose(); } catch { }
            setCamera.Capture = null;
            Thread.Sleep(800);
            try { setCamera.Capture = new VideoCapture(global.Port, _captureApi); } catch { }
            Thread.Sleep(300);

            if (IsCaptureValid()) {
                ReadConfigCamera();
            }
        }

        // ── Mode registration ─────────────────────────────────────────────────

        private void CheckMode() {
            if (global.StepTest.Contains(CameraMode.Read2d)) {
                flag.Read2d = true;
                processToolStripMenuItem.Visible        = true;
                digitSNToolStripMenuItem.Visible         = true;
                adjustDegreeToolStripMenuItem.Visible    = true;
                Application.Idle += Read2dMode;

            } else if (global.StepTest.Contains(CameraMode.ComPar) ||
                       global.StepTest.Contains(CameraMode.ComPear)) {
                flag.ComPear = true;
                addStepComparToolStripMenuItem.Visible = true;
                ctms_RoiCrop_.Visible                 = true;
                setPath.FolderCompare = Path.Combine(AppFilePath.ConfigRoot, global.StepTest) + Path.DirectorySeparatorChar;
                if (!Directory.Exists(setPath.FolderCompare))
                    Directory.CreateDirectory(setPath.FolderCompare);
                Application.Idle += CompearImageMode;

            } else if (global.StepTest.Contains(CameraMode.CheckLed)) {
                flag.CheckLed = true;
                addStepLedToolStripMenuItem.Visible = true;
                setPath.FolderCheckLed = Path.Combine(AppFilePath.ConfigRoot, global.StepTest) + Path.DirectorySeparatorChar;
                if (!Directory.Exists(setPath.FolderCheckLed))
                    Directory.CreateDirectory(setPath.FolderCheckLed);
                InitCheckLedMode();
                Application.Idle += CheckLedMode;

            } else if (global.StepTest.Contains(CameraMode.BlinkLed)) {
                flag.BlinkLed = true;
                setPath.FolderBlinkLed = Path.Combine(AppFilePath.ConfigRoot, global.StepTest) + Path.DirectorySeparatorChar;
                if (!Directory.Exists(setPath.FolderBlinkLed))
                    Directory.CreateDirectory(setPath.FolderBlinkLed);
                Application.Idle += BlinkLedMode;

            } else {
                // Normal mode — hide settings menus that are mode-specific
                setCameraToolStripMenuItem.Visible   = false;
                setDebugToolStripMenuItem.Visible    = false;
                frameToolStripMenuItem.Visible       = false;
                configLeToolStripMenuItem.Visible    = false;
                Application.Idle += NormalMode;
            }
        }
    }
}
