using System.IO;
using System.Text;

using WinColor = System.Drawing.Color;
using WinLabel = System.Windows.Forms.Label;

namespace NIDAQTester12;

/// <summary>
/// 12-board DUT tester GUI.
///
/// Layout — 4 rows × 3 columns of board cards:
///   Col 0: Boards  1–4   (cDAQ1Mod2 ai0–3)
///   Col 1: Boards  5–8   (cDAQ1Mod3 ai0–3)
///   Col 2: Boards 9–12   (cDAQ1Mod4 ai0–3)
///
/// Voltage unit and current unit are selected globally in the toolbar.
/// GUI refreshes at a fixed ~4 Hz (250 ms timer) regardless of DAQ block rate
/// to eliminate flicker. Board cards turn red/green based on pass/fail spec.
/// </summary>
public sealed class MainForm : Form
{
    private const int N = 12;

    // ── DAQ ──────────────────────────────────────────────────────────
    private readonly DaqManager12    _daq  = new();
    private readonly MeasurementSpec _spec = new();
    private CancellationTokenSource? _cts;

    // Latest measurement written by the background thread, read by the UI timer.
    // Class reference assignment is atomic on all .NET platforms.
    private volatile DaqMeasurement12? _latest;

    // ── State ────────────────────────────────────────────────────────
    private bool          _running;
    private int           _totalBlocks;
    private DateTime      _startTime;
    private StreamWriter? _logWriter;

    // ── Global scale factors ─────────────────────────────────────────
    private double _scaleV = 1.0;   // 1 = V, 1e3 = mV, 1e6 = µV
    private double _scaleI = 1.0;   // 1 = A, 1e3 = mA, 1e6 = µA

    // ── Per-board UI references [0..11] ──────────────────────────────
    private readonly GroupBox[]   _cards       = new GroupBox[N];
    private readonly WinLabel[]   _lblTp37     = new WinLabel[N];
    private readonly WinLabel[]   _lblTp33     = new WinLabel[N];
    private readonly WinLabel[]   _lblI        = new WinLabel[N];
    private readonly WinLabel[]   _lblUnitTp37 = new WinLabel[N];
    private readonly WinLabel[]   _lblUnitTp33 = new WinLabel[N];
    private readonly WinLabel[]   _lblUnitI    = new WinLabel[N];

    // ── Toolbar controls ─────────────────────────────────────────────
    private Button        _btnStart    = null!;
    private NumericUpDown _nudSR       = null!;
    private NumericUpDown _nudSPB      = null!;
    private CheckBox      _chkLog      = null!;
    private ComboBox      _cmbVoltUnit = null!;
    private ComboBox      _cmbCurrUnit = null!;

    // ── Status bar ───────────────────────────────────────────────────
    private WinLabel _lblStatus = null!;
    private WinLabel _lblTime   = null!;
    private WinLabel _lblBlocks = null!;

    // ── UI refresh timer (4 Hz) ───────────────────────────────────────
    private readonly System.Windows.Forms.Timer _uiTimer = new() { Interval = 250 };

    // ── Pass/fail card colours ────────────────────────────────────────
    private static readonly WinColor CardDefault = WinColor.FromArgb(245, 247, 250);
    private static readonly WinColor CardPass    = WinColor.FromArgb(220, 245, 225);
    private static readonly WinColor CardFail    = WinColor.FromArgb(255, 220, 215);

    // ── Accent colors ─────────────────────────────────────────────────
    private static readonly WinColor AccentTp37 = WinColor.FromArgb(30, 100, 210);
    private static readonly WinColor AccentTp33 = WinColor.FromArgb(200, 75, 25);
    private static readonly WinColor AccentI    = WinColor.FromArgb(0, 148, 60);

