using CommunityToolkit.Maui.Alerts;
using HtmlToPdf.Maui;
using HtmlToPdf.Maui.Models;

namespace Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void GenerateClicked(object sender, EventArgs e)
        {
            string html = "<b> Hello World </b>";

            if (await ToPdfService.ToPdfAsync(html, "sampleNamePdf") is ToFileResult result)
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
