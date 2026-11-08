using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace OCRClient.Services
{
    /// <summary>
    /// Xuất văn bản OCR ra file Word (.docx) bằng Open XML SDK.
    /// Không cần cài Microsoft Word trên máy.
    /// </summary>
    public static class DocxExporter
    {
        private const string FontName = "Times New Roman";

        public static void Save(string path, string text)
        {
            using (WordprocessingDocument document =
                WordprocessingDocument.Create(
                    path,
                    WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();

                Body body = mainPart.Document.AppendChild(new Body());

                string[] lines =
                    (text ?? "").Replace("\r\n", "\n").Split('\n');

                bool seenFirstPage = false;

                foreach (string line in lines)
                {
                    bool isPageHeader = line.StartsWith("===== TRANG");

                    Paragraph paragraph = new Paragraph();

                    // Từ trang thứ 2 của PDF: ngắt trang
                    if (isPageHeader && seenFirstPage)
                    {
                        paragraph.AppendChild(
                            new ParagraphProperties(
                                new PageBreakBefore()));
                    }

                    if (isPageHeader)
                        seenFirstPage = true;

                    RunProperties runProperties = new RunProperties(
                        new RunFonts
                        {
                            Ascii = FontName,
                            HighAnsi = FontName,
                            EastAsia = FontName,
                            ComplexScript = FontName
                        },
                        new FontSize { Val = "26" }); // 13pt

                    if (isPageHeader)
                        runProperties.InsertAt(new Bold(), 1);

                    Run run = new Run(runProperties);

                    run.AppendChild(
                        new Text(line)
                        {
                            Space = SpaceProcessingModeValues.Preserve
                        });

                    paragraph.AppendChild(run);
                    body.AppendChild(paragraph);
                }

                mainPart.Document.Save();
            }
        }
    }
}
