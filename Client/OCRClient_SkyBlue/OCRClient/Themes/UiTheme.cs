using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;

namespace OCRClient.Themes
{
    /// <summary>Kiểu nút: quyết định bảng màu của nút ở mọi trạng thái.</summary>
    public enum ButtonKind
    {
        Primary,    // Nút chính (OCR, Chụp)  - xanh đậm, chữ trắng
        Secondary,  // Nút phụ (Mở file...)  - trắng, chữ xanh
        Danger,     // Nút xóa               - trắng, chữ đỏ
        Tool        // Nút icon ở thanh công cụ bên trái
    }

    /// <summary>Mã ký tự icon của font "Segoe MDL2 Assets" (Windows 10/11).</summary>
    public static class Icons
    {
        public const string Open = "\uE838";
        public const string Camera = "\uE722";
        public const string Save = "\uE74E";
        public const string Play = "\uE768";
        public const string Delete = "\uE74D";
        public const string Previous = "\uE76B";
        public const string Next = "\uE76C";
        public const string Crop = "\uE7A8";
        public const string Check = "\uE73E";
        public const string ZoomIn = "\uE8A3";
        public const string ZoomOut = "\uE71F";
        public const string RotateLeft = "\uE7A7";
        public const string RotateRight = "\uE7A6";
        public const string Settings = "\uE713";
        public const string Sync = "\uE895";
    }

    /// <summary>
    /// Giao diện dùng chung: tông xanh da trời + trắng.
    /// Chỉ chứa phần hiển thị, không chứa logic nghiệp vụ.
    /// </summary>
    public static class UiTheme
    {
        // ===== BẢNG MÀU =====
        public static readonly Color Sky50 = Color.FromArgb(240, 249, 255);
        public static readonly Color Sky100 = Color.FromArgb(224, 242, 254);
        public static readonly Color Sky200 = Color.FromArgb(186, 230, 253);
        public static readonly Color Sky400 = Color.FromArgb(56, 189, 248);
        public static readonly Color Sky500 = Color.FromArgb(14, 165, 233);
        public static readonly Color Sky600 = Color.FromArgb(2, 132, 199);
        public static readonly Color Sky700 = Color.FromArgb(3, 105, 161);
        public static readonly Color Sky800 = Color.FromArgb(7, 89, 133);
        public static readonly Color Sky900 = Color.FromArgb(12, 74, 110);
        public static readonly Color Sky950 = Color.FromArgb(8, 47, 73);

        // ===== Màu pastel dùng cho nút chính / nền gradient =====
        public static readonly Color Primary = Color.FromArgb(62, 146, 212);
        public static readonly Color PrimaryHover = Color.FromArgb(46, 128, 194);
        public static readonly Color PrimaryPressed = Color.FromArgb(34, 110, 170);
        public static readonly Color GradientStart = Color.FromArgb(120, 190, 240);
        public static readonly Color GradientEnd = Color.FromArgb(178, 223, 250);
        public static readonly Color ToolIcon = Color.FromArgb(58, 134, 196);
        public static readonly Color Canvas = Color.FromArgb(234, 245, 253);
        public static readonly Color BoxBack = Color.FromArgb(247, 251, 255);

        public static readonly Color TextBody = Color.FromArgb(30, 41, 59);
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);

        public static readonly Color Danger = Color.FromArgb(220, 38, 38);
        public static readonly Color DangerBorder = Color.FromArgb(254, 202, 202);
        public static readonly Color DangerSoft = Color.FromArgb(254, 242, 242);
        public static readonly Color DangerPressed = Color.FromArgb(254, 226, 226);

        public const string IconFontName = "Segoe MDL2 Assets";

        // ===== THÔNG TIN NÚT =====
        private sealed class ButtonInfo
        {
            public ButtonKind Kind;
            public string Glyph;
            public int IconSize;
            public bool Active;
        }

        private static readonly Dictionary<string, Bitmap> IconCache =
            new Dictionary<string, Bitmap>();

        private static float screenDpi;

        private static float ScreenDpi
        {
            get
            {
                if (screenDpi <= 0)
                {
                    using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                    {
                        screenDpi = g.DpiX;
                    }
                }

                return screenDpi;
            }
        }

        // ============================================================
        // ICON
        // ============================================================

