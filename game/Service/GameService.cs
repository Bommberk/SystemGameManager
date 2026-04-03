namespace Krassheiten.Game.Service;

using Krassheiten.Game.Entity;

class GameService
{
    public void SetInstalledGames()
    {
        SetGamesWithGameFolder();
    }

    private void SetGamesWithGameFolder()
    {
        List<Game.Record> installedGames = [];

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
}