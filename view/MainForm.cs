using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SystemGameManager.Games.Entity;
using SystemGameManager.Handler;
using SystemGameManager.Games.Controller;
using SystemGameManager.Plugin.HotKeys;
using SystemGameManager.User.Service;
using UserEntity = SystemGameManager.User.Entity.User;

namespace SystemGameManager.View2;

public partial class MainForm : Form
{
    private readonly GameAudioController gameAudioController = new ();
    private readonly TaskCompletionSource<bool> initializationCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private NotifyIcon? notifyIcon;
    private bool isExiting;
    private readonly bool startInBackground;
    private readonly System.Windows.Forms.Timer windowSettingsTimer = new() { Interval = 500 };

    public Task Initialization => initializationCompletion.Task;

    public MainForm(bool startInBackground = false, UserEntity? userSettings = null)
    {
        userSettings ??= UserEntity.GetUser();
        UserStartupService.Update(userSettings);
        this.startInBackground = startInBackground && userSettings.EnableBackgroundRunning;
        UpdateTrayIcon(userSettings.EnableBackgroundRunning);

        var web = new WebView2
        {
            Dock = DockStyle.Fill
        };

        Text = "System Game Manager v" + GlobalConfig.Settings.AppConfig.Version;
        if(!GlobalConfig.Settings.AppConfig.State.Equals("stable", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(GlobalConfig.Settings.AppConfig.State))
            Text += " - " + GlobalConfig.Settings.AppConfig.State;
        RestoreWindowSettings(userSettings);

        Controls.Add(web);

        Move += (_, _) => ScheduleWindowSettingsSave();
        Resize += (_, _) => ScheduleWindowSettingsSave();
        windowSettingsTimer.Tick += (_, _) =>
        {
            windowSettingsTimer.Stop();
            SaveWindowSettings();
        };

        Load += async (_, _) =>
        {
            try
            {
                HotKeyListener.Start();
                await web.EnsureCoreWebView2Async();

                WebApiHandler.Initialize(web, updatedUser => UpdateTrayIcon(updatedUser.EnableBackgroundRunning));

                var viewRoot = Path.Combine(AppContext.BaseDirectory, "view");

                if(GlobalConfig.Settings.AppConfig.Environment == "dev")
                {
                    viewRoot = Path.GetFullPath(Path.Combine(
                        AppContext.BaseDirectory,
                        "..","..", "..","view"
                    ));
                }

                web.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "systemgamemanager",
                    viewRoot,
                    CoreWebView2HostResourceAccessKind.Allow);

                ConfigureLocalImageRequests(web.CoreWebView2);
                web.Source = new Uri("https://systemgamemanager/index.html");
                initializationCompletion.TrySetResult(true);
            }
            catch (Exception ex)
            {
                initializationCompletion.TrySetException(ex);
                ErrorHandler.Handle(ex, ErrorSeverity.Fatal);
            }
        };

        Shown += (_, _) =>
        {
            if (this.startInBackground)
            {
                Hide();
            }
        };

        FormClosing += (_, e) =>
        {
            var settings = UserEntity.GetUser();
            SaveWindowSettings(settings);
            if (!isExiting && settings.EnableBackgroundRunning && settings.CloseToBackground)
            {
                e.Cancel = true;
                Hide();
            }
        };

        FormClosed += (_, _) =>
        {
            HotKeyListener.Stop();
            gameAudioController.Dispose();
            windowSettingsTimer.Dispose();
            if (notifyIcon is not null)
            {
                notifyIcon.ContextMenuStrip?.Dispose();
                notifyIcon.Dispose();
            }
        };
    }

    private void UpdateTrayIcon(bool enabled)
    {
        if (notifyIcon is null)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Open", null, (_, _) => RestoreFromTray());
            menu.Items.Add("Exit", null, (_, _) =>
            {
                isExiting = true;
                Close();
            });

            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "System Game Manager",
                ContextMenuStrip = menu
            };
            notifyIcon.DoubleClick += (_, _) => RestoreFromTray();
        }

        notifyIcon.Visible = enabled;
    }

    internal void RestoreFromTray()
    {
        Show();
        Activate();
    }

    private void RestoreWindowSettings(UserEntity settings)
    {
        if (settings.WindowX is int x
            && settings.WindowY is int y
            && settings.WindowWidth is > 0 and int width
            && settings.WindowHeight is > 0 and int height)
        {
            var bounds = new Rectangle(x, y, width, height);
            if (!Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(bounds)))
            {
                var area = Screen.PrimaryScreen?.WorkingArea ?? Screen.AllScreens[0].WorkingArea;
                width = Math.Min(width, area.Width);
                height = Math.Min(height, area.Height);
                bounds = new Rectangle(
                    Math.Clamp(x, area.Left, area.Right - width),
                    Math.Clamp(y, area.Top, area.Bottom - height),
                    width,
                    height);
            }

            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
            WindowState = settings.IsWindowMaximized == true
                ? FormWindowState.Maximized
                : FormWindowState.Normal;
            return;
        }

        WindowState = FormWindowState.Maximized;
    }

    private void ScheduleWindowSettingsSave()
    {
        if (WindowState != FormWindowState.Minimized)
        {
            windowSettingsTimer.Stop();
            windowSettingsTimer.Start();
        }
    }

    private void SaveWindowSettings()
    {
        SaveWindowSettings(UserEntity.GetUser());
    }

    private void SaveWindowSettings(UserEntity settings)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            return;
        }

        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        settings.WindowX = bounds.X;
        settings.WindowY = bounds.Y;
        settings.WindowWidth = bounds.Width;
        settings.WindowHeight = bounds.Height;
        settings.IsWindowMaximized = WindowState == FormWindowState.Maximized;
        UserEntity.SaveUser(settings);
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