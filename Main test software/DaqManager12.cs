using System;

namespace NIDAQTester12;

/// <summary>One averaged / RMS measurement snapshot for all 12 boards.</summary>
/// <remarks>
/// All three arrays are indexed 0–11, where index 0 = Board 1.
/// Values are always in SI units (Volts, Amperes) regardless of display scaling.
/// <code>
///   DaqMeasurement12 m = daq.ReadBlock();
///
///   double tp37V = m.Tp37Voltage[boardIndex];     // DC average, Volts
///   double tp33V = m.Tp33Voltage[boardIndex];     // DC average, Volts
///   double iRms  = m.LineCurrentRms[boardIndex];  // true RMS, Amperes
/// </code>
/// </remarks>
public sealed class DaqMeasurement12
{
    public DateTime Timestamp      { get; init; }
    public double[] Tp37Voltage    { get; init; } = Array.Empty<double>();
    public double[] Tp33Voltage    { get; init; } = Array.Empty<double>();
    public double[] LineCurrentRms { get; init; } = Array.Empty<double>();
}

/// <summary>
/// Acquires voltage (NI-9205 @ cDAQ1Mod1) and current (NI-9227 @ cDAQ1Mod2–4)
/// for all boards defined in <see cref="ChannelMap"/>.
///
/// Both tasks run at the same sample rate. ReadBlock() is blocking; call it from
/// a background thread. Returns one DaqMeasurement12 per call:
///   Voltage : DC average   V_avg = (1/N) · Σ vᵢ
///   Current : True RMS     I_rms = √( (1/N) · Σ iᵢ² )
///
/// Rate constraint: NI-9205 has a 250 kS/s aggregate limit across 24 channels,
/// giving ≤ 10 416 S/s per channel. Keep sample rate at or below 10 000 S/s.
/// </summary>
/// <remarks>
/// Typical usage — call ReadBlock on a background thread so the UI stays responsive:
/// <code>
///   using var daq = new DaqManager12();
///   daq.Initialize(sampleRate: 5000, samplesPerBlock: 500);
///   daq.Start();
///
///   var cts = new CancellationTokenSource();
///   await Task.Run(() =>
///   {
///       while (!cts.Token.IsCancellationRequested)
///       {
///           DaqMeasurement12 m = daq.ReadBlock();   // blocks ~100 ms per call
///           // process m.Tp37Voltage, m.Tp33Voltage, m.LineCurrentRms ...
///       }
///   });
///
///   daq.Stop();
///   // Dispose() is called automatically by the using statement.
/// </code>
///
/// To change pin assignments, edit ChannelMap — that is the only place that needs updating.
/// To use in your own project, add a ProjectReference to NIDAQTester12.Core.csproj,
/// or copy NiDaqmx.cs and DaqManager12.cs into your project.
/// No NuGet packages are required — only the NI-DAQmx driver (nicaiu.dll).
/// </remarks>
public sealed class DaqManager12 : IDisposable
{
    // ── Pin assignments — edit here to change any channel ────────────────────
    //
    // Each row is one board. To reassign a pin, change the string in that row.
    // To add or remove a board, add or remove a row (and update MainForm.N to match).
    // The row order determines the index in DaqMeasurement12 arrays (row 0 = Board 1).
    //
    private static readonly BoardChannels[] ChannelMap =
    [
        //              TP37                  TP33                  Current
        new("cDAQ1Mod1/ai0",  "cDAQ1Mod1/ai1",  "cDAQ1Mod2/ai0"),  // Board  1
        new("cDAQ1Mod1/ai2",  "cDAQ1Mod1/ai3",  "cDAQ1Mod2/ai1"),  // Board  2
        new("cDAQ1Mod1/ai4",  "cDAQ1Mod1/ai5",  "cDAQ1Mod2/ai2"),  // Board  3
        new("cDAQ1Mod1/ai6",  "cDAQ1Mod1/ai7",  "cDAQ1Mod2/ai3"),  // Board  4
        new("cDAQ1Mod1/ai8",  "cDAQ1Mod1/ai9",  "cDAQ1Mod3/ai0"),  // Board  5
        new("cDAQ1Mod1/ai10", "cDAQ1Mod1/ai11", "cDAQ1Mod3/ai1"),  // Board  6
        new("cDAQ1Mod1/ai12", "cDAQ1Mod1/ai13", "cDAQ1Mod3/ai2"),  // Board  7
        new("cDAQ1Mod1/ai14", "cDAQ1Mod1/ai15", "cDAQ1Mod3/ai3"),  // Board  8
        new("cDAQ1Mod1/ai16", "cDAQ1Mod1/ai17", "cDAQ1Mod4/ai0"),  // Board  9
        new("cDAQ1Mod1/ai18", "cDAQ1Mod1/ai19", "cDAQ1Mod4/ai1"),  // Board 10
        new("cDAQ1Mod1/ai20", "cDAQ1Mod1/ai21", "cDAQ1Mod4/ai2"),  // Board 11
        new("cDAQ1Mod1/ai22", "cDAQ1Mod1/ai23", "cDAQ1Mod4/ai3"),  // Board 12
    ];

