namespace Krassheiten.SystemGameManager.Service;

using Krassheiten.SystemGameManager.Entity;
using Microsoft.Win32;

class GameService
{
    public void SetInstalledGames()
    {
        SetGamesWithGameFolder();
        SetGamesWithRegistry();
        Game.InstalledGames = [.. (Game.InstalledGames ?? []).DistinctBy(game => game.InstallFolderPath)];
        SetSettedValuesToNull();
        Game.SaveGames();
    }

    private void SetGamesWithGameFolder()
    {
        List<Game.Record> installedGames = [.. Game.InstalledGames ?? []];
        if(Launcher.InstalledLaunchers is null) return;
        foreach (var launcher in Launcher.InstalledLaunchers)
        {
            string gameFolder = launcher.GameFolderPath;
            if (!Directory.Exists(gameFolder))
                continue;
            
            string[] games = Directory.GetDirectories(gameFolder);
            foreach (var game in games)
            {
                string gameName = Path.GetFileName(game);
                string exePath = GetGameExe(game);
                // string processName = GetProcessName(gameName, exePath);
                installedGames.Add(new Game.Record(gameName, game, exePath));
            }
        }

        Game.InstalledGames = [.. installedGames.DistinctBy(game => game.InstallFolderPath)];
    }

    private void SetGamesWithRegistry()
    {
        List<Game.Record> installedGames = [.. Game.InstalledGames ?? []];
        if(Launcher.InstalledLaunchers is null) return;
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
                    installedGames.Add(new Game.Record(resolvedGameName, installPath, exePath, processName));
                }
            }
        }
        Game.InstalledGames = [.. installedGames.DistinctBy(game => game.InstallFolderPath)];
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

    private static void SetSettedValuesToNull()
    {
        var gamesInDb = Game.GetGames();
        foreach(var gameInDb in gamesInDb ?? [])
        {
            if(gameInDb.MusicVolumePercent != null)
            {
                var game = Game.InstalledGames?
                    .FirstOrDefault(g => g.InstallFolderPath == gameInDb.InstallFolderPath);
                if (game != null)
                    game.MusicVolumePercent = gameInDb.MusicVolumePercent;
            }
            if(gameInDb.GameVolumePercent != null)
            {
                var game = Game.InstalledGames?
                    .FirstOrDefault(g => g.InstallFolderPath == gameInDb.InstallFolderPath);
                if (game != null)
                    game.GameVolumePercent = gameInDb.GameVolumePercent;
            }
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