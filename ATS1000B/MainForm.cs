using System;
using System.Drawing;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATS1000B
{
    public sealed class MainForm : Form
    {
        private AtsController _ctrl;
        private readonly System.Windows.Forms.Timer _pollTimer;
        private volatile bool _polling;

        // Connection bar
        private ComboBox      _cmbPort, _cmbBaud;
        private NumericUpDown _nudAddr;
        private Button        _btnConnect, _btnDisconnect, _btnRefreshPorts;
        private Label         _lblStatus;

        // Set-parameter inputs
        private NumericUpDown _nudVoltage, _nudFreq, _nudCurrLim, _nudRamp;
        private ComboBox      _cmbRange;
        private Button        _btnSetV, _btnSetF, _btnSetI, _btnSetRamp, _btnSetRange;

        // Measurement labels
        private Label  _lblV, _lblF, _lblI, _lblP, _lblPF, _lblOut;
        private Button _btnRefreshNow, _btnOutputOn, _btnOutputOff;

        // Log
        private RichTextBox _rtbLog;

        public MainForm()
        {
            BuildUi();
            _pollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _pollTimer.Tick += (s, e) => Poll();
        }

        // ── UI construction ────────────────────────────────────────────────

        private void BuildUi()
        {
            Text          = "ATS-1000B  ·  Programmable AC Power Supply Controller";
            Size          = new Size(920, 700);
            MinimumSize   = new Size(880, 640);
            StartPosition = FormStartPosition.CenterScreen;
            Font          = new Font("Segoe UI", 9f);

            Controls.Add(BuildSplit());    // Fill  – added first
            Controls.Add(BuildLog());      // Bottom
            Controls.Add(BuildConnBar());  // Top

            SetParamControlsEnabled(false);
        }

        private GroupBox BuildConnBar()
        {
            var grp  = new GroupBox { Text = "Connection", Dock = DockStyle.Top, Height = 65, Padding = new Padding(6, 2, 6, 4) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };

            flow.Controls.Add(Lbl("Port:"));
            _cmbPort = new ComboBox { Width = 90, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(2, 4, 6, 0) };
            RefreshPorts();
            flow.Controls.Add(_cmbPort);

            flow.Controls.Add(Lbl("Baud:"));
            _cmbBaud = new ComboBox { Width = 95, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(2, 4, 6, 0) };
            foreach (int b in new[] { 600, 9600, 19200, 38400, 57600, 115200 }) _cmbBaud.Items.Add(b);
            _cmbBaud.SelectedItem = 9600;
            flow.Controls.Add(_cmbBaud);

            flow.Controls.Add(Lbl("Device Address:"));
            _nudAddr = new NumericUpDown { Width = 60, Minimum = 1, Maximum = 255, Value = 1, Margin = new Padding(2, 4, 10, 0) };
            flow.Controls.Add(_nudAddr);

            _btnConnect      = MakeBtn("Connect",    Color.FromArgb(144, 238, 144), BtnConnect_Click);
            _btnDisconnect   = MakeBtn("Disconnect", Color.FromArgb(255, 160, 160), BtnDisconnect_Click);
            _btnRefreshPorts = MakeBtn("⟳ Ports",   Color.FromArgb(173, 216, 230), (s, e) => RefreshPorts());
            _btnDisconnect.Enabled = false;

            flow.Controls.Add(_btnConnect);
            flow.Controls.Add(_btnDisconnect);
            flow.Controls.Add(_btnRefreshPorts);

            _lblStatus = new Label
            {
                Text      = "● Disconnected",
                ForeColor = Color.Gray,
                Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                AutoSize  = true,
                Padding   = new Padding(12, 5, 0, 0)
            };
            flow.Controls.Add(_lblStatus);

            grp.Controls.Add(flow);
            return grp;
        }

        private GroupBox BuildLog()
        {
            var grp = new GroupBox { Text = "Communication Log", Dock = DockStyle.Bottom, Height = 140, Padding = new Padding(5) };
            _rtbLog = new RichTextBox
            {
                Dock       = DockStyle.Fill,
                ReadOnly   = true,
                Font       = new Font("Consolas", 8.5f),
                BackColor  = Color.FromArgb(18, 18, 18),
                ForeColor  = Color.LimeGreen,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            grp.Controls.Add(_rtbLog);
            return grp;
        }

        private SplitContainer BuildSplit()
        {
            var split = new SplitContainer { Dock = DockStyle.Fill };
            Load += (s, e) =>
            {
                split.Panel1MinSize    = 280;
                split.Panel2MinSize    = 380;
                split.SplitterDistance = 310;
            };
            BuildSetPanel(split.Panel1);
            BuildMeasPanel(split.Panel2);
            return split;
        }

        private void BuildSetPanel(SplitterPanel panel)
        {
            var grp = new GroupBox { Text = "Set Parameters", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 8) };
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 5 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
            for (int i = 0; i < 5; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            _nudVoltage = new NumericUpDown { Minimum = 0, Maximum = 9999, DecimalPlaces = 1, Increment = 1m,    Value = 220m, Dock = DockStyle.Fill };
            _btnSetV    = SetBtn();
            _btnSetV.Click += (s, e) => SafeRun(() => { _ctrl.SetVoltage((double)_nudVoltage.Value);     Log($"Set Voltage → {_nudVoltage.Value:F1} V"); });
            AddRow(tbl, 0, "Voltage (V):", _nudVoltage, _btnSetV);

            _nudFreq = new NumericUpDown { Minimum = 0, Maximum = 999, DecimalPlaces = 1, Increment = 0.1m, Value = 50m, Dock = DockStyle.Fill };
            _btnSetF = SetBtn();
            _btnSetF.Click += (s, e) => SafeRun(() => { _ctrl.SetFrequency((double)_nudFreq.Value);       Log($"Set Frequency → {_nudFreq.Value:F1} Hz"); });
            AddRow(tbl, 1, "Frequency (Hz):", _nudFreq, _btnSetF);

            _nudCurrLim = new NumericUpDown { Minimum = 0, Maximum = 9999, DecimalPlaces = 1, Increment = 0.1m, Value = 10m, Dock = DockStyle.Fill };
            _btnSetI    = SetBtn();
            _btnSetI.Click += (s, e) => SafeRun(() => { _ctrl.SetCurrentLimit((double)_nudCurrLim.Value); Log($"Set Current Limit → {_nudCurrLim.Value:F1} A"); });
            AddRow(tbl, 2, "Current Limit (A):", _nudCurrLim, _btnSetI);

            _nudRamp    = new NumericUpDown { Minimum = 0, Maximum = 9999, DecimalPlaces = 1, Increment = 0.1m, Value = 0m, Dock = DockStyle.Fill };
            _btnSetRamp = SetBtn();
            _btnSetRamp.Click += (s, e) => SafeRun(() => { _ctrl.SetRampUpTime((double)_nudRamp.Value);   Log($"Set Ramp-up Time → {_nudRamp.Value:F1} s"); });
            AddRow(tbl, 3, "Ramp-up Time (s):", _nudRamp, _btnSetRamp);

            _cmbRange    = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            _cmbRange.Items.AddRange(new object[] { "Auto", "Low", "High" });
            _cmbRange.SelectedIndex = 0;
            _btnSetRange = SetBtn();
            _btnSetRange.Click += (s, e) => SafeRun(() => { _ctrl.SetVoltageRange((byte)_cmbRange.SelectedIndex); Log($"Set Voltage Range → {_cmbRange.Text}"); });
            AddRow(tbl, 4, "Voltage Range:", _cmbRange, _btnSetRange);

            grp.Controls.Add(tbl);
            panel.Controls.Add(grp);
        }

        private void BuildMeasPanel(SplitterPanel panel)
        {
            var grp = new GroupBox { Text = "Live Measurements  (auto-refresh every 1 s)", Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 8) };
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8 };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            for (int i = 0; i < 6; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Refresh button
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Output ON/OFF buttons

            var valFont = new Font("Consolas", 15f, FontStyle.Bold);
            Label MLabel(string t) => new Label { Text = t, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 12, 0) };
            Label MValue() => new Label { Text = "--", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = valFont, ForeColor = Color.DarkBlue };

            tbl.Controls.Add(MLabel("Voltage:"),       0, 0); _lblV  = MValue(); tbl.Controls.Add(_lblV,  1, 0);
            tbl.Controls.Add(MLabel("Frequency:"),     0, 1); _lblF  = MValue(); tbl.Controls.Add(_lblF,  1, 1);
            tbl.Controls.Add(MLabel("Current:"),       0, 2); _lblI  = MValue(); tbl.Controls.Add(_lblI,  1, 2);
            tbl.Controls.Add(MLabel("Power:"),         0, 3); _lblP  = MValue(); tbl.Controls.Add(_lblP,  1, 3);
            tbl.Controls.Add(MLabel("Power Factor:"),  0, 4); _lblPF = MValue(); tbl.Controls.Add(_lblPF, 1, 4);
            tbl.Controls.Add(MLabel("Output Status:"), 0, 5);
            _lblOut = new Label { Text = "--", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 14f, FontStyle.Bold) };
            tbl.Controls.Add(_lblOut, 1, 5);

            _btnRefreshNow = new Button { Text = "⟳  Refresh Now", Dock = DockStyle.Fill, Margin = new Padding(5, 5, 5, 3) };
            _btnRefreshNow.Click += (s, e) => Poll();
            tbl.SetColumnSpan(_btnRefreshNow, 2);
            tbl.Controls.Add(_btnRefreshNow, 0, 6);

            var pnlOutput = new Panel { Dock = DockStyle.Fill };
            _btnOutputOn = new Button
            {
                Text      = "▶  Output ON",
                BackColor = Color.FromArgb(144, 238, 144),
                UseVisualStyleBackColor = false
            };
            _btnOutputOff = new Button
            {
                Text      = "■  Output OFF",
                BackColor = Color.FromArgb(255, 160, 160),
                UseVisualStyleBackColor = false
            };
            _btnOutputOn.Click  += (s, e) => SafeRun(() => { _ctrl.SetOutput(true);  Log("Output ON"); });
            _btnOutputOff.Click += (s, e) => SafeRun(() => { _ctrl.SetOutput(false); Log("Output OFF"); });
            pnlOutput.Controls.Add(_btnOutputOn);
            pnlOutput.Controls.Add(_btnOutputOff);
            pnlOutput.Resize += (s, e) =>
            {
                int half = pnlOutput.ClientSize.Width / 2 - 4;
                _btnOutputOn.SetBounds(0, 2, half, pnlOutput.ClientSize.Height - 4);
                _btnOutputOff.SetBounds(half + 8, 2, half, pnlOutput.ClientSize.Height - 4);
            };
            tbl.SetColumnSpan(pnlOutput, 2);
            tbl.Controls.Add(pnlOutput, 0, 7);

            grp.Controls.Add(tbl);
            panel.Controls.Add(grp);
        }

        // ── Event handlers ─────────────────────────────────────────────────

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (_cmbPort.SelectedItem == null)
            {
                MessageBox.Show("Select a COM port first.", "No Port Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                int  baud = (int)_cmbBaud.SelectedItem;
                byte addr = (byte)_nudAddr.Value;
                _ctrl = new AtsController(_cmbPort.SelectedItem.ToString(), baud, addr);

                _btnConnect.Enabled    = false;
                _btnDisconnect.Enabled = true;
                _lblStatus.Text        = "● Connected";
                _lblStatus.ForeColor   = Color.Green;
                SetParamControlsEnabled(true);
                _pollTimer.Start();
                Log($"Connected  {_cmbPort.SelectedItem}  @  {baud} bps  |  address 0x{addr:X2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"Connection error: {ex.Message}");
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            _pollTimer.Stop();
            var ctrl = _ctrl;
            _ctrl = null;
            Task.Run(() => { try { ctrl?.Dispose(); } catch { } });

            _btnConnect.Enabled    = true;
            _btnDisconnect.Enabled = false;
            _lblStatus.Text        = "● Disconnected";
            _lblStatus.ForeColor   = Color.Gray;
            SetParamControlsEnabled(false);
            ResetMeasurements();
            Log("Disconnected.");
        }

        // ── Polling ────────────────────────────────────────────────────────

        private void Poll()
        {
            var ctrl = _ctrl;
            if (ctrl == null || _polling) return;
            _polling = true;

            Task.Run(() =>
            {
                try
                {
                    double v  = ctrl.ReadVoltage();
                    double f  = ctrl.ReadFrequency();
                    double i  = ctrl.ReadCurrent();
                    double p  = ctrl.ReadPower();
                    double pf = ctrl.ReadPowerFactor();
                    bool   on = ctrl.ReadOutputStatus();

                    BeginInvoke(new Action(() =>
                    {
                        _lblV.Text        = $"{v:F1} V";
                        _lblF.Text        = $"{f:F1} Hz";
                        _lblI.Text        = $"{i:F3} A";
                        _lblP.Text        = $"{p:F1} W";
                        _lblPF.Text       = pf.ToString("F3");
                        _lblOut.Text      = on ? "ON" : "OFF";
                        _lblOut.ForeColor = on ? Color.DarkGreen : Color.DarkRed;
                    }));
                }
                catch (Exception ex)
                {
                    BeginInvoke(new Action(() => Log($"Poll error: {ex.Message}")));
                }
                finally
                {
                    _polling = false;
                }
            });
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private void SafeRun(Action action)
        {
            if (_ctrl == null)
            {
                MessageBox.Show("Not connected to device.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try   { action(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Command failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Log($"Error: {ex.Message}");
            }
        }

        private void RefreshPorts()
        {
            string current = _cmbPort.SelectedItem?.ToString();
            _cmbPort.Items.Clear();
            _cmbPort.Items.AddRange(SerialPort.GetPortNames());
            if (current != null && _cmbPort.Items.Contains(current))
                _cmbPort.SelectedItem = current;
            else if (_cmbPort.Items.Count > 0)
                _cmbPort.SelectedIndex = 0;
        }

        private void SetParamControlsEnabled(bool enabled)
        {
            foreach (Button b in new[] { _btnSetV, _btnSetF, _btnSetI, _btnSetRamp, _btnSetRange, _btnRefreshNow, _btnOutputOn, _btnOutputOff })
                b.Enabled = enabled;
            foreach (NumericUpDown n in new[] { _nudVoltage, _nudFreq, _nudCurrLim, _nudRamp })
                n.Enabled = enabled;
            _cmbRange.Enabled = enabled;
        }

        private void ResetMeasurements()
        {
            foreach (Label l in new[] { _lblV, _lblF, _lblI, _lblP, _lblPF }) l.Text = "--";
            _lblOut.Text      = "--";
            _lblOut.ForeColor = Color.DarkBlue;
        }

        private void Log(string msg)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => Log(msg))); return; }
            _rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}]  {msg}\n");
            _rtbLog.ScrollToCaret();
        }

        private static void AddRow(TableLayoutPanel tbl, int row, string label, Control input, Button btn)
        {
            tbl.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(0, 0, 6, 0) }, 0, row);
            tbl.Controls.Add(input, 1, row);
            tbl.Controls.Add(btn,   2, row);
        }

        private static Label  Lbl(string text) => new Label { Text = text, AutoSize = true, Padding = new Padding(0, 7, 2, 0) };
        private static Button SetBtn() => new Button { Text = "Set", Dock = DockStyle.Fill, Margin = new Padding(3, 4, 0, 4) };

        private static Button MakeBtn(string text, Color bg, EventHandler onClick)
        {
            var b = new Button { Text = text, Width = 92, BackColor = bg, UseVisualStyleBackColor = false, Margin = new Padding(0, 3, 4, 0) };
            b.Click += onClick;
            return b;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _pollTimer.Stop();
            _ctrl?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
