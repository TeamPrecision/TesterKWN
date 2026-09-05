using Emgu.CV.CvEnum;

namespace camera_show {
    /// <summary>
    /// Re-applies the baseline camera settings (contrast, brightness, sharpness) when
    /// a recognition attempt fails, in case the settings were perturbed by another process.
    /// </summary>
    public class ReAdjust {
        private bool _initialised;
        private int  _contrast, _brightness, _sharpness;

        public void Run(Form1 form) {
            if (form.flag.Debug) return;

            if (!_initialised) {
                _initialised = true;
                try {
                    _contrast   = int.Parse(form.setupPay.read_text(form.setPath.HeadCsv + HeadConfigKey.Contrast,   form.setPath.StepCsv));
                    _brightness = int.Parse(form.setupPay.read_text(form.setPath.HeadCsv + HeadConfigKey.Brightness, form.setPath.StepCsv));
                    _sharpness  = int.Parse(form.setupPay.read_text(form.setPath.HeadCsv + HeadConfigKey.Sharpness,  form.setPath.StepCsv));
                } catch {
                    _contrast   = (int)form.setCamera.Capture.GetCaptureProperty(CapProp.Contrast);
                    _brightness = (int)form.setCamera.Capture.GetCaptureProperty(CapProp.Brightness);
                    _sharpness  = (int)form.setCamera.Capture.GetCaptureProperty(CapProp.Sharpness);
                }
            }

            form.setCamera.SetCapture(CapProp.Contrast,   _contrast);
            form.setCamera.SetCapture(CapProp.Brightness, _brightness);
            form.setCamera.SetCapture(CapProp.Sharpness,  _sharpness);
        }

        /// <summary>Reset so the next Run() call re-reads the baseline from the CSV.</summary>
        public void Reset() => _initialised = false;
    }
}
