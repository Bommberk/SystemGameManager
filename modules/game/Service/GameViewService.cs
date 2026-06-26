using SystemGameManager.Games.Entity;

namespace SystemGameManager.Games.Service;

class GameViewService
{
    public Launcher[] GetInstalledLauncher()
    {
        var launchers = Launcher.GetLaunchers();
        return launchers ?? Array.Empty<Launcher>();
    }

    public Game[] GetInstalledGames()
    {
        var games = Game.GetGames();
        return games ?? Array.Empty<Game>();
    }
}