using System.Drawing.Drawing2D;

namespace PlcScada;

public sealed class MainForm : Form
{
    // ── Infrastructure ────────────────────────────────────────────────────────
    private readonly PlcCommunication _plc = new();
    private readonly System.Windows.Forms.Timer _pollTimer;

    // ── Connection bar ────────────────────────────────────────────────────────
    private readonly TextBox _txtIp;
    private readonly NumericUpDown _nudPort;
    private readonly Button _btnConnect;
    private readonly Panel _connLed;
    private readonly Label _lblConnText;

    // ── Left column: X input LEDs ─────────────────────────────────────────────
    private readonly Panel _ledX0, _ledX1, _ledX2, _ledX3, _ledX4, _ledX5;
    private readonly Panel _ledX8, _ledX9, _ledX10, _ledX11, _ledX12, _ledX13;
    private readonly Panel _ledX14, _ledX15;

    // ── Center column: timer display labels ───────────────────────────────────
    private readonly Label _lblElapsed;      // live countdown while running
    private readonly Label _lblSetTime;      // V1000
    private readonly Label _lblResetTime;    // V1001
    private readonly Label _lblDelayStop1;   // V1002
    private readonly Label _lblDelayStop2;   // V1003
    private readonly Label _lblDelayStop3;   // V1004  ×100ms
    private readonly Label _lblDelaySen3;    // V1005

    // ── Center column: timer spinners (editable) ──────────────────────────────
    private readonly NumericUpDown _nudSetTime;
    private readonly NumericUpDown _nudResetTime;
    private readonly NumericUpDown _nudDelayStop1;
    private readonly NumericUpDown _nudDelayStop2;
    private readonly NumericUpDown _nudDelayStop3;
    private readonly NumericUpDown _nudDelaySen3;

    // ── Center column: machine buttons ────────────────────────────────────────
    private readonly Button _btnStart;
    private readonly Button _btnStop;

    // ── Right column: Y output LEDs ───────────────────────────────────────────
    private readonly Panel _ledRed, _ledYellow, _ledGreen, _ledBuzzer;
    private readonly Panel _ledMotor, _ledStop1, _ledStop2, _ledStop3;
    private readonly Panel _ledSolTop, _ledSolBot;

    // ── Right column: M status LEDs ───────────────────────────────────────────
    private readonly Panel _ledReady, _ledOutPass, _ledOutFail;
    private readonly Panel _ledGenAlarm, _ledTimeAlarm, _ledFullAlarm;
    private readonly Panel _ledPcbAlarm, _ledAreaAlarm;

    // ── Right column: PC→PLC toggle buttons ───────────────────────────────────
    private readonly Button _btnInPass;
    private readonly Button _btnInFail;
    private readonly Button _btnRstAlarm;
    private readonly Button _btnDefault;
    private bool _stateInPass, _stateInFail, _stateRstAlarm;

    // ── Status bar ────────────────────────────────────────────────────────────
    private readonly Label _lblStatus;

    // ── Palette ───────────────────────────────────────────────────────────────
    private static readonly Color BgForm    = Color.FromArgb(30, 30, 35);
    private static readonly Color BgPanel   = Color.FromArgb(40, 41, 50);
    private static readonly Color BgInput   = Color.FromArgb(55, 57, 70);
    private static readonly Color BgTimer   = Color.FromArgb(20, 20, 25);
    private static readonly Color FgMain    = Color.FromArgb(215, 218, 228);
    private static readonly Color FgDim     = Color.FromArgb(130, 132, 145);
    private static readonly Color FgHeader  = Color.FromArgb(160, 165, 215);
    private static readonly Color AccBlue   = Color.FromArgb(0, 122, 204);
    private static readonly Color LedOff    = Color.FromArgb(55, 58, 72);
    private static readonly Color LedGreen  = Color.FromArgb(50, 205, 80);
    private static readonly Color LedRed    = Color.FromArgb(215, 50, 50);
    private static readonly Color LedYellow = Color.FromArgb(240, 195, 0);
    private static readonly Color LedBlue   = Color.FromArgb(40, 148, 235);
    private static readonly Color LedOrange = Color.FromArgb(235, 130, 20);

