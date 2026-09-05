namespace PlcScada;

/// <summary>
/// Modbus address map — Haiwell AC10S0T/P + A16XDT/P.
/// Source: Map_I_O_testing2.pdf (rev 2).
///
/// Haiwell coil/register layout (0-based PDU, EasyModbusTCP):
///   X inputs  → Discrete Inputs   0x0000+  (FC02)   addr = xN
///   Y outputs → Coils             0x0600+  (FC01/05) addr = 1536 + yN
///   M relays  → Coils             0x0C00+  (FC01/05) addr = 3072 + mN
///   V regs    → Holding Registers 0x0200+  (FC03/06) addr = 512  + vN
/// </summary>
internal static class PlcAddresses
{
    // ── X Discrete Inputs (FC02) ───────────────────────────────────────────────
    public const int X_Base       = 0;
    public const int X_BatchCount = 16;   // x0–x15

    public const int Xi_Stop        = 0;  // x0  NC
    public const int Xi_Start       = 1;  // x1  NO
    public const int Xi_Emer        = 2;  // x2  NC
    public const int Xi_ReedTopUp   = 3;  // x3
    public const int Xi_ReedTopDwn  = 4;  // x4
    public const int Xi_ReedBotUp   = 5;  // x5
    // 6, 7 unused
    public const int Xi_ReedBotDwn  = 8;  // x8
    public const int Xi_Sen1        = 9;  // x9
    public const int Xi_Sen2        = 10; // x10
    public const int Xi_Sen3        = 11; // x11
    public const int Xi_Sen4        = 12; // x12
    public const int Xi_Sen5        = 13; // x13
    public const int Xi_AreaSen1    = 14; // x14
    public const int Xi_AreaSen2    = 15; // x15

    // ── Y Output Coils (FC01 read / FC05 write) ────────────────────────────────
    public const int Y_Base       = 1536; // 0x0600
    public const int Y_BatchCount = 15;   // Y0–Y14

    // Relative indices into Y batch array
    public const int Yi_Red          = 0;  // Y0  addr 1536
    public const int Yi_Yellow       = 1;  // Y1  addr 1537
    public const int Yi_Green        = 2;  // Y2  addr 1538
    public const int Yi_Buzzer       = 3;  // Y3  addr 1539
    // Y4–Y7 unused
    public const int Yi_MotorRelay   = 8;  // Y8  addr 1544
    public const int Yi_Stopper1     = 9;  // Y9  addr 1545
    public const int Yi_Stopper2     = 10; // Y10 addr 1546
    public const int Yi_Stopper3     = 11; // Y11 addr 1547
    public const int Yi_SolenoidTop  = 12; // Y12 addr 1548
    // Y13 unused
    public const int Yi_SolenoidBot  = 14; // Y14 addr 1550

    // ── M Relay Coils (FC01 read / FC05 write) ────────────────────────────────
    public const int M_Base       = 3072; // 0x0C00
    public const int M_BatchCount = 30;   // M0–M29

    // Absolute addresses for WriteSingleCoil (From-PC coils)
    public const int M_Stop       = M_Base + 0;  // M0
    public const int M_Start      = M_Base + 1;  // M1
    public const int M_Default    = M_Base + 8;  // M8
    public const int M_InPass     = M_Base + 21; // M21
    public const int M_InFail     = M_Base + 22; // M22
    public const int M_RstAlarm   = M_Base + 23; // M23

    // Relative indices into M batch (m[i] = Mi where i = Mn - M_Base)
    public const int Mi_Stop       = 0;  // M0
    public const int Mi_Start      = 1;  // M1
    public const int Mi_Default    = 8;  // M8
    public const int Mi_OutPass    = 11; // M11  view only
    public const int Mi_OutFail    = 12; // M12  view only
    public const int Mi_Ready      = 20; // M20  TO-->PC
    public const int Mi_InPass     = 21; // M21  From PC
    public const int Mi_InFail     = 22; // M22  From PC
    public const int Mi_RstAlarm   = 23; // M23  From PC
    public const int Mi_GenAlarm   = 25; // M25  TO-->PC
    public const int Mi_TimeAlarm  = 26; // M26  TO-->PC
    public const int Mi_FullAlarm  = 27; // M27  TO-->PC
    public const int Mi_PcbAlarm   = 28; // M28  TO-->PC
    public const int Mi_AreaAlarm  = 29; // M29  TO-->PC

    // ── V Holding Registers (FC03 read / FC06 write) ──────────────────────────
    public const int V_Base         = 512;

    public const int V_SetTime      = 1512; // V1000  Test time (s)         default 150
    public const int V_ResetTime    = 1513; // V1001  Stop-hold reset delay  default 3 s
    public const int V_DelayStop1   = 1514; // V1002  Stopper1 OFF delay     default 1 s
    public const int V_DelayStop2   = 1515; // V1003  Stopper2 OFF delay     default 1 s
    public const int V_DelayStop3   = 1516; // V1004  Stopper3 delay         default 5 ×100 ms
    public const int V_DelaySen3    = 1517; // V1005  Sensor3 ON delay       default 1 s

    public const int V_RegCount     = 6;    // read V1000–V1005 in one call
}
