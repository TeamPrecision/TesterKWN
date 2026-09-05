//----------------------------------------------------------------------------
// Form1 (partial) — startup setup and configuration loading
//----------------------------------------------------------------------------
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.VisualBasic.Devices;

namespace camera_show {
    public partial class Form1 {

        // ── Startup helpers ───────────────────────────────────────────────────

        private void GetHead() {
            global.Head = File.ReadAllText(AppFilePath.Head);
            File.WriteAllText(AppFilePath.TricExe, string.Empty);
        }

        private void GetStepTest() {
            try {
                global.StepTest = File.ReadAllText(
                    AppFilePath.Folder + global.Head + AppFilePath.StepTest);
            } catch {
                global.StepTest = CameraMode.Normal;
            }
        }

        private void SetAllPath() {
            setPath.HeadTxt   = AppFilePath.Folder + global.Head;
            setPath.HeadCsv   = MinMaxKey.Head      + global.Head;
            setPath.MinmaxCsv = MinMaxKey.NameFile   + global.StepTest;
            setPath.ResultTxt = "test_head_" + global.Head + "_result.txt";
            setPath.StepCsv   = "cameraStep_" + global.StepTest;
        }

        private void GenFileList() {
            if (!File.Exists(AppFilePath.List))
                File.WriteAllText(AppFilePath.List, string.Empty);
        }

        private void GetSetPort() {
            try {
                File.ReadAllText(AppFilePath.SetPort);  // just check existence
                File.Delete(AppFilePath.SetPort);
                flag.SetPort = true;
            } catch { }
        }

        private void GetDebug() {
            try {
                flag.Debug = Convert.ToBoolean(
                    File.ReadAllText(setPath.HeadTxt + AppFilePath.Debug));
            } catch { }
        }

        private void GetTimeOut() {
            try {
                global.TimeOut = Convert.ToInt32(
                    File.ReadAllText(setPath.HeadTxt + AppFilePath.TimeOut));
            } catch { }
            SetEditValueCamDiv3();
        }

        private void SetEditValueCamDiv3() {
            editValueCamDiv3.Time   = global.TimeOut / 3;
            editValueCamDiv3.State1 = false;
            editValueCamDiv3.State2 = false;
        }

        private void GetAutoFocus() {
            try {
                flag.AutoFocus = Convert.ToBoolean(
                    File.ReadAllText(setPath.HeadTxt + AppFilePath.AutoFocus));
            } catch { }
        }

        private void GetPort() {
            try {
                global.Port = Convert.ToInt32(
                    setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Port, setPath.StepCsv));
            } catch {
                global.StepTest = CameraMode.Normal;
            }
        }

        private void WindowCheck() {
            var info = new ComputerInfo();
            if (info.OSFullName.Contains("Windows 7"))
                _captureApi = Emgu.CV.VideoCapture.API.Any;
        }

        // ── Config file loading ───────────────────────────────────────────────

        // ── Public accessors for helper classes (designer controls are private) ─

        /// <summary>Process timeout text box value; used by AutoScale.</summary>
        public string ProcessTimeoutText    => Process_timeout_?.Text    ?? "500";
        /// <summary>Process ROI offset text box value; used by AutoScale.</summary>
        public string ProcessRoiText        => Process_roi_?.Text        ?? "10";
        /// <summary>Process scale limit text box value; used by AutoScale.</summary>
        public string ProcessScaleLimitText => Process_scale_limit_?.Text ?? "40";
        /// <summary>Process scale next text box value; used by AutoScale.</summary>
        public string ProcessScaleNextText  => Process_scale_next_?.Text  ?? "2";
        /// <summary>Whether the ROI-set checkbox is checked; used by AutoScale.</summary>
        public bool ProcessRoiSetChecked    => process_roi_set?.Checked   ?? false;