    // ─────────────────────────────────────────────────────────────────────────

    public MainForm()
    {
        SuspendLayout();

        Text            = "Haiwell PLC SCADA  ·  Machine_PC_test-d24";
        ClientSize      = new Size(1080, 680);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = BgForm;
        ForeColor       = FgMain;
        Font            = new Font("Segoe UI", 9f);

        // ── Connection bar ────────────────────────────────────────────────────
        var pConn = MakePanel(this, 6, 6, 1068, 44);
        MakeLabel(pConn, "IP Address:", 8, 13);
        _txtIp = MakeTextBox(pConn, "192.168.1.111", 88, 12, 150);
        MakeLabel(pConn, "Port:", 252, 13);
        _nudPort    = MakePort(pConn, 290, 11);
        _btnConnect = MakeButton(pConn, "Connect", 378, 8, 90, 28, AccBlue);
        _connLed    = MakeLed(pConn, 480, 12);
        _lblConnText = MakeLabel(pConn, "DISCONNECTED", 508, 13, FgDim);
        MakeLabel(pConn, "Read/Write Settings:", 660, 13);
        var btnReadAll  = MakeButton(pConn, "Read", 800, 8, 66, 28, BgInput);
        var btnWriteAll = MakeButton(pConn, "Write", 874, 8, 66, 28, AccBlue);
        btnReadAll.Click  += (_, _) => ReadAllSettings();
        btnWriteAll.Click += (_, _) => WriteAllSettings();

        // ═══════════════════════════════════════════════════════════════════════
        // LEFT COLUMN — X Inputs (y=56, w=260)
        // ═══════════════════════════════════════════════════════════════════════
        var pIn = MakeSection("Inputs  (X Discrete)", 6, 56, 260, 580);

        int row = 36;
        _ledX0  = MakeLedRow(pIn, "x0   Stop  (NC)",       12, row); row += 34;
        _ledX1  = MakeLedRow(pIn, "x1   Start  (NO)",      12, row); row += 34;
        _ledX2  = MakeLedRow(pIn, "x2   Emer  (NC)",       12, row); row += 34;
        _ledX3  = MakeLedRow(pIn, "x3   Reed_Top_Up",      12, row); row += 34;
        _ledX4  = MakeLedRow(pIn, "x4   Reed_Top_Dwn",     12, row); row += 34;
        _ledX5  = MakeLedRow(pIn, "x5   Reed_Bot_Up",      12, row); row += 34;
        MakeLabel(pIn, "x6, x7  — unused", 40, row, FgDim);           row += 28;
        _ledX8  = MakeLedRow(pIn, "x8   Reed_Bot_Dwn",     12, row); row += 34;
        _ledX9  = MakeLedRow(pIn, "x9   Sen1",             12, row); row += 34;
        _ledX10 = MakeLedRow(pIn, "x10  Sen2",             12, row); row += 34;
        _ledX11 = MakeLedRow(pIn, "x11  Sen3",             12, row); row += 34;
        _ledX12 = MakeLedRow(pIn, "x12  Sen4",             12, row); row += 34;
        _ledX13 = MakeLedRow(pIn, "x13  Sen5",             12, row); row += 34;
        _ledX14 = MakeLedRow(pIn, "x14  Area Sen1",        12, row); row += 34;
        _ledX15 = MakeLedRow(pIn, "x15  Area Sen2",        12, row);

        // ═══════════════════════════════════════════════════════════════════════
        // CENTER COLUMN — Timers + Machine Control (x=272, w=402)
        // ═══════════════════════════════════════════════════════════════════════
        var pCenter = MakeSection("Timer Settings  (V Registers)", 272, 56, 402, 430);

        // Elapsed counter display
        var pElapsed = MakePanel(pCenter, 10, 30, 380, 50);
        pElapsed.BackColor = BgTimer;
        MakeLabel(pElapsed, "Elapsed / Count", 8, 6, FgDim);
        _lblElapsed = new Label
        {
            Text = "– – –", Location = new Point(8, 22),
            AutoSize = true, Font = new Font("Consolas", 14f, FontStyle.Bold),
            ForeColor = LedGreen
        };
        pElapsed.Controls.Add(_lblElapsed);

        // Timer rows — each: label | NUD | unit hint | "live" display
        int tr = 90;
        (_nudSetTime,    _lblSetTime)    = MakeTimerRow(pCenter, "V1000  Set Time",       tr,  150, "s",      1, 9999); tr += 52;
        (_nudResetTime,  _lblResetTime)  = MakeTimerRow(pCenter, "V1001  Reset Time",     tr,    3, "s",      1, 9999); tr += 52;
        (_nudDelayStop1, _lblDelayStop1) = MakeTimerRow(pCenter, "V1002  Delay Stopper1", tr,    1, "s",      0, 9999); tr += 52;
        (_nudDelayStop2, _lblDelayStop2) = MakeTimerRow(pCenter, "V1003  Delay Stopper2", tr,    1, "s",      0, 9999); tr += 52;
        (_nudDelayStop3, _lblDelayStop3) = MakeTimerRow(pCenter, "V1004  Delay Stopper3", tr,    5, "×100ms", 0, 9999); tr += 52;
        (_nudDelaySen3,  _lblDelaySen3)  = MakeTimerRow(pCenter, "V1005  Delay Sen3",     tr,    1, "s",      0, 9999);

        // Machine buttons
        var pCtrl = MakeSection("Machine Control", 272, 494, 402, 142);
        _btnStart = MakeBigBtn(pCtrl, "▶   START",  10,  34, 180, 52, Color.FromArgb(38, 158, 55));
        _btnStop  = MakeBigBtn(pCtrl, "■   STOP",   212, 34, 180, 52, Color.FromArgb(178, 40, 40));
        MakeLabel(pCtrl, "M1  →  PLC", 10, 94, FgDim);
        MakeLabel(pCtrl, "M0  →  PLC", 212, 94, FgDim);

        // ═══════════════════════════════════════════════════════════════════════
        // RIGHT COLUMN — Outputs + Status + PC→PLC (x=680, w=394)
        // ═══════════════════════════════════════════════════════════════════════

        // Y Outputs
        var pOut = MakeSection("Outputs  (Y Coils)", 680, 56, 394, 232);
        int oy = 32;
        // Left sub-column
        _ledRed    = MakeLedRow(pOut, "Y0   Red",           12, oy); oy += 28;
        _ledYellow = MakeLedRow(pOut, "Y1   Yellow",        12, oy); oy += 28;
        _ledGreen  = MakeLedRow(pOut, "Y2   Green",         12, oy); oy += 28;
        _ledBuzzer = MakeLedRow(pOut, "Y3   Buzzer",        12, oy); oy += 28;
        _ledMotor  = MakeLedRow(pOut, "Y8   Motor Relay1",  12, oy); oy += 28;
        _ledStop1  = MakeLedRow(pOut, "Y9   Stopper1",      12, oy);
        // Right sub-column
        _ledStop2  = MakeLedRow(pOut, "Y10  Stopper2",      200, 32);
        _ledStop3  = MakeLedRow(pOut, "Y11  Stopper3",      200, 60);
        _ledSolTop = MakeLedRow(pOut, "Y12  Solenoid_Top",  200, 88);
        _ledSolBot = MakeLedRow(pOut, "Y14  Solenoid_Bot",  200, 116);

        // M Status (TO-->PC)
        var pStat = MakeSection("System Status  (M Coils)", 680, 294, 394, 182);
        int sy = 32;
        _ledReady     = MakeLedRow(pStat, "M20  Ready",         12, sy); sy += 28;
        _ledOutPass   = MakeLedRow(pStat, "M11  Out_Pass",      12, sy); sy += 28;
        _ledOutFail   = MakeLedRow(pStat, "M12  Out_Fail",      12, sy); sy += 28;
        _ledGenAlarm  = MakeLedRow(pStat, "M25  Gen_Alarm",    206, 32);
        _ledTimeAlarm = MakeLedRow(pStat, "M26  Time Alarm",   206, 60);
        _ledFullAlarm = MakeLedRow(pStat, "M27  Full Alarm",   206, 88);
        _ledPcbAlarm  = MakeLedRow(pStat, "M28  PCB_Alarm",    206, 116);
        _ledAreaAlarm = MakeLedRow(pStat, "M29  Area_Alarm",   206, 144);

        // PC → PLC control buttons
        var pPc = MakeSection("PC  →  PLC  (From-PC Coils)", 680, 482, 394, 154);
        _btnInPass  = MakeButton(pPc, "M21  In_Pass",    10,  32, 176, 36, LedOff);
        _btnInFail  = MakeButton(pPc, "M22  In_Fail",    10,  76, 176, 36, LedOff);
        _btnRstAlarm = MakeButton(pPc, "M23  RST Alarm", 10, 110, 176, 36, LedOff);
        _btnDefault = MakeButton(pPc, "M8   MS_Default", 204, 32, 176, 36, Color.FromArgb(80, 42, 10));
        MakeLabel(pPc, "toggle", 10,  118, FgDim);
        MakeLabel(pPc, "toggle", 10,  154, FgDim);

        // ── Status bar ────────────────────────────────────────────────────────
        _lblStatus = MakeLabel(this, "   Ready  ·  Not connected", 6, 648, FgDim);
        _lblStatus.Font = new Font("Segoe UI", 8.5f);

        // ── Wire events ───────────────────────────────────────────────────────
        _btnConnect.Click += OnConnectClick;
        _btnStart.Click   += (_, _) => PulseCoil(PlcAddresses.M_Start, _btnStart, "START → M1 pulse");
        _btnStop.Click    += (_, _) => PulseCoil(PlcAddresses.M_Stop,  _btnStop,  "STOP  → M0 pulse");
        _btnDefault.Click += (_, _) => SendCoil(PlcAddresses.M_Default, true, "Default → M8=1");
        _btnInPass.Click  += (_, _) => ToggleCoil(PlcAddresses.M_InPass,   ref _stateInPass,   _btnInPass,  "M21 In_Pass",  LedGreen);
        _btnInFail.Click  += (_, _) => ToggleCoil(PlcAddresses.M_InFail,   ref _stateInFail,   _btnInFail,  "M22 In_Fail",  LedRed);
        _btnRstAlarm.Click += (_, _) => ToggleCoil(PlcAddresses.M_RstAlarm, ref _stateRstAlarm, _btnRstAlarm, "M23 RST Alarm", LedOrange);

        _pollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _pollTimer.Tick += OnPollTick;
        FormClosing += (_, _) => { _pollTimer.Stop(); _plc.Dispose(); };

        SetControlsEnabled(false);
        ResumeLayout(false);
    }

