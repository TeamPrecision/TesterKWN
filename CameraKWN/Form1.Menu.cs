//----------------------------------------------------------------------------
// Form1 (partial) — menu click handlers + snapshot (Issue #2) + ProcessCmdKey
//----------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV.CvEnum;

namespace camera_show {
    public partial class Form1 {

        // ── Context menu — basic actions ──────────────────────────────────────

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) {
            if (flag.ResultPass)
                File.WriteAllText(setPath.ResultTxt, global.ResultBackup + "\r\nPASS");
            else
                File.WriteAllText(setPath.ResultTxt, "Unreadable\r\nFAIL");
            this.Close();
        }

        private void setCameraToolStripMenuItem_Click(object sender, EventArgs e) {
            setCamera.Show();
        }

        private void setDebugToolStripMenuItem_Click(object sender, EventArgs e) {
            flag.Debug = false;
        }

        private void setPortToolStripMenuItem_Click(object sender, EventArgs e) {
            File.WriteAllText(AppFilePath.SetPort, string.Empty);
            setCamera.Capture?.Dispose();
            Application.Restart();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e) {
            createMinMax.Run(this);
        }

        // ── Frame-size menu ───────────────────────────────────────────────────

        private void Frame_height_Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Frame Height\r\nDefault = 800", "Frame Height", Frame_height.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            Frame_height.Text = result.ToString();
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.FrameHeight, result.ToString(), setPath.StepCsv);
        }

        private void Frame_width_Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Frame Width\r\nDefault = 600", "Frame Width", Frame_width.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            Frame_width.Text = result.ToString();
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.FrameWidth, result.ToString(), setPath.StepCsv);
        }

        // ── Process-parameter menus ───────────────────────────────────────────

        private void Process_timeout__Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Process Timeout\r\nDefault = 500", "Process Timeout", Process_timeout_.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            Process_timeout_.Text = result.ToString();
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.StartTimeOut, result.ToString(), setPath.StepCsv);
        }

        private void Process_roi__Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Process Roi\r\nDefault = 10", "Process Roi", Process_roi_.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            Process_roi_.Text = result.ToString();
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.RoiMove, result.ToString(), setPath.StepCsv);
        }

        private void process_roi_set_Click(object sender, EventArgs e) {
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.RoiMoveSet,
                process_roi_set.Checked.ToString(), setPath.StepCsv);
        }

        private void Process_scale_limit__Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Process Scale Limit\r\nDefault = 40", "Process Scale Limit",
                Process_scale_limit_.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            Process_scale_limit_.Text = result.ToString();
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.AutoScaleLimit, result.ToString(), setPath.StepCsv);
        }

        private void Process_scale_next__Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Process Scale Next\r\nDefault = 2", "Process Scale Next",
                Process_scale_next_.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            Process_scale_next_.Text = result.ToString();
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.AutoScaleNext, result.ToString(), setPath.StepCsv);
        }

        // ── Auto-focus toggle ─────────────────────────────────────────────────

        private void ctms_focusAutoTrue_Click(object sender, EventArgs e) {
            ctms_focusAutoTrue.Checked  = true;
            ctms_focusAutoFalse.Checked = false;
            File.WriteAllText(setPath.HeadTxt + AppFilePath.AutoFocus, true.ToString());
            flag.AutoFocus = true;
        }

        private void ctms_focusAutoFalse_Click(object sender, EventArgs e) {
            ctms_focusAutoTrue.Checked  = false;
            ctms_focusAutoFalse.Checked = true;
            File.WriteAllText(setPath.HeadTxt + AppFilePath.AutoFocus, false.ToString());
            flag.AutoFocus = false;
        }

        // ── Digit SN and rotation ─────────────────────────────────────────────

        private void ctms_digitSn_Click(object sender, EventArgs e) {
            // BUG FIX: original default was Process_scale_limit_.Text — should be ctms_digitSn.Text
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Digit SN\r\nDefault = 13", "Digit SN", ctms_digitSn.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            ctms_digitSn.Text = result.ToString();
            global.DigitSn    = result;
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.DigitSN, result.ToString(), setPath.StepCsv);
        }

        private void ctms_adjustDegree_Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Adjust Degree\r\nDefault = 0", "Adjust Degree", ctms_adjustDegree.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            ctms_adjustDegree.Text = result.ToString();
            global.AdjustDegree    = result;
            setupPay.write_text(setPath.HeadCsv + HeadConfigKey.AdjustDegree, result.ToString(), setPath.StepCsv);
        }

        // ── Compare-image step navigation ────────────────────────────────────

        private void ctms_compareNext_Click(object sender, EventArgs e) {
            global.NumberCompare++;
            ctms_compareNumber.Text  = global.NumberCompare.ToString();
            ctms_compareNext.Visible = false;
        }

        private void ctms_compareNumber_Click(object sender, EventArgs e) {
            while (true) {
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Keys Number Step in Compare Image\r\nInteger = 1 - 100",
                    "Number Compare Image", ctms_compareNumber.Text, 500, 300);
                if (string.IsNullOrEmpty(input)) return;
                if (!int.TryParse(input, out int result) || result < 1 || result > 100) {
                    MessageBox.Show("not format"); continue;
                }
                global.NumberCompare    = result;
                ctms_compareNumber.Text = result.ToString();
                break;
            }
        }

        // ── Check-LED step navigation ─────────────────────────────────────────

        private void ctms_checkLedNext_Click(object sender, EventArgs e) {
            global.NumberCheckLed++;
            ctms_checkLedNumber.Text  = global.NumberCheckLed.ToString();
            ctms_checkLedNext.Visible = false;

            // Sequential debug mode: "next" can add a new LED slot beyond the current total.
            // Extend all per-LED arrays so CheckLedMode does not call FinishLedMode prematurely.
            if (_sequentialDebugMode && global.NumberCheckLed > _ledTotal) {
                int newIdx = global.NumberCheckLed - 1;
                _ledTotal  = global.NumberCheckLed;
                Array.Resize(ref _ledSustainSw, _ledTotal);
                Array.Resize(ref _ledStatuses,  _ledTotal);
                Array.Resize(ref _ledPrevOn,    _ledTotal);
                if (_ledSustainSw[newIdx] == null) _ledSustainSw[newIdx] = new Stopwatch();
                while (_ledRects.Count <= newIdx) _ledRects.Add(Form1.Rect);
                if (_ledRects[newIdx] == Rectangle.Empty) _ledRects[newIdx] = Form1.Rect;
                Form1.Rect = _ledRects[newIdx];
                // Persist the new slot so it survives a program restart.
                try {
                    var bmp = new System.Drawing.Bitmap(1, 1,
                        System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                    bmp.Save(setPath.FolderCheckLed + global.Head + "Image" + global.NumberCheckLed + ".png");
                    SaveRectangle(setPath.FolderCheckLed, global.NumberCheckLed);
                } catch { }
            }
        }

        private void ctms_checkLedNumber_Click(object sender, EventArgs e) {
            while (true) {
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Keys Number Step in Check Led\r\nInteger = 1 - 100",
                    "Number Check Led", ctms_checkLedNumber.Text, 500, 300);
                if (string.IsNullOrEmpty(input)) return;
                if (!int.TryParse(input, out int result) || result < 1 || result > 100) {
                    MessageBox.Show("not format"); continue;
                }
                global.NumberCheckLed    = result;
                ctms_checkLedNumber.Text = result.ToString();
                break;
            }
        }

        // ── Camera address / gamma ────────────────────────────────────────────

        private void ctms_setGammaAddressCsv_Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Set gamma address in csv", "Gamma Address", ctms_setGammaAddressCsv.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!CheckPasswordSetAddress()) return;
            ctms_setGammaAddressCsv.Text = input;
            setupPay.write_text(HeadConfigKey.Head + global.Head + HeadConfigKey.Gamma, input, setPath.StepCsv);
        }

        private void ctms_setGammaAddress_Click(object sender, EventArgs e) {
            if (!CheckPasswordSetAddress()) return;
            ctms_setGammaAddress.Checked = true;
            ctms_usePort.Checked         = false;
            setupPay.write_text(HeadConfigKey.Head + global.Head + HeadConfigKey.SetAddress,
                CameraAddress.Gamma, setPath.StepCsv);
        }

        private void ctms_usePort_Click(object sender, EventArgs e) {
            if (!CheckPasswordSetAddress()) return;
            ctms_setGammaAddress.Checked = false;
            ctms_usePort.Checked         = true;
            setupPay.write_text(HeadConfigKey.Head + global.Head + HeadConfigKey.SetAddress,
                CameraAddress.Port, setPath.StepCsv);
        }

        private void ctms_propertySetting_Click(object sender, EventArgs e) {
            if (!CheckPasswordSetAddress()) return;
            setCamera.SetCapture(CapProp.Settings, 0);
        }

        private void ctms_saveConfig_Click(object sender, EventArgs e) {
            if (!CheckPasswordSetAddress()) return;
            ReadConfigCameraToCsvFile();
        }

        private void ctms_roiCrop_Click(object sender, EventArgs e) {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Keys Roi Crop\r\nDefault = 10", "Roi Crop", ctms_roiCrop.Text, 500, 300);
            if (string.IsNullOrEmpty(input)) return;
            if (!int.TryParse(input, out int result)) { MessageBox.Show("not format"); return; }
            ctms_roiCrop.Text = result.ToString();
            File.WriteAllText("../../config/CameraShow_RoiCrop.txt", ctms_roiCrop.Text);
        }

        // ── SetCamera form closed ─────────────────────────────────────────────

        public void SetCameraFormClosed(object sender, FormClosedEventArgs e) {
            string headSup = HeadConfigKey.Head + global.Head;

            if (flag.CheckLed || flag.BlinkLed) {
                string colourSup;
                if (setCamera.HsvFlag) {
                    colourSup = setCamera.HsvLow.Hue        + " " + setCamera.HsvHigh.Hue        + " " +
                                setCamera.HsvLow.Satuation  + " " + setCamera.HsvHigh.Satuation  + " " +
                                setCamera.HsvLow.Value      + " " + setCamera.HsvHigh.Value;
                } else {
                    colourSup = setCamera.BgrLow.Blue   + " " + setCamera.BgrHigh.Blue   + " " +
                                setCamera.BgrLow.Green  + " " + setCamera.BgrHigh.Green  + " " +
                                setCamera.BgrLow.Red    + " " + setCamera.BgrHigh.Red;
                }
                setupPay.write_text(headSup + HeadConfigKey.HsvFormat,  colourSup,                        setPath.StepCsv);
                setupPay.write_text(headSup + HeadConfigKey.HsvFlag,    setCamera.HsvFlag.ToString(),      setPath.StepCsv);
                setupPay.write_text(headSup + HeadConfigKey.HsvMask,    setCamera.HsvMask.ToString(),      setPath.StepCsv);
                setupPay.write_text(headSup + HeadConfigKey.HsvTimeOut, setCamera.HsvTimeout.ToString(),   setPath.StepCsv);
            }

            setCamera.FlagOpen   = false;
            autoScale.FlagIntro  = false;
        }

        // ── Cancel / set-port callbacks (called by FormCancelDialog) ──────────

        public void ButtonCancelClick(object sender, EventArgs e) {
            flag.CloseForm = true;
            formCancel.Form.Close();
        }

        public void ButtonSetPortClick(object sender, EventArgs e) {
            File.WriteAllText(AppFilePath.SetPort, string.Empty);
            flag.CloseForm = true;
            Application.Restart();
        }

        // ── Snapshot (Issue #2) ───────────────────────────────────────────────

        private ToolStripMenuItem _snapshotMenuItem;

        /// <summary>
        /// Adds a "Snapshot (F5)" toggle item to the context menu at startup.
        /// </summary>
        private void AddSnapshotMenuItem() {
            _snapshotMenuItem = new ToolStripMenuItem("Snapshot (F5)");
            _snapshotMenuItem.Click += (s, e2) => ToggleSnapshot();
            contextMenuStrip1.Items.Add(_snapshotMenuItem);
        }

        /// <summary>
        /// Toggles snapshot mode on/off.
        /// ON  — freezes the displayed frame (processing continues on live frames).
        /// OFF — resumes live display.
        /// </summary>
        private void ToggleSnapshot() {
            if (!flag.Snapshot) {
                // Turn ON — capture and freeze the current frame
                if (global.Image == null) {
                    MessageBox.Show("No frame available for snapshot.");
                    return;
                }
                _snapshotImage?.Dispose();
                _snapshotImage = global.Image.Copy();
                flag.Snapshot = true;
                _snapshotMenuItem.Text = "Snapshot: ON (F5)";
                this.Text = "[SNAP] [" + global.Head + "]." + global.StepTest;
            } else {
                // Turn OFF — resume live display
                flag.Snapshot = false;
                _snapshotImage?.Dispose();
                _snapshotImage = null;
                _snapshotMenuItem.Text = "Snapshot (F5)";
                this.Text = "[" + global.Head + "]." + global.StepTest;
            }
        }

        /// <summary>F5 key toggles snapshot mode from anywhere in the form.</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.F5) {
                ToggleSnapshot();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
