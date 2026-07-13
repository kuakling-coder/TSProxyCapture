using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TSProxyCapture.Services;

/// <summary>
/// Uses GetExtendedTcpTable P/Invoke to map a local TCP port to its owning PID.
/// Much faster than shelling out to netstat.exe (~1ms vs ~200ms).
/// </summary>
public static class PidLookup
{
    private const int AF_INET = 2;
    private const int TCP_TABLE_OWNER_PID_ALL = 5;
    private const uint NO_ERROR = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID
    {
        public uint dwState;
        public uint dwLocalAddr;
        public byte localPort1;  // high byte (network order)
        public byte localPort2;  // low byte
        public byte localPort3;  // always 0
        public byte localPort4;  // always 0
        public uint dwRemoteAddr;
        public byte remotePort1;
        public byte remotePort2;
        public byte remotePort3;
        public byte remotePort4;
        public int dwOwningPid;

        public ushort LocalPort => (ushort)((localPort1 << 8) | localPort2);
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(
        IntPtr pTcpTable, ref int pdwSize, bool bOrder,
        int ulAf, int tableClass, uint reserved);

    /// <summary>
    /// Find PID and process name by local TCP port.
    /// Retries up to <paramref name="maxRetries"/> times to handle TCP table timing.
    /// </summary>
    public static (int Pid, string ProcessName) GetByLocalPort(int localPort, int maxRetries = 5)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var result = QueryTcpTable(localPort);
            if (result.Pid > 0)
                return result;

            Thread.Sleep(100);
        }

        return (0, "Unknown");
    }

    private static (int Pid, string ProcessName) QueryTcpTable(int localPort)
    {
        int bufferSize = 0;

        // First call: get required buffer size
        GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);

        IntPtr tablePtr = Marshal.AllocHGlobal(bufferSize);
        try
        {
            uint result = GetExtendedTcpTable(tablePtr, ref bufferSize, true,
                AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);

            if (result != NO_ERROR)
                return (0, "Unknown");

            int numEntries = Marshal.ReadInt32(tablePtr);
            int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
            IntPtr rowPtr = tablePtr + Marshal.SizeOf<MIB_TCPTABLE_OWNER_PID>();

            for (int i = 0; i < numEntries; i++)
            {
                var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                if (row.LocalPort == localPort && row.dwOwningPid > 0)
                {
                    int pid = row.dwOwningPid;
                    string name = "Unknown";
                    try
                    {
                        name = Process.GetProcessById(pid).ProcessName;
                    }
                    catch { }
                    return (pid, name);
                }
                rowPtr += rowSize;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(tablePtr);
        }

        return (0, "Unknown");
    }
}