    // ─── Connection ───────────────────────────────────────────────────────────

    private void OnConnectClick(object? sender, EventArgs e)
    {
        if (_plc.IsConnected)
        {
            _pollTimer.Stop();
            _plc.Disconnect();
            ApplyConnectionState(false);
            SetStatus("Disconnected.");
            return;
        }
        _btnConnect.Enabled = false;
        SetStatus("Connecting …");
        bool ok = _plc.Connect(_txtIp.Text.Trim(), (int)_nudPort.Value);
        _btnConnect.Enabled = true;
        ApplyConnectionState(ok);
        if (ok)
        {
            SetStatus($"Connected  {_txtIp.Text}:{(int)_nudPort.Value}");
            _pollTimer.Start();
        }
        else
        {
            SetStatus($"Connection failed  –  {_plc.LastError}");
        }
    }

    // ─── 1-second poll ────────────────────────────────────────────────────────

    private void OnPollTick(object? sender, EventArgs e)
    {
        try
        {
            bool[] x = _plc.ReadDiscreteInputs(PlcAddresses.X_Base, PlcAddresses.X_BatchCount);
            bool[] y = _plc.ReadCoils(PlcAddresses.Y_Base, PlcAddresses.Y_BatchCount);
            bool[] m = _plc.ReadCoils(PlcAddresses.M_Base, PlcAddresses.M_BatchCount);
            int[]  v = _plc.ReadHoldingRegisters(PlcAddresses.V_SetTime, PlcAddresses.V_RegCount);

            // X inputs — NC signals shown inverted (active when wire is closed = false from PLC)
            SetLed(_ledX0,  !x[PlcAddresses.Xi_Stop],      LedGreen);  // NC
            SetLed(_ledX1,   x[PlcAddresses.Xi_Start],     LedGreen);
            SetLed(_ledX2,  !x[PlcAddresses.Xi_Emer],      LedRed);    // NC
            SetLed(_ledX3,   x[PlcAddresses.Xi_ReedTopUp], LedBlue);
            SetLed(_ledX4,   x[PlcAddresses.Xi_ReedTopDwn],LedBlue);
            SetLed(_ledX5,   x[PlcAddresses.Xi_ReedBotUp], LedBlue);
            SetLed(_ledX8,   x[PlcAddresses.Xi_ReedBotDwn],LedBlue);
            SetLed(_ledX9,   x[PlcAddresses.Xi_Sen1],      LedGreen);
            SetLed(_ledX10,  x[PlcAddresses.Xi_Sen2],      LedGreen);
            SetLed(_ledX11,  x[PlcAddresses.Xi_Sen3],      LedGreen);
            SetLed(_ledX12,  x[PlcAddresses.Xi_Sen4],      LedGreen);
            SetLed(_ledX13,  x[PlcAddresses.Xi_Sen5],      LedGreen);
            SetLed(_ledX14,  x[PlcAddresses.Xi_AreaSen1],  LedOrange);
            SetLed(_ledX15,  x[PlcAddresses.Xi_AreaSen2],  LedOrange);

            // Y outputs
            SetLed(_ledRed,    y[PlcAddresses.Yi_Red],        LedRed);
            SetLed(_ledYellow, y[PlcAddresses.Yi_Yellow],     LedYellow);
            SetLed(_ledGreen,  y[PlcAddresses.Yi_Green],      LedGreen);
            SetLed(_ledBuzzer, y[PlcAddresses.Yi_Buzzer],     LedOrange);
            SetLed(_ledMotor,  y[PlcAddresses.Yi_MotorRelay], LedBlue);
            SetLed(_ledStop1,  y[PlcAddresses.Yi_Stopper1],   LedBlue);
            SetLed(_ledStop2,  y[PlcAddresses.Yi_Stopper2],   LedBlue);
            SetLed(_ledStop3,  y[PlcAddresses.Yi_Stopper3],   LedBlue);
            SetLed(_ledSolTop, y[PlcAddresses.Yi_SolenoidTop],LedBlue);
            SetLed(_ledSolBot, y[PlcAddresses.Yi_SolenoidBot],LedBlue);

            // M status
            SetLed(_ledReady,     m[PlcAddresses.Mi_Ready],     LedGreen);
            SetLed(_ledOutPass,   m[PlcAddresses.Mi_OutPass],   LedGreen);
            SetLed(_ledOutFail,   m[PlcAddresses.Mi_OutFail],   LedRed);
            SetLed(_ledGenAlarm,  m[PlcAddresses.Mi_GenAlarm],  LedRed);
            SetLed(_ledTimeAlarm, m[PlcAddresses.Mi_TimeAlarm], LedOrange);
            SetLed(_ledFullAlarm, m[PlcAddresses.Mi_FullAlarm], LedOrange);
            SetLed(_ledPcbAlarm,  m[PlcAddresses.Mi_PcbAlarm],  LedRed);
            SetLed(_ledAreaAlarm, m[PlcAddresses.Mi_AreaAlarm], LedRed);

            // Sync toggle button states from PLC echo
            _stateInPass   = m[PlcAddresses.Mi_InPass];
            _stateInFail   = m[PlcAddresses.Mi_InFail];
            _stateRstAlarm = m[PlcAddresses.Mi_RstAlarm];
            _btnInPass.BackColor   = _stateInPass   ? LedGreen  : LedOff;
            _btnInFail.BackColor   = _stateInFail   ? LedRed    : LedOff;
            _btnRstAlarm.BackColor = _stateRstAlarm ? LedOrange : LedOff;

            // V registers → live display labels
            _lblSetTime.Text    = $"{v[0]} s";
            _lblResetTime.Text  = $"{v[1]} s";
            _lblDelayStop1.Text = $"{v[2]} s";
            _lblDelayStop2.Text = $"{v[3]} s";
            _lblDelayStop3.Text = $"{v[4]} ×100ms";
            _lblDelaySen3.Text  = $"{v[5]} s";

            // Alarm flash in status bar
            bool anyAlarm = m[PlcAddresses.Mi_GenAlarm] || m[PlcAddresses.Mi_TimeAlarm] ||
                            m[PlcAddresses.Mi_FullAlarm] || m[PlcAddresses.Mi_PcbAlarm]  ||
                            m[PlcAddresses.Mi_AreaAlarm];
            if (anyAlarm)
                SetStatus("⚠  ALARM active — check M25–M29", LedOrange);
        }
        catch (Exception ex)
        {
            _pollTimer.Stop();
            _plc.Disconnect();
            ApplyConnectionState(false);
            SetStatus($"Communication error  –  {ex.Message}");
        }
    }

