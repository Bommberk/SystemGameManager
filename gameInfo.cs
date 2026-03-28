using System.Text.Json;
using Microsoft.Win32;

namespace Infos;

class GameInfo
{
    public KnownLauncher[]? installedLaunchers { get; } = getInstalledLaunchers();
    public string[]? Games { get; }
    private static KnownLauncher[]? knownLaunchers = GetKnownLaunchers();

    public GameInfo()
    {
        
    }

    public void Write()
    {
        Console.WriteLine("==============================");
        Console.WriteLine("      Game-Informationen      ");
        Console.WriteLine("==============================");

        Console.WriteLine("Launchers:");
        if (installedLaunchers != null)
        {
            foreach (var launcher in installedLaunchers)
            {
                Console.WriteLine($"- {launcher.displayName} -> Installationspfad: {launcher.installPath}");
            }
        }
        else
        {
            Console.WriteLine("Keine Informationen zu Launchern verfügbar.");
        }

        Console.WriteLine("\nGames:");
        if (Games != null)
        {
            foreach (var game in Games)
            {
                Console.WriteLine($"- {game}");
            }
        }
        else
        {
            Console.WriteLine("Keine Informationen zu Spielen verfügbar.");
        }
    }

    private static KnownLauncher[]? GetKnownLaunchers()
    {
        string path = "knownLaunchers.json";
        string json = File.ReadAllText(path);
        KnownLauncher[] launchers = JsonSerializer.Deserialize<KnownLauncher[]>(json);
        Console.WriteLine(launchers);
        return launchers;
    }

    private static KnownLauncher[]? getInstalledLaunchers()
    {
        string[] paths = {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        if(knownLaunchers == null)
        {
            return null;
        }
        KnownLauncher[] launchers = [];
        foreach(var knownLauncher in knownLaunchers){
            bool found = false;
            foreach (var path in paths)
            {
                using var key = Registry.LocalMachine.OpenSubKey(path);

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    string displayName = subKey.GetValue("DisplayName") as string;

                    if (!string.IsNullOrEmpty(displayName) &&
                        displayName.Contains(knownLauncher.searchName, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"{knownLauncher.displayName} Launcher gefunden!");
                        found = true;
                        string installPath = subKey.GetValue("InstallLocation") as string ?? knownLauncher.installPath;
                        launchers = launchers.Append(knownLauncher with { installPath = installPath }).ToArray();
                        break;
                    }
                }
                if(found)
                    break;
            }
        }
        return launchers;
    }

    private void CheckUbisoftExe()
    {
        string[] drives = Directory.GetLogicalDrives();
        foreach (var drive in drives)
        {
            Console.WriteLine($"Durchsuche {drive} nach Ubisoft Launcher...");
            try
            {
                var files = Directory.GetFiles(drive,"UbisoftConnect.exe", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    Console.WriteLine("Ubisoft Launcher gefunden!");
                    return;
                }
            }
            catch(UnauthorizedAccessException)
            {
                Console.WriteLine("Fehler");
                // Zugriff auf bestimmte Verzeichnisse verweigert, einfach ignorieren
            }

        }
    }
    public record KnownLauncher(
        string displayName,
        string searchName,
        string stdPath,
        string installPath
    );
}