using System.Runtime.InteropServices;
using System.Text;

namespace NIDAQTester12;

/// <summary>
/// Minimal P/Invoke bindings for the NI-DAQmx C API (nicaiu.dll).
///
/// nicaiu.dll is installed by the NI-DAQmx driver (free download from ni.com).
/// After installing the driver, the DLL is placed in C:\Windows\System32\
/// and is automatically found by the Windows DLL loader.
/// </summary>
/// <remarks>
/// This class is an internal implementation detail — callers use
/// <see cref="DaqManager12"/> and never call Daqmx directly.
///
/// If you need to add a new NI-DAQmx API function:
///   1. Declare the P/Invoke signature here, matching the nicaiu.dll C header.
///   2. Always wrap the return value with Daqmx.Check() in the caller:
/// <code>
///   Daqmx.Check(Daqmx.DAQmxSomeNewFunction(handle, args), "context label");
/// </code>
///   Check() is a no-op on success (code == 0) and throws a descriptive
///   InvalidOperationException on any non-zero NI-DAQmx error code.
/// </remarks>
internal static class Daqmx
{
    private const string Dll = "nicaiu";

    public const int Success = 0;

    // Terminal configuration
    public const int Val_RSE  = 10083;
    public const int Val_NRSE = 10078;
    public const int Val_Diff = 10106;

    // Units
    public const int Val_Volts = 10348;
    public const int Val_Amps  = 10342;

    // Sample clock timing
    public const int Val_ContSamps = 10123;
    public const int Val_Rising    = 10280;

    // Read fill mode
    public const int Val_GroupByChannel    = 0;
    public const int Val_GroupByScanNumber = 1;

    // Shunt resistor location
    public const int Val_Internal = 10200;
    public const int Val_External = 10167;

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxCreateTask(
        [MarshalAs(UnmanagedType.LPStr)] string taskName,
        out IntPtr taskHandle);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxCreateAIVoltageChan(
        IntPtr taskHandle,
        [MarshalAs(UnmanagedType.LPStr)] string physicalChannel,
        [MarshalAs(UnmanagedType.LPStr)] string nameToAssign,
        int terminalConfig,
        double minVal,
        double maxVal,
        int units,
        [MarshalAs(UnmanagedType.LPStr)] string? customScaleName);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxCreateAICurrentChan(
        IntPtr taskHandle,
        [MarshalAs(UnmanagedType.LPStr)] string physicalChannel,
        [MarshalAs(UnmanagedType.LPStr)] string nameToAssign,
        int terminalConfig,
        double minVal,
        double maxVal,
        int units,
        int shuntResistorLoc,
        double extShuntResistorVal,
        [MarshalAs(UnmanagedType.LPStr)] string? customScaleName);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxCfgSampClkTiming(
        IntPtr taskHandle,
        [MarshalAs(UnmanagedType.LPStr)] string? source,
        double rate,
        int activeEdge,
        int sampleMode,
        ulong sampsPerChanBufSize);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxStartTask(IntPtr taskHandle);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxStopTask(IntPtr taskHandle);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxClearTask(IntPtr taskHandle);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxReadAnalogF64(
        IntPtr taskHandle,
        int numSampsPerChan,
        double timeout,
        int fillMode,
        [Out] double[] readArray,
        uint arraySizeInSamps,
        out int sampsPerChanRead,
        IntPtr reserved);

    [DllImport(Dll, CallingConvention = CallingConvention.StdCall)]
    public static extern int DAQmxGetExtendedErrorInfo(
        [Out] byte[] errorString,
        uint bufferSize);

    public static string GetErrorString()
    {
        var buf = new byte[4096];
        DAQmxGetExtendedErrorInfo(buf, (uint)buf.Length);
        return Encoding.UTF8.GetString(buf).TrimEnd('\0');
    }

    public static void Check(int code, string context = "")
    {
        if (code == Success) return;
        string detail = GetErrorString();
        throw new InvalidOperationException(
            $"NI-DAQmx error {code}" +
            (string.IsNullOrEmpty(context) ? "" : $" [{context}]") +
            (string.IsNullOrEmpty(detail)  ? "" : $":\n{detail}"));
    }
}
