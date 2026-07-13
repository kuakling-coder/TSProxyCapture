namespace TSProxyCapture;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // === Panel Top ===
    private Panel panelTop;
    private Label lblServer;
    private ComboBox cboServer;
    private Label lblPort;
    private TextBox txtPort;
    private Label lblXor;
    private TextBox txtXor;
    private Button btnStartStop;

    // === Panel Left ===
    private Panel panelLeft;
    private Label lblFilter;
    private ListView lvFilter;
    private Splitter splitterLeft;

    // === Panel Main ===
    private Panel panelMain;
    private Button btnCapture;
    private RichTextBox rtbLog;
    private Button btnClear;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        this.panelTop = new Panel();
        this.lblServer = new Label();
        this.cboServer = new ComboBox();
        this.lblPort = new Label();
        this.txtPort = new TextBox();
        this.lblXor = new Label();
        this.txtXor = new TextBox();
        this.btnStartStop = new Button();

        this.panelLeft = new Panel();
        this.lblFilter = new Label();
        this.lvFilter = new ListView();
        this.splitterLeft = new Splitter();

        this.panelMain = new Panel();
        this.btnCapture = new Button();
        this.rtbLog = new RichTextBox();
        this.btnClear = new Button();

        this.panelTop.SuspendLayout();
        this.panelLeft.SuspendLayout();
        this.panelMain.SuspendLayout();
        this.SuspendLayout();

        // ============================================================
        // panelTop
        // ============================================================
        this.panelTop.BackColor = Color.FromArgb(45, 45, 48);
        this.panelTop.Dock = DockStyle.Top;
        this.panelTop.Height = 50;
        this.panelTop.Controls.Add(this.btnStartStop);
        this.panelTop.Controls.Add(this.txtXor);
        this.panelTop.Controls.Add(this.lblXor);
        this.panelTop.Controls.Add(this.txtPort);
        this.panelTop.Controls.Add(this.lblPort);
        this.panelTop.Controls.Add(this.cboServer);
        this.panelTop.Controls.Add(this.lblServer);

        // lblServer
        this.lblServer.AutoSize = true;
        this.lblServer.ForeColor = Color.White;
        this.lblServer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        this.lblServer.Location = new Point(14, 16);
        this.lblServer.Text = "Server";

        // cboServer
        this.cboServer.DropDownStyle = ComboBoxStyle.DropDownList;
        this.cboServer.Location = new Point(68, 12);
        this.cboServer.Size = new Size(280, 25);
        this.cboServer.FlatStyle = FlatStyle.Flat;

        // lblPort
        this.lblPort.AutoSize = true;
        this.lblPort.ForeColor = Color.White;
        this.lblPort.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        this.lblPort.Location = new Point(365, 16);
        this.lblPort.Text = "Port";

        // txtPort
        this.txtPort.Location = new Point(400, 12);
        this.txtPort.Size = new Size(70, 25);
        this.txtPort.Text = "6414";
        this.txtPort.TextAlign = HorizontalAlignment.Center;
        this.txtPort.BackColor = Color.FromArgb(62, 62, 66);
        this.txtPort.ForeColor = Color.White;
        this.txtPort.BorderStyle = BorderStyle.FixedSingle;

        // lblXor
        this.lblXor.AutoSize = true;
        this.lblXor.ForeColor = Color.White;
        this.lblXor.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        this.lblXor.Location = new Point(485, 16);
        this.lblXor.Text = "Xor";

        // txtXor
        this.txtXor.Location = new Point(515, 12);
        this.txtXor.Size = new Size(55, 25);
        this.txtXor.Text = "173";
        this.txtXor.TextAlign = HorizontalAlignment.Center;
        this.txtXor.BackColor = Color.FromArgb(62, 62, 66);
        this.txtXor.ForeColor = Color.White;
        this.txtXor.BorderStyle = BorderStyle.FixedSingle;

        // btnStartStop
        this.btnStartStop.Location = new Point(590, 9);
        this.btnStartStop.Size = new Size(100, 32);
        this.btnStartStop.Text = "Start";
        this.btnStartStop.FlatStyle = FlatStyle.Flat;
        this.btnStartStop.BackColor = Color.FromArgb(0, 122, 204);
        this.btnStartStop.ForeColor = Color.White;
        this.btnStartStop.FlatAppearance.BorderSize = 0;
        this.btnStartStop.Cursor = Cursors.Hand;
        this.btnStartStop.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        this.btnStartStop.Click += new EventHandler(this.BtnStartStop_Click);

        // ============================================================
        // panelLeft
        // ============================================================
        this.panelLeft.BackColor = Color.FromArgb(37, 37, 38);
        this.panelLeft.Dock = DockStyle.Left;
        this.panelLeft.Width = 200;
        this.panelLeft.Padding = new Padding(5, 5, 5, 5);
        this.panelLeft.Controls.Add(this.lvFilter);
        this.panelLeft.Controls.Add(this.lblFilter);

        // lblFilter
        this.lblFilter.Dock = DockStyle.Top;
        this.lblFilter.ForeColor = Color.White;
        this.lblFilter.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        this.lblFilter.Height = 30;
        this.lblFilter.Text = "  Process";
        this.lblFilter.TextAlign = ContentAlignment.MiddleLeft;

        // lvFilter
        this.lvFilter.Dock = DockStyle.Fill;
        this.lvFilter.View = View.Details;
        this.lvFilter.FullRowSelect = true;
        this.lvFilter.HeaderStyle = ColumnHeaderStyle.None;
        this.lvFilter.MultiSelect = false;
        this.lvFilter.BackColor = Color.FromArgb(37, 37, 38);
        this.lvFilter.ForeColor = Color.White;
        this.lvFilter.Font = new Font("Segoe UI", 9F);
        this.lvFilter.BorderStyle = BorderStyle.None;
        this.lvFilter.Columns.Add("Process", 180);
        this.lvFilter.HideSelection = false;
        this.lvFilter.SelectedIndexChanged += new EventHandler(this.LvFilter_SelectedIndexChanged);

        // ============================================================
        // splitterLeft
        // ============================================================
        this.splitterLeft.BackColor = Color.FromArgb(63, 63, 70);
        this.splitterLeft.Dock = DockStyle.Left;
        this.splitterLeft.MinExtra = 300;
        this.splitterLeft.MinSize = 150;
        this.splitterLeft.Name = "splitterLeft";
        this.splitterLeft.Size = new Size(5, 600);
        this.splitterLeft.TabIndex = 3;
        this.splitterLeft.TabStop = false;

        // ============================================================
        // panelMain
        // ============================================================
        this.panelMain.BackColor = Color.FromArgb(30, 30, 30);
        this.panelMain.Dock = DockStyle.Fill;
        this.panelMain.Padding = new Padding(5, 5, 5, 5);

        // rtbLog (added first = lowest z-order = Dock.Fill processed last)
        this.rtbLog.Dock = DockStyle.Fill;
        this.rtbLog.BackColor = Color.Black;
        this.rtbLog.ForeColor = Color.White;
        this.rtbLog.ReadOnly = true;
        this.rtbLog.Font = new Font("Consolas", 9.5F);
        this.rtbLog.BorderStyle = BorderStyle.None;
        this.rtbLog.ScrollBars = RichTextBoxScrollBars.Vertical;
        this.rtbLog.WordWrap = true;

        // btnClear (Dock.Bottom)
        this.btnClear.Dock = DockStyle.Bottom;
        this.btnClear.Height = 35;
        this.btnClear.Text = "Clear";
        this.btnClear.FlatStyle = FlatStyle.Flat;
        this.btnClear.BackColor = Color.FromArgb(63, 63, 70);
        this.btnClear.ForeColor = Color.White;
        this.btnClear.FlatAppearance.BorderSize = 0;
        this.btnClear.Cursor = Cursors.Hand;
        this.btnClear.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        this.btnClear.Click += new EventHandler(this.BtnClear_Click);

        // btnCapture (Dock.Top, added last = highest z-order = Dock.Top processed first)
        this.btnCapture.Dock = DockStyle.Top;
        this.btnCapture.Height = 35;
        this.btnCapture.Text = "Capture";
        this.btnCapture.FlatStyle = FlatStyle.Flat;
        this.btnCapture.BackColor = Color.FromArgb(0, 122, 204);
        this.btnCapture.ForeColor = Color.White;
        this.btnCapture.FlatAppearance.BorderSize = 0;
        this.btnCapture.Cursor = Cursors.Hand;
        this.btnCapture.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        this.btnCapture.Enabled = false;
        this.btnCapture.Click += new EventHandler(this.BtnCapture_Click);

        // Add to panelMain: Fill first, then edge-docked controls
        this.panelMain.Controls.Add(this.rtbLog);
        this.panelMain.Controls.Add(this.btnClear);
        this.panelMain.Controls.Add(this.btnCapture);

        // ============================================================
        // MainForm
        // ============================================================
        // Add panels: Fill first, then edge panels (docking processed from highest z-order)
        this.Controls.Add(this.panelMain);
        this.Controls.Add(this.splitterLeft);
        this.Controls.Add(this.panelLeft);
        this.Controls.Add(this.panelTop);

        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.BackColor = Color.FromArgb(30, 30, 30);
        this.ClientSize = new Size(1050, 650);
        this.Font = new Font("Segoe UI", 9F);
        this.MinimumSize = new Size(850, 500);
        this.Text = "TS Proxy Capture";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Load += new EventHandler(this.MainForm_Load);

        this.panelTop.ResumeLayout(false);
        this.panelTop.PerformLayout();
        this.panelLeft.ResumeLayout(false);
        this.panelMain.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}
