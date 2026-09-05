using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace camera_show {
    /// <summary>
    /// Manages the VideoCapture instance and the "Set Camera" settings dialog.
    /// BUG FIXES:
    ///   • SetCapture now checks the skip list before applying a property.
    ///   • LogSendError uses a using block so StreamWriter is always released.
    ///   • SetCapture value == -1 is treated as "skip" (unchanged).
    /// </summary>
    public class SetCamera {
        // ── Camera capture ───────────────────────────────────────────────────
        public VideoCapture Capture    { get; set; }

        // ── Processing config ────────────────────────────────────────────────
        public int  ProcessInt   { get; set; } = 170;
        public bool ProcessFlag  { get; set; }

        // ── Color filter config ──────────────────────────────────────────────
        public bool HsvFlag      { get; set; }
        public int  HsvMask      { get; set; } = 10;
        public int  HsvTimeout   { get; set; } = 100;
        public bool HsvTestFlag  { get; set; }
        public Hsv  HsvLow       { get; set; }
        public Hsv  HsvHigh      { get; set; }
        public Bgr  BgrLow       { get; set; }
        public Bgr  BgrHigh      { get; set; }

        // ── Skip settings (Issue #3) ─────────────────────────────────────────
        public CameraSkipSettings SkipSettings { get; } = new CameraSkipSettings();

        // ── Settings form open flag ──────────────────────────────────────────
        public bool FlagOpen     { get; set; }

        // ── Scroll-bar UI controls (populated by Show()) ─────────────────────
        public Form        SettingsForm      { get; private set; }
        public HScrollBar  ZoomHScroll       { get; private set; }
        public Label       ZoomValue         { get; private set; }
        public HScrollBar  PanHScroll        { get; private set; }
        public Label       PanValue          { get; private set; }
        public HScrollBar  TiltHScroll       { get; private set; }
        public Label       TiltValue         { get; private set; }
        public HScrollBar  ContrastHScroll   { get; private set; }
        public Label       ContrastValue     { get; private set; }
        public HScrollBar  BrightnessHScroll { get; private set; }
        public Label       BrightnessValue   { get; private set; }
        public HScrollBar  FocusHScroll      { get; private set; }
        public Label       FocusValue        { get; private set; }
        public HScrollBar  ProcessHScroll    { get; private set; }
        public Label       ProcessValue      { get; private set; }
        public Button      ProcessButton     { get; private set; }
        public HScrollBar  SaturationHScroll { get; private set; }
        public Label       SaturationValue   { get; private set; }
        public HScrollBar  SharpnessHScroll  { get; private set; }
        public Label       SharpnessValue    { get; private set; }
        public HScrollBar  GainHScroll       { get; private set; }
        public Label       GainValue         { get; private set; }
        public HScrollBar  GammaHScroll      { get; private set; }
        public Label       GammaValue        { get; private set; }
        public TextBox     BgrTextBox        { get; private set; }
        public TextBox     HsvTextBox        { get; private set; }
        public Button      HsvButton         { get; private set; }
        public TextBox     HsvMaskTextBox    { get; private set; }
        public TextBox     TimeoutTextBox    { get; private set; }
        public CheckBox    ShowAllCheckBox   { get; private set; }

        private readonly Form1 _main;

        public SetCamera(Form1 main) {
            _main = main;
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Apply a camera property, respecting the skip list.
        /// Returns true if the property was applied (or skipped) successfully.
        /// </summary>
        public bool SetCapture(CapProp capProp, double value) {
            if (value == -1)                      return true; // sentinel: leave unchanged
            if (SkipSettings.ShouldSkip(capProp)) return true; // Issue #3: user-configured skip
            return Capture.SetCaptureProperty(capProp, value);
        }

        /// <summary>Read min or max calibration value from the minmax CSV.</summary>
        public int GetMinMaxValue(string type, string minmax) {
            string raw = _main.setupPay.read_text(
                _main.setPath.HeadCsv + type + minmax,
                _main.setPath.MinmaxCsv);
            return Convert.ToInt32(raw);
        }

        // ── Settings dialog ──────────────────────────────────────────────────

        public void Show() {
            BuildAndShowForm();
            FlagOpen = true;
        }

        private void BuildAndShowForm() {
            SettingsForm = new Form {
                Size = new Size(430, 420),
                Text = "Camera Settings"
            };
            SettingsForm.FormClosed += _main.SetCameraFormClosed;

            int y = 1;

            // Properties cannot be used as `out` targets — use locals then assign.
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Zoom,       CapProp.Zoom,       ref y, out s, out l, false); ZoomHScroll       = s; ZoomValue       = l; }
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Pan,        CapProp.Pan,        ref y, out s, out l, false); PanHScroll        = s; PanValue        = l; }
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Tilt,       CapProp.Tilt,       ref y, out s, out l, false); TiltHScroll       = s; TiltValue       = l; }
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Contrast,   CapProp.Contrast,   ref y, out s, out l, false); ContrastHScroll   = s; ContrastValue   = l; }
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Brightness, CapProp.Brightness, ref y, out s, out l, false); BrightnessHScroll = s; BrightnessValue = l; }

            // Focus row (disabled scroll — auto-focus managed separately)
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Focus, null, ref y, out s, out l, true); FocusHScroll = s; FocusValue = l; }
            var focusAutoBtn = new Button {
                Text = "Auto", Visible = false, Size = new Size(40, 20),
                Location = new Point(FocusHScroll.Size.Width + 40, FocusHScroll.Location.Y)
            };
            focusAutoBtn.Click += FocusClick;
            SettingsForm.Controls.Add(focusAutoBtn);

            // Process threshold row
            var processLabel = new Label { Text = HeadConfigKey.Process, Size = new Size(300, 15), Location = new Point(1, y) };
            y += 15;
            SettingsForm.Controls.Add(processLabel);
            ProcessHScroll  = new HScrollBar { Minimum = 0, Maximum = 255, Value = ProcessInt, LargeChange = 1, Size = new Size(300, 20), Location = new Point(1, y), Enabled = false };
            ProcessHScroll.Scroll += ProcessScroll;
            ProcessValue    = new Label { Text = ProcessHScroll.Value.ToString(), Size = new Size(30, 15), Location = new Point(306, y + 2) };
            ProcessButton   = new Button { Text = ProcessFlag.ToString(), Size = new Size(40, 20), Visible = false, Location = new Point(346, y) };
            ProcessButton.Click += ProcessClick;
            SettingsForm.Controls.Add(ProcessHScroll);
            SettingsForm.Controls.Add(ProcessValue);
            SettingsForm.Controls.Add(ProcessButton);
            y += 25;

            // BGR colour range
            var bgrLabel = new Label { Text = "bgr: \"BlueLow BlueHigh GreenLow GreenHigh RedLow RedHigh\"", Size = new Size(420, 15), Location = new Point(1, y) };
            y += 15;
            SettingsForm.Controls.Add(bgrLabel);
            BgrTextBox = new TextBox {
                Text = $"{BgrLow.Blue} {BgrHigh.Blue} {BgrLow.Green} {BgrHigh.Green} {BgrLow.Red} {BgrHigh.Red}",
                Size = new Size(180, 20), Location = new Point(1, y)
            };
            BgrTextBox.KeyDown += BgrKeyDown;
            SettingsForm.Controls.Add(BgrTextBox);
            var maskLabel = new Label { Text = "Mask:", Size = new Size(40, 15), Location = new Point(265, y + 2) };
            SettingsForm.Controls.Add(maskLabel);
            HsvMaskTextBox = new TextBox { Text = HsvMask.ToString(), Size = new Size(75, 20), Location = new Point(305, y) };
            HsvMaskTextBox.KeyDown += HsvMaskKeyDown;
            SettingsForm.Controls.Add(HsvMaskTextBox);
            var exampleBtn = new Button { Text = "Example", Size = new Size(60, 20), Location = new Point(185, y) };
            exampleBtn.Click += ExampleButtonClick;
            SettingsForm.Controls.Add(exampleBtn);
            y += 25;

            // HSV colour range
            var hsvLabel = new Label { Text = "hsv: \"HueLow HueHigh SatLow SatHigh ValLow ValHigh\"", Size = new Size(420, 15), Location = new Point(1, y) };
            y += 15;
            SettingsForm.Controls.Add(hsvLabel);
            HsvTextBox = new TextBox {
                Text = $"{HsvLow.Hue} {HsvHigh.Hue} {HsvLow.Satuation} {HsvHigh.Satuation} {HsvLow.Value} {HsvHigh.Value}",
                Size = new Size(180, 20), Location = new Point(1, y)
            };
            HsvTextBox.KeyDown += HsvKeyDown;
            SettingsForm.Controls.Add(HsvTextBox);
            var timeoutLabel = new Label { Text = "Timeout:", Size = new Size(50, 15), Location = new Point(185, y + 2) };
            SettingsForm.Controls.Add(timeoutLabel);
            TimeoutTextBox = new TextBox { Text = HsvTimeout.ToString(), Size = new Size(60, 20), Location = new Point(238, y) };
            TimeoutTextBox.KeyDown += TimeoutKeyDown;
            SettingsForm.Controls.Add(TimeoutTextBox);
            var msLabel = new Label { Text = "ms", Size = new Size(25, 15), Location = new Point(302, y + 2) };
            SettingsForm.Controls.Add(msLabel);
            HsvButton = new Button { Text = HsvTestFlag.ToString(), Size = new Size(40, 20), Location = new Point(330, y) };
            HsvButton.Click += HsvClick;
            SettingsForm.Controls.Add(HsvButton);
            y += 25;

            // "Show all" expander
            ShowAllCheckBox = new CheckBox { Location = new Point(385, 0) };
            ShowAllCheckBox.Click += ShowAllClick;
            SettingsForm.Controls.Add(ShowAllCheckBox);

            // Extended rows (hidden until ShowAll is checked)
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Saturation + " (Address)", CapProp.Saturation, ref y, out s, out l, false, hidden: true); SaturationHScroll = s; SaturationValue = l; }
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Sharpness,                CapProp.Sharpness,  ref y, out s, out l, false, hidden: true); SharpnessHScroll  = s; SharpnessValue  = l; }
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Gain + " (Address)",      CapProp.Gain,       ref y, out s, out l, false, hidden: true); GainHScroll       = s; GainValue       = l; }
            { HScrollBar s; Label l; AddScrollRow(HeadConfigKey.Gamma + " (Address)",     CapProp.Gamma,      ref y, out s, out l, false, hidden: true); GammaHScroll      = s; GammaValue      = l; }

            // Skip Settings editor (Issue #3)
            y += 5;
            var skipLabel = new Label { Text = "Skip Settings (one CapProp per line):", Size = new Size(300, 15), Location = new Point(1, y) };
            SettingsForm.Controls.Add(skipLabel);
            y += 15;
            var skipBox = new TextBox {
                Multiline = true, Size = new Size(300, 60), Location = new Point(1, y),
                ScrollBars = ScrollBars.Vertical,
                Text = SkipSettings.ToDisplayText()
            };
            var skipSaveBtn = new Button { Text = "Save", Size = new Size(50, 20), Location = new Point(310, y) };
            skipSaveBtn.Click += (s, e) => {
                SkipSettings.SaveFromText(_main.setPath.HeadTxt, skipBox.Text);
                MessageBox.Show("Skip settings saved.");
            };
            SettingsForm.Controls.Add(skipBox);
            SettingsForm.Controls.Add(skipSaveBtn);
            y += 70;

            SettingsForm.ClientSize = new Size(430, y + 10);
            SettingsForm.Show();
        }

        /// <summary>
        /// Helper: adds a labelled horizontal scroll row to the settings form.
        /// If <paramref name="capProp"/> is null the scrollbar is read-only (no Scroll event).
        /// Uses local variables for the lambda closure — <c>out</c> params cannot be
        /// captured in lambdas (CS1628) and properties cannot be passed as <c>out</c>
        /// targets (CS0206), so locals are used and assigned to the out params at the end.
        /// </summary>
        private void AddScrollRow(string labelText, CapProp? capProp, ref int y,
            out HScrollBar outScroll, out Label outValueLabel,
            bool disabled, bool hidden = false)
        {
            var lbl = new Label { Text = labelText, Size = new Size(300, 15), Location = new Point(1, y), Visible = !hidden };
            SettingsForm.Controls.Add(lbl);
            y += 15;

            // Local vars — lambdas can close over locals but not over out/ref params.
            var scrollLocal = new HScrollBar {
                Minimum = -999, Maximum = 999, LargeChange = 1,
                Size = new Size(300, 20), Location = new Point(1, y),
                Enabled = !disabled, Visible = !hidden
            };

            // Create the label first so the lambda can reference it.
            var valueLabelLocal = new Label {
                Text = "0", Size = new Size(60, 15),
                Location = new Point(306, y + 2), Visible = !hidden
            };

            if (capProp.HasValue) {
                try { scrollLocal.Value = (int)Capture.GetCaptureProperty(capProp.Value); } catch { }
                valueLabelLocal.Text = scrollLocal.Value.ToString();
                // Capture locals in the closure (cannot capture out params).
                var sc = scrollLocal;
                var vl = valueLabelLocal;
                var cp = capProp.Value;
                scrollLocal.Scroll += (s, e) => {
                    vl.Text = sc.Value.ToString();
                    SetCapture(cp, sc.Value);
                };
            }

            SettingsForm.Controls.Add(scrollLocal);
            SettingsForm.Controls.Add(valueLabelLocal);
            y += 25;

            // Assign back to out params after locals are fully initialised.
            outScroll      = scrollLocal;
            outValueLabel  = valueLabelLocal;
        }

        // ── Scroll/button event handlers ─────────────────────────────────────

        private void ProcessScroll(object sender, ScrollEventArgs e) {
            ProcessValue.Text = ProcessHScroll.Value.ToString();
            ProcessInt = ProcessHScroll.Value;
        }
        private void ProcessClick(object sender, EventArgs e) {
            ProcessFlag = !ProcessFlag;
            ProcessButton.Text = ProcessFlag.ToString();
        }
        private void FocusClick(object sender, EventArgs e) {
            FocusValue.Text = FocusHScroll.Value.ToString();
        }
        private void BgrKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            int[] v = ParseSixInts(BgrTextBox.Text);
            if (v == null) { MessageBox.Show("Invalid format"); return; }
            BgrLow  = new Bgr(v[0], v[2], v[4]);
            BgrHigh = new Bgr(v[1], v[3], v[5]);
            HsvFlag = false;
        }
        private void HsvKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            int[] v = ParseSixInts(HsvTextBox.Text);
            if (v == null) { MessageBox.Show("Invalid format"); return; }
            HsvLow  = new Hsv(v[0], v[2], v[4]);
            HsvHigh = new Hsv(v[1], v[3], v[5]);
            HsvFlag = true;
        }
        private void HsvMaskKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            if (int.TryParse(HsvMaskTextBox.Text, out int val))
                HsvMask = val;
            else
                MessageBox.Show("Invalid format");
        }
        private void HsvClick(object sender, EventArgs e) {
            HsvTestFlag = !HsvTestFlag;
            HsvButton.Text = HsvTestFlag.ToString();
        }
        private void TimeoutKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyValue != 13) return;
            if (int.TryParse(TimeoutTextBox.Text, out int val))
                HsvTimeout = val;
            else
                MessageBox.Show("Invalid format");
        }
        private void ExampleButtonClick(object sender, EventArgs e) {
            MessageBox.Show("green : bgr : 0 100 100 255 0 50\r\nred   : hsv : 0 60 0 255 150 255");
        }
        private void ShowAllClick(object sender, EventArgs e) {
            bool show = ShowAllCheckBox.Checked;
            foreach (Control c in new Control[] {
                SaturationHScroll, SaturationValue,
                SharpnessHScroll, SharpnessValue,
                GainHScroll, GainValue,
                GammaHScroll, GammaValue
            }) {
                c.Visible = show;
            }
            SettingsForm.ClientSize = show
                ? new Size(430, SettingsForm.ClientSize.Height + 104)
                : new Size(430, SettingsForm.ClientSize.Height - 104);
        }

        // ── Utility ──────────────────────────────────────────────────────────

        private static int[] ParseSixInts(string text) {
            if (text == null) return null;
            var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6) return null;
            int[] r = new int[6];
            for (int i = 0; i < 6; i++)
                if (!int.TryParse(parts[i], out r[i])) return null;
            return r;
        }
    }
}
