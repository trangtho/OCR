using OCRClient.Themes;
using System.Drawing;
using System.Windows.Forms;

namespace OCRClient.Forms
{
    // Phần giao diện chạy lúc runtime (icon, hover, gradient, viền).
    // Không chứa logic nghiệp vụ.
    public partial class OcrForm
    {
        private void ApplyModernStyle()
        {
            // ===== Thanh công cụ trên cùng: gradient xanh trời =====
            UiTheme.PaintGradient(pnlFileToolbar, UiTheme.GradientStart, UiTheme.GradientEnd);

            UiTheme.StyleButton(btnOpenFile, ButtonKind.Secondary, Icons.Open);
            UiTheme.StyleButton(btnScan, ButtonKind.Secondary, Icons.Camera);
            UiTheme.StyleButton(btnSave, ButtonKind.Secondary, Icons.Save);
            UiTheme.StyleButton(btnOcr, ButtonKind.Primary, Icons.Play);
            UiTheme.StyleButton(btnClear, ButtonKind.Danger, Icons.Delete);
            UiTheme.StyleButton(btnSettings, ButtonKind.Secondary, Icons.Settings);
            toolTip1.SetToolTip(btnSettings, "Cài đặt server OCR");

            // ===== Thanh chọn loại chữ =====
            UiTheme.PaintBorders(pnlTextType, UiTheme.Sky200, false, false, false, true);
            UiTheme.StyleChip(chkPrinted);
            UiTheme.StyleChip(chkHandwritten);

            // ===== Thanh công cụ bên trái =====
            UiTheme.PaintBorders(pnlLeftTools, UiTheme.Sky200, false, false, true, false);

            UiTheme.StyleButton(btnPrevPage, ButtonKind.Tool, Icons.Previous, 20);
            UiTheme.StyleButton(btnNextPage, ButtonKind.Tool, Icons.Next, 20);
            UiTheme.StyleButton(btnCrop, ButtonKind.Tool, Icons.Crop, 20);
            UiTheme.StyleButton(btnZoomIn, ButtonKind.Tool, Icons.ZoomIn, 20);
            UiTheme.StyleButton(btnZoomOut, ButtonKind.Tool, Icons.ZoomOut, 20);
            UiTheme.StyleButton(btnRotateLeft, ButtonKind.Tool, Icons.RotateLeft, 20);
            UiTheme.StyleButton(btnRotateRight, ButtonKind.Tool, Icons.RotateRight, 20);

            // ===== Khu vực xem ảnh / kết quả =====
            UiTheme.SetDoubleBuffered(picPreview);

            UiTheme.PaintBorders(pnlResult, UiTheme.Sky200, true, false, false, false);
            // Khung bao quanh phần kết quả OCR
            UiTheme.PaintBorders(pnlResultBox, UiTheme.Sky200, true, true, true, true);

            // Chia đôi ngang: vùng ảnh = vùng kết quả
            pnlMain.Resize += delegate { UpdateSplitLayout(); };
            UpdateSplitLayout();
        }

        private void UpdateSplitLayout()
        {
            int available =
                pnlMain.ClientSize.Width - pnlLeftTools.Width;

            if (available <= 0)
                return;

            pnlResult.Width = available / 2;
        }

        private void DrawEmptyState(Graphics g)
        {
            using (Font font = new Font("Segoe UI", 11F))
            using (SolidBrush brush = new SolidBrush(UiTheme.TextMuted))
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                g.DrawString(
                    "Mở file hoặc Scan để bắt đầu",
                    font,
                    brush,
                    picPreview.ClientRectangle,
                    format);
            }
        }
    }
}
