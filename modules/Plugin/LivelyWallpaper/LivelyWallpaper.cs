using SystemGameManager.Games.Entity;
using System.Diagnostics;
using UserEntity = SystemGameManager.User.Entity.User;

namespace SystemGameManager.Plugin;


class LivelyWallpaper
{
    private const int LivelySearchRetrySeconds = 60;
    private const int LivelyProcessTimeoutMs = 5000;

    private static int settingsVersion;

    public static LivelyWallpaper Instance { get; } = new();

    /// <summary>Muss aufgerufen werden, wenn der Nutzer Wallpaper-Einstellungen ändert.</summary>
    public static void NotifySettingsChanged()
    {
        Interlocked.Increment(ref settingsVersion);
        Task.Run(Instance.ApplyDefaultNow);
    }

    // Verhindert, dass sich überlappende Timer-Ticks das Wallpaper mehrfach setzen.
    private readonly object stateLock = new();

    // Nach einem Sofort-Apply bleibt das Wallpaper, bis wieder ein Spiel im Vordergrund war.
    private bool holdUntilGameSeen;

    private int loadedSettingsVersion = -1;
    private string? manualLivelyPath;
    private string? userDefaultWallpaperPath;
    private string? livelyExePath;
    private DateTime nextLivelySearch = DateTime.MinValue;
    private bool isLivelyMissingLogged;
    private bool isDefaultMissingLogged;

    private string? appliedGameWallpaper;
    private int lastAppliedMonitorNumber = 1;

    private LivelyWallpaper()
    {
        // Das Windows-Wallpaper wird beim Start gemerkt, bevor ein Spiel-Wallpaper gesetzt wird.
        WindowsWallpaperService.CaptureStartupWallpaper();
    }

    public void SetWallpaperForRunningGame(Game? game, string? gameWallpaper = null)
    {
        // Läuft bereits ein Tick, wird dieser übersprungen statt parallel ausgeführt.
        if (!Monitor.TryEnter(stateLock))
            return;

        try
        {
            ReloadSettingsIfChanged();

            if (game is not null)
                holdUntilGameSeen = false;

            if (string.IsNullOrWhiteSpace(gameWallpaper))
            {
                if (holdUntilGameSeen)
                    return;

                RevertToDefault();
            }
            else if (!string.Equals(appliedGameWallpaper, gameWallpaper, StringComparison.OrdinalIgnoreCase))
            {
                ApplyGameWallpaper(game, gameWallpaper);
            }
        }
        finally
        {
            Monitor.Exit(stateLock);
        }
    }

    /// <summary>Setzt das Wallpaper des Spiels sofort, z.B. nachdem es in der UI geändert wurde.</summary>
    public void ApplyNow(Game game)
    {
        if (string.IsNullOrWhiteSpace(game.GameImage))
            return;

        lock (stateLock)
        {
            ReloadSettingsIfChanged();
            if (ApplyGameWallpaper(game, game.GameImage))
                holdUntilGameSeen = true;
        }
    }

    /// <summary>Wendet das Default-Wallpaper sofort an, sofern gerade kein Spiel-Wallpaper aktiv ist.</summary>
    private void ApplyDefaultNow()
    {
        lock (stateLock)
        {
            ReloadSettingsIfChanged();

            if (appliedGameWallpaper is not null && !holdUntilGameSeen)
                return;

            holdUntilGameSeen = false;
            RevertToDefault(force: true);
        }
    }

    private bool ApplyGameWallpaper(Game? game, string gameWallpaper)
    {
        mlog($"Setting wallpaper for running game: {gameWallpaper}");
        lastAppliedMonitorNumber = game?.LivelyWallpaperScreen ?? 1;
        if (!SetWallpaper(gameWallpaper))
            return false;

        appliedGameWallpaper = gameWallpaper;
        return true;
    }

    private void RevertToDefault(bool force = false)
    {
        if (appliedGameWallpaper is null && !force)
            return;

        var defaultWallpaper = GetDefaultWallpaperPath();
        if (defaultWallpaper is null)
        {
            if (!isDefaultMissingLogged)
            {
                mlog("No default wallpaper available, keeping the current wallpaper.");
                isDefaultMissingLogged = true;
            }
            appliedGameWallpaper = null;
            return;
        }

        mlog("Reverting to default wallpaper.");
        if (SetWallpaper(defaultWallpaper))
            appliedGameWallpaper = null;
    }

    private void ReloadSettingsIfChanged()
    {
        var currentVersion = Volatile.Read(ref settingsVersion);
        if (currentVersion == loadedSettingsVersion)
            return;

        var user = UserEntity.GetUser();
        manualLivelyPath = user.LivelyExePath;
        userDefaultWallpaperPath = user.DefaultWallpaperPath;
        livelyExePath = null;
        nextLivelySearch = DateTime.MinValue;
        isLivelyMissingLogged = false;
        isDefaultMissingLogged = false;
        loadedSettingsVersion = currentVersion;
    }

    private string? GetDefaultWallpaperPath()
    {
        if (!string.IsNullOrWhiteSpace(userDefaultWallpaperPath) && File.Exists(userDefaultWallpaperPath))
            return userDefaultWallpaperPath;

        var windowsWallpaper = WindowsWallpaperService.StartupWallpaperPath;
        return windowsWallpaper is not null && File.Exists(windowsWallpaper) ? windowsWallpaper : null;
    }

    private bool SetWallpaper(string path)
    {
        var exePath = getLivelyWallpaperPath();
        if (exePath is null)
            return false;

        var monitorNumber = lastAppliedMonitorNumber;

        if (monitorNumber < 1 || !File.Exists(path))
            return true;

        var wallpaperFile = WallpaperCropper.PrepareForMonitor(path, monitorNumber);

        var startInfo = new ProcessStartInfo
        {
            FileName = exePath,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        startInfo.ArgumentList.Add("setwp");
        startInfo.ArgumentList.Add("--file");
        startInfo.ArgumentList.Add(wallpaperFile);
        startInfo.ArgumentList.Add("--monitor");
        startInfo.ArgumentList.Add(monitorNumber.ToString());

        using var process = Process.Start(startInfo);
        if (process is not null && !process.WaitForExit(LivelyProcessTimeoutMs))
        {
            mlog("Lively did not finish in time.");
        }
        return true;
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

    private string? getLivelyWallpaperPath()
    {
        if (livelyExePath is not null && File.Exists(livelyExePath))
            return livelyExePath;

        if (DateTime.UtcNow < nextLivelySearch)
            return null;

        livelyExePath = LivelyLocator.FindLivelyExe(manualLivelyPath);
        if (livelyExePath is null)
        {
            nextLivelySearch = DateTime.UtcNow.AddSeconds(LivelySearchRetrySeconds);
            if (!isLivelyMissingLogged)
            {
                mlog("Lively Wallpaper executable not found.");
                isLivelyMissingLogged = true;
            }
        }
        return livelyExePath;
    }
}