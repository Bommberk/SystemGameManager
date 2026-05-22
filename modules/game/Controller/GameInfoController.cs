namespace SystemGameManager.Games.Controller;

using SystemGameManager.Database.Controller;
using SystemGameManager.Games.Entity;
using SystemGameManager.Games.Service;

class GameInfoController
{
    private readonly LauncherService launcherService = new();
    private readonly GameService gameService = new();

    public GameInfoController()
    {
        launcherService.SetKnownLaunchers();
        launcherService.SetInstalledLaunchers();
        
        var databaseController = new DatabaseController();
        databaseController.GetDatabaseService().RecordManager(Launcher.InstalledLaunchers);

        gameService.SetInstalledGames();
    }

    private void WriteHeadline()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==============================");
        Console.WriteLine("      Game-Informationen      ");
        Console.WriteLine("==============================");
        Console.ResetColor();
    }

    public void Write()
    {
        Launcher.Record[]? InstalledLaunchers = Launcher.InstalledLaunchers;
        Game.Record[]? InstalledGames = Game.InstalledGames;
        
        WriteHeadline();

        Console.WriteLine("Launchers:");
        if (InstalledLaunchers != null)
        {
            foreach (var launcher in InstalledLaunchers)
            {
                Console.WriteLine($"- {launcher.Name}:");
                Console.WriteLine($"  -> Installationspfad: {launcher.InstallPath}");
                Console.WriteLine($"  -> Spielordnerpfade: {string.Join(", ", launcher.GameFolderPath)}");
            }
        }
        else
        {
            Console.WriteLine("Keine Informationen zu Launchern verfügbar.");
        }

        Console.WriteLine("\nGames:");
        if (InstalledGames != null && InstalledGames.Length > 0)
        {
            foreach (var game in InstalledGames)
            {
                Console.WriteLine($"- {game.Name}");
                Console.WriteLine($"  -> Installationspfad: {game.InstallFolderPath}");
                Console.WriteLine($"  -> Exe-Pfad: {game.ExePath}");
            }
        }
        else
        {
            Console.WriteLine("Keine Informationen zu Spielen verfügbar.");
        }
    }

}
