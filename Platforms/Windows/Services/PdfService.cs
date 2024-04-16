using HtmlToPdf.Maui.Interfaces;
using HtmlToPdf.Maui.Models;

namespace HtmlToPdf.Maui
{
    public class PdfService : IPdfService
    {
        public bool IsAvailable => throw new NotImplementedException();

        public Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin)
        {
            throw new NotImplementedException();
        }
    }
}
