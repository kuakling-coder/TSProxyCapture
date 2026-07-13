using System.Runtime.InteropServices;
using System.Text;
using TSProxyCapture.Models;
using TSProxyCapture.Services;

namespace TSProxyCapture;

public partial class MainForm : Form
{
    private readonly ProxyServer _proxy = new();
    private readonly List<PacketRecord> _packetBuffer = new();
    private readonly object _bufferLock = new();
    private bool _isCapturing;
    private int _currentFilterPid = -1; // -1 = All
    private List<ServerEntry> _servers = new();

    // Track connected processes for the filter list (PID -> ProcessName)
    private readonly Dictionary<int, string> _connectedProcesses = new();
    // Track Account Id assigned to processes (PID -> AccountId)
    private readonly Dictionary<int, string> _accountIds = new();
    // Track pending Account Id detected from C->S login packet (PID -> AccountId)
    private readonly Dictionary<int, string> _pendingLoginAccountIds = new();

    // For suppressing RichTextBox redraw during bulk updates
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    private const int WM_SETREDRAW = 0x000B;

    public MainForm()
    {
        InitializeComponent();
    }

    // ================================================================
    // Initialization
    // ================================================================

    private void MainForm_Load(object? sender, EventArgs e)
    {
        LoadServers();
        InitializeFilterList();
        SetupProxyEvents();
        SetupContextMenus();
        UpdateUIState();

        lvFilter.Resize += (s, e) => AdjustFilterColumnWidth();
    }

    private void LoadServers()
    {
        string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Server.ini");
        _servers = ServerIniParser.Parse(iniPath);

        cboServer.Items.Clear();
        foreach (var server in _servers)
            cboServer.Items.Add(server);

        if (cboServer.Items.Count > 0)
            cboServer.SelectedIndex = 0;
    }

    private void InitializeFilterList()
    {
        lvFilter.Items.Clear();
        var allItem = new ListViewItem("📋  All") { Tag = -1 };
        lvFilter.Items.Add(allItem);
        lvFilter.Items[0].Selected = true;
        AdjustFilterColumnWidth();
    }

    private void AdjustFilterColumnWidth()
    {
        if (lvFilter.Columns.Count > 0)
        {
            lvFilter.Columns[0].Width = -1; // auto-size to longest item text
            int contentWidth = lvFilter.Columns[0].Width;
            int clientWidth = lvFilter.ClientSize.Width - 4;
            lvFilter.Columns[0].Width = Math.Max(contentWidth, Math.Max(clientWidth, 100));
        }
    }

