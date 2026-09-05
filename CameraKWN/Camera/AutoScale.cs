using System;
using System.Diagnostics;
using System.Drawing;

namespace camera_show {
    /// <summary>
    /// Automatically shifts the ROI rectangle in a spiral pattern to help the
    /// barcode reader find the symbol when it is slightly off-centre.
    /// Most camera-parameter sweeps are disabled (commented out) in the original;
    /// only the ROI shift logic is active.
    /// </summary>
    public class AutoScale {
        // ── Configuration (loaded from CSV on first call per job) ─────────────
        private int _scaleLimit    = 40;
        private int _scaleNext     = 2;
        private int _roiMove       = 10;
        private int _startTimeOut  = 500;
        public  bool FlagIntro     { get; set; }

        // ── ROI spiral table ──────────────────────────────────────────────────
        private static readonly int[] RectX = {
            -1,  0,  1, -1, 1, -1, 0, 1, -2, -1,  0,  1,  2, -2, -1,  0,  1,  2, -2, -1, 1, 2, -2, -1, 0, 1, 2, -2, -1,
             0, 1, 2, -3, -2, -1,  0,  1,  2,  3, -3, -2, -1,  0,  1,  2,  3, -3, -2, -1,  0,  1,  2,  3, -3, -2, -1, 1,
             2, 3, -3, -2, -1, 0, 1, 2, 3, -3, -2, -1, 0, 1, 2, 3, -3, -2, -1, 0, 1, 2, 3 };
        private static readonly int[] RectY = {
            -1, -1, -1,  0, 0,  1, 1, 1, -2, -2, -2, -2, -2, -1, -1, -1, -1, -1,  0,  0, 0, 0,  1,  1, 1, 1, 1,  2,  2,
             2, 2, 2, -3, -3, -3, -3, -3, -3, -3, -2, -2, -2, -2, -2, -2, -2, -1, -1, -1, -1, -1, -1, -1,  0,  0,  0, 0,
             0, 0,  1,  1,  1, 1, 1, 1, 1,  2,  2,  2, 2, 2, 2, 2,  3,  3,  3, 3, 3, 3, 3 };

        private int      _rectIndex;
        private int      _stepNumber;
        private readonly Stopwatch _startWatch = new Stopwatch();

        // ── Public entry point ────────────────────────────────────────────────

        public void Run(Form1 form) {
            if (!FlagIntro) {
                Initialise(form);
                FlagIntro   = true;
                _stepNumber = 0;
                _startWatch.Restart();
            }

            if (_roiMove == 0) _rectIndex = RectX.Length; // skip ROI sweep

            switch (_stepNumber) {
                case 0:
                    if (_startWatch.ElapsedMilliseconds > _startTimeOut) {
                        _startWatch.Stop();
                        _stepNumber++;
                    }
                    break;

                case 1:
                    if (!form.ProcessRoiSetChecked) {
                        _stepNumber = 0; // ROI sweep disabled — stay in wait loop
                        break;
                    }
                    if (_rectIndex >= RectX.Length) {
                        Form1.Rect   = form.RectSup;
                        _rectIndex   = 0;
                        break;
                    }
                    var r = form.RectSup;
                    Form1.Rect = new Rectangle(
                        r.X + RectX[_rectIndex] * _roiMove,
                        r.Y + RectY[_rectIndex] * _roiMove,
                        r.Width, r.Height);
                    _rectIndex++;
                    break;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private void Initialise(Form1 form) {
            // Read UI values via public accessors (designer controls are private)
            if (int.TryParse(form.ProcessTimeoutText,    out int to)) _startTimeOut = to;
            if (int.TryParse(form.ProcessRoiText,        out int rm)) _roiMove = rm;
            if (int.TryParse(form.ProcessScaleLimitText, out int sl)) _scaleLimit = sl;
            if (int.TryParse(form.ProcessScaleNextText,  out int sn)) _scaleNext = sn;
            _rectIndex = 0;
        }
    }
}
