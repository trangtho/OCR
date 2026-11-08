using System.Collections.Generic;

namespace OCRClient.Models
{
    // Kết quả OCR của cả file PDF
    public class OcrPdfResponse
    {
        public bool success { get; set; }

        public string filename { get; set; }

        public int page_count { get; set; }

        // Toàn bộ nội dung PDF (đã ghép các trang)
        public string text { get; set; }

        public List<OcrPdfPage> pages { get; set; }

        public double request_time { get; set; }
    }

    // Kết quả OCR của từng trang
    public class OcrPdfPage
    {
        public int page { get; set; }

        public string text { get; set; }

        public double confidence { get; set; }

        public double ocr_time { get; set; }
    }
}