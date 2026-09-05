using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace ATS1000B   // Change this to match your target project's namespace
{
    /// <summary>
    /// RS232 driver for the ATS-1000B Programmable AC Power Supply.
    ///
    /// Frame format (all commands):
    ///   [addr][cmd][dataLo][dataHi][checksum]
    ///   checksum = (addr + cmd + dataLo + dataHi) &amp; 0xFF
    ///
    /// SET success response  (4 bytes): [addr][cmd][0x01][cs]
    /// READ success response (5 bytes): [addr][0x50][dataLo][dataHi][cs]
    /// Error response        (4 bytes): [addr][0xFF][errorCode][cs]
    ///
    /// Thread-safe: all port access is protected by an internal lock.
    /// </summary>
    public sealed class AtsController : IDisposable
    {
        private readonly SerialPort _port;
        private readonly object     _lock = new object();
        private readonly byte       _address;

        /// <param name="portName">COM port, e.g. "COM3"</param>
        /// <param name="baudRate">Must match the device's RS232 baud setting</param>
        /// <param name="address">Device address byte (factory default: 0x01)</param>
        public AtsController(string portName, int baudRate, byte address = 0x01)
        {
            _address = address;
            _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
            {
                ReadTimeout  = 1000,
                WriteTimeout = 1000
            };
            _port.Open();
        }

        // ── Private helpers ────────────────────────────────────────────────

        private byte Checksum(byte a, byte b, byte c, byte d)
            => (byte)((a + b + c + d) & 0xFF);

        private byte[] MakeSetCmd(byte cmd, byte lo, byte hi)
            => new byte[] { _address, cmd, lo, hi, Checksum(_address, cmd, lo, hi) };

        private byte[] MakeReadCmd(byte subCmd)
            => new byte[] { _address, 0x50, 0x00, subCmd, Checksum(_address, 0x50, 0x00, subCmd) };

        private byte[] SendReceive(byte[] command, int expectedBytes)
        {
            lock (_lock)
            {
                _port.DiscardInBuffer();
                _port.Write(command, 0, command.Length);

                var buf = new List<byte>(expectedBytes);
                try
                {
                    while (buf.Count < expectedBytes)
                    {
                        buf.Add((byte)_port.ReadByte());
                        if (buf.Count >= 4 && buf[1] == 0xFF)
                            break;
                    }
                }
                catch (TimeoutException)
                {
                    if (buf.Count < 4)
                        throw new TimeoutException("Device did not respond within timeout.");
                }
                return buf.ToArray();
            }
        }

        private void CheckSetResponse(byte[] resp, byte expectedCmd)
        {
            if (resp[1] == 0xFF)
                throw new Exception($"Device error code: 0x{resp[2]:X2}");
            if (resp[1] != expectedCmd || resp[2] != 0x01)
                throw new Exception($"Unexpected response: {BitConverter.ToString(resp)}");
        }

        private void CheckReadResponse(byte[] resp)
        {
            if (resp[1] == 0xFF)
                throw new Exception($"Device error code: 0x{resp[2]:X2}");
            if (resp.Length < 5 || resp[1] != 0x50)
                throw new Exception($"Unexpected read response: {BitConverter.ToString(resp)}");
        }

        // ── SET commands ───────────────────────────────────────────────────

        /// <summary>
        /// Sets the AC output voltage.
        /// Cmd 0x01 — value encoding: volts × 10 (little-endian 16-bit)
        /// </summary>
        public void SetVoltage(double volts)
        {
            int raw = (int)Math.Round(volts * 10);
            CheckSetResponse(SendReceive(MakeSetCmd(0x01, (byte)(raw & 0xFF), (byte)(raw >> 8)), 4), 0x01);
        }

        /// <summary>
        /// Sets the AC output frequency.
        /// Cmd 0x02 — value encoding: hz × 10 (little-endian 16-bit)
        /// </summary>
        public void SetFrequency(double hz)
        {
            int raw = (int)Math.Round(hz * 10);
            CheckSetResponse(SendReceive(MakeSetCmd(0x02, (byte)(raw & 0xFF), (byte)(raw >> 8)), 4), 0x02);
        }

        /// <summary>
        /// Sets the upper current limit.
        /// Cmd 0x03 — value encoding: amps × 10 (little-endian 16-bit)
        /// </summary>
        public void SetCurrentLimit(double amps)
        {
            int raw = (int)Math.Round(amps * 10);
            CheckSetResponse(SendReceive(MakeSetCmd(0x03, (byte)(raw & 0xFF), (byte)(raw >> 8)), 4), 0x03);
        }

        /// <summary>
        /// Sets the output voltage ramp-up time.
        /// Cmd 0x04 — value encoding: seconds × 10 (little-endian 16-bit)
        /// </summary>
        public void SetRampUpTime(double seconds)
        {
            int raw = (int)Math.Round(seconds * 10);
            CheckSetResponse(SendReceive(MakeSetCmd(0x04, (byte)(raw & 0xFF), (byte)(raw >> 8)), 4), 0x04);
        }

        /// <summary>
        /// Sets the output voltage range.
        /// Cmd 0x10 — dataHi: 0x00 = Auto, 0x01 = Low, 0x02 = High
        /// </summary>
        public void SetVoltageRange(byte range)
        {
            CheckSetResponse(SendReceive(MakeSetCmd(0x10, 0x00, range), 4), 0x10);
        }

        /// <summary>
        /// Turns the AC output ON or OFF.
        /// Cmd 0x11 — dataHi: 0xFF = ON, 0x00 = OFF
        /// </summary>
        public void SetOutput(bool on)
        {
            byte val = on ? (byte)0xFF : (byte)0x00;
            CheckSetResponse(SendReceive(MakeSetCmd(0x11, 0x00, val), 4), 0x11);
        }

        // ── READ commands ──────────────────────────────────────────────────
        // All read commands use cmd byte 0x50 with a sub-command in dataHi.
        // Response: [addr][0x50][dataLo][dataHi][cs]

        /// <summary>
        /// Reads the measured AC output voltage in volts.
        /// Sub-cmd 0x01 — response value ÷ 10
        /// </summary>
        public double ReadVoltage()
        {
            var resp = SendReceive(MakeReadCmd(0x01), 5);
            CheckReadResponse(resp);
            return (resp[2] | resp[3] << 8) / 10.0;
        }

        /// <summary>
        /// Reads the measured AC output frequency in Hz.
        /// Sub-cmd 0x02 — response value ÷ 10
        /// </summary>
        public double ReadFrequency()
        {
            var resp = SendReceive(MakeReadCmd(0x02), 5);
            CheckReadResponse(resp);
            return (resp[2] | resp[3] << 8) / 10.0;
        }

        /// <summary>
        /// Reads the measured AC output current in amps.
        /// Sub-cmd 0x03 — response value ÷ 1000
        /// </summary>
        public double ReadCurrent()
        {
            var resp = SendReceive(MakeReadCmd(0x03), 5);
            CheckReadResponse(resp);
            return (resp[2] | resp[3] << 8) / 1000.0;
        }

        /// <summary>
        /// Reads the measured AC output power in watts.
        /// Sub-cmd 0x04 — response value ÷ 10
        /// </summary>
        public double ReadPower()
        {
            var resp = SendReceive(MakeReadCmd(0x04), 5);
            CheckReadResponse(resp);
            return (resp[2] | resp[3] << 8) / 10.0;
        }

        /// <summary>
        /// Reads the measured power factor (0.000 – 1.000).
        /// Sub-cmd 0x05 — response value ÷ 1000
        /// </summary>
        public double ReadPowerFactor()
        {
            var resp = SendReceive(MakeReadCmd(0x05), 5);
            CheckReadResponse(resp);
            return (resp[2] | resp[3] << 8) / 1000.0;
        }

        /// <summary>
        /// Reads whether the AC output relay is closed (output ON).
        /// Sub-cmd 0x10 — dataHi: 0xFF = ON, 0x00 = OFF
        /// </summary>
        public bool ReadOutputStatus()
        {
            var resp = SendReceive(MakeReadCmd(0x10), 5);
            CheckReadResponse(resp);
            return resp[3] == 0xFF;
        }

        // ── IDisposable ────────────────────────────────────────────────────

        public void Dispose()
        {
            try { if (_port.IsOpen) _port.Close(); } catch { }
            _port.Dispose();
        }

        // ── Port scanning ──────────────────────────────────────────────────

        /// <summary>
        /// Tries every available COM port at the given baud rate and returns the
        /// first one whose device answers a ReadVoltage query correctly.
        /// Returns null if no port responds.
        /// </summary>
        public static string Scan(int baudRate, byte address = 0x01, int timeoutMs = 500)
        {
            byte[] cmd = MakeScanCmd(address);
            foreach (string portName in SerialPort.GetPortNames())
            {
                try
                {
                    using (SerialPort p = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                    {
                        ReadTimeout  = timeoutMs,
                        WriteTimeout = timeoutMs
                    })
                    {
                        p.Open();
                        p.DiscardInBuffer();
                        p.Write(cmd, 0, cmd.Length);

                        int b0 = p.ReadByte(); // addr
                        int b1 = p.ReadByte(); // cmd (0x50) or 0xFF on error
                        if (b1 == 0x50) return portName;
                    }
                }
                catch { }
            }
            return null;
        }

        private static byte[] MakeScanCmd(byte address)
        {
            // ReadVoltage sub-cmd 0x01
            byte cs = (byte)((address + 0x50 + 0x00 + 0x01) & 0xFF);
            return new byte[] { address, 0x50, 0x00, 0x01, cs };
        }
    }
}