        /// <summary>Vẽ icon từ font Segoe MDL2 Assets thành Bitmap (có cache).</summary>
        public static Bitmap GetIcon(string glyph, int size, Color color)
        {
            int px = (int)Math.Round(size * ScreenDpi / 96f);

            if (px < 8)
                px = 8;

            string key = glyph + "|" + px + "|" + color.ToArgb();

            Bitmap cached;

            if (IconCache.TryGetValue(key, out cached))
                return cached;

            Bitmap bitmap = new Bitmap(px, px);

            using (Graphics g = Graphics.FromImage(bitmap))
            using (Font font = new Font(
                IconFontName,
                px * 0.9f,
                FontStyle.Regular,
                GraphicsUnit.Pixel))
            using (SolidBrush brush = new SolidBrush(color))
            using (StringFormat format = new StringFormat())
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                g.DrawString(
                    glyph,
                    font,
                    brush,
                    new RectangleF(0, 0, px, px),
                    format);
            }

            IconCache[key] = bitmap;

            return bitmap;
        }

        // ============================================================
        // NÚT
        // ============================================================

        /// <summary>Áp dụng kiểu + icon cho nút. Màu sẽ tự đổi khi Enabled thay đổi.</summary>
        public static void StyleButton(
            Button button,
            ButtonKind kind,
            string glyph,
            int iconSize = 18)
        {
            button.Tag = new ButtonInfo
            {
                Kind = kind,
                Glyph = glyph,
                IconSize = iconSize
            };

            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;

            button.TextImageRelation = TextImageRelation.ImageBeforeText;

            button.EnabledChanged -= OnButtonEnabledChanged;
            button.EnabledChanged += OnButtonEnabledChanged;

            button.SizeChanged -= OnButtonSizeChanged;
            button.SizeChanged += OnButtonSizeChanged;

            Refresh(button);
            CenterContent(button);
        }

        private static void OnButtonSizeChanged(object sender, EventArgs e)
        {
            CenterContent((ButtonBase)sender);
        }

        /// <summary>Căn giữa cả nhóm "icon + chữ" trong nút.</summary>
        private static void CenterContent(ButtonBase button)
        {
            if (button.Image == null || string.IsNullOrEmpty(button.Text))
            {
                button.Padding = Padding.Empty;
                button.ImageAlign = ContentAlignment.MiddleCenter;
                button.TextAlign = ContentAlignment.MiddleCenter;
                return;
            }

            Size textSize = TextRenderer.MeasureText(
                button.Text,
                button.Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding);

            int contentWidth = button.Image.Width + 6 + textSize.Width;

            int left = Math.Max(
                0,
                (button.ClientSize.Width - contentWidth) / 2);

            button.ImageAlign = ContentAlignment.MiddleLeft;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(left, 0, 0, 0);
        }

        private static void OnButtonEnabledChanged(object sender, EventArgs e)
        {
            Refresh((Button)sender);
        }

        /// <summary>Đổi icon của nút (ví dụ nút Crop chuyển sang dấu ✓).</summary>
        public static void SetIcon(Button button, string glyph)
        {
            ButtonInfo info = button.Tag as ButtonInfo;

            if (info == null || info.Glyph == glyph)
                return;

            info.Glyph = glyph;

            Refresh(button);
        }

        /// <summary>Đánh dấu nút đang ở trạng thái "bật" (ví dụ đang ở chế độ Crop).</summary>
        public static void SetActive(Button button, bool active)
        {
            ButtonInfo info = button.Tag as ButtonInfo;

            if (info == null)
                return;

            info.Active = active;
        }

        /// <summary>Tính lại màu / icon của nút theo Kind, Enabled, Active.</summary>
        public static void Refresh(Button button)
        {
            ButtonInfo info = button.Tag as ButtonInfo;

            if (info == null)
                return;

            Color back;
            Color fore;
            Color border;
            Color hover;
            Color pressed;
            int borderSize = 0;

            if (!button.Enabled)
            {
                if (info.Kind == ButtonKind.Tool)
                {
                    back = Color.White;
                    fore = Color.FromArgb(203, 213, 225);
                }
                else
                {
                    back = Color.FromArgb(226, 238, 248);
                    fore = Color.FromArgb(140, 165, 190);
                }

                border = back;
                hover = back;
                pressed = back;
            }
            else
            {
                switch (info.Kind)
                {
                    case ButtonKind.Primary:
                        back = Primary;
                        fore = Color.White;
                        border = Primary;
                        hover = PrimaryHover;
                        pressed = PrimaryPressed;
                        break;

                    case ButtonKind.Danger:
                        back = Color.White;
                        fore = Danger;
                        border = DangerBorder;
                        hover = DangerSoft;
                        pressed = DangerPressed;
                        borderSize = 1;
                        break;

                    case ButtonKind.Tool:
                        if (info.Active)
                        {
                            back = Primary;
                            fore = Color.White;
                            border = Primary;
                            hover = PrimaryHover;
                            pressed = PrimaryPressed;
                        }
                        else
                        {
                            back = Color.White;
                            fore = ToolIcon;
                            border = Color.White;
                            hover = Sky100;
                            pressed = Sky200;
                        }
                        break;

                    default: // Secondary
                        back = Color.White;
                        fore = Sky700;
                        border = Sky200;
                        hover = Sky100;
                        pressed = Sky200;
                        borderSize = 1;
                        break;
                }
            }

            button.BackColor = back;
            button.ForeColor = fore;

            button.FlatAppearance.BorderSize = borderSize;
            button.FlatAppearance.BorderColor = border;
            button.FlatAppearance.MouseOverBackColor = hover;
            button.FlatAppearance.MouseDownBackColor = pressed;

            if (!string.IsNullOrEmpty(info.Glyph))
                button.Image = GetIcon(info.Glyph, info.IconSize, fore);
        }

        // ============================================================
        // CHECKBOX DẠNG "CHIP"
        // ============================================================

        public static void StyleChip(CheckBox chip)
        {
            chip.Appearance = Appearance.Button;
            chip.FlatStyle = FlatStyle.Flat;
            chip.UseVisualStyleBackColor = false;
            chip.Cursor = Cursors.Hand;
            chip.TextAlign = ContentAlignment.MiddleCenter;
            chip.ImageAlign = ContentAlignment.MiddleCenter;
            chip.TextImageRelation = TextImageRelation.ImageBeforeText;
            chip.FlatAppearance.BorderSize = 1;
            chip.FlatAppearance.MouseDownBackColor = Sky200;

            chip.CheckedChanged += delegate { RefreshChip(chip); };
            chip.SizeChanged += OnButtonSizeChanged;

            RefreshChip(chip);
        }

        private static void RefreshChip(CheckBox chip)
        {
            if (chip.Checked)
            {
                chip.BackColor = Primary;
                chip.ForeColor = Color.White;
                chip.FlatAppearance.BorderColor = Primary;
                chip.FlatAppearance.CheckedBackColor = Primary;
                chip.FlatAppearance.MouseOverBackColor = PrimaryHover;
                chip.Image = GetIcon(Icons.Check, 14, Color.White);
            }
            else
            {
                chip.BackColor = Color.White;
                chip.ForeColor = TextBody;
                chip.FlatAppearance.BorderColor = Sky200;
                chip.FlatAppearance.CheckedBackColor = Primary;
                chip.FlatAppearance.MouseOverBackColor = Sky100;
                chip.Image = null;
            }

            CenterContent(chip);
        }

        // ============================================================
        // VẼ NỀN / VIỀN
        // ============================================================

        /// <summary>Vẽ nền gradient ngang cho một Panel.</summary>
        public static void PaintGradient(Control control, Color start, Color end)
        {
            SetDoubleBuffered(control);

            control.Paint += delegate (object sender, PaintEventArgs e)
            {
                Rectangle rect = control.ClientRectangle;

                if (rect.Width <= 0 || rect.Height <= 0)
                    return;

                rect.Inflate(1, 1);

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect,
                    start,
                    end,
                    LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            };

            control.Resize += delegate { control.Invalidate(); };
        }

        /// <summary>Vẽ viền 1px ở các cạnh được chọn (thay cho BorderStyle.FixedSingle màu đen).</summary>
        public static void PaintBorders(
            Control control,
            Color color,
            bool left,
            bool top,
            bool right,
            bool bottom)
        {
            control.Paint += delegate (object sender, PaintEventArgs e)
            {
                Rectangle rect = control.ClientRectangle;

                using (Pen pen = new Pen(color, 1))
                {
                    if (left)
                        e.Graphics.DrawLine(pen, 0, 0, 0, rect.Height);

                    if (top)
                        e.Graphics.DrawLine(pen, 0, 0, rect.Width, 0);

                    if (right)
                        e.Graphics.DrawLine(pen, rect.Width - 1, 0, rect.Width - 1, rect.Height);

                    if (bottom)
                        e.Graphics.DrawLine(pen, 0, rect.Height - 1, rect.Width, rect.Height - 1);
                }
            };

            control.Resize += delegate { control.Invalidate(); };
        }

        /// <summary>Bật DoubleBuffered để kéo / zoom ảnh không bị nhấp nháy.</summary>
        public static void SetDoubleBuffered(Control control)
        {
            PropertyInfo property = typeof(Control).GetProperty(
                "DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (property != null)
                property.SetValue(control, true, null);
        }
    }
}
