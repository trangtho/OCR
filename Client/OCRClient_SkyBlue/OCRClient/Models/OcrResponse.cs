using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRClient.Models
{
    public class OcrResponse
    {
        public bool success { get; set; }

        public string filename { get; set; }

        public string text { get; set; }

        public string layout_text { get; set; }

        public double confidence { get; set; }

        public double quality { get; set; }

        public double score { get; set; }

        public string preprocess { get; set; }

        public int psm { get; set; }

        public double ocr_time { get; set; }

        public double request_time { get; set; }
    }
}
