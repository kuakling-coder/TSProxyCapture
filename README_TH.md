# TS Proxy Capture (คู่มือสำหรับนักพัฒนา)

**TS Proxy Capture** คือโปรแกรม TCP Proxy Relay & Packet Sniffer ที่พัฒนาขึ้นสำหรับเกม **TS Online** โดยเฉพาะ สร้างด้วย **C# .NET 8.0 Windows Forms** สามารถดักจับ ถอดรหัส XOR และวิเคราะห์ข้อมูลแพ็คเกตระหว่างตัวเกมกับเซิร์ฟเวอร์จริงได้แบบ Real-time

---

## ✨ คุณสมบัติเด่น (Features)

- **🔑 ถอดรหัส XOR แบบ Real-Time**  
  ถอดรหัสแพ็คเกตแบบสองทิศทาง (`Client -> Server` และ `Server -> Client`) แบบ Real-time ตามคีย์ XOR ที่ตั้งค่าได้ (ค่าเริ่มต้น: `173` / `0xAD`)

- **🎯 ระบุและกรองข้อมูลตาม Process (PID)**  
  ใช้ Win32 Extended TCP Tables เพื่อเชื่อมโยงพอร์ต TCP กับ Process ID (`PID`) และชื่อโปรแกรม (`aLogin.exe`)
  - **สลับตัวกรองทันทีโดยไม่หน่วง**: สลับดูข้อมูลทั้งหมด (`All`) หรือเลือกเฉพาะ Process ที่ต้องการได้ทันทีด้วยระบบ Batch RTF Rendering
  - **ขยายความกว้างคอลัมน์อัตโนมัติ**: คอลัมน์รายการ Process ปรับความกว้างอัตโนมัติตามความยาวข้อความเสมอ

- **📜 จัดรูปแบบ Hex Dump ตามโครงสร้าง TS Packet (`FormatTsHexDump`)**  
  ตัดบรรทัดตามโครงสร้างโปรโตคอลของเกม TS Online (`F4 44 [ushort Length]`):
  - **Bundled Packets**: แยกแพ็คเกตที่ถูกส่งรวมกันใน 1 TCP buffer ออกเป็นบรรทัดละ 1 แพ็คเกตอย่างแม่นยำ
  - **TCP Fragmentation**: จัดการแพ็คเกตที่ถูกตัดข้าม TCP buffer ทำให้ทุกส่วนหัวแพ็คเกตใหม่ (`F4 44`) เริ่มต้นที่ต้นบรรทัดใหม่อย่างเป็นระเบียบ

- **👤 ตรวจจับและติดป้ายชื่อ Account ID อัตโนมัติและกำหนดเอง**  
  - **ตรวจจับอัตโนมัติ**: อ่าน Account ID จากแพ็คเกต Login (`C -> S` OpCode `0x01`) และยืนยันเมื่อเข้าสู่เกม (`S -> C` Enter World `14 08`) โดยเปลี่ยนป้ายชื่อเป็น `🎮 aLogin (PID: 40196 [1547318])` อัตโนมัติ
  - **สลับบัญชีอัตโนมัติ**: หากผู้เล่นสลับตัวละครหรือเปลี่ยนบัญชีบน Process (PID) เดิม ระบบจะอัปเดตป้ายชื่อ Account ID ใหม่อัตโนมัติทันที
  - **กำหนดเองได้ตลอดเวลา**: คลิกขวาที่ชื่อ Process แล้วเลือก **Set Account Id...** เพื่อตั้งชื่อหรือแก้ไขป้ายชื่อด้วยตนเอง

- **🖥️ หน้าต่าง Dark Theme พร้อมแถบ Splitter**  
  - ดีไซน์ Dark Theme สบายตา ภาษาอังกฤษ 100%
  - มีแถบ **Splitter** ระหว่างพาเนลซ้ายกับขวา สามารถคลิกลากปรับขนาดความกว้างได้อย่างอิสระ
  - รองรับเมนูคลิกขวาในกล่องข้อความ Log (`Copy`, `Select All`, `Clear`)

---

## 🛠️ ความต้องการของระบบและการเตรียมไฟล์

1. **ระบบปฏิบัติการ**: Windows 10/11 ที่ติดตั้ง **[.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)**
2. **ไฟล์รายชื่อเซิร์ฟเวอร์ (`Server.ini`)**:  
   คัดลอกไฟล์ `Server.ini` จากโฟลเดอร์เกม TS Online มาวางไว้ในโฟลเดอร์เดียวกับ `TSProxyCapture.exe`
   
   **ตัวอย่างรูปแบบไฟล์ (`Server.ini`)**:
   ```ini
   24.ง]ญนงKถO16(ฅxฦW)*210.242.243.134
   19.ง]ญนงKถO15(ญปดไ)*210.242.243.119
   16.ง]ญนงKถO14-1(ฅxฦW)*210.242.243.134
   ```

---

## 📂 โครงสร้างสถาปัตยกรรมโปรเจกต์ (Project Architecture)

```
TSProxyCapture/
│
├── Models/
│   ├── ServerEntry.cs         # โครงสร้างข้อมูลรายชื่อเซิร์ฟเวอร์จาก Server.ini
│   └── PacketRecord.cs        # จัดเก็บข้อมูลแพ็คเกตและอัลกอริทึม FormatTsHexDump
│
├── Services/
│   ├── ServerIniParser.cs     # อ่านไฟล์ Server.ini รองรับภาษาไทย (Windows-874)
│   ├── PidLookup.cs           # ค้นหา PID จากพอร์ต TCP ผ่าน Win32 GetExtendedTcpTable
│   ├── ProxyServer.cs         # เซิร์ฟเวอร์ TCP Relay Listener
│   └── ProxySession.cs        # จัดการเซสชันรับ-ส่งและถอดรหัส XOR สองทิศทาง
│
├── MainForm.cs                # ส่วนควบคุม UI, Auto Account ID Detection และ RTF Engine
└── MainForm.Designer.cs       # ออกแบบหน้าต่าง Dark Theme, Splitter และ Controls
```