    // ─── Settings read / write ────────────────────────────────────────────────

    private void ReadAllSettings()
    {
        try
        {
            int[] v = _plc.ReadHoldingRegisters(PlcAddresses.V_SetTime, PlcAddresses.V_RegCount);
            _nudSetTime.Value    = v[0];
            _nudResetTime.Value  = v[1];
            _nudDelayStop1.Value = v[2];
            _nudDelayStop2.Value = v[3];
            _nudDelayStop3.Value = v[4];
            _nudDelaySen3.Value  = v[5];
            _lblSetTime.Text    = $"{v[0]} s";
            _lblResetTime.Text  = $"{v[1]} s";
            _lblDelayStop1.Text = $"{v[2]} s";
            _lblDelayStop2.Text = $"{v[3]} s";
            _lblDelayStop3.Text = $"{v[4]} ×100ms";
            _lblDelaySen3.Text  = $"{v[5]} s";
            SetStatus("Settings read from PLC.");
        }
        catch (Exception ex) { SetStatus($"Read error  –  {ex.Message}"); }
    }

    private void WriteAllSettings()
    {
        try
        {
            _plc.WriteRegister(PlcAddresses.V_SetTime,    (int)_nudSetTime.Value);
            _plc.WriteRegister(PlcAddresses.V_ResetTime,  (int)_nudResetTime.Value);
            _plc.WriteRegister(PlcAddresses.V_DelayStop1, (int)_nudDelayStop1.Value);
            _plc.WriteRegister(PlcAddresses.V_DelayStop2, (int)_nudDelayStop2.Value);
            _plc.WriteRegister(PlcAddresses.V_DelayStop3, (int)_nudDelayStop3.Value);
            _plc.WriteRegister(PlcAddresses.V_DelaySen3,  (int)_nudDelaySen3.Value);
            SetStatus("Settings written to PLC.");
        }
        catch (Exception ex) { SetStatus($"Write error  –  {ex.Message}"); }
    }

