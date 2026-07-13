using System.Net;
using System.Net.Sockets;
using TSProxyCapture.Models;

namespace TSProxyCapture.Services;

/// <summary>
/// Manages a single client ↔ server TCP relay.
/// Captures and XOR-decodes all traffic flowing through.
/// </summary>
public class ProxySession : IDisposable
{
    public int Pid { get; }
    public string ProcessName { get; }
    public string SessionId { get; } = Guid.NewGuid().ToString("N")[..6];

    private readonly TcpClient _client;
    private readonly string _targetIp;
    private readonly int _targetPort;
    private readonly byte _xorKey;
    private TcpClient? _server;
    private bool _disposed;

    public ProxySession(TcpClient client, string targetIp, int targetPort, byte xorKey)
    {
        _client = client;
        _targetIp = targetIp;
        _targetPort = targetPort;
        _xorKey = xorKey;

        // Lookup PID from the client's ephemeral source port
        int clientPort = ((IPEndPoint)client.Client.RemoteEndPoint!).Port;
        (Pid, ProcessName) = PidLookup.GetByLocalPort(clientPort);
    }

    /// <summary>
    /// Start bidirectional relay between client and real server.
    /// </summary>
    public async Task StartAsync(Action<PacketRecord> onPacket, CancellationToken ct)
    {
        try
        {
            _server = new TcpClient();
            await _server.ConnectAsync(_targetIp, _targetPort, ct);

            var clientStream = _client.GetStream();
            var serverStream = _server.GetStream();

            // Relay both directions concurrently
            var toServer = RelayAsync(clientStream, serverStream,
                PacketDirection.ClientToServer, onPacket, ct);
            var toClient = RelayAsync(serverStream, clientStream,
                PacketDirection.ServerToClient, onPacket, ct);

            // When either direction closes, the session ends
            await Task.WhenAny(toServer, toClient);
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        finally
        {
            Dispose();
        }
    }

    private async Task RelayAsync(
        NetworkStream input, NetworkStream output,
        PacketDirection direction, Action<PacketRecord> onPacket,
        CancellationToken ct)
    {
        byte[] buffer = new byte[8192];
        try
        {
            while (!ct.IsCancellationRequested)
            {
                int bytesRead = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
                if (bytesRead == 0) break;

                // XOR decode for display only
                byte[] decoded = new byte[bytesRead];
                for (int i = 0; i < bytesRead; i++)
                    decoded[i] = (byte)(buffer[i] ^ _xorKey);

                // Notify listener (UI) about captured packet
                onPacket(new PacketRecord(
                    DateTime.Now,
                    direction,
                    decoded,
                    Pid,
                    ProcessName
                ));

                // Relay the ORIGINAL (raw/encoded) bytes to the other side
                await output.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try { _client.Close(); } catch { }
        try { _server?.Close(); } catch { }
    }
}
