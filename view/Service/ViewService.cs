namespace SystemGameManager.View.Service;

using SystemGameManager.Games.Service;
using System.Drawing.Drawing2D;
using SystemGameManager.View.Components;
using SystemGameManager.Games.Entity;

class ViewService
{
    public static string GetVersionFromConfig()
    {
        var version = GlobalConfig.Settings.AppConfig.Version;
        return version ?? "unknown";
    }

    // public static void OpenGameDirectory(string path)
    // {
    //     var gameViewService = new GameViewService();
    //     if (!gameViewService.TryOpenDirectory(path, out var errorMessage))
    //     {
    //         MessageBox.Show(errorMessage, "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information);
    //     }
    // }

    public static string GetSystemTheme()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                var appsUseLightTheme = key.GetValue("AppsUseLightTheme");
                if (appsUseLightTheme != null && appsUseLightTheme is int value)
                {
                    return value == 0 ? "Dark" : "Light";
                }
            }
        }
        catch
        {
            // Ignore any exceptions and return default theme
        }

        return "Dark"; // Default to dark theme if detection fails
    }

    public static void MakeRoundedPanel(Panel panel, int radius)
    {
        GraphicsPath path = new GraphicsPath();

        path.AddArc(0, 0, radius, radius, 180, 90);
        path.AddArc(panel.Width - radius, 0, radius, radius, 270, 90);
        path.AddArc(panel.Width - radius, panel.Height - radius, radius, radius, 0, 90);
        path.AddArc(0, panel.Height - radius, radius, radius, 90, 90);

        path.CloseFigure();

        panel.Region = new Region(path);
    }
    public static TableLayoutPanel GetNewSection(string title, bool isDropdown = false)
    {
        var section = new TableLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 12),
        };
        var sectionTitlePanel = new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
        };
        var sectionTitle = new Label()
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        sectionTitlePanel.Controls.Add(sectionTitle);
        section.Controls.Add(sectionTitlePanel);
        return section;
    }
    public static void OpenDirectory(NormalButton openGameDirectoryButton, Game game)
    {
        openGameDirectoryButton.Click += (sender, e) =>
        {
            if (System.IO.Directory.Exists(game.InstallFolderPath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = game.InstallFolderPath,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Ordner existiert nicht oder Pfad ist ungültig.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
    }
}