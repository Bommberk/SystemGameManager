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

    public static string? GetInstallPath(Launcher.Record launcher)
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

        // 3. Fallback: übergeordnetes Verzeichnis des stdPath prüfen
        string stdDir = Path.GetDirectoryName(launcher.StdPath) ?? launcher.InstallPath;
        if (Directory.Exists(stdDir))
            return stdDir;

        // 4. Letzter Fallback: Standard-Installationspfad aus JSON
        return null;
    }

    public void SetInstalledLaunchers(Launcher.Record[]? knownLaunchers = null)
    {
        knownLaunchers ??= Launcher.KnownLaunchers;
        if (knownLaunchers == null)
        {
            Launcher.InstalledLaunchers = [];
            return;
        }

        Launcher.Record[] launchers = [];
        foreach (var knownLauncher in knownLaunchers)
        {
            bool foundInRegistry = false;
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
                        Console.WriteLine($"{knownLauncher.DisplayName} Launcher gefunden!");
                        foundInRegistry = true;
                        string? installPath = GetInstallPath(knownLauncher);
                        if(installPath == null)
                            installPath = "nothing found";
                        launchers = launchers.Append(knownLauncher with { InstallPath = installPath }).ToArray();
                        break;
                    }
                }
                if (foundInRegistry)
                    break;
            }
        }
        Launcher.InstalledLaunchers = launchers;
    }
}