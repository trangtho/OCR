using OCRClient.Models;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OCRClient.Services
{
    public class OcrService
    {
        // Địa chỉ server lấy từ phần Cài đặt (đổi IP là có tác dụng ngay)
        private string serverUrl { get { return AppSettings.Current.ServerUrl; } }

        private readonly HttpClient client;

        public OcrService()
        {
            client = new HttpClient();

            // Tắt timeout mặc định của HttpClient.
            // Thời gian chờ được đặt riêng cho từng lần gọi (xem PostWithTimeoutAsync).
            client.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
        }

        // =====================================================
        // GỌI API CÓ GIỚI HẠN THỜI GIAN (lấy từ Cài đặt)
        // =====================================================

        private async Task<HttpResponseMessage> PostWithTimeoutAsync(
            string url,
            HttpContent content)
        {
            int seconds = AppSettings.Current.TimeoutSeconds;

            using (var cts = new System.Threading.CancellationTokenSource(
                       TimeSpan.FromSeconds(seconds)))
            {
                try
                {
                    return await client.PostAsync(
                        url,
                        content,
                        cts.Token);
                }
                catch (TaskCanceledException)
                {
                    throw new TimeoutException(
                        "Server OCR không phản hồi sau " +
                        seconds + " giây.");
                }
            }
        }

        // =====================================================
        // KIỂM TRA SERVER
        // =====================================================

        public async Task<bool> CheckServerAsync()
        {
            try
            {
                string url = serverUrl + "/health";

                // Kiểm tra server chỉ chờ tối đa 5 giây
                using (var cts = new System.Threading.CancellationTokenSource(
                           TimeSpan.FromSeconds(5)))
                {
                    HttpResponseMessage response =
                        await client.GetAsync(url, cts.Token);

                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        // =====================================================
        // OCR TỪ FILE ẢNH
        // =====================================================

        public async Task<OcrResponse> OcrImageAsync(
            string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file ảnh.",
                    imagePath);
            }

            byte[] imageBytes =
                File.ReadAllBytes(imagePath);

            return await OcrBytesAsync(
                imageBytes,
                Path.GetFileName(imagePath));
        }

        // =====================================================
        // OCR TỪ BITMAP
        //
        // Dùng cho:
        // - PDF từng trang
        // - Crop
        // - Ảnh đang hiển thị
        // =====================================================

        public async Task<OcrResponse> OcrBitmapAsync(
            Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(
                    nameof(bitmap));
            }

            using (MemoryStream stream =
                   new MemoryStream())
            {
                bitmap.Save(
                    stream,
                    System.Drawing.Imaging.ImageFormat.Png);

                byte[] imageBytes =
                    stream.ToArray();

                return await OcrBytesAsync(
                    imageBytes,
                    "document.png");
            }
        }

        // =====================================================
        // HÀM LÕI GỬI ẢNH ĐẾN OCR SERVER (chữ in)
        // =====================================================

        private async Task<OcrResponse> OcrBytesAsync(
            byte[] imageBytes,
            string fileName)
        {
            if (imageBytes == null ||
                imageBytes.Length == 0)
            {
                throw new ArgumentException(
                    "Dữ liệu ảnh không hợp lệ.");
            }

            using (MultipartFormDataContent form =
                   new MultipartFormDataContent())
            {
                ByteArrayContent imageContent =
                    new ByteArrayContent(imageBytes);

                form.Add(
                    imageContent,
                    "file",
                    fileName);

                string url =
                    serverUrl + "/ocr";

                HttpResponseMessage response =
                    await PostWithTimeoutAsync(
                        url,
                        form);

                response.EnsureSuccessStatusCode();

                string json =
                    await response.Content
                        .ReadAsStringAsync();

                OcrResponse result =
                    JsonConvert.DeserializeObject<OcrResponse>(
                        json);

                return result;
            }
        }

        // =====================================================
        // OCR CHỮ VIẾT TAY
        // =====================================================

        public async Task<OcrResponse> OcrHandwritingBitmapAsync(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(
                    stream,
                    System.Drawing.Imaging.ImageFormat.Png);

                byte[] imageBytes = stream.ToArray();

                return await OcrHandwritingBytesAsync(
                    imageBytes,
                    "handwriting.png");
            }
        }

        private async Task<OcrResponse> OcrHandwritingBytesAsync(
            byte[] imageBytes,
            string fileName)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                throw new ArgumentException(
                    "Dữ liệu ảnh không hợp lệ.");

            using (MultipartFormDataContent form =
                   new MultipartFormDataContent())
            {
                ByteArrayContent imageContent =
                    new ByteArrayContent(imageBytes);

                form.Add(
                    imageContent,
                    "file",
                    fileName);

                // CÙNG SERVER, CHỈ KHÁC ENDPOINT
                string url = serverUrl + "/ocr-handwriting";

                HttpResponseMessage response =
                    await PostWithTimeoutAsync(url, form);

                response.EnsureSuccessStatusCode();

                string json =
                    await response.Content.ReadAsStringAsync();

                OcrResponse result =
                    JsonConvert.DeserializeObject<OcrResponse>(json);

                return result;
            }
        }
    }
}