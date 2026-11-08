using OCRClient.Services;
using OCRClient.Themes;
using System;
using System.Drawing;
using System.Net.Http;
using System.Windows.Forms;

namespace OCRClient.Forms
{
    /// <summary>Hộp thoại nhập địa chỉ IP / cổng của máy OCR server.</summary>
    public class SettingsForm : Form
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblHost;
        private TextBox txtHost;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblTimeout;
        private TextBox txtTimeout;
        private Label lblStatus;
        private Button btnTest;
        private Button btnSave;
        private Button btnCancel;

        public SettingsForm()
        {
            BuildUi();
            LoadValues();
        }

        private void BuildUi()
        {
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(460, 316);

            FormBorderStyle = FormBorderStyle.FixedDialog;

            MaximizeBox = false;

            MinimizeBox = false;

            ShowInTaskbar = false;

            StartPosition = FormStartPosition.CenterParent;

            Text = "CÀI ĐẶT KẾT NỐI";

            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 56;
            pnlHeader.BackColor = Color.FromArgb(120, 190, 240);

            /*lblTitle = new Label();
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI Semibold", 13F);
            lblTitle.ForeColor = UiTheme.Sky900;
            lblTitle.Location = new Point(20, 14);
            lblTitle.Text = "CÀI ĐẶT KẾT NỐI";
            pnlHeader.Controls.Add(lblTitle);*/

            lblHost = new Label();
            lblHost.AutoSize = true;
            lblHost.Font = new Font("Segoe UI Semibold", 10F);
            lblHost.ForeColor = UiTheme.Sky900;
            lblHost.Text = "Địa chỉ IP / tên máy OCR server";

            txtHost = new TextBox();
            txtHost.Size = new Size(412, 27);

            lblPort = new Label();
            lblPort.AutoSize = true;
            lblPort.Font = new Font("Segoe UI Semibold", 10F);
            lblPort.ForeColor = UiTheme.Sky900;
            lblPort.Text = "Cổng (port)";

            txtPort = new TextBox();
            txtPort.Size = new Size(120, 27);

            lblTimeout = new Label();
            lblTimeout.AutoSize = true;
            lblTimeout.Font = new Font("Segoe UI Semibold", 10F);
            lblTimeout.ForeColor = UiTheme.Sky900;
            lblTimeout.Location = new Point(176, 142);
            lblTimeout.Text = "Thời gian chờ tối đa (giây)";

            txtTimeout = new TextBox();
            txtTimeout.Location = new Point(176, 166);
            txtTimeout.Size = new Size(120, 27);

            lblStatus = new Label();
            lblStatus.Size = new Size(412, 48);
            lblStatus.ForeColor = UiTheme.TextMuted;
            lblStatus.Text = "Ví dụ: 192.168.1.50 và cổng 8000";

            btnTest = new Button();
            btnTest.Font = new Font("Segoe UI Semibold", 10F);
            btnTest.Size = new Size(160, 38);
            btnTest.Text = "Kiểm tra";
            btnTest.Click += btnTest_Click;

            btnSave = new Button();
            btnSave.Font = new Font("Segoe UI Semibold", 10F);
            btnSave.Size = new Size(80, 38);
            btnSave.Text = "Lưu";
            btnSave.Click += btnSave_Click;

            btnCancel = new Button();
            btnCancel.Font = new Font("Segoe UI Semibold", 10F);
            btnCancel.Size = new Size(80, 38);
            btnCancel.Text = "Hủy";
            btnCancel.DialogResult = DialogResult.Cancel;

            // --- TĂNG KHOẢNG CÁCH THOÁNG HƠN ĐỂ KHÔNG BỊ ĐÈ ---
            int currentY = 72; // Đẩy xuống một chút so với header

            lblHost.Location = new Point(24, currentY);
            currentY = lblHost.Bottom + 6; // Tăng khoảng hở dưới label Host

            txtHost.Location = new Point(24, currentY);
            currentY = txtHost.Bottom + 16; // Tăng khoảng hở trước khi sang cụm Port

            lblPort.Location = new Point(24, currentY);
            currentY = lblPort.Bottom + 6; // Tăng khoảng hở dưới label Port

            txtPort.Location = new Point(24, currentY);
            currentY = txtPort.Bottom + 16; // Tăng khoảng hở trước status

            lblTimeout.Location = new Point(24, currentY);
            currentY = lblTimeout.Bottom + 6; // Tăng khoảng hở dưới label Timeout

            txtTimeout.Location = new Point(24, currentY);
            
            currentY = txtTimeout.Bottom + 16; // Tăng khoảng hở trước status

            lblStatus.Location = new Point(24, currentY);
            currentY = lblStatus.Bottom + 16; // Tăng khoảng hở trước hàng nút bấm

            // Hàng nút bấm phía dưới
            btnTest.Location = new Point(24, currentY);
            btnSave.Location = new Point(270, currentY);
            btnCancel.Location = new Point(356, currentY);

            // Tự động co giãn form theo vị trí nút bấm thấp nhất + lề dưới
            ClientSize = new Size(460, btnCancel.Bottom + 24);

            // Add controls vào form
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(btnTest);
            Controls.Add(lblStatus);
            Controls.Add(txtTimeout);
            Controls.Add(lblTimeout);
            Controls.Add(txtPort);
            Controls.Add(lblPort);
            Controls.Add(txtHost);
            Controls.Add(lblHost);
            Controls.Add(pnlHeader);

            AcceptButton = btnSave;
            CancelButton = btnCancel;

            UiTheme.PaintGradient(pnlHeader, UiTheme.GradientStart, UiTheme.GradientEnd);
            UiTheme.StyleButton(btnTest, ButtonKind.Secondary, Icons.Sync);
            UiTheme.StyleButton(btnSave, ButtonKind.Primary, null);
            UiTheme.StyleButton(btnCancel, ButtonKind.Secondary, null);
        }

