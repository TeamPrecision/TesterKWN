namespace NIDAQTester12;

/// <summary>
/// Pass/fail limits for a single board. All voltage values are in Volts (raw);
/// all current values are in Amperes (raw). Scaling to display units is the
/// caller's responsibility.
///
/// Current spec supports three operating modes (Auto, Mode A/B/C).
/// <see cref="CheckCurrent"/> snaps the measured value to the nearest mode
/// center, then checks whether it falls within ±<see cref="CurrentTolerancePct"/> %.
/// </summary>
/// <remarks>
/// The default limits match the Ultra V1.0 board specification.
/// Construct once and optionally adjust before starting acquisition:
/// <code>
///   var spec = new MeasurementSpec();   // TP37 3.20–3.40 V, TP33 1.50–2.00 V, ±5 %
///
///   // Optional — override any limit:
///   spec.Tp37Min             = 3.25;   // tighten lower voltage bound
///   spec.CurrentTolerancePct = 3.0;    // tighten current tolerance to ±3 %
///   spec.CurrentModeACentre  = 0.010;  // shift Mode A centre to 10 mA
/// </code>
///
/// Per-board check (always pass raw SI values — Volts and Amperes, not mV/mA):
/// <code>
///   bool pass = spec.CheckAll(
///       m.Tp37Voltage[b],
///       m.Tp33Voltage[b],
///       m.LineCurrentRms[b]);
///
///   // Or check channels individually:
///   bool tp37Ok = spec.CheckTp37(m.Tp37Voltage[b]);
///   bool tp33Ok = spec.CheckTp33(m.Tp33Voltage[b]);
///   bool iOk    = spec.CheckCurrent(m.LineCurrentRms[b]);  // auto-snaps to nearest mode
/// </code>
/// </remarks>
public sealed class MeasurementSpec
{
    // ── Voltage limits ────────────────────────────────────────────────
    public double Tp37Min { get; set; } = 3.20;
    public double Tp37Max { get; set; } = 3.40;

    public double Tp33Min { get; set; } = 1.50;
    public double Tp33Max { get; set; } = 2.00;

    // ── Current mode centres (A) ──────────────────────────────────────
    public double CurrentModeACentre { get; set; } = 0.0093;   // 9.3 mA
    public double CurrentModeBCentre { get; set; } = 0.0328;   // 32.8 mA
    public double CurrentModeCCentre { get; set; } = 0.0563;   // 56.3 mA

    public double CurrentTolerancePct { get; set; } = 5.0;     // ±5 %

    // ── Validation ────────────────────────────────────────────────────

    public bool CheckTp37(double v)    => v >= Tp37Min && v <= Tp37Max;
    public bool CheckTp33(double v)    => v >= Tp33Min && v <= Tp33Max;

    public bool CheckCurrent(double i)
    {
        double[] centres = [CurrentModeACentre, CurrentModeBCentre, CurrentModeCCentre];
        double nearest   = centres.MinBy(c => Math.Abs(c - i));
        double tol       = nearest * CurrentTolerancePct / 100.0;
        return Math.Abs(i - nearest) <= tol;
    }

    /// <summary>True only when all three checks pass.</summary>
    public bool CheckAll(double tp37, double tp33, double iRms)
        => CheckTp37(tp37) && CheckTp33(tp33) && CheckCurrent(iRms);
}
