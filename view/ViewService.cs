namespace SystemGameManager.View.Service;

using SystemGameManager.Games.Service;

class ViewService
{
    public static string GetVersionFromReleases()
    {
        try
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                if (version.Revision > 0)
                {
                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        catch { }
        
        return "Unknown";
    }

    public static void OpenGameDirectory(string path)
    {
        var gameViewService = new GameViewService();
        if (!gameViewService.TryOpenDirectory(path, out var errorMessage))
        {
            MessageBox.Show(errorMessage, "Hinweis", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

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
}