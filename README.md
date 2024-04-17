# HtmlToPdf.Maui



A set of HtmlToPdf.Maui

NuGet Packages:
[HtmlToPdf.Maui](https://www.nuget.org/packages/HtmlToPdf.Maui)
Usage:

In MauiProgram.CreateMauiApp register the control:  <b>.UseHtmlToPdf()</b>

<pre>
public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                <b>.UseHtmlToPdf()</b>
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
          </pre>
