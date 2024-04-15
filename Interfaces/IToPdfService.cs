﻿using HtmlToPdf.Maui.Models;

namespace HtmlToPdf.Maui.Interfaces
{
    /// <summary>
    /// Html to pdf service.
    /// </summary>
    public interface IToPdfService
    {
        /// <summary>
        /// Html to PNG interface
        /// </summary>
        /// <param name="html">Html text (source)</param>
        /// <param name="fileName">name of PDF file (without suffix) to be stored in local storage</param>
        /// <param name="pageSize">Forms9Patch.PageSize for media size of PDF pages.</param>
        /// <param name="margin">Forms9Patch.PageMargin for margins of PDF pages.</param>
        /// <returns></returns>
		Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin);


        /// <summary>
        /// Determines if PDF printing is available on this platform;
        /// </summary>
        bool IsAvailable { get; }
    }
}
