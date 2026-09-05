using EasyModbus;

namespace PlcScada;

/// <summary>
/// Thread-safe wrapper around EasyModbus for Haiwell PLC communication.
/// All public methods may throw on network or Modbus errors; callers handle them.
/// </summary>
internal sealed class PlcCommunication : IDisposable
{
    private ModbusClient? _client;
    private readonly object _lock = new();

    public bool IsConnected
    {
        get { lock (_lock) { return _client?.Connected ?? false; } }
    }

    public string LastError { get; private set; } = string.Empty;

    // Returns true on success; sets LastError on failure.
    public bool Connect(string ip, int port)
    {
        lock (_lock)
        {
            try
            {
                _client?.Disconnect();
                _client = new ModbusClient(ip, port)
                {
                    ConnectionTimeout = 3000
                };
                _client.Connect();
                LastError = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _client = null;
                return false;
            }
        }
    }

    public void Disconnect()
    {
        lock (_lock)
        {
            try { _client?.Disconnect(); } catch { }
            _client = null;
        }
    }

    public bool[] ReadDiscreteInputs(int startAddress, int count)
    {
        lock (_lock)
        {
            EnsureConnected();
            return _client!.ReadDiscreteInputs(startAddress, count);
        }
    }

    public bool[] ReadCoils(int startAddress, int count)
    {
        lock (_lock)
        {
            EnsureConnected();
            return _client!.ReadCoils(startAddress, count);
        }
    }

    public int[] ReadHoldingRegisters(int startAddress, int count)
    {
        lock (_lock)
        {
            EnsureConnected();
            return _client!.ReadHoldingRegisters(startAddress, count);
        }
    }

    public void WriteCoil(int address, bool value)
    {
        lock (_lock)
        {
            EnsureConnected();
            _client!.WriteSingleCoil(address, value);
        }
    }

    public void WriteRegister(int address, int value)
    {
        lock (_lock)
        {
            EnsureConnected();
            _client!.WriteSingleRegister(address, value);
        }
    }

    private void EnsureConnected()
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("PLC is not connected.");
    }

    public void Dispose() => Disconnect();
}
