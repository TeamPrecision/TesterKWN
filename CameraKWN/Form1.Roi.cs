//----------------------------------------------------------------------------
// Form1 (partial) — ROI rectangle drawing (mouse drag on pictureBox1)
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ZXing;

namespace camera_show {
    public partial class Form1 {

        // ── Static ROI state ──────────────────────────────────────────────────
        /// <summary>
        /// The current region-of-interest rectangle.  Static so AutoScale can
        /// write to it via <c>Form1.Rect = ...</c>.
        /// </summary>
        public static Rectangle Rect { get; set; }

        private bool    _isMouseDown;
        private Point   _startLocation;
        private Point   _endLocation;

        // ── Mouse event handlers ──────────────────────────────────────────────

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (!setCamera.FlagOpen)           return;
            if (e.Button != MouseButtons.Left) return;
            _isMouseDown   = true;
            _startLocation = e.Location;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (_isMouseDown) {
                _endLocation = e.Location;
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) {
            if (!_isMouseDown) return;

            // Read optional crop padding from config
            int crop = 10;
            try {
                string txt = File.ReadAllText("../../config/CameraShow_RoiCrop.txt");
                if (!string.IsNullOrEmpty(txt))
                    crop = Convert.ToInt32(txt);
            } catch { }

            var r = GetRectangle();
            if (r.Width < crop || r.Height < crop) return;

            _endLocation = e.Location;
            _isMouseDown = false;

            if (global.StepTest.Contains(CameraMode.Read2d)) {
                if (global.Image != null) {
                    var img = global.Image.Copy();
                    img.ROI = r;
                    var cropped = img.Copy();
                    var reader = new BarcodeReader();
                    var result = reader.Decode(cropped.Bitmap);
                    if (result != null)
                        MessageBox.Show(result.ToString());
                }

            } else if (flag.ComPear) {
                if (global.Image != null) {
                    var img = global.Image.Copy();
                    img.ROI = r;
                    img.Copy().Save(setPath.FolderCompare + global.Head + "Image" + global.NumberCompare + ".png");
                }
                r.X      -= crop;
                r.Y      -= crop;
                r.Width  += crop * 2;
                r.Height += crop * 2;
                SaveRectangle(setPath.FolderCompare, global.NumberCompare);
                ctms_compareNext.Visible = true;
                if (global.NumberCompare != 1) {
                    Form1.Rect = r;
                    return;
                }

            } else if (flag.CheckLed) {
                // Save a 1×1 placeholder image so the folder entry exists
                var bmp = new System.Drawing.Bitmap(1, 1,
                    System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                bmp.Save(setPath.FolderCheckLed + global.Head + "Image" + global.NumberCheckLed + ".png");
                SaveRectangle(setPath.FolderCheckLed, global.NumberCheckLed);
                // Apply the new ROI to in-memory detection immediately.
                // Extends the LED arrays when a new slot is being configured.
                int ledIdx = global.NumberCheckLed - 1;
                if (_ledRects != null) {
                    while (_ledRects.Count <= ledIdx) _ledRects.Add(Rectangle.Empty);
                    _ledRects[ledIdx] = r;
                    if (ledIdx >= _ledTotal) {
                        _ledTotal = ledIdx + 1;
                        Array.Resize(ref _ledSustainSw, _ledTotal);
                        Array.Resize(ref _ledStatuses,  _ledTotal);
                        Array.Resize(ref _ledPrevOn,    _ledTotal);
                        if (_ledSustainSw[ledIdx] == null) _ledSustainSw[ledIdx] = new Stopwatch();
                    }
                }
                ctms_checkLedNext.Visible = true;
                if (global.NumberCheckLed != 1) {
                    Form1.Rect = r;
                    return;
                }
            }

            Form1.Rect = r;
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.RectX,      r.X.ToString(),      setPath.StepCsv);
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.RectY,      r.Y.ToString(),      setPath.StepCsv);
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.RectWidth,  r.Width.ToString(),  setPath.StepCsv);
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.RectHeight, r.Height.ToString(), setPath.StepCsv);
            RectSup = r;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            if (_isMouseDown)
                e.Graphics.DrawRectangle(Pens.Red, GetRectangle());
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Rectangle GetRectangle() {
            int x = Math.Min(_startLocation.X, _endLocation.X);
            int y = Math.Min(_startLocation.Y, _endLocation.Y);
            int w = Math.Abs(_startLocation.X - _endLocation.X);
            int h = Math.Abs(_startLocation.Y - _endLocation.Y);
            var r = new Rectangle(x, y, w, h);
            Form1.Rect = r;
            return r;
        }

        private void SaveRectangle(string folder, int number) {
            var rx = new List<string>();
            var ry = new List<string>();
            var rw = new List<string>();
            var rh = new List<string>();

            try {
                rx = File.ReadAllLines(folder + global.Head + AppFilePath.RectX).ToList();
                ry = File.ReadAllLines(folder + global.Head + AppFilePath.RectY).ToList();
                rw = File.ReadAllLines(folder + global.Head + AppFilePath.RectWidth).ToList();
                rh = File.ReadAllLines(folder + global.Head + AppFilePath.RectHeight).ToList();
            } catch {
                for (int i = 0; i < 100; i++) { rx.Add(""); ry.Add(""); rw.Add(""); rh.Add(""); }
            }

            var cur = Form1.Rect;
            rx[number - 1] = cur.X.ToString();
            ry[number - 1] = cur.Y.ToString();
            rw[number - 1] = cur.Width.ToString();
            rh[number - 1] = cur.Height.ToString();

            File.WriteAllLines(folder + global.Head + AppFilePath.RectX,      rx);
            File.WriteAllLines(folder + global.Head + AppFilePath.RectY,      ry);
            File.WriteAllLines(folder + global.Head + AppFilePath.RectWidth,  rw);
            File.WriteAllLines(folder + global.Head + AppFilePath.RectHeight, rh);
        }
    }
}
