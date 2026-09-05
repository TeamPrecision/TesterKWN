using Emgu.CV.CvEnum;

namespace camera_show {
    /// <summary>Calibrates and records per-head min/max values for each camera property.</summary>
    public class CreateMinMax {
        public void Run(Form1 form) {
            string savedTitle = form.Text;
            form.Text = "...";

            CapProp[] props = GetCapProps();
            string[]  keys  = GetKeys();

            for (int i = 0; i < props.Length; i++) {
                int begin = 0;
                try { begin = (int)form.setCamera.Capture.GetCaptureProperty(props[i]); } catch { }

                double min = GetMin(form, begin, 1000, props[i]);
                double max = GetMax(form, begin, 1000, props[i]);

                for (int head = 1; head <= 10; head++) {
                    if (IsHeadChecked(form, head)) {
                        string prefix = MinMaxKey.Head + head + keys[i];
                        form.setupPay.write_text(prefix + MinMaxKey.Min, min.ToString(), form.setPath.MinmaxCsv);
                        form.setupPay.write_text(prefix + MinMaxKey.Max, max.ToString(), form.setPath.MinmaxCsv);
                    }
                }

                form.setCamera.SetCapture(props[i], begin);
            }

            // Focus calibration is disabled in hardware (SetCapture for Focus is skipped);
            // write zeros so the CSV row exists.
            for (int head = 1; head <= 10; head++) {
                if (IsHeadChecked(form, head)) {
                    form.setupPay.write_text(MinMaxKey.Head + head + MinMaxKey.Focus + MinMaxKey.Min, "0", form.setPath.MinmaxCsv);
                    form.setupPay.write_text(MinMaxKey.Head + head + MinMaxKey.Focus + MinMaxKey.Max, "0", form.setPath.MinmaxCsv);
                }
            }

            form.Text = savedTitle;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static CapProp[] GetCapProps() =>
            new[] { CapProp.Zoom, CapProp.Pan, CapProp.Tilt,
                    CapProp.Contrast, CapProp.Brightness, CapProp.Sharpness };

        private static string[] GetKeys() =>
            new[] { MinMaxKey.Zoom, MinMaxKey.Pan, MinMaxKey.Tilt,
                    MinMaxKey.Contrast, MinMaxKey.Brightness, MinMaxKey.Sharpness };

        private static bool IsHeadChecked(Form1 form, int head) {
            // Use public accessor — config_camN controls are private in Form1.Designer.cs
            return form.IsCameraHeadChecked(head);
        }

        /// <summary>Binary-search downward to find the camera's minimum accepted value.</summary>
        private static int GetMin(Form1 form, int start, int step, CapProp prop) {
            int value   = start;
            int retries = 3;
            while (true) {
                if (step < 1) {
                    if (retries-- > 0) { step = 1; } else break;
                }
                form.setCamera.SetCapture(prop, value - step);
                double actual = form.setCamera.Capture.GetCaptureProperty(prop);
                if ((int)actual != value - step) {
                    step /= 2;
                } else {
                    value -= step;
                    step  /= 2;
                }
            }
            return value;
        }

        /// <summary>Binary-search upward to find the camera's maximum accepted value.</summary>
        private static int GetMax(Form1 form, int start, int step, CapProp prop) {
            int value   = start;
            int retries = 3;
            while (true) {
                if (step < 1) {
                    if (retries-- > 0) { step = 1; } else break;
                }
                form.setCamera.SetCapture(prop, value + step);
                double actual = form.setCamera.Capture.GetCaptureProperty(prop);
                if ((int)actual != value + step) {
                    step /= 2;
                } else {
                    value += step;
                    step  /= 2;
                }
            }
            return value;
        }
    }
}
