namespace Krassheiten.Game.Service;

using Microsoft.Win32;
using System.Text.Json;
using Krassheiten.Game.Entity;

class LauncherService
{
    public void SetKnownLaunchers()
    {
        string path = "assets/game/knownLaunchers.json";
        string json = File.ReadAllText(path);
        Launcher.Record[]? launchers = JsonSerializer.Deserialize<Launcher.Record[]>(json);
        Launcher.KnownLaunchers = launchers;
    }

    public void SetInstalledLaunchers()
    {
        Launcher.Record[]? knownLaunchers = Launcher.KnownLaunchers;
        if (knownLaunchers == null)
        {
            Launcher.InstalledLaunchers = null;
            return;
        }

        List<Launcher.Record> installedLaunchers = [];
        foreach (var knownLauncher in knownLaunchers)
        {
            if (!IsInstalledLauncher(knownLauncher))
                continue;

            installedLaunchers.Add(knownLauncher);
        }

        Launcher.InstalledLaunchers = installedLaunchers.ToArray();
        SetInstallPath();
        SetLibraryFolderPath();
    }

    private static bool IsInstalledLauncher(Launcher.Record knownLauncher)
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
        Launcher.Record[]? installedLaunchers = Launcher.InstalledLaunchers;
        if (installedLaunchers == null)
            return;

        for (int i = 0; i < installedLaunchers.Length; i++)
        {
            string installPath = ResolveInstallPath(installedLaunchers[i]) ?? "nothing found";
            installedLaunchers[i] = installedLaunchers[i] with { InstallPath = installPath };
        }

        Launcher.InstalledLaunchers = installedLaunchers;
    }

    private static string? ResolveInstallPath(Launcher.Record launcher)
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
        Launcher.Record[]? installedLaunchers = Launcher.InstalledLaunchers;
        if (installedLaunchers == null)
            return;

        for (int i = 0; i < installedLaunchers.Length; i++)
        {
            string installPath = installedLaunchers[i].InstallPath;
            string libraryFolderPath = ResolveLibraryFolderPath(installedLaunchers[i], installPath) ?? "nothing found";
            libraryFolderPath = Path.Combine(libraryFolderPath, installedLaunchers[i].StdGameFoldersPath);
            installedLaunchers[i] = installedLaunchers[i] with { GameFolderPath = libraryFolderPath };
        }

        Launcher.InstalledLaunchers = installedLaunchers;
    }

    private static string? ResolveLibraryFolderPath(Launcher.Record launcher, string installPath)
    {
        if (string.IsNullOrEmpty(installPath))
            return null;
        if (string.IsNullOrEmpty(launcher.StdLibraryFilePath))
            return null;
        
        var vdfPath = Path.Combine(installPath, launcher.StdLibraryFilePath);
        if (!File.Exists(vdfPath))
            return null;

        var vdfData = LauncherVdfService.LoadVdfAsArray(vdfPath);
        if (vdfData != null)        {
            return LauncherVdfService.GetLibraryFolderPathFromVdf(vdfData);
        }
        return null;
    }
}