// HaiwellController.cs — standalone Modbus TCP client for Haiwell AC10S0T/P + A16XDT/P.
// Address map: Map_I_O_testing2.pdf (rev 2).
// Dependency: EasyModbusTCP NuGet package

using EasyModbus;

namespace HaiwellPlc;

public static class PlcAddresses
{
    // X discrete inputs (FC02) ─────────────────────────────────────────────────
    public const int X_Base        = 0;
    public const int X_BatchCount  = 16;
    public const int Xi_Stop       = 0;   // x0  NC
    public const int Xi_Start      = 1;   // x1  NO
    public const int Xi_Emer       = 2;   // x2  NC
    public const int Xi_ReedTopUp  = 3;   // x3
    public const int Xi_ReedTopDwn = 4;   // x4
    public const int Xi_ReedBotUp  = 5;   // x5
    public const int Xi_ReedBotDwn = 8;   // x8
    public const int Xi_Sen1       = 9;   // x9
    public const int Xi_Sen2       = 10;  // x10
    public const int Xi_Sen3       = 11;  // x11
    public const int Xi_Sen4       = 12;  // x12
    public const int Xi_Sen5       = 13;  // x13
    public const int Xi_AreaSen1   = 14;  // x14
    public const int Xi_AreaSen2   = 15;  // x15

    // Y output coils (FC01/FC05)  base 0x0600 = 1536 ──────────────────────────
    public const int Y_Base        = 1536;
    public const int Y_BatchCount  = 15;  // Y0–Y14
    public const int Yi_Red        = 0;   // Y0
    public const int Yi_Yellow     = 1;   // Y1
    public const int Yi_Green      = 2;   // Y2
    public const int Yi_Buzzer     = 3;   // Y3
    public const int Yi_MotorRelay = 8;   // Y8
    public const int Yi_Stopper1   = 9;   // Y9
    public const int Yi_Stopper2   = 10;  // Y10
    public const int Yi_Stopper3   = 11;  // Y11
    public const int Yi_SolenoidTop = 12; // Y12
    public const int Yi_SolenoidBot = 14; // Y14

    // M relay coils (FC01/FC05)  base 0x0C00 = 3072 ───────────────────────────
    public const int M_Base        = 3072;
    public const int M_BatchCount  = 30;  // M0–M29
    public const int M_Stop        = M_Base + 0;   // M0  From PC
    public const int M_Start       = M_Base + 1;   // M1  From PC
    public const int M_Default     = M_Base + 8;   // M8  From PC
    public const int M_InPass      = M_Base + 21;  // M21 From PC
    public const int M_InFail      = M_Base + 22;  // M22 From PC
    public const int M_RstAlarm    = M_Base + 23;  // M23 From PC
    public const int Mi_OutPass    = 11; // M11  view only
    public const int Mi_OutFail    = 12; // M12  view only
    public const int Mi_Ready      = 20; // M20  TO-->PC
    public const int Mi_InPass     = 21; // M21
    public const int Mi_InFail     = 22; // M22
    public const int Mi_GenAlarm   = 25; // M25  TO-->PC
    public const int Mi_TimeAlarm  = 26; // M26  TO-->PC
    public const int Mi_FullAlarm  = 27; // M27  TO-->PC
    public const int Mi_PcbAlarm   = 28; // M28  TO-->PC
    public const int Mi_AreaAlarm  = 29; // M29  TO-->PC

    // V holding registers (FC03/FC06)  base 0x0200 = 512 ──────────────────────
    public const int V_SetTime     = 1512; // V1000  test time (s)
    public const int V_ResetTime   = 1513; // V1001  stop-hold reset delay (s)
    public const int V_DelayStop1  = 1514; // V1002  Stopper1 OFF delay (s)
    public const int V_DelayStop2  = 1515; // V1003  Stopper2 OFF delay (s)
    public const int V_DelayStop3  = 1516; // V1004  Stopper3 delay (×100 ms)
    public const int V_DelaySen3   = 1517; // V1005  Sensor3 ON delay (s)
    public const int V_RegCount    = 6;
}

public sealed record PlcStatus(
    bool Stop, bool Start, bool Emer,
    bool ReedTopUp, bool ReedTopDwn, bool ReedBotUp, bool ReedBotDwn,
    bool Sen1, bool Sen2, bool Sen3, bool Sen4, bool Sen5,
    bool AreaSen1, bool AreaSen2,
    bool Red, bool Yellow, bool Green, bool Buzzer,
    bool MotorRelay, bool Stopper1, bool Stopper2, bool Stopper3,
    bool SolenoidTop, bool SolenoidBot,
    bool Ready, bool OutPass, bool OutFail,
    bool GenAlarm, bool TimeAlarm, bool FullAlarm, bool PcbAlarm, bool AreaAlarm,
    bool InPass, bool InFail
);

public sealed record PlcSettings(
    int SetTime, int ResetTime, int DelayStop1, int DelayStop2,
    int DelayStop3, int DelaySen3
);

public sealed class HaiwellController : IDisposable
{
    private ModbusClient? _client;
    private readonly object _lock = new();

    public bool IsConnected
    {
        get { lock (_lock) { return _client?.Connected ?? false; } }
    }

    public string LastError { get; private set; } = string.Empty;

    public bool Connect(string ip, int port = 502)
    {
        lock (_lock)
        {
            try
            {
                _client?.Disconnect();
                _client = new ModbusClient(ip, port) { ConnectionTimeout = 3000 };
                _client.Connect();
                LastError = string.Empty;
                return true;
            }
            catch (Exception ex) { LastError = ex.Message; _client = null; return false; }
        }
    }

