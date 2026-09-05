using System.Diagnostics;
using Emgu.CV.CvEnum;

namespace camera_show.MiniClass {
    /// <summary>
    /// Applies a brief ±1 focus "nudge" at 1/3 and 2/3 of the timeout period to
    /// help unstick auto-focus on cameras that support it.
    /// BUG FIX: Both conditions changed from bitwise &amp; to logical &amp;&amp;.
    /// </summary>
    public class EditValueCamDiv3 {
        public readonly Form1 Main;

        /// <summary>Threshold time in ms (set to timeOut / 3 by SetEditValueCamDiv3).</summary>
        public int  Time   { get; set; }
        public bool State1 { get; set; }
        public bool State2 { get; set; }

        private readonly Stopwatch _innerWatch = new Stopwatch();

        public EditValueCamDiv3(Form1 main) {
            Main = main;
        }

        public void Process() {
            long elapsed = Main.global.StopWatch.ElapsedMilliseconds;

            // BUG FIX: was bitwise & — must be logical &&
            if (elapsed >= Time && elapsed < Time * 2 && !State1) {
                State1 = true;
                string sFocus = Main.setupPay.read_text(
                    Main.setPath.HeadCsv + HeadConfigKey.Focus,
                    Main.setPath.StepCsv);
                if (int.TryParse(sFocus, out int focus))
                    Main.setCamera.SetCapture(CapProp.Focus, focus - 1);
                _innerWatch.Restart();
            }

            // BUG FIX: was bitwise & — must be logical &&
            if (_innerWatch.ElapsedMilliseconds >= 1000 && !State2) {
                State2 = true;
                string sFocus = Main.setupPay.read_text(
                    Main.setPath.HeadCsv + HeadConfigKey.Focus,
                    Main.setPath.StepCsv);
                if (int.TryParse(sFocus, out int focus))
                    Main.setCamera.SetCapture(CapProp.Focus, focus);
            }
        }
    }
}
