namespace SystemGameManager.User.Entity;

using System.ComponentModel.DataAnnotations;
using SystemGameManager.Database.Controller;
using SystemGameManager.Plugin;

public sealed class User
{
    public const string TABLE_NAME = "User";

    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Theme { get; set; }
    public bool EnableBackgroundRunning { get; set; }
    public bool CloseToBackground { get; set; }
    public bool StartWithWindows { get; set; }
    public bool StartInBackground { get; set; }
    public int? WindowX { get; set; }
    public int? WindowY { get; set; }
    public int? WindowWidth { get; set; }
    public int? WindowHeight { get; set; }
    public bool? IsWindowMaximized { get; set; }
    public string? DefaultWallpaperPath { get; set; }
    public string? LivelyExePath { get; set; }

    public static User GetUser()
    {
        var databaseController = new DatabaseController();
        var user = databaseController.GetDatabaseService().GetTableRecords<User>(TABLE_NAME).FirstOrDefault();
        if (user is not null)
        {
            return user;
        }

        user = new User();
        SaveUser(user);
        return user;
    }

    public static void SaveUser(User user)
    {
        new DatabaseController().GetDatabaseService().RecordManager(user);
    }

    public static User UpdateName(string? name)
    {
        var user = GetUser();
        user.Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        SaveUser(user);
        return user;
    }

    public static User UpdateTheme(string? theme)
    {
        if (theme is null || !SupportedThemes.Contains(theme))
        {
            return GetUser();
        }

        var user = GetUser();
        user.Theme = SupportedThemes.First(supportedTheme =>
            string.Equals(supportedTheme, theme, StringComparison.OrdinalIgnoreCase));
        SaveUser(user);
        return user;
    }

    public static User UpdateBackgroundSettings(
        bool enableBackgroundRunning,
        bool closeToBackground,
        bool startWithWindows,
        bool startInBackground)
    {
        var user = GetUser();
        user.EnableBackgroundRunning = enableBackgroundRunning;
        user.CloseToBackground = enableBackgroundRunning && closeToBackground;
        user.StartWithWindows = startWithWindows;
        user.StartInBackground = enableBackgroundRunning && startWithWindows && startInBackground;
        SaveUser(user);
        return user;
    }

    public static User UpdateWallpaperSettings(string? defaultWallpaperPath, string? livelyExePath)
    {
        var wallpaper = defaultWallpaperPath?.Trim().Trim('"');
        var livelyPath = livelyExePath?.Trim().Trim('"');

        var user = GetUser();
        user.DefaultWallpaperPath = WallpaperCropper.IsSupportedImage(wallpaper) && File.Exists(wallpaper)
            ? wallpaper
            : null;
        user.LivelyExePath = LivelyLocator.IsLivelyExe(livelyPath) ? livelyPath : null;
        SaveUser(user);
        LivelyWallpaper.NotifySettingsChanged();
        return user;
    }

    private static readonly HashSet<string> SupportedThemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "dark",
        "light",
        "red",
        "pink",
        "yellow"
    };
}