        /// <summary>
        /// Returns true if the CreateMinMax form has camera head <paramref name="head"/> checked.
        /// Used by CreateMinMax to avoid accessing private designer controls directly.
        /// </summary>
        public bool IsCameraHeadChecked(int head) {
            ToolStripMenuItem[] items = new ToolStripMenuItem[] {
                config_cam1,  config_cam2,  config_cam3,
                config_cam4,  config_cam5,  config_cam6,
                config_cam7,  config_cam8,  config_cam9,  config_cam10
            };
            return head >= 1 && head <= 10 && items[head - 1].Checked;
        }

        // ── Config file loading ───────────────────────────────────────────────

        private void ReadConfigFile() {
            try {
                setCamera.ProcessInt = Convert.ToInt32(
                    setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Process, setPath.StepCsv));

                // ROI rectangle
                var r = Form1.Rect;
                r.X      = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.RectX,      setPath.StepCsv));
                r.Y      = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.RectY,      setPath.StepCsv));
                r.Width  = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.RectWidth,  setPath.StepCsv));
                r.Height = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.RectHeight, setPath.StepCsv));
                Form1.Rect = r;

                Process_timeout_.Text     = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.StartTimeOut,    setPath.StepCsv);
                process_roi_set.Checked   = Convert.ToBoolean(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.RoiMoveSet,      setPath.StepCsv));
                Process_roi_.Text         = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.RoiMove,         setPath.StepCsv);
                Process_scale_limit_.Text = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.AutoScaleLimit,  setPath.StepCsv);
                Process_scale_next_.Text  = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.AutoScaleNext,   setPath.StepCsv);
                global.DigitSn            = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.DigitSN,      setPath.StepCsv));
                global.AdjustDegree       = Convert.ToInt32(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.AdjustDegree, setPath.StepCsv));
                ctms_setGammaAddressCsv.Text = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.Gamma, setPath.StepCsv);
            } catch { }

            RectSup = Form1.Rect;
            ctms_digitSn.Text      = global.DigitSn.ToString();
            ctms_adjustDegree.Text = global.AdjustDegree.ToString();

            ctms_focusAutoTrue.Checked  = flag.AutoFocus;
            ctms_focusAutoFalse.Checked = !flag.AutoFocus;

            try {
                setCamera.HsvFlag    = Convert.ToBoolean(setupPay.read_text(setPath.HeadCsv + HeadConfigKey.HsvFlag,    setPath.StepCsv));
                setCamera.HsvMask    = Convert.ToInt32  (setupPay.read_text(setPath.HeadCsv + HeadConfigKey.HsvMask,    setPath.StepCsv));
                setCamera.HsvTimeout = Convert.ToInt32  (setupPay.read_text(setPath.HeadCsv + HeadConfigKey.HsvTimeOut, setPath.StepCsv));

                string   hsvAll = setupPay.read_text(setPath.HeadCsv + HeadConfigKey.HsvFormat, setPath.StepCsv);
                string[] parts  = hsvAll.Split(' ');
                int[]    sv     = new int[6];
                for (int i = 0; i < 6; i++) sv[i] = Convert.ToInt32(parts[i]);

                if (setCamera.HsvFlag) {
                    setCamera.HsvLow  = new Hsv(sv[0], sv[2], sv[4]);
                    setCamera.HsvHigh = new Hsv(sv[1], sv[3], sv[5]);
                    setCamera.BgrLow  = new Bgr();
                    setCamera.BgrHigh = new Bgr();
                } else {
                    setCamera.BgrLow  = new Bgr(sv[0], sv[2], sv[4]);
                    setCamera.BgrHigh = new Bgr(sv[1], sv[3], sv[5]);
                    setCamera.HsvLow  = new Hsv();
                    setCamera.HsvHigh = new Hsv();
                }
            } catch { }

            this.Text        = global.Head + "." + global.StepTest;
            this.ClientSize  = new Size(setCamera.Capture.Width, setCamera.Capture.Height);
            pictureBox1.Size = new Size(setCamera.Capture.Width, setCamera.Capture.Height);
            pictureBox1.Location = new Point(0, 0);

            global.StopWatchHsv.Restart();
            global.StopWatch.Restart();
        }
    }
}
