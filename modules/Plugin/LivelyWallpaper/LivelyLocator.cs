namespace SystemGameManager.Plugin;

using Microsoft.Win32;

public static class LivelyLocator
{
    public const string LivelyCliFileName = "livelycu.exe";

    private static readonly string[] UninstallRoots =
    {
        @"Software\Microsoft\Windows\CurrentVersion\Uninstall",
        @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    /// <summary>
    /// Sucht die Lively-CLI in dieser Reihenfolge: manueller Pfad, Registry, Standardordner, PATH.
    /// </summary>
    public static string? FindLivelyExe(string? manualPath)
    {
        if (IsLivelyExe(manualPath))
        {
            return manualPath;
        }

        return FindInRegistry() ?? FindInKnownFolders() ?? FindInPath();
    }

    public static bool IsLivelyExe(string? path)
    {
        return !string.IsNullOrWhiteSpace(path)
            && path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            && File.Exists(path);
    }

    private static string? FindInRegistry()
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            foreach (var root in UninstallRoots)
            {
                try
                {
                    using var uninstallKey = hive.OpenSubKey(root);
                    if (uninstallKey is null)
                    {
                        continue;
                    }

                    foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                    {
                        using var appKey = uninstallKey.OpenSubKey(subKeyName);
                        var displayName = appKey?.GetValue("DisplayName") as string;
                        if (displayName is null || !displayName.StartsWith("Lively Wallpaper", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var installFolder = appKey!.GetValue("InstallLocation") as string;
                        if (string.IsNullOrWhiteSpace(installFolder)
                            && appKey.GetValue("DisplayIcon") is string displayIcon)
                        {
                            installFolder = Path.GetDirectoryName(displayIcon.Split(',')[0].Trim('"'));
                        }

                        var found = FindInFolder(installFolder);
                        if (found is not null)
                        {
                            return found;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mlog($"Registry search for Lively failed: {ex.Message}");
                }
            }
        }

        return null;
    }

    private static string? FindInKnownFolders()
    {
        var folders = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Lively Wallpaper"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Lively Wallpaper"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Lively Wallpaper"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps")
        };

        return folders.Select(FindInFolder).FirstOrDefault(found => found is not null);
    }

    private static string? FindInPath()
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        return pathVariable
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(folder => FindInFolder(folder.Trim('"')))
            .FirstOrDefault(found => found is not null);
    }

    private static string? FindInFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }

        try
        {
            var candidate = Path.Combine(folder, LivelyCliFileName);
            return File.Exists(candidate) ? candidate : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
