using WinLabel = System.Windows.Forms.Label;

namespace NIDAQTester12;

/// <summary>
/// Modal dialog for editing pass/fail spec limits.
/// Edits a <see cref="MeasurementSpec"/> in place on OK.
/// </summary>
internal sealed class SpecSettingsDialog : Form
{
    private readonly MeasurementSpec _spec;

    // Voltage
    private readonly NumericUpDown _nudTp37Min = MakeNud(0, 10, 3);
    private readonly NumericUpDown _nudTp37Max = MakeNud(0, 10, 3);
    private readonly NumericUpDown _nudTp33Min = MakeNud(0, 10, 3);
    private readonly NumericUpDown _nudTp33Max = MakeNud(0, 10, 3);

    // Current centres — displayed in mA for readability
    private readonly NumericUpDown _nudIA  = MakeNud(0, 1000, 3);
    private readonly NumericUpDown _nudIB  = MakeNud(0, 1000, 3);
    private readonly NumericUpDown _nudIC  = MakeNud(0, 1000, 3);
    private readonly NumericUpDown _nudTol = MakeNud(0.1m, 50, 1);

    public SpecSettingsDialog(MeasurementSpec spec)
    {
        _spec = spec;

        Text            = "Pass / Fail Spec Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        Size            = new Size(360, 400);
        Font            = new Font("Segoe UI", 9f);
        BackColor       = Color.FromArgb(240, 242, 245);

        // Load current values
        _nudTp37Min.Value = (decimal)spec.Tp37Min;
        _nudTp37Max.Value = (decimal)spec.Tp37Max;
        _nudTp33Min.Value = (decimal)spec.Tp33Min;
        _nudTp33Max.Value = (decimal)spec.Tp33Max;
        _nudIA.Value      = (decimal)(spec.CurrentModeACentre * 1000);
        _nudIB.Value      = (decimal)(spec.CurrentModeBCentre * 1000);
        _nudIC.Value      = (decimal)(spec.CurrentModeCCentre * 1000);
        _nudTol.Value     = (decimal)spec.CurrentTolerancePct;

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            RowCount    = 11, ColumnCount = 3,
            Padding     = new Padding(14, 10, 14, 6)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24f));
        for (int i = 0; i < 11; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // ── Headers ──
        layout.Controls.Add(Hdr("Parameter"),  0, 0);
        layout.Controls.Add(Hdr("Min / Ctr"),  1, 0);
        layout.Controls.Add(Hdr("Max / Tol"),  2, 0);

        // ── Voltage rows ──
        layout.Controls.Add(Lbl("TP37  (V)"),     0, 1);
        layout.Controls.Add(_nudTp37Min,           1, 1);
        layout.Controls.Add(_nudTp37Max,           2, 1);

        layout.Controls.Add(Lbl("TP33  (V)"),     0, 2);
        layout.Controls.Add(_nudTp33Min,           1, 2);
        layout.Controls.Add(_nudTp33Max,           2, 2);

        // ── Separator ──
        var sep = new Panel { Height = 1, BackColor = Color.Silver, Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6) };
        layout.Controls.Add(sep, 0, 3);
        layout.SetColumnSpan(sep, 3);

        // ── Current rows ──
        layout.Controls.Add(Hdr2("Current modes — auto-detect nearest (mA)"), 0, 4);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 4)!, 3);

        layout.Controls.Add(Lbl("Mode A centre (mA)"), 0, 5);
        layout.Controls.Add(_nudIA,                     1, 5);
        layout.Controls.Add(new WinLabel(),              2, 5);

        layout.Controls.Add(Lbl("Mode B centre (mA)"), 0, 6);
        layout.Controls.Add(_nudIB,                     1, 6);
        layout.Controls.Add(new WinLabel(),              2, 6);

        layout.Controls.Add(Lbl("Mode C centre (mA)"), 0, 7);
        layout.Controls.Add(_nudIC,                     1, 7);
        layout.Controls.Add(new WinLabel(),              2, 7);

        layout.Controls.Add(Lbl("Tolerance  (±%)"),    0, 8);
        layout.Controls.Add(_nudTol,                    1, 8);
        layout.Controls.Add(new WinLabel(),              2, 8);

        // ── OK / Cancel ──
        var sep2 = new Panel { Height = 1, BackColor = Color.Silver, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
        layout.Controls.Add(sep2, 0, 9);
        layout.SetColumnSpan(sep2, 3);

        var btnPanel = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        var btnOk = new Button
        {
            Text         = "OK",
            DialogResult = DialogResult.OK,
            Width = 75, Height = 28,
            BackColor    = Color.FromArgb(28, 74, 140),
            ForeColor    = Color.White,
            FlatStyle    = FlatStyle.Flat
        };
        btnOk.FlatAppearance.BorderSize = 0;
        var btnCancel = new Button
        {
            Text         = "Cancel",
            DialogResult = DialogResult.Cancel,
            Width = 75, Height = 28
        };
        btnPanel.Controls.Add(btnOk);
        btnPanel.Controls.Add(btnCancel);
        layout.Controls.Add(btnPanel, 0, 10);
        layout.SetColumnSpan(btnPanel, 3);

        Controls.Add(layout);
        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (DialogResult != DialogResult.OK) return;

        if (_nudTp37Min.Value >= _nudTp37Max.Value ||
            _nudTp33Min.Value >= _nudTp33Max.Value)
        {
            MessageBox.Show("Min must be less than Max.", "Invalid Spec",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            e.Cancel = true;
            return;
        }

        _spec.Tp37Min = (double)_nudTp37Min.Value;
        _spec.Tp37Max = (double)_nudTp37Max.Value;
        _spec.Tp33Min = (double)_nudTp33Min.Value;
        _spec.Tp33Max = (double)_nudTp33Max.Value;
        _spec.CurrentModeACentre   = (double)_nudIA.Value  / 1000.0;
        _spec.CurrentModeBCentre   = (double)_nudIB.Value  / 1000.0;
        _spec.CurrentModeCCentre   = (double)_nudIC.Value  / 1000.0;
        _spec.CurrentTolerancePct  = (double)_nudTol.Value;
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static NumericUpDown MakeNud(decimal min, decimal max, int decimals) => new()
    {
        Minimum       = min,
        Maximum       = max,
        DecimalPlaces = decimals,
        Increment     = decimals == 1 ? 0.1m : 0.001m,
        Width         = 72,
        Margin        = new Padding(2, 2, 2, 4)
    };

    private static WinLabel Lbl(string t) => new()
    {
        Text     = t,
        AutoSize = true,
        Margin   = new Padding(0, 6, 6, 2)
    };

    private static WinLabel Hdr(string t) => new()
    {
        Text      = t,
        AutoSize  = true,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        Margin    = new Padding(0, 0, 0, 6)
    };

    private static WinLabel Hdr2(string t) => new()
    {
        Text      = t,
        AutoSize  = true,
        Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold | FontStyle.Italic),
        ForeColor = Color.FromArgb(28, 74, 140),
        Margin    = new Padding(0, 4, 0, 4)
    };
}
