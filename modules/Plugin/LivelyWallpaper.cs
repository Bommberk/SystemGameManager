using SystemGameManager.Games.Entity;
using System.Diagnostics;

namespace SystemGameManager.Plugin;


class LivelyWallpaper
{
    private const string DefaultWallpaperPath = "C:\\Users\\jimsm\\Pictures\\MSI Wallpaper\\MSI_MEG_ACE.jpg";
    private string? lastAppliedWallpaperPath = DefaultWallpaperPath;
    private int lastAppliedMonitorNumber = 1;
    public void SetWallpaperForRunningGame(Game? game, string? gameWallpaper = null)
    {
        if(string.IsNullOrWhiteSpace(gameWallpaper) && lastAppliedWallpaperPath != DefaultWallpaperPath)
        {
            mlog("Reverting to default wallpaper.");
            lastAppliedWallpaperPath = DefaultWallpaperPath;
            SetWallpaper(DefaultWallpaperPath);
            return;
        }else if(!string.IsNullOrWhiteSpace(gameWallpaper) && lastAppliedWallpaperPath == DefaultWallpaperPath){
            mlog($"Setting wallpaper for running game: {gameWallpaper}");
            lastAppliedWallpaperPath = gameWallpaper;
            lastAppliedMonitorNumber = game?.LivelyWallpaperScreen ?? 1;
            SetWallpaper(gameWallpaper);
            return;
        }
    }

    private void SetWallpaper(string path)
    {
        var livelyExePath = "P:\\Lively Wallpaper\\livelycu";
        var monitorNumber = lastAppliedMonitorNumber;
        

        if (lastAppliedMonitorNumber < 1 || !File.Exists(path))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = livelyExePath,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add("setwp");
        startInfo.ArgumentList.Add("--file");
        startInfo.ArgumentList.Add(path);
        startInfo.ArgumentList.Add("--monitor");
        startInfo.ArgumentList.Add(monitorNumber.ToString());

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }

    private int getRightMonitorNumber()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        if (screens.Length == 0)
        {
            return -1;
        }

        var rightMost = screens[0];

        foreach (var screen in screens)
        {
            // Use virtual desktop coordinates from Windows display arrangement.
            if (screen.Bounds.Right > rightMost.Bounds.Right
                || (screen.Bounds.Right == rightMost.Bounds.Right && screen.Bounds.Top < rightMost.Bounds.Top))
            {
                rightMost = screen;
            }
        }

        var digits = new string(rightMost.DeviceName.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out var monitorNumber))
        {
            return monitorNumber;
        }

        // Fallback to stable 1-based position in the Screen.AllScreens array.
        return Array.IndexOf(screens, rightMost) + 1;
    }

    private int getAmountOfMonitors()
    {
        return System.Windows.Forms.Screen.AllScreens.Length;
    }
}