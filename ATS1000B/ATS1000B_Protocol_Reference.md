# ATS-1000B Programmable AC Power Supply — RS232 Protocol Reference

## Port Settings

| Parameter   | Value                                      |
|-------------|--------------------------------------------|
| Baud rate   | 600 / 9600 / 19200 / 38400 / 57600 / 115200 (set on device) |
| Data bits   | 8                                          |
| Parity      | None                                       |
| Stop bits   | 1                                          |
| Flow control| None                                       |
| Connector   | DB-9 (Pin 2 = RXD, Pin 3 = TXD, Pin 5 = GND) |

---

## Frame Format

### Command frame (host → device) — always 5 bytes

| Byte | Field      | Description                          |
|------|------------|--------------------------------------|
| [0]  | Address    | Device address (factory default: 0x01) |
| [1]  | Command    | Command code                         |
| [2]  | Data Lo    | Low byte of data value               |
| [3]  | Data Hi    | High byte of data value              |
| [4]  | Checksum   | `(addr + cmd + dataLo + dataHi) & 0xFF` |

### SET success response (device → host) — 4 bytes

| Byte | Value            |
|------|------------------|
| [0]  | Device address   |
| [1]  | Command (echoed) |
| [2]  | 0x01             |
| [3]  | Checksum         |

### READ success response (device → host) — 5 bytes

| Byte | Value          |
|------|----------------|
| [0]  | Device address |
| [1]  | 0x50           |
| [2]  | Data Lo        |
| [3]  | Data Hi        |
| [4]  | Checksum       |

### Error response (device → host) — 4 bytes

| Byte | Value          |
|------|----------------|
| [0]  | Device address |
| [1]  | 0xFF           |
| [2]  | Error code     |
| [3]  | Checksum       |

---

## SET Commands

All SET commands expect a 4-byte success response.

| Command         | Cmd  | Data Lo               | Data Hi               | Notes |
|-----------------|------|-----------------------|-----------------------|-------|
| Set Voltage     | 0x01 | `(volts × 10) & 0xFF` | `(volts × 10) >> 8`   | e.g. 220.0 V → raw 2200 → Lo=0xA0, Hi=0x08 |
| Set Frequency   | 0x02 | `(hz × 10) & 0xFF`    | `(hz × 10) >> 8`      | e.g. 50.0 Hz → raw 500 → Lo=0xF4, Hi=0x01  |
| Set Current Lim | 0x03 | `(amps × 10) & 0xFF`  | `(amps × 10) >> 8`    | e.g. 10.0 A → raw 100 → Lo=0x64, Hi=0x00   |
| Set Ramp-up Time| 0x04 | `(sec × 10) & 0xFF`   | `(sec × 10) >> 8`     | e.g. 5.0 s → raw 50 → Lo=0x32, Hi=0x00     |
| Set Voltage Range| 0x10 | 0x00                 | 0x00=Auto, 0x01=Low, 0x02=High | |
| Set Output      | 0x11 | 0x00                  | 0xFF=ON, 0x00=OFF     | |

**Value encoding:** all numeric values are transmitted as `value × scale` packed as a 16-bit unsigned integer in little-endian order (Lo byte first, Hi byte second).

### Example — Set Voltage to 220.0 V (address 0x01)

```
raw = 220.0 × 10 = 2200 = 0x0898
Lo  = 0x98
Hi  = 0x08
cs  = (0x01 + 0x01 + 0x98 + 0x08) & 0xFF = 0xA2

Send:    01 01 98 08 A2
Receive: 01 01 01 03   ← success
```

---

## READ Commands

All READ commands share command byte **0x50**. The sub-command is placed in **Data Hi** (Data Lo is always 0x00). A 5-byte response is expected.

Command frame: `[addr][0x50][0x00][sub-cmd][cs]`

| Sub-cmd | Parameter     | Response scaling          | Unit  |
|---------|---------------|---------------------------|-------|
| 0x01    | Voltage       | `(dataLo \| dataHi << 8) / 10.0` | V  |
| 0x02    | Frequency     | `(dataLo \| dataHi << 8) / 10.0` | Hz |
| 0x03    | Current       | `(dataLo \| dataHi << 8) / 1000.0` | A |
| 0x04    | Power         | `(dataLo \| dataHi << 8) / 10.0` | W  |
| 0x05    | Power Factor  | `(dataLo \| dataHi << 8) / 1000.0` | — |
| 0x10    | Output Status | dataHi: **0xFF** = ON, **0x00** = OFF | — |

### Example — Read Voltage (address 0x01)

```
cs      = (0x01 + 0x50 + 0x00 + 0x01) & 0xFF = 0x52

Send:    01 50 00 01 52
Receive: 01 50 98 08 F2   ← dataLo=0x98, dataHi=0x08

Voltage = (0x98 | 0x08 << 8) / 10.0 = 2200 / 10.0 = 220.0 V
```

---

## C# Usage (AtsController class)

```csharp
// Open connection
using var ats = new AtsController("COM3", 9600, address: 0x01);

// SET
ats.SetVoltage(220.0);       // volts
ats.SetFrequency(50.0);      // Hz
ats.SetCurrentLimit(10.0);   // amps
ats.SetRampUpTime(0.0);      // seconds
ats.SetVoltageRange(0);      // 0=Auto, 1=Low, 2=High
ats.SetOutput(true);         // true=ON, false=OFF

// READ
double v  = ats.ReadVoltage();       // V
double f  = ats.ReadFrequency();     // Hz
double i  = ats.ReadCurrent();       // A
double p  = ats.ReadPower();         // W
double pf = ats.ReadPowerFactor();   // 0.000–1.000
bool   on = ats.ReadOutputStatus();  // true if output relay is closed
```

All methods throw `Exception` on device error and `TimeoutException` if no response arrives within 1 second.