        private void LoadValues()
        {
            Uri uri;

            if (Uri.TryCreate(AppSettings.Current.ServerUrl, UriKind.Absolute, out uri))
            {
                txtHost.Text = uri.Host;
                txtPort.Text = uri.Port.ToString();
            }
            else
            {
                txtHost.Text = "127.0.0.1";
                txtPort.Text = "8000";
            }
            txtTimeout.Text = AppSettings.Current.TimeoutSeconds.ToString();
        }

        private bool TryBuildUrl(out string url)
        {
            url = null;

            string host = txtHost.Text.Trim();

            host = host.Replace("http://", "").Replace("https://", "").Trim('/');

            int colon = host.LastIndexOf(':');
            string portText = txtPort.Text.Trim();

            if (colon > 0)
            {
                portText = host.Substring(colon + 1);
                host = host.Substring(0, colon);
            }

            int port;

            if (host.Length == 0)
            {
                ShowError("Vui lòng nhập địa chỉ IP hoặc tên máy.");
                txtHost.Focus();
                return false;
            }

            if (!int.TryParse(portText, out port) || port < 1 || port > 65535)
            {
                ShowError("Cổng phải là số từ 1 đến 65535.");
                txtPort.Focus();
                return false;
            }

            url = "http://" + host + ":" + port;

            return true;
        }

        private bool TryReadTimeout(out int seconds)
        {
            if (!int.TryParse(txtTimeout.Text.Trim(), out seconds) ||
                seconds < 10 || seconds > 3600)
            {
                ShowError("Thời gian chờ phải từ 10 đến 3600 giây.");
                txtTimeout.Focus();
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            lblStatus.ForeColor = UiTheme.Danger;
            lblStatus.Text = message;
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            string url;

            if (!TryBuildUrl(out url))
                return;

            btnTest.Enabled = false;

            lblStatus.ForeColor = UiTheme.TextMuted;
            lblStatus.Text = "Đang kiểm tra " + url + " ...";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    HttpResponseMessage response = await client.GetAsync(url);

                    lblStatus.ForeColor = Color.FromArgb(22, 130, 70);
                    lblStatus.Text =
                        "Kết nối được tới server (HTTP " +
                        (int)response.StatusCode + ").";
                }
            }
            catch (Exception ex)
            {
                ShowError("Không kết nối được: " + ex.Message);
            }
            finally
            {
                btnTest.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string url;

            if (!TryBuildUrl(out url))
                return;

            int timeoutSeconds;

            if (!TryReadTimeout(out timeoutSeconds))
                return;

            try
            {
                AppSettings.Current.ServerUrl = url;
                AppSettings.Current.TimeoutSeconds = timeoutSeconds;
                AppSettings.Current.Save();
            }
            catch (Exception ex)
            {
                ShowError("Không lưu được cài đặt: " + ex.Message);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /*private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }*/
    }
}