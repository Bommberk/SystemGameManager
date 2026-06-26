namespace SystemGameManager.Games.Service;

using Microsoft.Win32;
using System.Text.Json;
using SystemGameManager.Games.Entity;

class LauncherService
{
    public void SetKnownLaunchers()
    {
        string path = "assets/game/knownLaunchers.json";
        string json = File.ReadAllText(path);
        Launcher.KnownLaunchers = JsonSerializer.Deserialize<Launcher[]>(json);
    }

    public void SetInstalledLaunchers()
    {
        if(Launcher.KnownLaunchers is null){
            Launcher.InstalledLaunchers = null;
            return;
        }
    
        List<Launcher> installedLaunchers = new();
        foreach (var knownLauncher in Launcher.KnownLaunchers)
        {
            if (!IsInstalledLauncher(knownLauncher))
                continue;
            installedLaunchers.Add(knownLauncher);
        }

        Launcher.InstalledLaunchers = installedLaunchers.ToArray();
        SetInstallPath();
        SetLibraryFolderPath();
    }

    private static bool IsInstalledLauncher(Launcher knownLauncher)
    {
        foreach (var regPath in Launcher.RegistryUninstallPaths)
        {
            using var key = Registry.LocalMachine.OpenSubKey(regPath);
            if (key == null) continue;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                string? displayName = subKey.GetValue("DisplayName") as string;
                if (!string.IsNullOrEmpty(displayName) &&
                    displayName.Contains(knownLauncher.SearchName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void SetInstallPath()
    {
        if(Launcher.InstalledLaunchers is null) return;
        foreach(var installedLauncher in Launcher.InstalledLaunchers)
        {
            string installPath = ResolveInstallPath(installedLauncher) ?? "nothing found";
            installedLauncher.InstallPath = installPath;
        }
    }

    private static string? ResolveInstallPath(Launcher launcher)
    {
        // 1. Direkter Registry-Key, falls im JSON angegeben (z. B. Steam)
        if (!string.IsNullOrEmpty(launcher.DirectRegistryKey))
        {
            using var directKey = Registry.LocalMachine.OpenSubKey(launcher.DirectRegistryKey);
            string? directPath = directKey?.GetValue("InstallPath") as string;
            if (!string.IsNullOrEmpty(directPath))
                return directPath;
        }

        // 2. Registry: InstallLocation aus dem Uninstall-Schlüssel lesen
        foreach (var regPath in Launcher.RegistryUninstallPaths)
        {
            using var key = Registry.LocalMachine.OpenSubKey(regPath);
            if (key == null) continue;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                string? displayName = subKey.GetValue("DisplayName") as string;
                if (!string.IsNullOrEmpty(displayName) &&
                    displayName.Contains(launcher.SearchName, StringComparison.OrdinalIgnoreCase))
                {
                    string? installLocation = subKey.GetValue("InstallLocation") as string;
                    if (!string.IsNullOrEmpty(installLocation))
                        return installLocation;
                    break;
                }
            }
        }

        // 3. Fallback: übergeordnetes Verzeichnis des Standard-Installationspfads prüfen
        string stdDir = Path.GetDirectoryName(launcher.StdInstallPath) ?? launcher.InstallPath;
        if (Directory.Exists(stdDir))
            return stdDir;

        // 4. Letzter Fallback: Standard-Installationspfad aus JSON
        return null;
    }

    private static void SetLibraryFolderPath()
    {
        if(Launcher.InstalledLaunchers is null) return;
        foreach(var launcher in Launcher.InstalledLaunchers)
        {
            string installPath = launcher.InstallPath;
            string[] libraryFolderPaths = ResolveLibraryFolderPath(launcher, installPath);
            launcher.GameFolderPath = [.. libraryFolderPaths
                .Select(path => Path.Combine(path, launcher.StdGameFoldersPath))
                .Distinct(StringComparer.OrdinalIgnoreCase)];
        }
    }

    private static string[] ResolveLibraryFolderPath(Launcher launcher, string installPath)
    {
        if (string.IsNullOrEmpty(installPath))
            return [];
        if (string.IsNullOrEmpty(launcher.StdLibraryFilePath))
            return [installPath];
        
        var vdfPath = Path.Combine(installPath, launcher.StdLibraryFilePath);
        if (!File.Exists(vdfPath))
            return [installPath];

        var vdfData = LauncherVdfService.LoadVdfAsArray(vdfPath);
        if (vdfData == null)
            return [installPath];

        var folderPaths = LauncherVdfService.GetLibraryFolderPathFromVdf(vdfData);
        return folderPaths.Length > 0 ? folderPaths : [installPath];
    }
}