using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace ProjetDotNet.Service;

public interface IPdfParserService
{
    Task<string> ParsePdfToText(string filePath);
}

public class PdfParserService : IPdfParserService
{
    public async Task<string> ParsePdfToText(string filePath)
    {
        using (PdfReader pdfReader = new PdfReader(filePath))
        using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
        {
            StringBuilder text = new StringBuilder();
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);
                text.Append(PdfTextExtractor.GetTextFromPage(page));
            }
            return text.ToString();
        }
    }
}