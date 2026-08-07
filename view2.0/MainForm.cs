using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SystemGameManager.Games.Entity;
using SystemGameManager.Handler;
using SystemGameManager.Games.Controller;

namespace SystemGameManager.View2;

public partial class MainForm : Form
{
    private readonly GameAudioController gameAudioController = new ();

    public MainForm()
    {
        var web = new WebView2
        {
            Dock = DockStyle.Fill
        };

        WindowState = FormWindowState.Maximized;

        Controls.Add(web);

        Load += async (_, _) =>
        {
            await web.EnsureCoreWebView2Async();

            WebApiHandler.Initialize(web);

            var viewRoot = Path.Combine(AppContext.BaseDirectory, "view2.0");

            if(GlobalConfig.Settings.AppConfig.Environment == "dev")
            {
                viewRoot = Path.GetFullPath(Path.Combine(
                    AppContext.BaseDirectory,
                    "..","..", "..","view2.0"
                ));
            }

            web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "systemgamemanager",
                viewRoot,
                CoreWebView2HostResourceAccessKind.Allow);

            ConfigureLocalImageRequests(web.CoreWebView2);
            
            web.Source = new Uri("https://systemgamemanager/index.html");
        };

        FormClosed += (_, _) => gameAudioController.Dispose();
    }

    private static void ConfigureLocalImageRequests(CoreWebView2 webView)
    {
        const string localImageUrlPattern = "https://local-image/*";

        webView.AddWebResourceRequestedFilter(localImageUrlPattern, CoreWebView2WebResourceContext.Image);
        webView.WebResourceRequested += (_, args) =>
        {
            var encodedPath = new Uri(args.Request.Uri).AbsolutePath.TrimStart('/');
            var imagePath = Uri.UnescapeDataString(encodedPath);

            if (!Path.IsPathFullyQualified(imagePath) || !File.Exists(imagePath))
            {
                args.Response = webView.Environment.CreateWebResourceResponse(
                    null,
                    404,
                    "Not Found",
                    "Content-Type: text/plain");
                return;
            }

            var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            args.Response = webView.Environment.CreateWebResourceResponse(
                stream,
                200,
                "OK",
                $"Content-Type: {GetImageContentType(imagePath)}");
        };
    }

    private static string GetImageContentType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            ".jpeg" or ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}