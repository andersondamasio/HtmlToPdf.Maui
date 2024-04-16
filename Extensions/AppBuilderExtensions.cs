using HtmlToPdf.Maui.Interfaces;

namespace HtmlToPdf.Maui.Extensions
{
    public static class AppBuilderExtensions
    {
        public static MauiAppBuilder UseHtmlToPdf(this MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IPdfService, PdfService>();
            return builder;
        }
    }
}
