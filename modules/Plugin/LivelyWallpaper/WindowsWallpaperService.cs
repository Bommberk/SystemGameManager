namespace SystemGameManager.Plugin;

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

public static class WindowsWallpaperService
{
    private const uint SPI_GETDESKWALLPAPER = 0x0073;
    private const int MAX_PATH = 260;

    // Wird einmalig beim ersten Zugriff (App-Start) gelesen, bevor ein Spiel-Wallpaper gesetzt wird.
    private static readonly Lazy<string?> startupWallpaperPath = new(GetCurrentWallpaperPath);

    public static string? StartupWallpaperPath => startupWallpaperPath.Value;

    public static void CaptureStartupWallpaper()
    {
        _ = startupWallpaperPath.Value;
    }

    public static string? GetCurrentWallpaperPath()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {
            var buffer = new StringBuilder(MAX_PATH);
            if (SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, buffer, 0) && File.Exists(buffer.ToString()))
            {
                return buffer.ToString();
            }

            using var desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
            if (desktopKey?.GetValue("WallPaper") is string registryPath && File.Exists(registryPath))
            {
                return registryPath;
            }

            var transcodedPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft", "Windows", "Themes", "TranscodedWallpaper");
            return File.Exists(transcodedPath) ? transcodedPath : null;
        }
        catch (Exception ex)
        {
            mlog($"Could not read the Windows wallpaper: {ex.Message}");
            return null;
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint uiAction, int uiParam, StringBuilder pvParam, uint fWinIni);
}
