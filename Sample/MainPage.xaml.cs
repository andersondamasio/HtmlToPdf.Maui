using CommunityToolkit.Maui.Alerts;
using HtmlToPdf.Maui;
using HtmlToPdf.Maui.Models;

namespace Sample
{
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
