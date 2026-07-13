using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using TSProxyCapture.Models;

namespace TSProxyCapture.Services;

/// <summary>
/// TCP Proxy server that listens for incoming connections, relays them
/// to the real server, and captures all traffic.
/// </summary>
public class ProxyServer
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private readonly ConcurrentBag<ProxySession> _sessions = new();

    public string TargetIp { get; private set; } = "";
    public int TargetPort { get; private set; }
    public byte XorKey { get; private set; }
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Fired when a new client connects and PID is resolved.
    /// Called on a background thread — use Invoke for UI updates.
    /// </summary>
    public event Action<ProxySession>? SessionConnected;

    /// <summary>
    /// Fired for every packet captured (both directions).
    /// Called on a background thread — use Invoke for UI updates.
    /// </summary>
    public event Action<PacketRecord>? PacketCaptured;

    public void Start(int listenPort, string targetIp, int targetPort, byte xorKey)
    {
        if (IsRunning) return;

        TargetIp = targetIp;
        TargetPort = targetPort;
        XorKey = xorKey;

        _cts = new CancellationTokenSource();
        _listener = new TcpListener(IPAddress.Any, listenPort);
        _listener.Start();
        IsRunning = true;

        _ = AcceptLoopAsync(_cts.Token);
    }

    public void Stop()
    {
        if (!IsRunning) return;

        IsRunning = false;
        _cts?.Cancel();

        try { _listener?.Stop(); } catch { }

        // Dispose all active sessions
        foreach (var session in _sessions)
        {
            try { session.Dispose(); } catch { }
        }

        // Clear ConcurrentBag by replacing it (ConcurrentBag has no Clear method)
        while (_sessions.TryTake(out _)) { }

        _cts?.Dispose();
        _cts = null;
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync(ct);

                // Create session and resolve PID (may take ~100-500ms due to retries)
                var session = new ProxySession(client, TargetIp, TargetPort, XorKey);
                _sessions.Add(session);

                // Notify UI about new connection
                SessionConnected?.Invoke(session);

                // Start relaying in background (fire-and-forget)
                _ = session.StartAsync(
                    packet => PacketCaptured?.Invoke(packet),
                    ct
                );
            }
            catch (OperationCanceledException) { break; }
            catch (ObjectDisposedException) { break; }
            catch (Exception)
            {
                // Ignore individual accept errors, keep listening
            }
        }
    }
}
