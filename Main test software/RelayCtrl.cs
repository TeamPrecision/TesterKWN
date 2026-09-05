using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace ATS.Hardware
{

public class RelayCtrl
{
    public class WriteOperations
    {
        private readonly SerialPort _serialPort;

        public WriteOperations(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public bool Off_All(int countID = 63)
        {
            try
            {
                for (int i = 0; i <= 1; i++)
                {
                    for (int moduleId = 0; moduleId <= countID; moduleId++)
                    {
                        byte[] cmdPackage = new byte[8]
                        {
                            (byte)moduleId,
                            6,
                            0,
                            0,
                            8,
                            0,
                            0,
                            0
                        };
                        ushort crc = RelayCtrl.CalculateCRC(cmdPackage, 6);
                        cmdPackage[6] = (byte)(crc & 0xFFu);
                        cmdPackage[7] = (byte)((uint)(crc >> 8) & 0xFFu);
                        _serialPort.Write(cmdPackage, 0, cmdPackage.Length);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Off_All: " + ex.Message);
                return false;
            }
        }

        public bool OnChannel(int moduleID, int channel)
        {
            if (moduleID < 0 || moduleID > 63 || channel < 1 || channel > 16)
                return false;
            return SendCommand(moduleID, channel, 1);
        }

        public bool OffChannel(int moduleID, int channel)
        {
            if (moduleID < 0 || moduleID > 63 || channel < 1 || channel > 16)
                return false;
            return SendCommand(moduleID, channel, 2);
        }

        public bool LatchChannel(int moduleID, int channel)
        {
            return SendCommand(moduleID, channel, 4);
        }

        public bool ToggleChannel(int moduleID, int channel)
        {
            return SendCommand(moduleID, channel, 3);
        }

        private bool SendCommand(int moduleId, int channel, byte operation)
        {
            try
            {
                byte[] cmdPackage = CreateCommandPackage(moduleId, channel, operation);
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Console.WriteLine("Error: Serial port is not open.");
                    return false;
                }
                _serialPort.Write(cmdPackage, 0, cmdPackage.Length);

                int elapsed = 0;
                while (_serialPort.BytesToRead < 7 && elapsed < 250)
                {
                    Thread.Sleep(10);
                    elapsed += 10;
                }
                if (_serialPort.BytesToRead < 7)
                {
                    Console.WriteLine("Error: Timeout exceeded while waiting for response.");
                    return false;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(2000))
                {
                    if (_serialPort.BytesToRead >= 8)
                    {
                        stopwatch.Stop();
                        break;
                    }
                    Thread.Sleep(10);
                }
                if (stopwatch.IsRunning)
                {
                    return false;
                }

                byte[] response = new byte[8];
                _serialPort.Read(response, 0, response.Length);
                if (response.Length == 8 && response[1] == 6)
                {
                    return response[4] == 1;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SendCommand: " + ex.Message);
                return false;
            }
        }

        private byte[] CreateCommandPackage(int moduleId, int channel, byte operation)
        {
            byte[] cmd = new byte[8]
            {
                (byte)moduleId,
                6,
                0,
                (byte)channel,
                operation,
                0,
                0,
                0
            };
            ushort crc = RelayCtrl.CalculateCRC(cmd, 6);
            cmd[6] = (byte)(crc & 0xFFu);
            cmd[7] = (byte)((uint)(crc >> 8) & 0xFFu);
            return cmd;
        }
    }

    public class ReadOperations
    {
        private readonly SerialPort _serialPort;

        public ReadOperations(SerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public int? ReadAllChannel(int moduleID)
        {
            if (moduleID < 0 || moduleID > 63)
                return null;
            try
            {
                byte[] cmd = new byte[8]
                {
                    (byte)moduleID,
                    3,
                    0,
                    192,
                    0,
                    1,
                    0,
                    0
                };
                ushort crc = RelayCtrl.CalculateCRC(cmd, 6);
                cmd[6] = (byte)(crc & 0xFFu);
                cmd[7] = (byte)((uint)(crc >> 8) & 0xFFu);
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Console.WriteLine("Error: Serial port is not open.");
                    return null;
                }
                _serialPort.ReadExisting();
                _serialPort.Write(cmd, 0, cmd.Length);
                int timeout = 250;
                while (_serialPort.BytesToRead < 7 && timeout > 0)
                {
                    Thread.Sleep(20);
                    timeout -= 20;
                }
                if (_serialPort.BytesToRead < 7)
                {
                    Console.WriteLine("Error: Timeout waiting for response.");
                    return null;
                }
                byte[] response = new byte[7];
                _serialPort.Read(response, 0, response.Length);
                if (response.Length == 7 && response[1] == 3)
                {
                    int combinedValue = (response[3] << 8) | response[4];
                    Console.WriteLine($"Module {moduleID}: Value = {combinedValue}");
                    return combinedValue;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public bool ReadChannel(int moduleID, int channelInModule)
        {
            if (moduleID < 0 || moduleID > 63 || channelInModule < 1 || channelInModule > 16)
                return false;
            return SendCommand(moduleID, channelInModule);
        }

        private bool SendCommand(int moduleId, int channel)
        {
            try
            {
                byte[] cmdPackage = CreateCommandPackage(moduleId, channel);
                if (_serialPort == null || !_serialPort.IsOpen)
                {
                    Console.WriteLine("Error: Serial port is not open.");
                    return false;
                }
                _serialPort.ReadExisting();
                _serialPort.Write(cmdPackage, 0, cmdPackage.Length);

                int elapsed = 0;
                while (_serialPort.BytesToRead < 7 && elapsed < 250)
                {
                    Thread.Sleep(10);
                    elapsed += 10;
                }
                if (_serialPort.BytesToRead < 7)
                {
                    Console.WriteLine("Error: Timeout waiting for response.");
                    return false;
                }

                byte[] response = new byte[7];
                _serialPort.Read(response, 0, response.Length);
                if (response.Length == 7 && response[1] == 3)
                {
                    return response[4] == 1;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SendCommand: " + ex.Message);
                return false;
            }
        }

        private byte[] CreateCommandPackage(int moduleId, int channel)
        {
            byte[] cmd = new byte[8]
            {
                (byte)moduleId,
                3,
                0,
                (byte)(128 + channel),
                0,
                1,
                0,
                0
            };
            ushort crc = RelayCtrl.CalculateCRC(cmd, 6);
            cmd[6] = (byte)(crc & 0xFFu);
            cmd[7] = (byte)((uint)(crc >> 8) & 0xFFu);
            return cmd;
        }
    }

    private SerialPort _serialPort;

    public WriteOperations Write => new WriteOperations(_serialPort);
    public ReadOperations Read => new ReadOperations(_serialPort);

    public string ConnectRelay(string portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One)
    {
        try
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.DataBits = dataBits;
            _serialPort.Parity = parity;
            _serialPort.StopBits = stopBits;
            _serialPort.ReadTimeout = 250;
            _serialPort.WriteTimeout = 2000;
            _serialPort.Open();
            return "Connected to COM Port: " + portName;
        }
        catch (Exception ex)
        {
            return "Error: " + ex.Message;
        }
    }

    public string DisConnectRelay()
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                return "Disconnected successfully.";
            }
            return "Serial port was not open.";
        }
        catch (Exception ex)
        {
            return "Error while disconnecting: " + ex.Message;
        }
    }

    // Shared by both WriteOperations and ReadOperations via nested-class access
    private static ushort CalculateCRC(byte[] data, int length)
    {
        ushort crc = ushort.MaxValue;
        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if (((uint)crc & 1u) != 0)
                {
                    crc >>= 1;
                    crc = (ushort)(crc ^ 0xA001u);
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
        return crc;
    }
}

} // namespace ATS.Hardware