    // ─────────────────────────────────────────────────────────────────
    public MainForm()
    {
        Text          = "NI cDAQ — Ultra V1.0 DUT Tester  (12 Boards)";
        Size          = new Size(1500, 880);
        MinimumSize   = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Font          = new Font("Segoe UI", 9f);
        BackColor     = WinColor.FromArgb(235, 237, 240);
        DoubleBuffered = true;

        BuildLayout();

        _uiTimer.Tick += OnUiTimer;

        FormClosing += (_, _) =>
        {
            if (_running) StopAcquisition();
            _uiTimer.Dispose();
            _daq.Dispose();
        };
    }

    // ═════════════════════════════════════════════════════════════════
    // Layout construction
    // ═════════════════════════════════════════════════════════════════

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 3,
            ColumnCount = 1,
            Padding     = Padding.Empty
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        Controls.Add(root);

        root.Controls.Add(BuildToolbar(), 0, 0);
        root.Controls.Add(BuildGrid(),    0, 1);
        root.Controls.Add(BuildStatus(),  0, 2);
    }

    // ── Toolbar ───────────────────────────────────────────────────────
    private Control BuildToolbar()
    {
        var outer = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            BackColor   = WinColor.FromArgb(28, 74, 140),
            ColumnCount = 2,
            RowCount    = 1,
            Padding     = new Padding(10, 10, 10, 6)
        };
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var left = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false
        };

        left.Controls.Add(TLbl(
            "Device: cDAQ1  |  9205 @ Mod1 (Voltage)  |  9227 @ Mod2-4 (Current)",
            bold: true));
        left.Controls.Add(TLbl("  |  ", WinColor.FromArgb(90, 130, 200)));
        left.Controls.Add(TLbl("Sample Rate (S/s):"));

        _nudSR = new NumericUpDown
        {
            Minimum = 100, Maximum = 10_000, Value = 5_000,
            Increment = 500, ThousandsSeparator = true,
            Width = 80, Margin = new Padding(3, 3, 12, 3)
        };
        ToolTip.SetToolTip(_nudSR,
            "NI-9205 limit: 250 kS/s aggregate ÷ 24 channels ≈ 10 416 S/s max per channel.");
        left.Controls.Add(_nudSR);

        left.Controls.Add(TLbl("Samples/Block:"));
        _nudSPB = new NumericUpDown
        {
            Minimum = 100, Maximum = 20_000, Value = 500,
            Increment = 100, ThousandsSeparator = true,
            Width = 80, Margin = new Padding(3, 3, 12, 3)
        };
        ToolTip.SetToolTip(_nudSPB,
            "At 5 000 S/s → 500 samples = 100 ms = 6 full 60 Hz cycles.");
        left.Controls.Add(_nudSPB);

        left.Controls.Add(TLbl("  |  ", WinColor.FromArgb(90, 130, 200)));
        left.Controls.Add(TLbl("Voltage:"));
        _cmbVoltUnit = MakeUnitCombo(["V", "mV", "µV"]);
        _cmbVoltUnit.SelectedIndexChanged += (_, _) => OnVoltUnitChanged();
        left.Controls.Add(_cmbVoltUnit);

        left.Controls.Add(TLbl("Current:"));
        _cmbCurrUnit = MakeUnitCombo(["A", "mA", "µA"]);
        _cmbCurrUnit.SelectedIndexChanged += (_, _) => OnCurrUnitChanged();
        left.Controls.Add(_cmbCurrUnit);

        _chkLog = new CheckBox
        {
            Text      = "Log to CSV",
            ForeColor = WinColor.White,
            AutoSize  = true,
            Margin    = new Padding(12, 5, 0, 3)
        };
        left.Controls.Add(_chkLog);

        // Spec settings button
        left.Controls.Add(TLbl("  |  ", WinColor.FromArgb(90, 130, 200)));
        var btnSpec = new Button
        {
            Text      = "⚙  Spec Limits",
            AutoSize  = true,
            Height    = 26,
            BackColor = WinColor.FromArgb(50, 100, 170),
            ForeColor = WinColor.White,
            FlatStyle = FlatStyle.Flat,
            Margin    = new Padding(0, 4, 0, 2)
        };
        btnSpec.FlatAppearance.BorderSize = 0;
        btnSpec.Click += (_, _) =>
        {
            using var dlg = new SpecSettingsDialog(_spec);
            dlg.ShowDialog(this);
        };
        left.Controls.Add(btnSpec);

        outer.Controls.Add(left, 0, 0);

        _btnStart = new Button
        {
            Text      = "▶  START",
            Width     = 120, Height = 36,
            BackColor = WinColor.FromArgb(0, 175, 75),
            ForeColor = WinColor.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            Margin    = new Padding(0, 2, 0, 2)
        };
        _btnStart.FlatAppearance.BorderSize = 0;
        _btnStart.Click += OnStartStop;
        outer.Controls.Add(_btnStart, 1, 0);

        return outer;
    }

    private static readonly ToolTip ToolTip = new();

    private static WinLabel TLbl(string text, WinColor color = default, bool bold = false) => new()
    {
        Text      = text,
        ForeColor = color == default ? WinColor.White : color,
        AutoSize  = true,
        Font      = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular),
        Margin    = new Padding(0, 5, 6, 3)
    };

    private static ComboBox MakeUnitCombo(string[] options)
    {
        var c = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle     = FlatStyle.Flat,
            Font          = new Font("Segoe UI", 9f),
            Width         = 52,
            BackColor     = WinColor.WhiteSmoke,
            Margin        = new Padding(3, 3, 12, 3)
        };
        foreach (var o in options) c.Items.Add(o);
        c.SelectedIndex = 0;
        return c;
    }

    // ── 4-row × 3-column board grid ───────────────────────────────────
    private Control BuildGrid()
    {
        var grid = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 4,
            ColumnCount = 3,
            Padding     = new Padding(4)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

        for (int b = 0; b < N; b++)
            grid.Controls.Add(BuildBoardCard(b), b / 4, b % 4);

        return grid;
    }

    private Control BuildBoardCard(int b)
    {
        var gb = new GroupBox
        {
            Text      = $"Board {b + 1}",
            Dock      = DockStyle.Fill,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = WinColor.FromArgb(28, 74, 140),
            BackColor = CardDefault,
            Margin    = new Padding(3)
        };
        _cards[b] = gb;

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 3, ColumnCount = 3,
            Padding     = new Padding(4, 2, 4, 4),
            BackColor   = WinColor.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34f));

        string vUnit = (string)_cmbVoltUnit.SelectedItem!;
        string iUnit = (string)_cmbCurrUnit.SelectedItem!;

        AddMeasRow(layout, 0, "TP37", AccentTp37,
            out _lblTp37[b], out _lblUnitTp37[b], vUnit);
        AddMeasRow(layout, 1, "TP33", AccentTp33,
            out _lblTp33[b], out _lblUnitTp33[b], vUnit);
        AddMeasRow(layout, 2, "I",    AccentI,
            out _lblI[b],    out _lblUnitI[b],    iUnit);

        gb.Controls.Add(layout);
        return gb;
    }

    private static void AddMeasRow(
        TableLayoutPanel layout, int row,
        string chanName, WinColor accent,
        out WinLabel valueLabel, out WinLabel unitLabel, string defaultUnit)
    {
        layout.Controls.Add(new WinLabel
        {
            Text      = chanName,
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = accent,
            BackColor = WinColor.Transparent,
            Anchor    = AnchorStyles.Left | AnchorStyles.Top,
            Margin    = new Padding(0, 6, 8, 0)
        }, 0, row);

        valueLabel = new WinLabel
        {
            Text      = "---",
            Dock      = DockStyle.Fill,
            Font      = new Font("Consolas", 18f, FontStyle.Bold),
            ForeColor = accent,
            BackColor = WinColor.Transparent,
            TextAlign = ContentAlignment.MiddleRight,
            AutoSize  = false
        };
        layout.Controls.Add(valueLabel, 1, row);

        unitLabel = new WinLabel
        {
            Text      = defaultUnit,
            AutoSize  = true,
            Font      = new Font("Segoe UI", 9f),
            ForeColor = WinColor.DimGray,
            BackColor = WinColor.Transparent,
            Anchor    = AnchorStyles.Right | AnchorStyles.Bottom,
            Margin    = new Padding(6, 0, 0, 6)
        };
        layout.Controls.Add(unitLabel, 2, row);
    }

    // ── Status bar ────────────────────────────────────────────────────
    private Control BuildStatus()
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = WinColor.FromArgb(200, 205, 210) };
        _lblStatus = StsLbl("Status: Idle", 8);
        _lblTime   = StsLbl("Duration: --:--:--", 200);
        _lblBlocks = StsLbl("Blocks: 0", 390);
        p.Controls.AddRange(new Control[] { _lblStatus, _lblTime, _lblBlocks });
        return p;
    }

    private static WinLabel StsLbl(string text, int x) => new()
    {
        Text     = text,
        AutoSize = true,
        Location = new Point(x, 5),
        Font     = new Font("Segoe UI", 8.5f)
    };

    // ═════════════════════════════════════════════════════════════════
    // Unit change handlers
    // ═════════════════════════════════════════════════════════════════

    private static double UnitScale(ComboBox cmb) => cmb.SelectedIndex switch
    {
        1 => 1_000.0,
        2 => 1_000_000.0,
        _ => 1.0
    };

    private void OnVoltUnitChanged()
    {
        _scaleV = UnitScale(_cmbVoltUnit);
        string u = (string)_cmbVoltUnit.SelectedItem!;
        for (int b = 0; b < N; b++)
        {
            _lblUnitTp37[b].Text = u;
            _lblUnitTp33[b].Text = u;
        }
    }

    private void OnCurrUnitChanged()
    {
        _scaleI = UnitScale(_cmbCurrUnit);
        string u = (string)_cmbCurrUnit.SelectedItem!;
        for (int b = 0; b < N; b++)
            _lblUnitI[b].Text = u;
    }

    // ═════════════════════════════════════════════════════════════════
    // Start / Stop
    // ═════════════════════════════════════════════════════════════════

    private void OnStartStop(object? sender, EventArgs e)
    {
        if (_running) StopAcquisition();
        else          StartAcquisition();
    }

    private void StartAcquisition()
    {
        double sampleRate    = (double)_nudSR.Value;
        int    samplesPerBlk = (int)_nudSPB.Value;

        try
        {
            _daq.Initialize(sampleRate, samplesPerBlk);
            _daq.Start();
        }
        catch (DllNotFoundException)
        {
            MessageBox.Show(
                "nicaiu.dll not found.\n\n" +
                "Please install the NI-DAQmx driver:\n" +
                "  https://www.ni.com/en/support/downloads/drivers/download.ni-daq-mx.html\n\n" +
                "After installation, restart this application.",
                "NI-DAQmx Driver Missing",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start DAQ tasks:\n\n{ex.Message}\n\n" +
                "Check that:\n• The NI-DAQmx driver is installed\n" +
                "• The cDAQ chassis is connected and powered\n" +
                "• No other application is using the device.",
                "DAQ Initialization Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (_chkLog.Checked)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"DUT12_Log_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            _logWriter = new StreamWriter(path, append: false);

            string vUnit = (string)_cmbVoltUnit.SelectedItem!;
            string iUnit = (string)_cmbCurrUnit.SelectedItem!;
            var hdr = new StringBuilder("Timestamp");
            for (int b = 1; b <= N; b++)
                hdr.Append($",B{b}_TP37_{vUnit},B{b}_TP33_{vUnit},B{b}_I_{iUnit}");
            _logWriter.WriteLine(hdr);
        }

        _running     = true;
        _totalBlocks = 0;
        _startTime   = DateTime.Now;
        _latest      = null;

        _nudSR.Enabled       = false;
        _nudSPB.Enabled      = false;
        _chkLog.Enabled      = false;
        _cmbVoltUnit.Enabled = false;
        _cmbCurrUnit.Enabled = false;

        _btnStart.Text      = "■  STOP";
        _btnStart.BackColor = WinColor.FromArgb(190, 45, 25);
        _lblStatus.Text     = "Status: Running";

        _cts = new CancellationTokenSource();
        Task.Run(() => AcquisitionLoop(_cts.Token));
        _uiTimer.Start();
    }

    private void StopAcquisition()
    {
        _uiTimer.Stop();
        _cts?.Cancel();
        _daq.Stop();
        _running = false;

        _logWriter?.Flush();
        _logWriter?.Dispose();
        _logWriter = null;

        _nudSR.Enabled       = true;
        _nudSPB.Enabled      = true;
        _chkLog.Enabled      = true;
        _cmbVoltUnit.Enabled = true;
        _cmbCurrUnit.Enabled = true;

        _btnStart.Text      = "▶  START";
        _btnStart.BackColor = WinColor.FromArgb(0, 175, 75);
        _lblStatus.Text     = "Status: Stopped";

        // Reset card colours
        for (int b = 0; b < N; b++)
            _cards[b].BackColor = CardDefault;
    }

    // ═════════════════════════════════════════════════════════════════
    // Background acquisition loop — only stores latest measurement
    // ═════════════════════════════════════════════════════════════════

    private void AcquisitionLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            DaqMeasurement12 m;
            try
            {
                m = _daq.ReadBlock();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    BeginInvoke(() =>
                    {
                        StopAcquisition();
                        MessageBox.Show(
                            $"Read error:\n\n{ex.Message}",
                            "DAQ Read Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    });
                return;
            }

            // Write CSV on the background thread (StreamWriter is not thread-safe,
            // but only this thread writes, so it is fine).
            WriteCsvRow(m);

            _latest = m;   // publish for the UI timer
            _totalBlocks++;
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // UI timer tick — runs on UI thread at ~4 Hz
    // ═════════════════════════════════════════════════════════════════

    private void OnUiTimer(object? sender, EventArgs e)
    {
        var m = _latest;
        if (m == null || !_running) return;

        SuspendLayout();
        for (int b = 0; b < N; b++)
        {
            double v37  = m.Tp37Voltage[b]    * _scaleV;
            double v33  = m.Tp33Voltage[b]    * _scaleV;
            double iVal = m.LineCurrentRms[b] * _scaleI;

            _lblTp37[b].Text = $"{v37:F4}";
            _lblTp33[b].Text = $"{v33:F4}";
            _lblI[b].Text    = $"{iVal:F4}";

            // Pass/fail uses raw (unscaled) values to compare against spec limits in V / A
            bool pass = _spec.CheckAll(
                m.Tp37Voltage[b],
                m.Tp33Voltage[b],
                m.LineCurrentRms[b]);
            _cards[b].BackColor = pass ? CardPass : CardFail;
        }
        ResumeLayout(false);

        _lblTime.Text   = $"Duration: {DateTime.Now - _startTime:hh\\:mm\\:ss}";
        _lblBlocks.Text = $"Blocks: {_totalBlocks}";
    }

    // ═════════════════════════════════════════════════════════════════
    // CSV writing (called from background thread)
    // ═════════════════════════════════════════════════════════════════

    private void WriteCsvRow(DaqMeasurement12 m)
    {
        if (_logWriter == null) return;
        var row = new StringBuilder(m.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        for (int b = 0; b < N; b++)
        {
            row.Append($",{m.Tp37Voltage[b]    * _scaleV:F6}");
            row.Append($",{m.Tp33Voltage[b]    * _scaleV:F6}");
            row.Append($",{m.LineCurrentRms[b] * _scaleI:F6}");
        }
        _logWriter.WriteLine(row);
    }
}