    // ─── Coil helpers ─────────────────────────────────────────────────────────

    private void SendCoil(int address, bool value, string msg)
    {
        try { _plc.WriteCoil(address, value); SetStatus(msg); }
        catch (Exception ex) { SetStatus($"Write error  –  {ex.Message}"); }
    }

    private async void PulseCoil(int address, Button btn, string msg)
    {
        btn.Enabled = false;
        try
        {
            _plc.WriteCoil(address, true);
            SetStatus(msg);
            await Task.Delay(1000);
            _plc.WriteCoil(address, false);
        }
        catch (Exception ex) { SetStatus($"Write error  –  {ex.Message}"); }
        finally { btn.Enabled = _plc.IsConnected; }
    }

    private void ToggleCoil(int address, ref bool state, Button btn, string label, Color onColor)
    {
        bool next = !state;
        try
        {
            _plc.WriteCoil(address, next);
            state = next;
            btn.BackColor = next ? onColor : LedOff;
            SetStatus($"{label}  →  {(next ? "ON" : "OFF")}");
        }
        catch (Exception ex) { SetStatus($"Write error  –  {ex.Message}"); }
    }

    // ─── UI state helpers ─────────────────────────────────────────────────────

    private void ApplyConnectionState(bool connected)
    {
        _btnConnect.Text       = connected ? "Disconnect" : "Connect";
        _connLed.BackColor     = connected ? LedGreen : LedOff;
        _lblConnText.Text      = connected ? "CONNECTED" : "DISCONNECTED";
        _lblConnText.ForeColor = connected ? LedGreen : FgDim;
        SetControlsEnabled(connected);

        if (!connected)
        {
            foreach (var led in new[] { _ledX0, _ledX1, _ledX2, _ledX3, _ledX4, _ledX5,
                                        _ledX8, _ledX9, _ledX10, _ledX11, _ledX12, _ledX13,
                                        _ledX14, _ledX15,
                                        _ledRed, _ledYellow, _ledGreen, _ledBuzzer,
                                        _ledMotor, _ledStop1, _ledStop2, _ledStop3,
                                        _ledSolTop, _ledSolBot,
                                        _ledReady, _ledOutPass, _ledOutFail,
                                        _ledGenAlarm, _ledTimeAlarm, _ledFullAlarm,
                                        _ledPcbAlarm, _ledAreaAlarm })
                led.BackColor = LedOff;

            _stateInPass = _stateInFail = _stateRstAlarm = false;
            _btnInPass.BackColor = _btnInFail.BackColor = _btnRstAlarm.BackColor = LedOff;
            _lblElapsed.Text = "– – –";
            foreach (var lbl in new[] { _lblSetTime, _lblResetTime, _lblDelayStop1,
                                        _lblDelayStop2, _lblDelayStop3, _lblDelaySen3 })
                lbl.Text = "–";
        }
    }

