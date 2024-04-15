using HtmlToPdf.Maui.Interfaces;

namespace HtmlToPdf.Maui.Extensions
{
    public static class AppBuilderExtensions
    {
        public static MauiAppBuilder UseHtmlToPdf(this MauiAppBuilder builder, Action? backPressHandler = null)
        {
            builder.Services.AddSingleton<IToPdfService, ToPdfService>();
            return builder;
        }
    }
}
