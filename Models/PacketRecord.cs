namespace TSProxyCapture.Models;

/// <summary>
/// Represents a single captured packet (already XOR decoded).
/// </summary>
public record PacketRecord(
    DateTime Timestamp,
    PacketDirection Direction,
    byte[] DecodedBytes,
    int Pid,
    string ProcessName
)
{
    private string? _hexDump;
    public string HexDump => _hexDump ??= FormatTsHexDump(DecodedBytes);

    public static string FormatTsHexDump(byte[] data)
    {
        if (data == null || data.Length == 0) return string.Empty;

        var sb = new System.Text.StringBuilder(data.Length * 3);
        int i = 0;

        while (i < data.Length)
        {
            if (IsTsHeader(data, i))
            {
                int payloadLen = data[i + 2] | (data[i + 3] << 8);
                int totalPacketLen = payloadLen + 4;
                int chunkLen = Math.Min(totalPacketLen, data.Length - i);

                AppendHexBytes(sb, data, i, chunkLen);
                i += chunkLen;

                if (i < data.Length)
                    sb.AppendLine();
            }
            else
            {
                // Partial/leftover bytes before next F4 44 header
                int nextHeader = -1;
                for (int j = i + 1; j <= data.Length - 4; j++)
                {
                    if (IsTsHeader(data, j))
                    {
                        nextHeader = j;
                        break;
                    }
                }

                int chunkLen = nextHeader != -1 ? nextHeader - i : data.Length - i;
                AppendHexBytes(sb, data, i, chunkLen);
                i += chunkLen;

                if (i < data.Length)
                    sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static bool IsTsHeader(byte[] data, int i)
    {
        if (i + 3 >= data.Length) return false;
        if (data[i] != 0xF4 || data[i + 1] != 0x44) return false;

        int payloadLen = data[i + 2] | (data[i + 3] << 8);
        return payloadLen >= 0 && payloadLen <= 16384;
    }

    private static void AppendHexBytes(System.Text.StringBuilder sb, byte[] data, int start, int length)
    {
        for (int k = 0; k < length; k++)
        {
            if (k > 0) sb.Append(' ');
            sb.Append(data[start + k].ToString("X2"));
        }
    }
}