    public void Disconnect()
    {
        lock (_lock) { try { _client?.Disconnect(); } catch { } _client = null; }
    }

    public async Task SendStopAsync()  { WriteCoil(PlcAddresses.M_Stop,  true); await Task.Delay(1000); WriteCoil(PlcAddresses.M_Stop,  false); }
    public async Task SendStartAsync() { WriteCoil(PlcAddresses.M_Start, true); await Task.Delay(1000); WriteCoil(PlcAddresses.M_Start, false); }
    public void SetDefault()         => WriteCoil(PlcAddresses.M_Default,  true);
    public void SetInPass(bool v)    => WriteCoil(PlcAddresses.M_InPass,   v);
    public void SetInFail(bool v)    => WriteCoil(PlcAddresses.M_InFail,   v);
    public void SetRstAlarm(bool v)  => WriteCoil(PlcAddresses.M_RstAlarm, v);

    public PlcStatus ReadStatus()
    {
        bool[] x = ReadDiscreteInputs(PlcAddresses.X_Base,  PlcAddresses.X_BatchCount);
        bool[] y = ReadCoils(PlcAddresses.Y_Base,  PlcAddresses.Y_BatchCount);
        bool[] m = ReadCoils(PlcAddresses.M_Base,  PlcAddresses.M_BatchCount);
        return new PlcStatus(
            Stop:       x[PlcAddresses.Xi_Stop],
            Start:      x[PlcAddresses.Xi_Start],
            Emer:       x[PlcAddresses.Xi_Emer],
            ReedTopUp:  x[PlcAddresses.Xi_ReedTopUp],
            ReedTopDwn: x[PlcAddresses.Xi_ReedTopDwn],
            ReedBotUp:  x[PlcAddresses.Xi_ReedBotUp],
            ReedBotDwn: x[PlcAddresses.Xi_ReedBotDwn],
            Sen1:       x[PlcAddresses.Xi_Sen1],
            Sen2:       x[PlcAddresses.Xi_Sen2],
            Sen3:       x[PlcAddresses.Xi_Sen3],
            Sen4:       x[PlcAddresses.Xi_Sen4],
            Sen5:       x[PlcAddresses.Xi_Sen5],
            AreaSen1:   x[PlcAddresses.Xi_AreaSen1],
            AreaSen2:   x[PlcAddresses.Xi_AreaSen2],
            Red:        y[PlcAddresses.Yi_Red],
            Yellow:     y[PlcAddresses.Yi_Yellow],
            Green:      y[PlcAddresses.Yi_Green],
            Buzzer:     y[PlcAddresses.Yi_Buzzer],
            MotorRelay: y[PlcAddresses.Yi_MotorRelay],
            Stopper1:   y[PlcAddresses.Yi_Stopper1],
            Stopper2:   y[PlcAddresses.Yi_Stopper2],
            Stopper3:   y[PlcAddresses.Yi_Stopper3],
            SolenoidTop: y[PlcAddresses.Yi_SolenoidTop],
            SolenoidBot: y[PlcAddresses.Yi_SolenoidBot],
            Ready:      m[PlcAddresses.Mi_Ready],
            OutPass:    m[PlcAddresses.Mi_OutPass],
            OutFail:    m[PlcAddresses.Mi_OutFail],
            GenAlarm:   m[PlcAddresses.Mi_GenAlarm],
            TimeAlarm:  m[PlcAddresses.Mi_TimeAlarm],
            FullAlarm:  m[PlcAddresses.Mi_FullAlarm],
            PcbAlarm:   m[PlcAddresses.Mi_PcbAlarm],
            AreaAlarm:  m[PlcAddresses.Mi_AreaAlarm],
            InPass:     m[PlcAddresses.Mi_InPass],
            InFail:     m[PlcAddresses.Mi_InFail]
        );
    }

    public PlcSettings ReadSettings()
    {
        int[] v = ReadHoldingRegisters(PlcAddresses.V_SetTime, PlcAddresses.V_RegCount);
        return new PlcSettings(v[0], v[1], v[2], v[3], v[4], v[5]);
    }

    public void WriteSettings(PlcSettings s)
    {
        WriteRegister(PlcAddresses.V_SetTime,    s.SetTime);
        WriteRegister(PlcAddresses.V_ResetTime,  s.ResetTime);
        WriteRegister(PlcAddresses.V_DelayStop1, s.DelayStop1);
        WriteRegister(PlcAddresses.V_DelayStop2, s.DelayStop2);
        WriteRegister(PlcAddresses.V_DelayStop3, s.DelayStop3);
        WriteRegister(PlcAddresses.V_DelaySen3,  s.DelaySen3);
    }

    public bool[] ReadDiscreteInputs(int startAddress, int count)
    {
        lock (_lock) { EnsureConnected(); return _client!.ReadDiscreteInputs(startAddress, count); }
    }

    public bool[] ReadCoils(int startAddress, int count)
    {
        lock (_lock) { EnsureConnected(); return _client!.ReadCoils(startAddress, count); }
    }

    public int[] ReadHoldingRegisters(int startAddress, int count)
    {
        lock (_lock) { EnsureConnected(); return _client!.ReadHoldingRegisters(startAddress, count); }
    }

    public void WriteCoil(int address, bool value)
    {
        lock (_lock) { EnsureConnected(); _client!.WriteSingleCoil(address, value); }
    }

    public void WriteRegister(int address, int value)
    {
        lock (_lock) { EnsureConnected(); _client!.WriteSingleRegister(address, value); }
    }

    private void EnsureConnected()
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("PLC is not connected.");
    }

    public void Dispose() => Disconnect();
}
