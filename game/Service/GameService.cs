namespace Krassheiten.SystemGameManager.Service;

using Krassheiten.SystemGameManager.Entity;
using Microsoft.Win32;

class GameService
{
    public void SetInstalledGames()
    {
        SetGamesWithGameFolder();
        SetGamesWithRegistry();
        Game.InstalledGames = [.. (Game.InstalledGames ?? []).DistinctBy(game => game.InstallPath)];
    }

    private void SetGamesWithGameFolder()
    {
        List<Game.Record> installedGames = [.. Game.InstalledGames ?? []];

        foreach (var launcher in Launcher.InstalledLaunchers ?? [])
        {
            string gameFolder = launcher.GameFolderPath;
            if (!Directory.Exists(gameFolder))
                continue;
            
            string[] games = Directory.GetDirectories(gameFolder);
            foreach (var game in games)
            {
                string gameName = Path.GetFileName(game);
                installedGames.Add(new Game.Record(gameName, game));
            }
        }

        Game.InstalledGames = [.. installedGames.DistinctBy(game => game.InstallPath)];
    }

    private void SetGamesWithRegistry()
    {
        List<Game.Record> installedGames = [.. Game.InstalledGames ?? []];

        foreach(var launcher in Launcher.InstalledLaunchers ?? [])
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

                    installedGames.Add(new Game.Record(resolvedGameName, installPath));
                }
            }
        }
        Game.InstalledGames = [.. installedGames.DistinctBy(game => game.InstallPath)];
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