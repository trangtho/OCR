using OCRClient.Models;
using OCRClient.Services;
using OCRClient.Themes;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OCRClient.Forms
{
    public partial class OcrForm : Form
    {
        public OcrForm()
        {
            InitializeComponent();
            ApplyModernStyle();
            toolTip1.SetToolTip(btnOpenFile, "Mở file ảnh hoặc PDF");
            toolTip1.SetToolTip(btnScan, "Quét tài liệu từ máy scan");
            toolTip1.SetToolTip(btnSave, "Lưu kết quả OCR");
            toolTip1.SetToolTip(btnOcr, "Thực hiện OCR");
            toolTip1.SetToolTip(btnClear, "Xóa toàn bộ tài liệu và kết quả");
            toolTip1.SetToolTip(btnPrevPage, "Trang trước");
            toolTip1.SetToolTip(btnNextPage, "Trang sau");
            toolTip1.SetToolTip(btnCrop, "Chọn vùng cần OCR");
            toolTip1.SetToolTip(btnZoomIn, "Phóng to");
            toolTip1.SetToolTip(btnZoomOut, "Thu nhỏ");
            toolTip1.SetToolTip(btnRotateLeft, "Xoay trái");
            toolTip1.SetToolTip(btnRotateRight, "Xoay phải");
            toolTip1.SetToolTip(chkPrinted,"Sử dụng OCR cho chữ in");
            toolTip1.SetToolTip(chkHandwritten,"Sử dụng OCR cho chữ viết tay");
            // Khởi tạo service OCR
            ocrService = new OcrService();
            // Thiết lập trạng thái ban đầu
            ResetDocumentView();
        }

        private readonly OcrService ocrService;

        private string currentFilePath;
        private Bitmap currentImage;
        private OcrResponse currentOcrResult;

        private PdfDocument currentPdf;
        private bool isPdf = false;

        // Đang OCR -> khóa giao diện, chặn bấm lại
        private bool isBusy = false;

        private int currentPage = 0;
        private int totalPages = 0;

        private OcrPdfResponse currentOcrPdfResult;
        private bool isPdfOcrCompleted = false;

        // Crop
        private bool isCropMode = false;
        private bool isSelecting = false;

        private Point selectionStart;
        private Point selectionEnd;

        private Rectangle selectedImageRect = Rectangle.Empty;

        // Zoom
        private float zoom = 1.0f;

        private const float ZoomStep = 0.25f;
        private const float MinZoom = 0.25f;
        private const float MaxZoom = 5.0f;

        private Point imageOffset = Point.Empty;

        // Pan
        private bool isPanning = false;
        private Point panStart;
        private Point panStartOffset;

        private void ResetDocumentView()
        {
            currentFilePath = null;

            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
            }

            if (currentPdf != null)
            {
                currentPdf.Dispose();
                currentPdf = null;
            }

            isPdf = false;

            currentPage = 0;
            totalPages = 0;

            // ==========================================
            // PREVIEW
            // ==========================================

            picPreview.Image = null;

            // ==========================================
            // KẾT QUẢ OCR
            // ==========================================

            txtOcrResult.Clear();

            // ==========================================
            // NÚT OCR
            // ==========================================

            //btnOcrFull.Enabled = false;

            // OCR trang chỉ dành cho PDF
            //btnOcrCurrentPage.Visible = false;
            btnOcr.Enabled = false;

            // ==========================================
            // ĐIỀU HƯỚNG TRANG
            // ==========================================

            //btnPrevPage.Visible = false;
            //btnNextPage.Visible = false;
            lblPageInfo.Visible = false;

            btnPrevPage.Enabled = false;
            btnNextPage.Enabled = false;

            currentOcrResult = null;
            
            currentOcrPdfResult = null;
            isPdfOcrCompleted = false;

            UpdateButtonStates();
        }

        private Bitmap RenderPdfPageBitmap(int pageIndex)
        {
            if (currentPdf == null)
                return null;

            if (pageIndex < 0 ||
                pageIndex >= currentPdf.PageCount)
                return null;

            using (Image renderedImage =
                currentPdf.Render(
                    pageIndex,
                    150,
                    150,
                    PdfRenderFlags.Annotations))
            {
                return new Bitmap(renderedImage);
            }
        }

        private void ShowPdfPage(int pageIndex)
        {
            if (currentPdf == null)
                return;

            if (pageIndex < 0 ||
                pageIndex >= currentPdf.PageCount)
                return;

            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
            }

            picPreview.Image = null;

            currentImage = RenderPdfPageBitmap(pageIndex);

            if (currentImage == null)
                return;

            picPreview.Image = currentImage;

            currentPage = pageIndex;

            // Reset Crop
            isCropMode = false;
            isSelecting = false;
            selectedImageRect = Rectangle.Empty;

            picPreview.Cursor = Cursors.Default;

            // FIT PDF PAGE
            zoom = 1.0f;
            imageOffset = Point.Empty;

            FitImageToView();

            lblPageInfo.Text =
                $"Trang {currentPage + 1} / {totalPages}";

            lblPageInfo.Visible = true;

            ShowCurrentPageOcrResult();

            UpdatePageButtons();
            UpdateButtonStates();

            picPreview.Invalidate();
        }

        private void ShowCurrentPageOcrResult()
        {
            // Chưa OCR PDF
            if (!isPdfOcrCompleted ||
                currentOcrPdfResult == null ||
                currentOcrPdfResult.pages == null)
            {
                txtOcrResult.Clear();
                return;
            }

            // currentPage bắt đầu từ 0
            // OcrPdfPage.page bắt đầu từ 1
            int pageNumber = currentPage + 1;

            OcrPdfPage pageResult =
                currentOcrPdfResult.pages
                    .Find(p => p.page == pageNumber);

            if (pageResult != null)
            {
                txtOcrResult.Text =
                    pageResult.text ?? "";
            }
            else
            {
                txtOcrResult.Clear();
            }
        }

        private void UpdatePageButtons()
        {
            bool canNavigate =
                isPdf &&
                currentPdf != null &&
                totalPages > 1;

            btnPrevPage.Enabled =
                canNavigate &&
                currentPage > 0;

            btnNextPage.Enabled =
                canNavigate &&
                currentPage < totalPages - 1;

            if (isPdf && currentPdf != null)
            {
                lblPageInfo.Text =
                    $"Trang {currentPage + 1} / {totalPages}";

                lblPageInfo.Visible = true;
            }
            else
            {
                lblPageInfo.Visible = false;
            }
        }

        private void LoadPdfPreview(string filePath)
        {
            try
            {
                currentPdf = PdfDocument.Load(filePath);

                isPdf = true;

                totalPages = currentPdf.PageCount;
                currentPage = 0;

                currentOcrPdfResult = null;
                isPdfOcrCompleted = false;

                ShowPdfPage(0);

                lblPageInfo.Visible = true;

                UpdatePageButtons();
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể mở PDF:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadImagePreview(string filePath)
        {
            try
            {
                using (Bitmap temp = new Bitmap(filePath))
                {
                    currentImage = new Bitmap(temp);
                }

                picPreview.Image = currentImage;

                // Reset trạng thái
                isPdf = false;

                selectedImageRect = Rectangle.Empty;
                isSelecting = false;
                isCropMode = false;

                picPreview.Cursor = Cursors.Default;

                imageOffset = Point.Empty;

                // TỰ ĐỘNG FIT ẢNH VÀO KHUNG
                FitImageToView();

                UpdatePageButtons();
                UpdateButtonStates();

                picPreview.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể mở ảnh:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter =
                    "Tài liệu|*.jpg;*.jpeg;*.png;*.bmp;*.pdf|" +
                    "Ảnh|*.jpg;*.jpeg;*.png;*.bmp|" +
                    "PDF|*.pdf";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                // Xóa tài liệu cũ
                ResetDocumentView();

                // Lưu đường dẫn mới
                currentFilePath = dialog.FileName;

                string extension =
                    Path.GetExtension(currentFilePath)
                        .ToLower();

                // ==========================================
                // PDF
                // ==========================================

                if (extension == ".pdf")
                {
                    isPdf = true;

                    LoadPdfPreview(currentFilePath);

                    // Có tài liệu → OCR được
                    btnOcr.Enabled = true;

                    // Có PDF → hiện OCR trang
                    //btnOcrCurrentPage.Visible = true;
                    //btnOcrCurrentPage.Enabled = true;
                }

                // ==========================================
                // ẢNH
                // ==========================================

                else
                {
                    isPdf = false;

                    LoadImagePreview(currentFilePath);

                    // Có ảnh → OCR toàn trang
                    btnOcr.Enabled = true;

                    // Ảnh không cần OCR trang PDF
                    //btnOcrCurrentPage.Visible = false;
                    //btnOcrCurrentPage.Enabled = false;

                    // Ảnh không có phân trang
                    //btnPrevPage.Visible = false;
                    //btnNextPage.Visible = false;
                    lblPageInfo.Visible = false;
                }
            }
        }

        private void btnPrevPage_Click(object sender, EventArgs e)
        {
            if (currentPdf == null)
                return;

            if (currentPage <= 0)
                return;

            ShowPdfPage(currentPage - 1);
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (currentPdf == null)
                return;

            if (currentPage >= totalPages - 1)
                return;

            ShowPdfPage(currentPage + 1);
        }

        private async void btnOcr_Click(object sender, EventArgs e)
        {
            // Đang OCR rồi thì bỏ qua (chống bấm 2 lần)
            if (isBusy)
                return;

            if (currentImage == null)
            {
                MessageBox.Show(
                    "Vui lòng mở ảnh hoặc PDF trước.",
                    "OCR",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                SetBusy(true);

                // ==================================
                // 1. CÓ VÙNG CROP
                // ==================================

                if (selectedImageRect != Rectangle.Empty)
                {
                    await OcrSelectedRegionAsync();

                    return;
                }


                // ==================================
                // 2. PDF - OCR TOÀN BỘ
                // ==================================

                if (isPdf)
                {
                    await OcrEntirePdfAsync();

                    return;
                }


                // ==================================
                // 3. ẢNH - OCR TOÀN BỘ
                // ==================================

                txtOcrResult.Text =
                    "Đang thực hiện OCR...";

                OcrResponse result =
                    await OcrBitmapBySelectedTypeAsync(currentImage);

                currentOcrResult = result;

                txtOcrResult.Text =
                    result?.layout_text ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi OCR:\n" + ex.Message,
                    "OCR",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);

                UpdateButtonStates();
            }
        }

        // Khóa / mở khóa toàn bộ giao diện trong lúc chờ server OCR
        private void SetBusy(bool busy)
        {
            isBusy = busy;

            pnlFileToolbar.Enabled = !busy;
            pnlTextType.Enabled = !busy;
            pnlLeftTools.Enabled = !busy;

            UseWaitCursor = busy;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (SettingsForm form = new SettingsForm())
            {
                form.ShowDialog(this);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOcrResult.Text))
            {
                MessageBox.Show(
                    "Chưa có kết quả OCR để lưu.",
                    "Lưu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Lưu kết quả OCR";
                dialog.Filter =
                    "Tài liệu Word (*.docx)|*.docx|" +
                    "File văn bản (*.txt)|*.txt";
                dialog.DefaultExt = "docx";
                dialog.AddExtension = true;

                // Tự đặt tên theo file gốc
                string originalName =
                    Path.GetFileNameWithoutExtension(currentFilePath);

                dialog.FileName = originalName + "_ocr";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    string textToSave = GetTextToSave();

                    if (Path.GetExtension(dialog.FileName)
                            .Equals(".docx", StringComparison.OrdinalIgnoreCase))
                    {
                        DocxExporter.Save(dialog.FileName, textToSave);
                    }
                    else
                    {
                        File.WriteAllText(
                            dialog.FileName,
                            textToSave,
                            Encoding.UTF8);
                    }

                    MessageBox.Show(
                        "Đã lưu kết quả OCR thành công.",
                        "Lưu",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không thể lưu file:\n" + ex.Message,
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private string GetTextToSave()
        {
            // Nếu là PDF và đã OCR toàn bộ
            if (isPdf &&
                isPdfOcrCompleted &&
                currentOcrPdfResult != null &&
                currentOcrPdfResult.pages != null)
            {
                StringBuilder sb = new StringBuilder();

                foreach (OcrPdfPage page in currentOcrPdfResult.pages)
                {
                    sb.AppendLine(
                        $"===== TRANG {page.page} =====");

                    sb.AppendLine(page.text ?? "");

                    sb.AppendLine();
                    sb.AppendLine();
                }

                return sb.ToString();
            }

            // Ảnh hoặc OCR crop
            return txtOcrResult.Text;
        }

        private async Task<OcrResponse> OcrBitmapBySelectedTypeAsync(Bitmap bitmap)
        {
            if (chkHandwritten.Checked)
            {
                return await ocrService.OcrHandwritingBitmapAsync(bitmap);
            }

            return await ocrService.OcrBitmapAsync(bitmap);
        }

        private async Task OcrSelectedRegionAsync()
        {
            if (currentImage == null)
                return;

            if (selectedImageRect == Rectangle.Empty)
                return;

            Rectangle rect = selectedImageRect;

            // Đảm bảo vùng crop nằm trong ảnh
            rect.Intersect(
                new Rectangle(
                    0,
                    0,
                    currentImage.Width,
                    currentImage.Height));

            if (rect.Width <= 0 ||
                rect.Height <= 0)
            {
                MessageBox.Show(
                    "Vùng chọn không hợp lệ.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            txtOcrResult.Text =
                "Đang OCR vùng đã chọn...";

            using (Bitmap cropped =
                   currentImage.Clone(
                       rect,
                       currentImage.PixelFormat))
            {
                OcrResponse result =
                    await OcrBitmapBySelectedTypeAsync(cropped);

                currentOcrResult = result;

                txtOcrResult.Text =
                    result?.layout_text ?? "";
            }
        }

        private async Task OcrEntirePdfAsync()
        {
            if (currentPdf == null)
                return;

            if (currentPdf.PageCount <= 0)
                return;

            List<OcrPdfPage> pages =
                new List<OcrPdfPage>();

            DateTime startTime = DateTime.Now;

            txtOcrResult.Text =
                "Đang OCR toàn bộ PDF...";

            for (int i = 0;
                 i < currentPdf.PageCount;
                 i++)
            {
                int pageNumber = i + 1;

                txtOcrResult.Text =
                    $"Đang OCR PDF...\r\n" +
                    $"Trang {pageNumber} / {currentPdf.PageCount}";

                Application.DoEvents();

                Bitmap pageBitmap = null;

                try
                {
                    // Render trang thành Bitmap
                    pageBitmap =
                        RenderPdfPageBitmap(i);

                    if (pageBitmap == null)
                        continue;

                    // Gửi Bitmap lên /ocr
                    OcrResponse result =
                        await OcrBitmapBySelectedTypeAsync(pageBitmap);

                    if (result == null)
                        continue;

                    // Chuyển OcrResponse -> OcrPdfPage
                    OcrPdfPage page =
                        new OcrPdfPage();

                    page.page = pageNumber;
                    page.text = result.layout_text ?? "";
                    page.confidence = result.confidence;
                    page.ocr_time = result.ocr_time;

                    pages.Add(page);
                }
                finally
                {
                    pageBitmap?.Dispose();
                }
            }

            // =====================================================
            // Gom kết quả
            // =====================================================

            string fullText = "";

            foreach (OcrPdfPage page in pages)
            {
                if (!string.IsNullOrWhiteSpace(page.text))
                {
                    if (!string.IsNullOrEmpty(fullText))
                        fullText +=
                            Environment.NewLine +
                            Environment.NewLine;

                    fullText += page.text;
                }
            }

            double requestTime =
                (DateTime.Now - startTime).TotalSeconds;

            currentOcrPdfResult =
                new OcrPdfResponse
                {
                    success = pages.Count > 0,
                    filename =
                        Path.GetFileName(currentFilePath),
                    page_count =
                        currentPdf.PageCount,
                    text = fullText,
                    pages = pages,
                    request_time = requestTime
                };

            isPdfOcrCompleted = true;

            // Hiển thị kết quả trang hiện tại
            ShowCurrentPageOcrResult();

            MessageBox.Show(
                $"OCR PDF hoàn tất.\n\n" +
                $"Đã xử lý: {pages.Count} / " +
                $"{currentPdf.PageCount} trang.",
                "OCR hoàn tất",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            int x = Math.Min(p1.X, p2.X);
            int y = Math.Min(p1.Y, p2.Y);

            int width = Math.Abs(p2.X - p1.X);
            int height = Math.Abs(p2.Y - p1.Y);

            return new Rectangle(x, y, width, height);
        }

        private Rectangle GetImageDisplayRectangle()
        {
            if (picPreview.Image == null)
                return Rectangle.Empty;

            Image image = picPreview.Image;

            float imageRatio =
                (float)image.Width / image.Height;

            float boxRatio =
                (float)picPreview.ClientSize.Width /
                picPreview.ClientSize.Height;

            int width;
            int height;

            if (imageRatio > boxRatio)
            {
                // Ảnh rộng hơn PictureBox
                width = picPreview.ClientSize.Width;
                height = (int)(width / imageRatio);
            }
            else
            {
                // Ảnh cao hơn PictureBox
                height = picPreview.ClientSize.Height;
                width = (int)(height * imageRatio);
            }

            int x =
                (picPreview.ClientSize.Width - width) / 2;

            int y =
                (picPreview.ClientSize.Height - height) / 2;

            return new Rectangle(x, y, width, height);
        }

        private Point PictureBoxToImagePoint(Point point)
        {
            if (picPreview.Image == null)
                return Point.Empty;

            int x =
                (int)((point.X - imageOffset.X) / zoom);

            int y =
                (int)((point.Y - imageOffset.Y) / zoom);

            x = Math.Max(
                0,
                Math.Min(x, picPreview.Image.Width - 1));

            y = Math.Max(
                0,
                Math.Min(y, picPreview.Image.Height - 1));

            return new Point(x, y);
        }

        private Rectangle ImageToPictureBoxRectangle(
    Rectangle imageRect)
        {
            return new Rectangle(
                imageOffset.X +
                    (int)(imageRect.X * zoom),

                imageOffset.Y +
                    (int)(imageRect.Y * zoom),

                (int)(imageRect.Width * zoom),

                (int)(imageRect.Height * zoom)
            );
        }
        private void picPreview_MouseDown(object sender, MouseEventArgs e)
        {
            // Chuột phải: xóa vùng Crop
            if (e.Button == MouseButtons.Right)
            {
                selectedImageRect = Rectangle.Empty;
                isSelecting = false;

                picPreview.Invalidate();
                return;
            }

            if (e.Button != MouseButtons.Left)
                return;

            // ==========================
            // CHẾ ĐỘ CROP
            // ==========================
            if (isCropMode)
            {
                if (picPreview.Image == null)
                    return;

                Point imagePoint =
                    PictureBoxToImagePoint(e.Location);

                selectionStart = imagePoint;
                selectionEnd = imagePoint;

                isSelecting = true;

                picPreview.Invalidate();

                return;
            }

            // ==========================
            // CHẾ ĐỘ PAN
            // ==========================
            if (picPreview.Image != null)
            {
                isPanning = true;

                panStart = e.Location;
                panStartOffset = imageOffset;

                picPreview.Cursor = Cursors.SizeAll;
            }
        }

        private void picPreview_MouseMove(object sender, MouseEventArgs e)
        {
            // ==========================
            // CROP
            // ==========================
            if (isCropMode && isSelecting)
            {
                selectionEnd =
                    PictureBoxToImagePoint(e.Location);

                picPreview.Invalidate();

                return;
            }

            // ==========================
            // PAN
            // ==========================
            if (isPanning)
            {
                int dx = e.X - panStart.X;
                int dy = e.Y - panStart.Y;

                imageOffset = new Point(
                    panStartOffset.X + dx,
                    panStartOffset.Y + dy
                );

                picPreview.Invalidate();
            }
        }

        private void picPreview_MouseUp(object sender, MouseEventArgs e)
        {
            // ==========================
            // CROP
            // ==========================
            if (isCropMode)
            {
                if (e.Button != MouseButtons.Left)
                    return;

                if (!isSelecting)
                    return;

                isSelecting = false;

                selectionEnd =
                    PictureBoxToImagePoint(e.Location);

                selectedImageRect =
                    GetNormalizedRectangle(
                        selectionStart,
                        selectionEnd);

                if (selectedImageRect.Width < 5 ||
                    selectedImageRect.Height < 5)
                {
                    selectedImageRect = Rectangle.Empty;
                }

                picPreview.Invalidate();

                return;
            }

            // ==========================
            // PAN
            // ==========================
            if (e.Button == MouseButtons.Left)
            {
                isPanning = false;

                picPreview.Cursor = Cursors.Default;
            }
        }

        private void picPreview_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(picPreview.BackColor);

            if (picPreview.Image == null)
            {
                DrawEmptyState(e.Graphics);
                return;
            }

            Rectangle imageRect =
                GetZoomedImageRectangle();

            // Bóng đổ nhẹ phía sau trang tài liệu
            using (SolidBrush shadow =
                new SolidBrush(Color.FromArgb(38, 12, 74, 110)))
            {
                e.Graphics.FillRectangle(
                    shadow,
                    imageRect.X + 4,
                    imageRect.Y + 4,
                    imageRect.Width,
                    imageRect.Height);
            }

            e.Graphics.InterpolationMode =
                System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            e.Graphics.DrawImage(
                picPreview.Image,
                imageRect
            );

            using (Pen borderPen = new Pen(UiTheme.Sky200))
            {
                e.Graphics.DrawRectangle(
                    borderPen,
                    imageRect.X,
                    imageRect.Y,
                    imageRect.Width - 1,
                    imageRect.Height - 1);
            }

            // ==========================
            // VẼ VÙNG CROP
            // ==========================

            if (isCropMode &&
                (isSelecting ||
                 selectedImageRect != Rectangle.Empty))
            {
                Rectangle cropRect;

                if (isSelecting)
                {
                    Rectangle tempRect =
                        GetNormalizedRectangle(
                            selectionStart,
                            selectionEnd);

                    cropRect =
                        ImageToPictureBoxRectangle(
                            tempRect);
                }
                else
                {
                    cropRect =
                        ImageToPictureBoxRectangle(
                            selectedImageRect);
                }

                using (SolidBrush fill =
                    new SolidBrush(Color.FromArgb(50, 62, 146, 212)))
                using (Pen pen =
                    new Pen(UiTheme.PrimaryHover, 2))
                {
                    e.Graphics.FillRectangle(
                        fill,
                        cropRect);

                    e.Graphics.DrawRectangle(
                        pen,
                        cropRect);
                }
            }
        }

        private void btnCrop_Click(object sender, EventArgs e)
        {
            if (currentImage == null)
                return;

            if (!isCropMode)
            {
                // BẬT CROP
                isCropMode = true;

                // Chọn vùng mới
                selectedImageRect = Rectangle.Empty;
                isSelecting = false;

                picPreview.Cursor = Cursors.Cross;
            }
            else
            {
                // TẮT CROP
                isCropMode = false;

                // Xóa vùng cũ
                selectedImageRect = Rectangle.Empty;
                isSelecting = false;

                picPreview.Cursor = Cursors.Default;
            }

            UpdateButtonStates();
            picPreview.Invalidate();
        }

        private Size GetZoomedImageSize()
        {
            if (picPreview.Image == null)
                return Size.Empty;

            return new Size(
                (int)(picPreview.Image.Width * zoom),
                (int)(picPreview.Image.Height * zoom)
            );
        }

        private Rectangle GetZoomedImageRectangle()
        {
            if (picPreview.Image == null)
                return Rectangle.Empty;

            Size imageSize = GetZoomedImageSize();

            return new Rectangle(
                imageOffset.X,
                imageOffset.Y,
                imageSize.Width,
                imageSize.Height
            );
        }

        private void CenterImage()
        {
            if (currentImage == null)
                return;

            int imageWidth =
                (int)(currentImage.Width * zoom);

            int imageHeight =
                (int)(currentImage.Height * zoom);

            imageOffset = new Point(
                (picPreview.ClientSize.Width - imageWidth) / 2,
                (picPreview.ClientSize.Height - imageHeight) / 2
            );

            picPreview.Invalidate();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            ZoomImage(ZoomStep);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            ZoomImage(-ZoomStep);
        }

        private void ZoomImage(float amount)
        {
            if (picPreview.Image == null)
                return;

            float oldZoom = zoom;

            float newZoom = zoom + amount;

            if (newZoom < MinZoom)
                newZoom = MinZoom;

            if (newZoom > MaxZoom)
                newZoom = MaxZoom;

            if (newZoom == oldZoom)
                return;

            // Tâm của vùng hiển thị
            Point viewportCenter = new Point(
                picPreview.ClientSize.Width / 2,
                picPreview.ClientSize.Height / 2
            );

            // Tọa độ ảnh trước khi zoom
            float imageX =
                (viewportCenter.X - imageOffset.X) / oldZoom;

            float imageY =
                (viewportCenter.Y - imageOffset.Y) / oldZoom;

            zoom = newZoom;

            // Giữ nguyên điểm đó ở giữa
            imageOffset.X =
                viewportCenter.X -
                (int)(imageX * zoom);

            imageOffset.Y =
                viewportCenter.Y -
                (int)(imageY * zoom);

            picPreview.Invalidate();
        }

        private void ApplyRotation(
    RotateFlipType rotateType)
        {
            // PDF không cho xoay
            if (isPdf)
                return;

            if (currentImage == null)
                return;

            Bitmap rotatedImage =
                new Bitmap(currentImage);

            rotatedImage.RotateFlip(rotateType);

            currentImage.Dispose();
            currentImage = rotatedImage;

            picPreview.Image = currentImage;

            // Xóa crop cũ
            selectedImageRect = Rectangle.Empty;
            isSelecting = false;

            // Reset zoom
            zoom = 1.0f;

            FitImageToView();

            // Căn giữa
            CenterImage();

            picPreview.Invalidate();
        }
        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            if (picPreview.Image == null)
            {
                MessageBox.Show(
                    "Vui lòng mở ảnh hoặc PDF trước.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ApplyRotation(
                RotateFlipType.Rotate270FlipNone);
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            if (picPreview.Image == null)
            {
                MessageBox.Show(
                    "Vui lòng mở ảnh hoặc PDF trước.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ApplyRotation(
                RotateFlipType.Rotate90FlipNone);
        }

        private void SetButtonState(
            Button button,
            bool enabled,
            bool active = false)
        {
            UiTheme.SetActive(button, active);

            button.Enabled = enabled;

            // Màu / icon theo kiểu nút (xem Themes/UiTheme.cs)
            UiTheme.Refresh(button);
        }

        private void UpdateButtonStates()
        {
            bool hasImage = currentImage != null;
            bool hasPdf = currentPdf != null;

            bool hasMultiplePages =
                hasPdf && totalPages > 1;

            // =========================
            // PDF NAVIGATION
            // =========================

            SetButtonState(
                btnPrevPage,
                hasMultiplePages && currentPage > 0);

            SetButtonState(
                btnNextPage,
                hasMultiplePages &&
                currentPage < totalPages - 1);


            // =========================
            // CROP
            // =========================

            // CẢ ẢNH VÀ PDF đều Crop được
            // Icon Crop <-> dấu ✓ khi đang ở chế độ chọn vùng
            UiTheme.SetIcon(
                btnCrop,
                isCropMode ? Icons.Check : Icons.Crop);

            SetButtonState(
                btnCrop,
                hasImage,
                isCropMode);


            // =========================
            // ZOOM
            // =========================

            // CẢ ẢNH VÀ PDF đều Zoom được
            SetButtonState(
                btnZoomIn,
                hasImage);

            SetButtonState(
                btnZoomOut,
                hasImage);


            // =========================
            // ROTATE
            // =========================

            // CHỈ ẢNH được Rotate
            SetButtonState(
                btnRotateLeft,
                hasImage && !hasPdf);

            SetButtonState(
                btnRotateRight,
                hasImage && !hasPdf);


            // =========================
            // OCR
            // =========================

            SetButtonState(
                btnOcr,
                hasImage && !isBusy);


            // =========================
            // SCAN
            // =========================

            SetButtonState(
                btnScan,
                true);


            // =========================
            // SAVE / CLEAR
            // =========================

            bool hasOcrText =
                !string.IsNullOrWhiteSpace(
                    txtOcrResult.Text);

            SetButtonState(
                btnSave,
                hasOcrText);

            SetButtonState(
                btnClear,
                true);
        }

        private void FitImageToView()
        {
            if (currentImage == null)
                return;

            if (picPreview.ClientSize.Width <= 0 ||
                picPreview.ClientSize.Height <= 0)
                return;

            float scaleX =
                (float)picPreview.ClientSize.Width /
                currentImage.Width;

            float scaleY =
                (float)picPreview.ClientSize.Height /
                currentImage.Height;

            // Lấy tỷ lệ nhỏ hơn để toàn bộ ảnh nằm trong khung
            zoom = Math.Min(scaleX, scaleY);

            // Không phóng ảnh nhỏ lên quá lớn
            if (zoom > 1.0f)
                zoom = 1.0f;

            CenterImage();
        }

        private void chkPrinted_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkPrinted.Checked && !chkHandwritten.Checked)
            {
                chkPrinted.Checked = true;
                return;
            }

            currentOcrPdfResult = null;
            isPdfOcrCompleted = false;
            currentOcrResult = null;
            txtOcrResult.Clear();
        }

        private void chkHandwritten_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkPrinted.Checked && !chkHandwritten.Checked)
            {
                chkHandwritten.Checked = true;
                return;
            }

            currentOcrPdfResult = null;
            isPdfOcrCompleted = false;
            currentOcrResult = null;
            txtOcrResult.Clear();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // =========================
            // XÓA KẾT QUẢ OCR
            // =========================
            txtOcrResult.Clear();

            currentOcrResult = null;
            currentOcrPdfResult = null;
            isPdfOcrCompleted = false;

            // =========================
            // XÓA TRẠNG THÁI CROP
            // =========================
            isCropMode = false;
            isSelecting = false;
            selectedImageRect = Rectangle.Empty;

            picPreview.Cursor = Cursors.Default;

            // =========================
            // XÓA ẢNH HIỆN TẠI
            // =========================
            if (currentImage != null)
            {
                currentImage.Dispose();
                currentImage = null;
            }

            picPreview.Image = null;

            // =========================
            // ĐÓNG PDF
            // =========================
            if (currentPdf != null)
            {
                currentPdf.Dispose();
                currentPdf = null;
            }

            isPdf = false;
            currentFilePath = null;

            currentPage = 0;
            totalPages = 0;

            // =========================
            // RESET ZOOM / PAN
            // =========================
            zoom = 1.0f;
            imageOffset = Point.Empty;

            isPanning = false;
            panStart = Point.Empty;
            panStartOffset = Point.Empty;

            // =========================
            // RESET THÔNG TIN TRANG
            // =========================
            lblPageInfo.Text = "";
            lblPageInfo.Visible = false;

            // =========================
            // CẬP NHẬT GIAO DIỆN
            // =========================
            UpdatePageButtons();
            UpdateButtonStates();

            picPreview.Invalidate();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            using (CameraForm cameraForm =
           new CameraForm())
            {
                if (cameraForm.ShowDialog() ==
                    DialogResult.OK)
                {
                    if (cameraForm.CapturedImage != null)
                    {
                        if (currentImage != null)
                        {
                            currentImage.Dispose();
                            currentImage = null;
                        }

                        currentImage =
                            new Bitmap(
                                cameraForm.CapturedImage);

                        picPreview.Image = currentImage;

                        currentFilePath = null;

                        isPdf = false;

                        currentPdf = null;

                        currentPage = 0;
                        totalPages = 0;

                        currentOcrResult = null;
                        currentOcrPdfResult = null;
                        isPdfOcrCompleted = false;

                        txtOcrResult.Clear();

                        isCropMode = false;
                        isSelecting = false;
                        selectedImageRect = Rectangle.Empty;

                        picPreview.Cursor =
                            Cursors.Default;

                        zoom = 1.0f;
                        imageOffset = Point.Empty;

                        FitImageToView();

                        lblPageInfo.Visible = false;

                        UpdatePageButtons();
                        UpdateButtonStates();

                        picPreview.Invalidate();
                    }
                }
            }
        }
    }
}
