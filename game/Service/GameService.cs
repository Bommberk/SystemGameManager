namespace Krassheiten.SystemGameManager.Service;

using Krassheiten.SystemGameManager.Entity;
using Microsoft.Win32;

class GameService
{
    public void SetInstalledGames()
    {
        var gamesFromFolders = GetGamesWithGameFolder();
        var gamesFromRegistry = GetGamesWithRegistry();
        
        var allGames = gamesFromFolders.Concat(gamesFromRegistry)
                                       .DistinctBy(game => game.InstallFolderPath)
                                       .ToArray();
                                       
        Game.InstalledGames = allGames;
        ApplySavedSettings();
        Game.SaveGames();
    }

    private IEnumerable<Game.Record> GetGamesWithGameFolder()
    {
        List<Game.Record> games = new();
        if(Launcher.InstalledLaunchers is null) return games;
        
        foreach (var launcher in Launcher.InstalledLaunchers)
        {
            if (launcher.GameFolderPath is null || launcher.GameFolderPath.Length == 0)
                continue;

            foreach (var gameFolder in launcher.GameFolderPath.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(gameFolder))
                    continue;

                string[] gameDirs = Directory.GetDirectories(gameFolder);
                foreach (var gameDir in gameDirs)
                {
                    string gameName = Path.GetFileName(gameDir);
                    string exePath = GetGameExe(gameDir);
                    games.Add(new Game.Record(gameName, gameDir, exePath));
                }
            }
        }
        return games;
    }

    private IEnumerable<Game.Record> GetGamesWithRegistry()
    {
        List<Game.Record> games = new();
        if(Launcher.InstalledLaunchers is null) return games;
        
        foreach(var launcher in Launcher.InstalledLaunchers)
        {
            string? registryKeyPath = launcher.DirectRegistryKey;
            if (string.IsNullOrEmpty(registryKeyPath))
                continue;

            using var key = Registry.LocalMachine.OpenSubKey(registryKeyPath);
            if (key == null) continue;

            string[] gameNames = key.GetSubKeyNames();
            foreach (var gameName in gameNames)
            {
                using var subKey = key.OpenSubKey(gameName);
                if (subKey == null) continue;

                string? installPath = null;
                foreach (var keyName in RegistryInstallKeyNames)
                {
                    if (!string.IsNullOrEmpty(installPath)) break;
                    installPath = subKey.GetValue(keyName) as string;
                }

                if (!string.IsNullOrEmpty(installPath))
                {
                    string pathName = Path.GetFileName(installPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    var resolvedGameName = int.TryParse(gameName, out _)
                        ? pathName
                        : gameName;

                    string exePath = GetGameExe(installPath);
                    string processName = GetProcessName(resolvedGameName, exePath);
                    games.Add(new Game.Record(resolvedGameName, installPath, exePath, processName));
                }
            }
        }
        return games;
    }

    private string GetGameExe(string installPath)
    {
        if (!Directory.Exists(installPath))
            return string.Empty;

        var exeFiles = Directory.GetFiles(installPath, "*.exe", SearchOption.TopDirectoryOnly);
        if(exeFiles.Length == 0)
        {
            var firstSubDir = Directory.GetDirectories(installPath).FirstOrDefault();
            if (firstSubDir != null)
                exeFiles = Directory.GetFiles(firstSubDir, "*.exe", SearchOption.TopDirectoryOnly);
        }
        return exeFiles.FirstOrDefault() ?? string.Empty;
    }

    private static string GetProcessName(string fallbackName, string exePath)
    {
        if (!string.IsNullOrWhiteSpace(exePath))
        {
            string processName = Path.GetFileNameWithoutExtension(exePath);
            if (!string.IsNullOrWhiteSpace(processName))
                return processName;
        }

        return fallbackName;
    }

    private static void ApplySavedSettings()
    {
        var gamesInDb = Game.GetGames();
        foreach(var gameInDb in gamesInDb ?? [])
        {
            var game = Game.InstalledGames?
                .FirstOrDefault(g => g.InstallFolderPath == gameInDb.InstallFolderPath);
            if (game == null)
                continue;

            if(gameInDb.MusicVolumePercent != null)
                game.MusicVolumePercent = gameInDb.MusicVolumePercent;

            if(gameInDb.GameVolumePercent != null)
                game.GameVolumePercent = gameInDb.GameVolumePercent;

            if(gameInDb.AudioOutputDevice != null)
                game.AudioOutputDevice = gameInDb.AudioOutputDevice;
        }
    }

    private static readonly string[] RegistryInstallKeyNames =
    [
        "InstallLocation",
        "InstallFolder",
        "InstallPath",
        "InstallDir",
        "Install Dir",
        "UninstallString"
    ];
}