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

Example:<br/>

create a MainPage.xaml<br/>
In XAML it is necessary to insert a WebView with Visibility false.

        <VerticalStackLayout
            Padding="30,0"
            Spacing="25">
            <Button
                x:Name="ButtonGeneratePDF"
                Text="Generate" 
                Clicked="GenerateClicked"
                HorizontalOptions="Fill" />
            <WebView x:Name="WebViewPDF" IsVisible="False"/>
        </VerticalStackLayout>


In the code links put:
<pre>
    public partial class MainPage : ContentPage
    {
        private string html = "<b>Hello</a>";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void GenerateClicked(object sender, EventArgs e)
        {
            WebViewPDF.Source = new HtmlWebViewSource() {  Html = html  };

            if (await ToPdfService.ToPdfAsync(WebViewPDF, "sampleNamePdf") is ToFileResult result)
            {
                if (result.IsError)
                {
                    await Toast.Make(result.Result).Show();
                    return;
                }

                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(result.Result)
                });
            }
        }
    }
}
        </pre>

Currently available for Android and IOS.
