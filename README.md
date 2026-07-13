# TS Proxy Capture

**TS Proxy Capture** is a powerful, modern TCP Proxy Relay & Packet Sniffer designed specifically for **TS Online**. Built with **C# .NET 8.0 Windows Forms**, it captures, decrypts, and inspects network packets flowing between game clients and real servers in real-time.

---

## ✨ Features

- **🔑 Real-Time XOR Decryption**  
  Automatically decrypts bidirectional traffic (`Client -> Server` and `Server -> Client`) using configurable XOR keys (Default: `173` / `0xAD`).

- **🎯 Process (PID) Identification & Filtering**  
  Uses Windows Kernel Extended TCP Tables to accurately map every incoming TCP connection to its originating client Process ID (`PID`) and Process Name (`aLogin.exe`).
  - **Instant Filter Switching**: Switch between `All` processes or a specific `PID` instantly with zero UI lag thanks to batch RTF rendering.
  - **Dynamic Auto-Resizing**: Process list columns automatically expand to fit longer process names and account labels.

- **📜 Smart TS Packet Line-Breaking (`FormatTsHexDump`)**  
  Properly formats Hex Dumps according to the TS Online protocol (`F4 44 [ushort Length]`):
  - **Bundled Packets**: Automatically breaks multiple concatenated TS packets within a single TCP read onto separate lines.
  - **TCP Fragmentation**: Seamlessly formats split packets across buffer boundaries so every new TS header (`F4 44`) starts on a clean new line.

- **👤 Automatic & Manual Account ID Labeling**  
  - **Auto-Detection**: Captures Account IDs during Login (`C -> S` OpCode `0x01`) and confirms them upon entering the world (`S -> C` Enter World packet `14 08`). Automatically labels process items as `🎮 aLogin (PID: 40196 [1547318])`.
  - **Dynamic Account Switching**: Automatically updates the Account ID label if a player logs out and logs in with a different character on the same running process.
  - **Manual Override**: Right-click any process in the list and select **Set Account Id...** to manually tag or edit an account identifier.

- **🖥️ Modern Dark UI & Splitter Control**  
  - Clean, eye-friendly Dark Theme interface.
  - Interactive **Splitter** bar between the left process list and main log area for customizable layout sizing.
  - Full context menu support (`Copy`, `Select All`, `Clear`) inside the RichTextBox log view.

---

## 🛠️ Prerequisites & Installation

1. **System Requirements**: Windows 10/11 with **[.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)** installed.
2. **Server Configuration (`Server.ini`)**:
   Ensure a valid `Server.ini` file is placed in the same directory as `TSProxyCapture.exe` (copied from your game folder).
   
   **Format Example (`Server.ini`)**:
   ```ini
   24.ง]ญนงKถO16(ฅxฦW)*210.242.243.134
   19.ง]ญนงKถO15(ญปดไ)*210.242.243.119
   16.ง]ญนงKถO14-1(ฅxฦW)*210.242.243.134
   ```

---

## 🚀 How to Use

1. **Launch the Program**:  
   Run `TSProxyCapture.exe` located in `bin\Debug\net8.0-windows\TSProxyCapture.exe` (or your build output directory).

2. **Configure Proxy Settings (Top Panel)**:
   - **Server**: Select the target TS Online server from the dropdown list loaded from `Server.ini`.
   - **Port**: Enter the local listening port and target server port (Default: `6414`).
   - **Xor**: Enter the XOR decryption key (Default: `173` decimal = `0xAD`).

3. **Start Proxy Relay & Capture**:
   - Click the blue **Start** button.
   - The proxy will start listening on `127.0.0.1:6414` and relay traffic to the selected server IP while automatically recording packets in real-time.

4. **Connect Game Client**:
   - Point your TS Online client or login tool to connect via `127.0.0.1:6414`.
   - Connected processes will immediately appear in the left process list (`Panel Left`).

5. **Filter & Inspect Packets**:
   - Click **All** to view traffic from all game clients simultaneously.
   - Click any specific process (e.g., `🎮 aLogin (PID: 40196 [1547318])`) to isolate and view only packets belonging to that session.
   - Click **Pause** / **Capture** at any time to freeze log scrolling while continuing proxy relaying in the background.

6. **Manage Account IDs & Logs**:
   - **Right-click a Process**: Select **Set Account Id...** to manually label a process.
   - **Right-click the Log Box**: Select **Copy**, **Select All**, or **Clear** to manage captured packet text.

---

## 📂 Project Architecture

```
TSProxyCapture/
│
├── Models/
│   ├── ServerEntry.cs         # Represents Server.ini entries (Name * IP)
│   └── PacketRecord.cs        # Stores decoded packets & FormatTsHexDump line-breaking
│
├── Services/
│   ├── ServerIniParser.cs     # Windows-874 parser for Server.ini
│   ├── PidLookup.cs           # Win32 GetExtendedTcpTable P/Invoke port-to-PID lookup
│   ├── ProxyServer.cs         # Async TCP Relay server listener
│   └── ProxySession.cs        # Individual client-server bidirectional relay session
│
├── MainForm.cs                # Core UI logic, auto Account ID detection, RTF engine
└── MainForm.Designer.cs       # Dark theme layout, Splitter, ListView & RichTextBox
```

---

## 📄 License

Developed for TS Online packet research, proxying, and analysis.