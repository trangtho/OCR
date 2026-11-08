namespace OCRClient.Forms
{
    partial class OcrForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlFileToolbar = new System.Windows.Forms.Panel();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnOcr = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.pnlTextType = new System.Windows.Forms.Panel();
            this.chkHandwritten = new System.Windows.Forms.CheckBox();
            this.chkPrinted = new System.Windows.Forms.CheckBox();
            this.lblTextType = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlImage = new System.Windows.Forms.Panel();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.pnlResult = new System.Windows.Forms.Panel();
            this.pnlResultBox = new System.Windows.Forms.Panel();
            this.txtOcrResult = new System.Windows.Forms.RichTextBox();
            this.lblResultTitle = new System.Windows.Forms.Label();
            this.pnlLeftTools = new System.Windows.Forms.Panel();
            this.btnRotateRight = new System.Windows.Forms.Button();
            this.btnRotateLeft = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.btnZoomIn = new System.Windows.Forms.Button();
            this.btnCrop = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pnlFileToolbar.SuspendLayout();
            this.pnlTextType.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.pnlResult.SuspendLayout();
            this.pnlResultBox.SuspendLayout();
            this.pnlLeftTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFileToolbar
            // 
            this.pnlFileToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(190)))), ((int)(((byte)(240)))));
            this.pnlFileToolbar.Controls.Add(this.btnSettings);
            this.pnlFileToolbar.Controls.Add(this.btnClear);
            this.pnlFileToolbar.Controls.Add(this.btnOcr);
            this.pnlFileToolbar.Controls.Add(this.btnSave);
            this.pnlFileToolbar.Controls.Add(this.btnScan);
            this.pnlFileToolbar.Controls.Add(this.btnOpenFile);
            this.pnlFileToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFileToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlFileToolbar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlFileToolbar.Name = "pnlFileToolbar";
            this.pnlFileToolbar.Padding = new System.Windows.Forms.Padding(24, 15, 24, 15);
            this.pnlFileToolbar.Size = new System.Drawing.Size(1920, 96);
            this.pnlFileToolbar.TabIndex = 0;
            // 
            // btnSettings
            // 
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSettings.BackColor = System.Drawing.Color.White;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(230)))), ((int)(((byte)(253)))));
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(105)))), ((int)(((byte)(161)))));
            this.btnSettings.Location = new System.Drawing.Point(1830, 18);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(66, 60);
            this.btnSettings.TabIndex = 5;
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.White;
            this.btnClear.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClear.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(254)))), ((int)(((byte)(202)))), ((int)(((byte)(202)))));
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.btnClear.Location = new System.Drawing.Point(756, 18);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(150, 60);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Xóa";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnOcr
            // 
            this.btnOcr.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(146)))), ((int)(((byte)(212)))));
            this.btnOcr.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOcr.FlatAppearance.BorderSize = 0;
            this.btnOcr.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOcr.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOcr.ForeColor = System.Drawing.Color.White;
            this.btnOcr.Location = new System.Drawing.Point(573, 18);
            this.btnOcr.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOcr.Name = "btnOcr";
            this.btnOcr.Size = new System.Drawing.Size(168, 60);
            this.btnOcr.TabIndex = 3;
            this.btnOcr.Text = "OCR";
            this.btnOcr.UseVisualStyleBackColor = false;
            this.btnOcr.Click += new System.EventHandler(this.btnOcr_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.White;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(230)))), ((int)(((byte)(253)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(105)))), ((int)(((byte)(161)))));
            this.btnSave.Location = new System.Drawing.Point(408, 18);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 60);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Lưu";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnScan
            // 
            this.btnScan.BackColor = System.Drawing.Color.White;
            this.btnScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScan.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(230)))), ((int)(((byte)(253)))));
            this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScan.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnScan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(105)))), ((int)(((byte)(161)))));
            this.btnScan.Location = new System.Drawing.Point(231, 18);
            this.btnScan.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(162, 60);
            this.btnScan.TabIndex = 1;
            this.btnScan.Text = "Scan";
            this.btnScan.UseVisualStyleBackColor = false;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.BackColor = System.Drawing.Color.White;
            this.btnOpenFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpenFile.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(230)))), ((int)(((byte)(253)))));
            this.btnOpenFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFile.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(105)))), ((int)(((byte)(161)))));
            this.btnOpenFile.Location = new System.Drawing.Point(24, 18);
            this.btnOpenFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(192, 60);
            this.btnOpenFile.TabIndex = 0;
            this.btnOpenFile.Text = "Mở file";
            this.btnOpenFile.UseVisualStyleBackColor = false;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // pnlTextType
            // 
            this.pnlTextType.BackColor = System.Drawing.Color.White;
            this.pnlTextType.Controls.Add(this.chkHandwritten);
            this.pnlTextType.Controls.Add(this.chkPrinted);
            this.pnlTextType.Controls.Add(this.lblTextType);
            this.pnlTextType.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTextType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(74)))), ((int)(((byte)(110)))));
            this.pnlTextType.Location = new System.Drawing.Point(0, 96);
            this.pnlTextType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlTextType.Name = "pnlTextType";
            this.pnlTextType.Padding = new System.Windows.Forms.Padding(30, 9, 30, 9);
            this.pnlTextType.Size = new System.Drawing.Size(1920, 72);
            this.pnlTextType.TabIndex = 1;
            // 
            // chkHandwritten
            // 
            this.chkHandwritten.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkHandwritten.BackColor = System.Drawing.Color.White;
            this.chkHandwritten.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkHandwritten.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(230)))), ((int)(((byte)(253)))));
            this.chkHandwritten.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkHandwritten.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkHandwritten.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.chkHandwritten.Location = new System.Drawing.Point(324, 12);
            this.chkHandwritten.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkHandwritten.Name = "chkHandwritten";
            this.chkHandwritten.Size = new System.Drawing.Size(204, 48);
            this.chkHandwritten.TabIndex = 2;
            this.chkHandwritten.Text = "Chữ viết tay";
            this.chkHandwritten.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkHandwritten.UseVisualStyleBackColor = false;
            this.chkHandwritten.CheckedChanged += new System.EventHandler(this.chkHandwritten_CheckedChanged);
            // 
            // chkPrinted
            // 
            this.chkPrinted.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPrinted.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(146)))), ((int)(((byte)(212)))));
            this.chkPrinted.Checked = true;
            this.chkPrinted.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrinted.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkPrinted.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(146)))), ((int)(((byte)(212)))));
            this.chkPrinted.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkPrinted.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkPrinted.ForeColor = System.Drawing.Color.White;
            this.chkPrinted.Location = new System.Drawing.Point(156, 12);
            this.chkPrinted.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkPrinted.Name = "chkPrinted";
            this.chkPrinted.Size = new System.Drawing.Size(156, 48);
            this.chkPrinted.TabIndex = 1;
            this.chkPrinted.Text = "Chữ in";
            this.chkPrinted.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkPrinted.UseVisualStyleBackColor = false;
            this.chkPrinted.CheckedChanged += new System.EventHandler(this.chkPrinted_CheckedChanged);
            // 
            // lblTextType
            // 
            this.lblTextType.AutoSize = true;
            this.lblTextType.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTextType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(74)))), ((int)(((byte)(110)))));
            this.lblTextType.Location = new System.Drawing.Point(30, 21);
            this.lblTextType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTextType.Name = "lblTextType";
            this.lblTextType.Size = new System.Drawing.Size(93, 28);
            this.lblTextType.TabIndex = 0;
            this.lblTextType.Text = "Loại chữ:";
            // 
            // pnlMain
            // 
            this.pnlMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.pnlMain.Controls.Add(this.pnlImage);
            this.pnlMain.Controls.Add(this.pnlBottom);
            this.pnlMain.Controls.Add(this.pnlResult);
            this.pnlMain.Controls.Add(this.pnlLeftTools);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 168);
            this.pnlMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(1920, 1002);
            this.pnlMain.TabIndex = 2;
            // 
            // pnlImage
            // 
            this.pnlImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.pnlImage.Controls.Add(this.picPreview);
            this.pnlImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlImage.Location = new System.Drawing.Point(96, 0);
            this.pnlImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlImage.Name = "pnlImage";
            this.pnlImage.Size = new System.Drawing.Size(1044, 948);
            this.pnlImage.TabIndex = 3;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(1044, 948);
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            this.picPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.picPreview_Paint);
            this.picPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseDown);
            this.picPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseMove);
            this.picPreview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picPreview_MouseUp);
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(245)))), ((int)(((byte)(253)))));
            this.pnlBottom.Controls.Add(this.lblPageInfo);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(96, 948);
            this.pnlBottom.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1044, 54);
            this.pnlBottom.TabIndex = 2;
            // 
            // lblPageInfo
            // 
            this.lblPageInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblPageInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPageInfo.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPageInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(74)))), ((int)(((byte)(110)))));
            this.lblPageInfo.Location = new System.Drawing.Point(0, 0);
            this.lblPageInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(1044, 54);
            this.lblPageInfo.TabIndex = 0;
            this.lblPageInfo.Text = "Trang 1 / 1";
            this.lblPageInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlResult
            // 
            this.pnlResult.BackColor = System.Drawing.Color.White;
            this.pnlResult.Controls.Add(this.pnlResultBox);
            this.pnlResult.Controls.Add(this.lblResultTitle);
            this.pnlResult.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlResult.Location = new System.Drawing.Point(1140, 0);
            this.pnlResult.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlResult.Name = "pnlResult";
            this.pnlResult.Padding = new System.Windows.Forms.Padding(30, 24, 30, 24);
            this.pnlResult.Size = new System.Drawing.Size(780, 1002);
            this.pnlResult.TabIndex = 1;
            // 
            // pnlResultBox
            // 
            this.pnlResultBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(251)))), ((int)(((byte)(255)))));
            this.pnlResultBox.Controls.Add(this.txtOcrResult);
            this.pnlResultBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlResultBox.Location = new System.Drawing.Point(30, 71);
            this.pnlResultBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlResultBox.Name = "pnlResultBox";
            this.pnlResultBox.Padding = new System.Windows.Forms.Padding(24, 24, 24, 24);
            this.pnlResultBox.Size = new System.Drawing.Size(720, 907);
            this.pnlResultBox.TabIndex = 0;
            // 
            // txtOcrResult
            // 
            this.txtOcrResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(251)))), ((int)(((byte)(255)))));
            this.txtOcrResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtOcrResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOcrResult.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOcrResult.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtOcrResult.Location = new System.Drawing.Point(24, 24);
            this.txtOcrResult.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtOcrResult.Name = "txtOcrResult";
            this.txtOcrResult.Size = new System.Drawing.Size(672, 859);
            this.txtOcrResult.TabIndex = 0;
            this.txtOcrResult.Text = "";
            // 
            // lblResultTitle
            // 
            this.lblResultTitle.AutoSize = true;
            this.lblResultTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblResultTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblResultTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResultTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(74)))), ((int)(((byte)(110)))));
            this.lblResultTitle.Location = new System.Drawing.Point(30, 24);
            this.lblResultTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResultTitle.Name = "lblResultTitle";
            this.lblResultTitle.Padding = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.lblResultTitle.Size = new System.Drawing.Size(167, 47);
            this.lblResultTitle.TabIndex = 1;
            this.lblResultTitle.Text = "KẾT QUẢ OCR";
            // 
            // pnlLeftTools
            // 
            this.pnlLeftTools.BackColor = System.Drawing.Color.White;
            this.pnlLeftTools.Controls.Add(this.btnRotateRight);
            this.pnlLeftTools.Controls.Add(this.btnRotateLeft);
            this.pnlLeftTools.Controls.Add(this.btnZoomOut);
            this.pnlLeftTools.Controls.Add(this.btnZoomIn);
            this.pnlLeftTools.Controls.Add(this.btnCrop);
            this.pnlLeftTools.Controls.Add(this.btnNextPage);
            this.pnlLeftTools.Controls.Add(this.btnPrevPage);
            this.pnlLeftTools.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeftTools.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftTools.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlLeftTools.Name = "pnlLeftTools";
            this.pnlLeftTools.Padding = new System.Windows.Forms.Padding(10, 15, 10, 15);
            this.pnlLeftTools.Size = new System.Drawing.Size(96, 1002);
            this.pnlLeftTools.TabIndex = 0;
            // 
            // btnRotateRight
            // 
            this.btnRotateRight.BackColor = System.Drawing.Color.White;
            this.btnRotateRight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRotateRight.FlatAppearance.BorderSize = 0;
            this.btnRotateRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRotateRight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(134)))), ((int)(((byte)(196)))));
            this.btnRotateRight.Location = new System.Drawing.Point(10, 540);
            this.btnRotateRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRotateRight.Name = "btnRotateRight";
            this.btnRotateRight.Size = new System.Drawing.Size(75, 72);
            this.btnRotateRight.TabIndex = 6;
            this.btnRotateRight.UseVisualStyleBackColor = false;
            this.btnRotateRight.Click += new System.EventHandler(this.btnRotateRight_Click);
            // 
            // btnRotateLeft
            // 
            this.btnRotateLeft.BackColor = System.Drawing.Color.White;
            this.btnRotateLeft.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRotateLeft.FlatAppearance.BorderSize = 0;
            this.btnRotateLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRotateLeft.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(134)))), ((int)(((byte)(196)))));
            this.btnRotateLeft.Location = new System.Drawing.Point(10, 459);
            this.btnRotateLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRotateLeft.Name = "btnRotateLeft";
            this.btnRotateLeft.Size = new System.Drawing.Size(75, 72);
            this.btnRotateLeft.TabIndex = 5;
            this.btnRotateLeft.UseVisualStyleBackColor = false;
            this.btnRotateLeft.Click += new System.EventHandler(this.btnRotateLeft_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.BackColor = System.Drawing.Color.White;
            this.btnZoomOut.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnZoomOut.FlatAppearance.BorderSize = 0;
            this.btnZoomOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomOut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(134)))), ((int)(((byte)(196)))));
            this.btnZoomOut.Location = new System.Drawing.Point(10, 360);
            this.btnZoomOut.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(75, 72);
            this.btnZoomOut.TabIndex = 4;
            this.btnZoomOut.UseVisualStyleBackColor = false;
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.BackColor = System.Drawing.Color.White;
            this.btnZoomIn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnZoomIn.FlatAppearance.BorderSize = 0;
            this.btnZoomIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnZoomIn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(134)))), ((int)(((byte)(196)))));
            this.btnZoomIn.Location = new System.Drawing.Point(10, 279);
            this.btnZoomIn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(75, 72);
            this.btnZoomIn.TabIndex = 3;
            this.btnZoomIn.UseVisualStyleBackColor = false;
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnCrop
            // 
            this.btnCrop.BackColor = System.Drawing.Color.White;
            this.btnCrop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCrop.FlatAppearance.BorderSize = 0;
            this.btnCrop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCrop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(134)))), ((int)(((byte)(196)))));
            this.btnCrop.Location = new System.Drawing.Point(10, 198);
            this.btnCrop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCrop.Name = "btnCrop";
            this.btnCrop.Size = new System.Drawing.Size(75, 72);
            this.btnCrop.TabIndex = 2;
            this.btnCrop.UseVisualStyleBackColor = false;
            this.btnCrop.Click += new System.EventHandler(this.btnCrop_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.BackColor = System.Drawing.Color.White;
            this.btnNextPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextPage.Enabled = false;
            this.btnNextPage.FlatAppearance.BorderSize = 0;
            this.btnNextPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(134)))), ((int)(((byte)(196)))));
            this.btnNextPage.Location = new System.Drawing.Point(10, 99);
            this.btnNextPage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 72);
            this.btnNextPage.TabIndex = 1;
            this.btnNextPage.UseVisualStyleBackColor = false;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.BackColor = System.Drawing.Color.White;
            this.btnPrevPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrevPage.Enabled = false;
            this.btnPrevPage.FlatAppearance.BorderSize = 0;
            this.btnPrevPage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevPage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(134)))), ((int)(((byte)(196)))));
            this.btnPrevPage.Location = new System.Drawing.Point(10, 18);
            this.btnPrevPage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(75, 72);
            this.btnPrevPage.TabIndex = 0;
            this.btnPrevPage.UseVisualStyleBackColor = false;
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            // 
            // OcrForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1920, 1170);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlTextType);
            this.Controls.Add(this.pnlFileToolbar);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1489, 932);
            this.Name = "OcrForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ỨNG DỤNG NHẬN DẠNG OCR";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.pnlFileToolbar.ResumeLayout(false);
            this.pnlTextType.ResumeLayout(false);
            this.pnlTextType.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.pnlImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlResult.ResumeLayout(false);
            this.pnlResult.PerformLayout();
            this.pnlResultBox.ResumeLayout(false);
            this.pnlLeftTools.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlFileToolbar;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnOcr;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.Panel pnlTextType;
        private System.Windows.Forms.CheckBox chkHandwritten;
        private System.Windows.Forms.CheckBox chkPrinted;
        private System.Windows.Forms.Label lblTextType;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Panel pnlLeftTools;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnCrop;
        private System.Windows.Forms.Button btnRotateRight;
        private System.Windows.Forms.Button btnRotateLeft;
        private System.Windows.Forms.Panel pnlResult;
        private System.Windows.Forms.Panel pnlResultBox;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.Panel pnlImage;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.RichTextBox txtOcrResult;
        private System.Windows.Forms.Label lblResultTitle;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
