# TS Proxy Capture — Beginner User Guide

Welcome to **TS Proxy Capture**! This tool allows you to easily capture and view TS Online network packets in real time while you play.

---

## 📋 What You Need Before Starting

1. **Windows 10 or 11 PC**
2. **Server.ini file**: Copy the `Server.ini` file from your TS Online game folder and paste it into the exact same folder where `TSProxyCapture.exe` is located.

---

## 🎮 Step-by-Step Instructions

### Step 1: Open the Program
Double-click `TSProxyCapture.exe`. You will see a dark-themed window with settings at the top, a process list on the left, and a log area on the right.

### Step 2: Select Your Server
- **Server**: Click the dropdown box at the top and select the TS Online server you want to play on.
- **Port**: Leave as default (`6414`).
- **Xor**: Leave as default (`173`).

### Step 3: Start the Proxy
Click the blue **Start** button at the top right.  
*Once started, the tool begins listening for your game connection.*

### Step 4: Connect Your Game
Open your TS Online game client or bot tool and configure its connection to:
- **IP Address**: `127.0.0.1`
- **Port**: `6414`

When your game connects, you will immediately see your game process appear in the **Process List** on the left panel!

---

## 🔍 Viewing & Filtering Packets

- **View All Traffic**: Click **📋 All** at the top of the left panel to see packets from all open game windows.
- **Filter by Game Window**: Click any specific process name (for example, `🎮 aLogin (PID: 40196 [1547318])`) to see only the packets belonging to that game character.
- **Automatic Account ID Display**: As soon as your character logs into the game world, the tool automatically identifies your Account ID and attaches it to the process name!
- **Manually Label an Account**: Right-click any process in the left list and choose **Set Account Id...** to type your own custom label or account number.

---

## ⏸️ Pausing & Copying Logs

- **Pause Log Scrolling**: Click the **Pause** button above the log box to freeze the display so you can read or copy text comfortably. Your game will continue running smoothly in the background without disconnecting.
- **Resume Log Scrolling**: Click **Capture** to resume live scrolling.
- **Copy / Clear Text**: Right-click anywhere inside the log box to **Copy** selected text, **Select All**, or **Clear** the log screen.

---

## 💡 Layout Adjustment
You can drag the divider bar between the left process list and the main log box left or right to adjust the width of the process panel to your liking.