using Android.Graphics;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Views;
using global::Android.Webkit;
using HtmlToPdf.Maui.Interfaces;
using HtmlToPdf.Maui.Models;
using Java.Interop;
using Java.Lang;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

namespace HtmlToPdf.Maui
{
    public class ToPdfService : Java.Lang.Object, IToPdfService
    {
        public bool IsAvailable => Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

        public async Task<ToFileResult> ToPdfAsync(string html, string fileName, PageSize pageSize, PageMargin margin = default)
        {
            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();

            if (pageSize is null || pageSize.Width <= 0 || pageSize.Height <= 0)
                pageSize = PageSize.Default;

            margin = margin ?? new PageMargin();

            if (pageSize.Width - margin.HorizontalThickness < 1 || pageSize.Height - margin.VerticalThickness < 1)
                return new ToFileResult(true, "Page printable area (page size - margins) has zero width or height.");

            ToPdf(taskCompletionSource, html, fileName, pageSize, margin);
            return await taskCompletionSource.Task;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0067:Dispose objects before losing scope", Justification = "CustomWebView is disposed in Callback.Compete")]
        public void ToPdf(TaskCompletionSource<ToFileResult> taskCompletionSource, string html, string fileName, PageSize pageSize, PageMargin margin)
        {

            var webView = new Android.Webkit.WebView(Android.App.Application.Context);
            webView.Settings.JavaScriptEnabled = true;
#pragma warning disable CS0618 // Type or member is obsolete
            webView.DrawingCacheEnabled = true;
#pragma warning restore CS0618 // Type or member is obsolete
            webView.SetLayerType(LayerType.Software, null);

            //webView.Layout(0, 0, (int)((size.Width - 0.5) * 72), (int)((size.Height - 0.5) * 72));
            webView.Layout(0, 0, (int)System.Math.Ceiling(pageSize.Width), (int)System.Math.Ceiling(pageSize.Height));

            webView.LoadData(html, "text/html; charset=utf-8", "UTF-8");
            webView.SetWebViewClient(new WebViewCallBack(taskCompletionSource, fileName, pageSize, margin, OnPageFinished));
        }

        public async Task<ToFileResult> ToPdfAsync(Microsoft.Maui.Controls.WebView webView, string fileName, PageSize pageSize, PageMargin margin = default)
        {
            //if (!await XamarinEssentialsExtensions.ConfirmOrRequest<Xamarin.Essentials.Permissions.StorageWrite>())
            //    return new ToFileResult(true, "Write External Stoarge permission must be granted for PNG images to be available.");
            var taskCompletionSource = new TaskCompletionSource<ToFileResult>();

            if (pageSize is null || pageSize.Width <= 0 || pageSize.Height <= 0)
                pageSize = PageSize.Default;

            margin = margin ?? new PageMargin();

            if (pageSize.Width - margin.HorizontalThickness < 1 || pageSize.Height - margin.VerticalThickness < 1)
                return new ToFileResult(true, "Page printable area (page size - margins) has zero width or height.");

            ToPdf(taskCompletionSource, webView, fileName, pageSize, margin);
            return await taskCompletionSource.Task;
        }

        public void ToPdf(TaskCompletionSource<ToFileResult> taskCompletionSource, Microsoft.Maui.Controls.WebView xfWebView, string fileName, PageSize pageSize, PageMargin margin)
        {
            if (Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.CreateRendererWithContext(xfWebView, Microsoft.Maui.ApplicationModel.Platform.CurrentActivity) is IVisualElementRenderer renderer)
            {
                var droidWebView = renderer.View as Android.Webkit.WebView;
                if (droidWebView == null && renderer.View is WebViewRenderer xfWebViewRenderer)
                    droidWebView = xfWebViewRenderer.Control;
                if (droidWebView != null)
                {
                    //var size = new Size(8.5, 11);

                    //var externalPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                    //using (var dir = new Java.IO.File(externalPath))
                    //using (var file = new Java.IO.File(dir + "/" + fileName + ".pdf"))
                    //{
                    //if (!dir.Exists())
                    //    dir.Mkdir();
                    //if (file.Exists())
                    //    file.Delete();

                    droidWebView.SetLayerType(LayerType.Software, null);
                    droidWebView.Settings.JavaScriptEnabled = true;
#pragma warning disable CS0618 // Type or member is obsolete
                    droidWebView.DrawingCacheEnabled = true;
                    droidWebView.BuildDrawingCache();
#pragma warning restore CS0618 // Type or member is obsolete

                    droidWebView.SetWebViewClient(new WebViewCallBack(taskCompletionSource, fileName, pageSize, margin, OnPageFinished));
                    //}
                }
            }
        }


            async Task OnPageFinished(Android.Webkit.WebView webView, string fileName, PageSize pageSize, PageMargin margin, TaskCompletionSource<ToFileResult> taskCompletionSource)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                await Task.Delay(5);
                var builder = new PrintAttributes.Builder();
                //builder.SetMediaSize(PrintAttributes.MediaSize.NaLetter);
                builder.SetMediaSize(new PrintAttributes.MediaSize(pageSize.Name, pageSize.Name, (int)(pageSize.Width * 1000 / 72), (int)(pageSize.Height * 1000 / 72)));
                builder.SetResolution(new PrintAttributes.Resolution("pdf", "pdf", 72, 72));
                if (margin is null)
                    builder.SetMinMargins(PrintAttributes.Margins.NoMargins);
                else
                    builder.SetMinMargins(new PrintAttributes.Margins((int)(margin.Left * 1000 / 72), (int)(margin.Top * 1000 / 72), (int)(margin.Right * 1000 / 72), (int)(margin.Bottom * 1000 / 72)));
                var attributes = builder.Build();

                var adapter = webView.CreatePrintDocumentAdapter(Guid.NewGuid().ToString());

                var layoutResultCallback = new PdfLayoutResultCallback();
                layoutResultCallback.Adapter = adapter;
                layoutResultCallback.TaskCompletionSource = taskCompletionSource;
                layoutResultCallback.FileName = fileName;
                adapter.OnLayout(null, attributes, null, layoutResultCallback, null);
            }
        }
    }

    class WebViewCallBack : WebViewClient
    {
        bool _complete;
        readonly string _fileName;
        readonly PageSize _pageSize;
        readonly PageMargin _margin;
        readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
        readonly Func<Android.Webkit.WebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> _onPageFinished;

        public WebViewCallBack(TaskCompletionSource<ToFileResult> taskCompletionSource, string fileName, PageSize pageSize, PageMargin margin, Func<Android.Webkit.WebView, string, PageSize, PageMargin, TaskCompletionSource<ToFileResult>, Task> onPageFinished)
        {
            _fileName = fileName;
            _pageSize = pageSize;
            _margin = margin;
            _taskCompletionSource = taskCompletionSource;
            _onPageFinished = onPageFinished;
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            System.Diagnostics.Debug.WriteLine("WebViewCallBack" + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnPageStarted(view, url, favicon);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "Needed to invoke async code on main thread.")]
        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine("WebViewCallBack" + P42.Utils.ReflectionExtensions.CallerString() + ": SUCCESS!");
            if (!_complete)
            {
                _complete = true;

                Device.BeginInvokeOnMainThread(() =>
                {
                    _onPageFinished?.Invoke(view, _fileName, _pageSize, _margin, _taskCompletionSource);
                });
            }
        }

        public override void OnReceivedError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceError error)
        {
            base.OnReceivedError(view, request, error);
            _taskCompletionSource.SetResult(new ToFileResult(true, error.Description));
        }

        public override void OnReceivedHttpError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            base.OnReceivedHttpError(view, request, errorResponse);
            _taskCompletionSource.SetResult(new ToFileResult(true, errorResponse.ReasonPhrase));
        }

        public override bool OnRenderProcessGone(Android.Webkit.WebView view, RenderProcessGoneDetail detail)
        {
            System.Diagnostics.Debug.WriteLine("WebViewCallBack" + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            return base.OnRenderProcessGone(view, detail);
        }

        public override void OnLoadResource(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine("WebViewCallBack" + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnLoadResource(view, url);
            Device.StartTimer(TimeSpan.FromSeconds(10), () =>
            {
                if (!_complete)
                    OnPageFinished(view, url);
                return false;
            });
        }

        public override void OnPageCommitVisible(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine("WebViewCallBack" + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnPageCommitVisible(view, url);
        }

        public override void OnUnhandledKeyEvent(Android.Webkit.WebView view, KeyEvent e)
        {
            System.Diagnostics.Debug.WriteLine("WebViewCallBack" + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnUnhandledKeyEvent(view, e);
        }

        public override void OnUnhandledInputEvent(Android.Webkit.WebView view, InputEvent e)
        {
            System.Diagnostics.Debug.WriteLine("WebViewCallBack" + P42.Utils.ReflectionExtensions.CallerString() + ": ");
            base.OnUnhandledInputEvent(view, e);
        }
    }

    [Register("android/print/PdfLayoutResultCallback")]
    public class PdfLayoutResultCallback : PrintDocumentAdapter.LayoutResultCallback
    {
        public TaskCompletionSource<ToFileResult> TaskCompletionSource { get; set; }
        public string FileName { get; set; }
        public PrintDocumentAdapter Adapter { get; set; }

        public PdfLayoutResultCallback(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public PdfLayoutResultCallback() : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            if (!(Handle != IntPtr.Zero))
            {
                unsafe
                {
                    JniObjectReference val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                    SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                    JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                }
            }

        }

        public override void OnLayoutCancelled()
        {
            base.OnLayoutCancelled();
            TaskCompletionSource.SetResult(new ToFileResult(true, "PDF Layout was cancelled"));
        }

        public override void OnLayoutFailed(ICharSequence error)
        {
            base.OnLayoutFailed(error);
            TaskCompletionSource.SetResult(new ToFileResult(true, error.ToString()));
        }

        public override void OnLayoutFinished(PrintDocumentInfo info, bool changed)
        {
            //using (var _dir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments))
            //using (var _dir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads))
            //using (var _dir = Forms9Patch.Droid.Settings.Context.FilesDir)
            using (var _dir = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity.CacheDir)
            {
                if (!_dir.Exists())
                    _dir.Mkdir();

     
                var file = Java.IO.File.CreateTempFile(FileName + ".", ".pdf", _dir);

                var fileDescriptor = ParcelFileDescriptor.Open(file, ParcelFileMode.ReadWrite);

                var writeResultCallback = new PdfWriteResultCallback(TaskCompletionSource, file.AbsolutePath);

                Adapter.OnWrite(new Android.Print.PageRange[] { PageRange.AllPages }, fileDescriptor, new CancellationSignal(), writeResultCallback);

                file.Dispose();

                //Android.Media.MediaScannerConnection.ScanFile(Forms9Patch.Droid.Settings.Context, new string[] { file.AbsolutePath }, new string[] { "application/pdf" }, null);
                //Android.Media.MediaScannerConnection.ScanFile(file.AbsolutePath, "application/pdf");
            }
            base.OnLayoutFinished(info, changed);
        }


    }

    [Register("android/print/PdfWriteResult")]
    public class PdfWriteResultCallback : PrintDocumentAdapter.WriteResultCallback
    {
        readonly TaskCompletionSource<ToFileResult> _taskCompletionSource;
        readonly string _path;

        public PdfWriteResultCallback(TaskCompletionSource<ToFileResult> taskCompletionSource, string path, IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            _taskCompletionSource = taskCompletionSource;
            _path = path;
        }

        public PdfWriteResultCallback(TaskCompletionSource<ToFileResult> taskCompletionSource, string path) : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            if (!(Handle != IntPtr.Zero))
            {
                unsafe
                {
                    JniObjectReference val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                    SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                    JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                }
            }
            _taskCompletionSource = taskCompletionSource;
            _path = path;
        }


        public override void OnWriteFinished(PageRange[] pages)
        {
            base.OnWriteFinished(pages);
            _taskCompletionSource.SetResult(new ToFileResult(false, _path));
        }

        public override void OnWriteCancelled()
        {
            base.OnWriteCancelled();
            _taskCompletionSource.SetResult(new ToFileResult(true, "PDF Write was cancelled"));
        }

        public override void OnWriteFailed(ICharSequence error)
        {
            base.OnWriteFailed(error);
            _taskCompletionSource.SetResult(new ToFileResult(true, error.ToString()));
        }
    }
}
