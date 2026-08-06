using System.Text.Json;
using Microsoft.Web.WebView2.WinForms;
using SystemGameManager.Games.Entity;
using SystemGameManager.Games.Controller;
using SystemGameManager.Handler;

namespace SystemGameManager.View2;

public partial class MainForm : Form
{
    private readonly GameAudioController gameAudioController = new();

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
                Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            web.Source = new Uri("https://systemgamemanager/index.html");
        };

        FormClosed += (_, _) => gameAudioController.Dispose();
    }
}