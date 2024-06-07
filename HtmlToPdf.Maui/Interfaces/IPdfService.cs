using HtmlToPdf.Maui.Models;

namespace HtmlToPdf.Maui.Interfaces
{
    /// <summary>
    /// Html to pdf service.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Html to PDF interface
        /// </summary>
        /// <param name="html">Html text (source)</param>
        /// <param name="fileName">name of PDF file (without suffix) to be stored in local storage</param>
        /// <param name="pageSize">Forms9Patch.PageSize for media size of PDF pages.</param>
        /// <param name="margin">Forms9Patch.PageMargin for margins of PDF pages.</param>
        /// <returns></returns>
		//Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin);


        /// <summary>
        /// Creates a PDF from the contents of a Microsoft.Maui.Controls.WebView
        /// WebView must be on screen and not instantiated via code
        /// </summary>
        /// <param name="webView"> Microsoft.Maui.Controls.WebView</param>
        /// <param name="fileName">Name (not path), excluding suffix, of PDF file</param>
        /// <param name="pageSize">PDF page size, in points. (default based upon user's region)</param>
        /// <param name="margin">PDF page's margin, in points. (default is zero)</param>
        /// <returns>Forms9Patch.ToFileResult</returns>
        Task<ToFileResult> ToPdfAsync(WebView webView, string fileName, PageSize pageSize, PageMargin margin);
        

        /// <summary>
        /// Determines if PDF printing is available on this platform;
        /// </summary>
        bool IsAvailable { get; }
    }
}
