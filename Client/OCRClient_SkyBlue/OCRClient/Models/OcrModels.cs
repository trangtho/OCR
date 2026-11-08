/*// ============================================================
// STUB - CHỈ ĐỂ PROJECT BUILD ĐƯỢC KHI XEM THỬ GIAO DIỆN.
// Hãy XÓA file này và dùng lại file Models gốc của bạn.
// ============================================================
using System.Collections.Generic;

namespace OCRClient.Models
{
    public class OcrResponse
    {
        public string layout_text { get; set; }
        public double confidence { get; set; }
        public double ocr_time { get; set; }
    }

    public class OcrPdfPage
    {
        public int page { get; set; }
        public string text { get; set; }
        public double confidence { get; set; }
        public double ocr_time { get; set; }
    }

    public class OcrPdfResponse
    {
        public bool success { get; set; }
        public string filename { get; set; }
        public int page_count { get; set; }
        public string text { get; set; }
        public List<OcrPdfPage> pages { get; set; }
        public double request_time { get; set; }
    }
}
*/