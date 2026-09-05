namespace NIDAQTester12;

/// <summary>One averaged / RMS measurement snapshot for all 12 boards.</summary>
public sealed class DaqMeasurement12
{
    public DateTime Timestamp      { get; init; }
    public double[] Tp37Voltage    { get; init; } = Array.Empty<double>(); // [0..11] DC average (V)
    public double[] Tp33Voltage    { get; init; } = Array.Empty<double>(); // [0..11] DC average (V)
    public double[] LineCurrentRms { get; init; } = Array.Empty<double>(); // [0..11] true RMS (A)
}

/// <summary>
/// Acquires data from:
///   NI-9205  @ cDAQ1Mod1 — 24 RSE voltage channels (ai0–ai23)
///     ai(2b)   = TP37 for board b+1   (b = 0..11)
///     ai(2b+1) = TP33 for board b+1
///
///   NI-9227  @ cDAQ1Mod2 — 4 differential current channels (ai0–ai3), boards 1–4
///   NI-9227  @ cDAQ1Mod3 — 4 differential current channels (ai0–ai3), boards 5–8
///   NI-9227  @ cDAQ1Mod4 — 4 differential current channels (ai0–ai3), boards 9–12
///
/// Both tasks run at the same sample rate. ReadBlock() is blocking; call it from
/// a background thread. Returns one DaqMeasurement12 per call:
///   Voltage : DC average   V_avg = (1/N) · Σ vᵢ
///   Current : True RMS     I_rms = √( (1/N) · Σ iᵢ² )
///
/// Rate constraint: NI-9205 has a 250 kS/s aggregate limit across 24 channels,
/// giving ≤ 10 416 S/s per channel. Keep sample rate at or below 10 000 S/s.
/// </summary>
public sealed class DaqManager12 : IDisposable
{
    private const int BoardCount = 12;

    // NI-9205 — all 24 voltage channels in one string (RSE)
    private const string VChans = "cDAQ1Mod1/ai0:23";

    // 3× NI-9227 — added to one task; all use internal 50 mΩ shunt, ±5 A range
    private static readonly string[] IModules =
    [
        "cDAQ1Mod2/ai0:3",   // boards 1–4
        "cDAQ1Mod3/ai0:3",   // boards 5–8
        "cDAQ1Mod4/ai0:3",   // boards 9–12
    ];

    private IntPtr _vTask = IntPtr.Zero;
    private IntPtr _iTask = IntPtr.Zero;
    private bool   _running;
    private int    _samplesPerBlock;

    // ── Initialization ────────────────────────────────────────────────

    public void Initialize(double sampleRate, int samplesPerBlock)
    {
        ClearTasks();
        _samplesPerBlock = samplesPerBlock;
        ulong hwBuf = (ulong)Math.Max(samplesPerBlock * 4, 50_000);

        // Voltage task
        Daqmx.Check(Daqmx.DAQmxCreateTask("VoltTask12", out _vTask), "CreateTask(Volt)");
        Daqmx.Check(Daqmx.DAQmxCreateAIVoltageChan(
            _vTask, VChans, "",
            Daqmx.Val_RSE, -10.0, 10.0, Daqmx.Val_Volts, null),
            "CreateAIVoltageChan");
        Daqmx.Check(Daqmx.DAQmxCfgSampClkTiming(
            _vTask, null, sampleRate,
            Daqmx.Val_Rising, Daqmx.Val_ContSamps, hwBuf),
            "CfgSampClkTiming(Volt)");

        // Current task — one task spanning all three 9227 modules
        Daqmx.Check(Daqmx.DAQmxCreateTask("CurrTask12", out _iTask), "CreateTask(Curr)");
        foreach (string mod in IModules)
        {
            Daqmx.Check(Daqmx.DAQmxCreateAICurrentChan(
                _iTask, mod, "",
                Daqmx.Val_Diff, -5.0, 5.0, Daqmx.Val_Amps,
                Daqmx.Val_Internal, 0.05, null),
                $"CreateAICurrentChan({mod})");
        }
        Daqmx.Check(Daqmx.DAQmxCfgSampClkTiming(
            _iTask, null, sampleRate,
            Daqmx.Val_Rising, Daqmx.Val_ContSamps, hwBuf),
            "CfgSampClkTiming(Curr)");
    }