    private void SetControlsEnabled(bool on)
    {
        foreach (var c in new Control[] { _btnStart, _btnStop, _btnDefault,
                                          _btnInPass, _btnInFail, _btnRstAlarm })
            c.Enabled = on;
    }

    private static void SetLed(Panel led, bool on, Color onColor) =>
        led.BackColor = on ? onColor : LedOff;

    private void SetStatus(string msg, Color? color = null)
    {
        _lblStatus.Text      = $"   {DateTime.Now:HH:mm:ss}   {msg}";
        _lblStatus.ForeColor = color ?? FgDim;
    }

    // ─── UI factory helpers ───────────────────────────────────────────────────

    private Panel MakePanel(Control parent, int x, int y, int w, int h)
    {
        var p = new Panel { Location = new Point(x, y), Size = new Size(w, h), BackColor = BgPanel };
        parent.Controls.Add(p);
        return p;
    }

    private Panel MakeSection(string title, int x, int y, int w, int h)
    {
        var p = MakePanel(this, x, y, w, h);
        p.Controls.Add(new Label
        {
            Text = title, Location = new Point(10, 6), AutoSize = true,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = FgHeader
        });
        return p;
    }

    private Label MakeLabel(Control parent, string text, int x, int y, Color? color = null)
    {
        var lbl = new Label
        {
            Text = text, Location = new Point(x, y), AutoSize = true,
            ForeColor = color ?? FgMain
        };
        parent.Controls.Add(lbl);
        return lbl;
    }