    private sealed record BoardChannels(string Tp37, string Tp33, string Current);

    // ─────────────────────────────────────────────────────────────────────────

    private IntPtr _vTask = IntPtr.Zero;
    private IntPtr _iTask = IntPtr.Zero;
    private bool   _running;
    private int    _samplesPerBlock;

    // ── Initialization ────────────────────────────────────────────────────────

    public void Initialize(double sampleRate, int samplesPerBlock)
    {
        ClearTasks();
        _samplesPerBlock = samplesPerBlock;
        ulong hwBuf = (ulong)Math.Max(samplesPerBlock * 4, 50_000);

        // Voltage task — channels are added in ChannelMap order: TP37 then TP33 per board.
        // This pairing is what ReadBlock() relies on to index the buffer correctly.
        Daqmx.Check(Daqmx.DAQmxCreateTask("VoltTask12", out _vTask), "CreateTask(Volt)");
        foreach (var board in ChannelMap)
        {
            Daqmx.Check(Daqmx.DAQmxCreateAIVoltageChan(
                _vTask, board.Tp37, "",
                Daqmx.Val_RSE, -10.0, 10.0, Daqmx.Val_Volts, null),
                $"CreateAIVoltageChan({board.Tp37})");
            Daqmx.Check(Daqmx.DAQmxCreateAIVoltageChan(
                _vTask, board.Tp33, "",
                Daqmx.Val_RSE, -10.0, 10.0, Daqmx.Val_Volts, null),
                $"CreateAIVoltageChan({board.Tp33})");
        }
        Daqmx.Check(Daqmx.DAQmxCfgSampClkTiming(
            _vTask, null, sampleRate,
            Daqmx.Val_Rising, Daqmx.Val_ContSamps, hwBuf),
            "CfgSampClkTiming(Volt)");

        // Current task — one channel per board, in ChannelMap order.
        Daqmx.Check(Daqmx.DAQmxCreateTask("CurrTask12", out _iTask), "CreateTask(Curr)");
        foreach (var board in ChannelMap)
        {
            Daqmx.Check(Daqmx.DAQmxCreateAICurrentChan(
                _iTask, board.Current, "",
                Daqmx.Val_Diff, -5.0, 5.0, Daqmx.Val_Amps,
                Daqmx.Val_Internal, 0.05, null),
                $"CreateAICurrentChan({board.Current})");
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

    public bool IsRunning => _running;

    // ── Blocking read ─────────────────────────────────────────────────────────

    /// <summary>
    /// Blocks until samplesPerBlock samples are available in the hardware buffer,
    /// then returns one DaqMeasurement12 with all boards computed.
    /// Call from a background thread (Task.Run).
    /// </summary>
    public DaqMeasurement12 ReadBlock()
    {
        int n          = _samplesPerBlock;
        int boardCount = ChannelMap.Length;

        // 2 voltage channels per board (TP37 + TP33), 1 current channel per board.
        double[] vArr = new double[2 * boardCount * n];
        double[] iArr = new double[    boardCount * n];

        Daqmx.Check(Daqmx.DAQmxReadAnalogF64(
            _vTask, n, 10.0,
            Daqmx.Val_GroupByChannel,
            vArr, (uint)(2 * boardCount * n),
            out _, IntPtr.Zero), "ReadAnalogF64(Volt)");

        Daqmx.Check(Daqmx.DAQmxReadAnalogF64(
            _iTask, n, 10.0,
            Daqmx.Val_GroupByChannel,
            iArr, (uint)(boardCount * n),
            out _, IntPtr.Zero), "ReadAnalogF64(Curr)");

        double[] tp37 = new double[boardCount];
        double[] tp33 = new double[boardCount];
        double[] irms = new double[boardCount];

        for (int b = 0; b < boardCount; b++)
        {
            // GroupByChannel buffer layout: [ channel 0 × N | channel 1 × N | … ]
            // Initialize() adds channels as: TP37[0], TP33[0], TP37[1], TP33[1], …
            // So for board b: TP37 is at voltage channel index 2b, TP33 at 2b+1.
            // Current channels are added one-per-board, so board b is at current channel b.
            int tp37Offset = (2 * b)     * n;
            int tp33Offset = (2 * b + 1) * n;
            int iOffset    =  b          * n;

            double v37Sum = 0, v33Sum = 0, iSqSum = 0;
            for (int k = 0; k < n; k++)
            {
                v37Sum  += vArr[tp37Offset + k];
                v33Sum  += vArr[tp33Offset + k];
                iSqSum  += iArr[iOffset    + k] * iArr[iOffset + k];
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

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void ClearTasks()
    {
        _running = false;
        if (_vTask != IntPtr.Zero) { Daqmx.DAQmxClearTask(_vTask); _vTask = IntPtr.Zero; }
        if (_iTask != IntPtr.Zero) { Daqmx.DAQmxClearTask(_iTask); _iTask = IntPtr.Zero; }
    }

    public void Dispose() { Stop(); ClearTasks(); }
}