    private void SetupProxyEvents()
    {
        _proxy.SessionConnected += session =>
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnSessionConnected(session));
                return;
            }
            OnSessionConnected(session);
        };

        _proxy.PacketCaptured += packet =>
        {
            CheckAutoAccountIdDetection(packet);

            // Always buffer the packet (thread-safe)
            lock (_bufferLock)
            {
                _packetBuffer.Add(packet);
            }

            // Display in real-time only if actively capturing and matches selected filter
            if (_isCapturing)
            {
                BeginInvoke(() =>
                {
                    if (_isCapturing && (_currentFilterPid == -1 || packet.Pid == _currentFilterPid))
                    {
                        AppendPacketToLog(packet);
                    }
                });
            }
        };
    }

    private void CheckAutoAccountIdDetection(PacketRecord packet)
    {
        int pid = packet.Pid;
        if (pid <= 0) return;

        byte[] data = packet.DecodedBytes;

        if (packet.Direction == PacketDirection.ClientToServer)
        {
            // Client Login Packet: F4 44 [len] 01 [pwLen] [AccId 4 bytes]
            if (data.Length >= 10 && data[0] == 0xF4 && data[1] == 0x44 && data[4] == 0x01)
            {
                uint accIdNum = BitConverter.ToUInt32(data, 6);
                if (accIdNum > 0)
                {
                    lock (_bufferLock)
                    {
                        _pendingLoginAccountIds[pid] = accIdNum.ToString();
                    }
                }
            }
        }
        else if (packet.Direction == PacketDirection.ServerToClient)
        {
            // Server Enter World Packet: contains F4 44 xx xx 14 08
            lock (_bufferLock)
            {
                if (_pendingLoginAccountIds.TryGetValue(pid, out var pendingAccId))
                {
                    if (ContainsEnterWorldPacket(data))
                    {
                        bool isNewOrChanged = !_accountIds.TryGetValue(pid, out var currentAccId)
                                              || currentAccId != pendingAccId;

                        _accountIds[pid] = pendingAccId;
                        _pendingLoginAccountIds.Remove(pid);

                        if (isNewOrChanged)
                        {
                            BeginInvoke(() =>
                            {
                                UpdateFilterItemText(pid);
                                AppendSystemMessage($"Auto-detected Account Id for PID {pid} → [{pendingAccId}]");
                            });
                        }
                    }
                }
            }
        }
    }

    private static bool ContainsEnterWorldPacket(byte[] data)
    {
        if (data.Length < 6) return false;
        for (int i = 0; i <= data.Length - 6; i++)
        {
            if (data[i] == 0xF4 && data[i + 1] == 0x44 && data[i + 4] == 0x14 && data[i + 5] == 0x08)
            {
                return true;
            }
        }
        return false;
    }

    private void SetupContextMenus()
    {
        // === Process Filter ListView Context Menu ===
        var ctxFilter = new ContextMenuStrip
        {
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.White,
            ShowImageMargin = false
        };

        var mnuAccountId = new ToolStripMenuItem("Set Account Id...");
        mnuAccountId.Click += (s, e) => OpenAccountIdModalForSelectedProcess();
        ctxFilter.Items.Add(mnuAccountId);

        ctxFilter.Opening += (s, e) =>
        {
            int pid = GetSelectedFilterPid();
            mnuAccountId.Enabled = pid > 0;
        };

        lvFilter.ContextMenuStrip = ctxFilter;
        lvFilter.MouseUp += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = lvFilter.HitTest(e.Location);
                if (hit.Item != null)
                {
                    hit.Item.Selected = true;
                }
            }
        };

        // === RichTextBox Log Context Menu ===
        var ctxLog = new ContextMenuStrip
        {
            BackColor = Color.FromArgb(45, 45, 48),
            ForeColor = Color.White,
            ShowImageMargin = false
        };

        var mnuCopy = new ToolStripMenuItem("Copy");
        mnuCopy.Click += (s, e) => { if (rtbLog.SelectionLength > 0) rtbLog.Copy(); };

        var mnuSelectAll = new ToolStripMenuItem("Select All");
        mnuSelectAll.Click += (s, e) => rtbLog.SelectAll();

        var mnuClear = new ToolStripMenuItem("Clear");
        mnuClear.Click += (s, e) => BtnClear_Click(null, EventArgs.Empty);

        ctxLog.Items.Add(mnuCopy);
        ctxLog.Items.Add(mnuSelectAll);
        ctxLog.Items.Add(new ToolStripSeparator());
        ctxLog.Items.Add(mnuClear);

        ctxLog.Opening += (s, e) =>
        {
            mnuCopy.Enabled = rtbLog.SelectionLength > 0;
            mnuSelectAll.Enabled = rtbLog.TextLength > 0;
            mnuClear.Enabled = rtbLog.TextLength > 0;
        };

        rtbLog.ContextMenuStrip = ctxLog;
    }

    private void OpenAccountIdModalForSelectedProcess()
    {
        int pid = GetSelectedFilterPid();
        if (pid <= 0) return;

        string procName = _connectedProcesses.TryGetValue(pid, out var pName) ? pName : "Process";
        _accountIds.TryGetValue(pid, out var currentAccId);

        string? newAccId = ShowInputBox(
            $"Enter Account Id for {procName} (PID: {pid}):",
            "Set Account Id",
            currentAccId ?? ""
        );

        if (newAccId != null)
        {
            if (string.IsNullOrWhiteSpace(newAccId))
            {
                _accountIds.Remove(pid);
            }
            else
            {
                _accountIds[pid] = newAccId.Trim();
            }

            UpdateFilterItemText(pid);
            RefreshLogWithFilter();
        }
    }

    private void UpdateFilterItemText(int pid)
    {
        foreach (ListViewItem item in lvFilter.Items)
        {
            if (item.Tag is int itemPid && itemPid == pid)
            {
                string procName = _connectedProcesses.TryGetValue(pid, out var name) ? name : "Unknown";
                _accountIds.TryGetValue(pid, out var accId);

                item.Text = string.IsNullOrEmpty(accId)
                    ? $"🎮  {procName} (PID: {pid})"
                    : $"🎮  {procName} (PID: {pid} [{accId}])";
                break;
            }
        }
        AdjustFilterColumnWidth();
    }

    private static string? ShowInputBox(string prompt, string title, string defaultValue = "")
    {
        using var form = new Form
        {
            Width = 360,
            Height = 170,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(37, 37, 38),
            ForeColor = Color.White
        };

        var label = new Label
        {
            Left = 15,
            Top = 15,
            Width = 315,
            Text = prompt,
            ForeColor = Color.White
        };

        var textBox = new TextBox
        {
            Left = 15,
            Top = 45,
            Width = 315,
            Text = defaultValue,
            BackColor = Color.FromArgb(62, 62, 66),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        var buttonOk = new Button
        {
            Text = "OK",
            Left = 145,
            Width = 85,
            Top = 85,
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };

        var buttonCancel = new Button
        {
            Text = "Cancel",
            Left = 245,
            Width = 85,
            Top = 85,
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(63, 63, 70),
            ForeColor = Color.White
        };

        form.Controls.Add(label);
        form.Controls.Add(textBox);
        form.Controls.Add(buttonOk);
        form.Controls.Add(buttonCancel);
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;

        return form.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : null;
    }

    private int GetSelectedFilterPid()
    {
        if (lvFilter.SelectedItems.Count > 0 && lvFilter.SelectedItems[0].Tag is int pid)
            return pid;
        return -1;
    }

    private void OnSessionConnected(ProxySession session)
    {
        if (session.Pid > 0 && !_connectedProcesses.ContainsKey(session.Pid))
        {
            _connectedProcesses[session.Pid] = session.ProcessName;

            _accountIds.TryGetValue(session.Pid, out var accId);
            string label = string.IsNullOrEmpty(accId)
                ? $"🎮  {session.ProcessName} (PID: {session.Pid})"
                : $"🎮  {session.ProcessName} (PID: {session.Pid} [{accId}])";

            var item = new ListViewItem(label)
            {
                Tag = session.Pid
            };
            lvFilter.Items.Add(item);
            AdjustFilterColumnWidth();
        }
    }

    // ================================================================
    // Button Handlers
    // ================================================================

    private void BtnStartStop_Click(object? sender, EventArgs e)
    {
        if (_proxy.IsRunning)
            StopProxy();
        else
            StartProxy();
    }

    private void BtnCapture_Click(object? sender, EventArgs e)
    {
        if (_isCapturing)
            PauseCapture();
        else
            StartCapture();
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        rtbLog.Clear();
        lock (_bufferLock)
        {
            _packetBuffer.Clear();
        }
    }

    private void LvFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int filterPid = -1; // -1 = All
        if (lvFilter.SelectedItems.Count > 0 && lvFilter.SelectedItems[0].Tag is int pid)
        {
            filterPid = pid;
        }

        if (_currentFilterPid == filterPid) return;
        _currentFilterPid = filterPid;

        RefreshLogWithFilter();
    }

    // ================================================================
    // Proxy Control
    // ================================================================

    private void StartProxy()
    {
        // Validate inputs
        if (cboServer.SelectedItem is not ServerEntry server)
        {
            MessageBox.Show("Please select a Server.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
        {
            MessageBox.Show("Invalid Port number (1-65535).", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!byte.TryParse(txtXor.Text, out byte xorKey))
        {
            MessageBox.Show("Invalid XOR key (0-255).", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _proxy.Start(port, server.IpAddress, port, xorKey);

            // Reset state
            _connectedProcesses.Clear();
            _accountIds.Clear();
            _pendingLoginAccountIds.Clear();
            InitializeFilterList();
            lock (_bufferLock) { _packetBuffer.Clear(); }
            rtbLog.Clear();

            UpdateUIState();

            AppendSystemMessage(
                $"Proxy started: 127.0.0.1:{port} → {server.IpAddress}:{port}  " +
                $"(XOR: 0x{xorKey:X2})");

            StartCapture();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start proxy:\n{ex.Message}",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void StopProxy()
    {
        _isCapturing = false;
        _proxy.Stop();
        UpdateUIState();
        AppendSystemMessage("Proxy stopped.");
    }

    private void StartCapture()
    {
        _isCapturing = true;
        UpdateUIState();
        AppendSystemMessage("Capture started — packets are being logged in real-time.");
    }

    private void PauseCapture()
    {
        _isCapturing = false;
        UpdateUIState();
        AppendSystemMessage("Capture paused — use Process filter to view specific packets.");

        // Apply current filter selection
        RefreshLogWithFilter();
    }

    // ================================================================
    // UI State Management
    // ================================================================

    private void UpdateUIState()
    {
        bool proxyRunning = _proxy.IsRunning;
        bool capturing = _isCapturing;

        // Individual controls inside panelTop
        // (these persist even when panelTop is re-enabled)
        cboServer.Enabled = !proxyRunning;
        txtPort.Enabled = !proxyRunning;
        txtXor.Enabled = !proxyRunning;

        // Whole panelTop disabled during capture (including Stop button)
        panelTop.Enabled = !capturing;

        // Panel Left is always enabled so user can filter by process in real-time or when paused
        panelLeft.Enabled = true;

        // Start/Stop button appearance
        btnStartStop.Text = proxyRunning ? "Stop" : "Start";
        btnStartStop.BackColor = proxyRunning
            ? Color.FromArgb(200, 50, 50)   // Red for Stop
            : Color.FromArgb(0, 122, 204);  // Blue for Start

        // Capture button (in panelMain, always accessible)
        btnCapture.Enabled = proxyRunning;
        btnCapture.Text = capturing ? "Pause" : "Capture";
        btnCapture.BackColor = capturing
            ? Color.FromArgb(200, 150, 0)   // Orange/amber for Pause
            : Color.FromArgb(0, 122, 204);  // Blue for Capture
    }

    // ================================================================
    // Log Display
    // ================================================================

    private void AppendPacketToLog(PacketRecord packet)
    {
        string dirStr;
        Color headerColor;

        if (packet.Direction == PacketDirection.ClientToServer)
        {
            dirStr = "C -> S";
            headerColor = Color.DodgerBlue;
        }
        else
        {
            dirStr = "S -> C";
            headerColor = Color.LimeGreen;
        }

        string pidDisplay = FormatPidDisplay(packet.Pid, packet.ProcessName);

        string header = $"[{packet.Timestamp:HH:mm:ss.fff}] {dirStr} " +
                         $"({pidDisplay}) [{packet.DecodedBytes.Length} bytes]:";
        string hexDump = packet.HexDump;

        AppendColoredText(header + Environment.NewLine, headerColor);
        AppendColoredText(hexDump + Environment.NewLine + Environment.NewLine, Color.White);

        // Auto-scroll to bottom
        rtbLog.SelectionStart = rtbLog.TextLength;
        rtbLog.ScrollToCaret();
    }

    private void AppendSystemMessage(string message)
    {
        AppendColoredText(
            $"[{DateTime.Now:HH:mm:ss.fff}] ⚙  {message}" + Environment.NewLine,
            Color.Gray);
    }

    private void AppendColoredText(string text, Color color)
    {
        rtbLog.SelectionStart = rtbLog.TextLength;
        rtbLog.SelectionLength = 0;
        rtbLog.SelectionColor = color;
        rtbLog.AppendText(text);
    }

    /// <summary>
    /// Regenerate the entire RichTextBox content from the buffer in ~5ms
    /// using direct RTF generation instead of thousands of AppendText calls.
    /// </summary>
    private void RefreshLogWithFilter()
    {
        int filterPid = _currentFilterPid;

        // Get filtered packets from buffer
        List<PacketRecord> packets;
        lock (_bufferLock)
        {
            packets = filterPid == -1
                ? _packetBuffer.ToList()
                : _packetBuffer.Where(p => p.Pid == filterPid).ToList();
        }

        // Cap displayed packets to last 2000 to keep UI responsive
        if (packets.Count > 2000)
            packets = packets.TakeLast(2000).ToList();

        SendMessage(rtbLog.Handle, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        try
        {
            rtbLog.Rtf = BuildRtfLog(packets);
        }
        finally
        {
            SendMessage(rtbLog.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            rtbLog.Invalidate();
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();
        }
    }

    private string FormatPidDisplay(int pid, string processName)
    {
        if (pid <= 0) return $"Session {processName}";

        if (_accountIds.TryGetValue(pid, out var accId) && !string.IsNullOrEmpty(accId))
        {
            return $"PID:{pid} {processName} [{accId}]";
        }
        return $"PID:{pid} {processName}";
    }

    private string BuildRtfLog(List<PacketRecord> packets)
    {
        var sb = new StringBuilder(packets.Count * 256);
        sb.Append(@"{\rtf1\ansi\deff0{\fonttbl{\f0 Consolas;}}");
        sb.Append(@"{\colortbl ;\red30\green144\blue255;\red50\green205\blue50;\red255\green255\blue255;}");
        sb.Append(@"\viewkind4\uc1\f0\fs19 ");

        foreach (var p in packets)
        {
            string dirStr = p.Direction == PacketDirection.ClientToServer ? "C -> S" : "S -> C";
            int colorIdx = p.Direction == PacketDirection.ClientToServer ? 1 : 2;

            string pidDisplay = FormatPidDisplay(p.Pid, p.ProcessName);

            string header = $"[{p.Timestamp:HH:mm:ss.fff}] {dirStr} ({pidDisplay}) [{p.DecodedBytes.Length} bytes]:";
            string rtfHexDump = p.HexDump.Replace("\r\n", @"\line ").Replace("\n", @"\line ");

            sb.Append(@"\cf").Append(colorIdx).Append(' ').Append(header).Append(@"\line ");
            sb.Append(@"\cf3 ").Append(rtfHexDump).Append(@"\line\line ");
        }

        sb.Append('}');
        return sb.ToString();
    }

    // ================================================================
    // Cleanup
    // ================================================================

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _isCapturing = false;
        _proxy.Stop();
        base.OnFormClosing(e);
    }
}
