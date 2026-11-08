using OCRClient.Themes;

namespace OCRClient.Forms
{
    // Phần giao diện chạy lúc runtime. Không chứa logic nghiệp vụ.
    public partial class CameraForm
    {
        private void ApplyModernStyle()
        {
            UiTheme.PaintGradient(pnlHeader, UiTheme.GradientStart, UiTheme.GradientEnd);
            UiTheme.PaintBorders(pnlBottom, UiTheme.Sky200, false, true, false, false);

            UiTheme.StyleButton(btnCapture, ButtonKind.Primary, Icons.Camera, 20);

            UiTheme.SetDoubleBuffered(picCamera);
        }
    }
}