    private TextBox MakeTextBox(Control parent, string text, int x, int y, int w)
    {
        var tb = new TextBox
        {
            Text = text, Location = new Point(x, y), Width = w,
            BackColor = BgInput, ForeColor = FgMain, BorderStyle = BorderStyle.FixedSingle
        };
        parent.Controls.Add(tb);
        return tb;
    }

    private NumericUpDown MakePort(Control parent, int x, int y)
    {
        var n = new NumericUpDown
        {
            Minimum = 1, Maximum = 65535, Value = 502,
            Location = new Point(x, y), Width = 74,
            BackColor = BgInput, ForeColor = FgMain, BorderStyle = BorderStyle.FixedSingle
        };
        parent.Controls.Add(n);
        return n;
    }

    private Button MakeButton(Control parent, string text, int x, int y, int w, int h, Color back)
    {
        var b = new Button
        {
            Text = text, Location = new Point(x, y), Size = new Size(w, h),
            BackColor = back, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(80, 82, 100);
        b.FlatAppearance.BorderSize  = 1;
        parent.Controls.Add(b);
        return b;
    }

    private Button MakeBigBtn(Control parent, string text, int x, int y, int w, int h, Color back)
    {
        var b = MakeButton(parent, text, x, y, w, h, back);
        b.Font      = new Font("Segoe UI", 12f, FontStyle.Bold);
        b.TextAlign = ContentAlignment.MiddleCenter;
        return b;
    }

    private Panel MakeLed(Control parent, int x, int y, Color? color = null)
    {
        var p = new Panel { Location = new Point(x, y), Size = new Size(18, 18), BackColor = color ?? LedOff };
        var path = new GraphicsPath();
        path.AddEllipse(0, 0, 17, 17);
        p.Region = new Region(path);
        parent.Controls.Add(p);
        return p;
    }

    private Panel MakeLedRow(Control parent, string label, int x, int y)
    {
        var led = MakeLed(parent, x, y + 1);
        MakeLabel(parent, label, x + 24, y + 2);
        return led;
    }

    private (NumericUpDown nud, Label liveLabel) MakeTimerRow(
        Control parent, string title, int y, int defaultVal, string unit, int min, int max)
    {
        MakeLabel(parent, title, 10, y, FgHeader);

        var nud = new NumericUpDown
        {
            Minimum = min, Maximum = max, Value = defaultVal,
            Location = new Point(10, y + 18), Width = 80,
            BackColor = BgInput, ForeColor = FgMain, BorderStyle = BorderStyle.FixedSingle
        };
        parent.Controls.Add(nud);

        MakeLabel(parent, unit, 96, y + 21, FgDim);

        var live = new Label
        {
            Text = "–", Location = new Point(200, y + 18),
            AutoSize = true, ForeColor = LedGreen,
            Font = new Font("Consolas", 10f, FontStyle.Bold)
        };
        MakeLabel(parent, "PLC:", 168, y + 21, FgDim);
        parent.Controls.Add(live);

        return (nud, live);
    }
}