    public void Start()
    {
        Daqmx.Check(Daqmx.DAQmxStartTask(_vTask), "StartTask(Volt)");
        Daqmx.Check(Daqmx.DAQmxStartTask(_iTask), "StartTask(Curr)");
        _running = true;
    }

    public void Stop()
    {
        _running = false;
        if (_vTask != IntPtr.Zero) { try { Daqmx.DAQmxStopTask(_vTask); } catch { } }
        if (_iTask != IntPtr.Zero) { try { Daqmx.DAQmxStopTask(_iTask); } catch { } }
    }

    // ── Blocking read ─────────────────────────────────────────────────

    /// <summary>
    /// Blocks until samplesPerBlock samples are available, then returns one
    /// DaqMeasurement12. Call from a background thread (Task.Run).
    ///
    /// DAQmxReadAnalogF64 with GroupByChannel layout:
    ///   vArr : [ch0×N, ch1×N, …, ch23×N]
    ///          board b → TP37 = ch(2b), TP33 = ch(2b+1)
    ///   iArr : [Mod2ch0×N, Mod2ch1×N, …, Mod4ch3×N]
    ///          board b (0-indexed) → ch b
    /// </summary>
    public DaqMeasurement12 ReadBlock()
    {
        int n = _samplesPerBlock;
        double[] vArr = new double[24 * n];
        double[] iArr = new double[12 * n];

        Daqmx.Check(Daqmx.DAQmxReadAnalogF64(
            _vTask, n, 10.0,
            Daqmx.Val_GroupByChannel,
            vArr, (uint)(24 * n),
            out _, IntPtr.Zero), "ReadAnalogF64(Volt)");

        Daqmx.Check(Daqmx.DAQmxReadAnalogF64(
            _iTask, n, 10.0,
            Daqmx.Val_GroupByChannel,
            iArr, (uint)(12 * n),
            out _, IntPtr.Zero), "ReadAnalogF64(Curr)");

        double[] tp37 = new double[BoardCount];
        double[] tp33 = new double[BoardCount];
        double[] irms = new double[BoardCount];

        for (int b = 0; b < BoardCount; b++)
        {
            int v37Base = (2 * b) * n;
            int v33Base = (2 * b + 1) * n;
            int iBase   = b * n;

            double v37Sum = 0, v33Sum = 0, iSqSum = 0;
            for (int k = 0; k < n; k++)
            {
                v37Sum  += vArr[v37Base + k];
                v33Sum  += vArr[v33Base + k];
                iSqSum  += iArr[iBase + k] * iArr[iBase + k];
            }
            tp37[b] = v37Sum / n;
            tp33[b] = v33Sum / n;
            irms[b] = Math.Sqrt(iSqSum / n);
        }

        return new DaqMeasurement12
        {
            Timestamp      = DateTime.Now,
            Tp37Voltage    = tp37,
            Tp33Voltage    = tp33,
            LineCurrentRms = irms,
        };
    }

    public bool IsRunning => _running;

    // ── Cleanup ───────────────────────────────────────────────────────

    private void ClearTasks()
    {
        _running = false;
        if (_vTask != IntPtr.Zero) { Daqmx.DAQmxClearTask(_vTask); _vTask = IntPtr.Zero; }
        if (_iTask != IntPtr.Zero) { Daqmx.DAQmxClearTask(_iTask); _iTask = IntPtr.Zero; }
    }

    public void Dispose() { Stop(); ClearTasks(); }
}
