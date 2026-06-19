using SystemGameManager.Games.Entity;

namespace SystemGameManager.Games.Service;

class GameViewService
{
    public Launcher.Record[] GetInstalledLauncher()
    {
        var launchers = Launcher.GetLaunchers();
        return launchers ?? Array.Empty<Launcher.Record>();
    }

    public Game.Record[] GetInstalledGames()
    {
        var games = Game.GetGames();
        return games ?? Array.Empty<Game.Record>();
    }
}