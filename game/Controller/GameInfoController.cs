namespace Krassheiten.SystemGameManager.Controller;

using Krassheiten.SystemGameManager.Service;
using Krassheiten.SystemGameManager.Entity;

class GameInfoController
{
    private readonly LauncherService launcherService = new();
    private readonly GameService gameService = new();

    public GameInfoController()
    {
        launcherService.SetKnownLaunchers();
        launcherService.SetInstalledLaunchers();
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
        // GameService.GetInstalledGames();
        // return;
        Launcher.Record[]? InstalledLaunchers = Launcher.InstalledLaunchers;
        Game.Record[]? InstalledGames = Game.InstalledGames;
        
        WriteHeadline();

        Console.WriteLine("Launchers:");
        if (InstalledLaunchers != null)
        {
            foreach (var launcher in InstalledLaunchers)
            {
                Console.WriteLine($"- {launcher.DisplayName}:");
                Console.WriteLine($"  -> Installationspfad: {launcher.InstallPath}");
                Console.WriteLine($"  -> Spielordnerpfad: {launcher.GameFolderPath}");
            }
        }
        else
        {
            Console.WriteLine("Keine Informationen zu Launchern verfügbar.");
        }

        Console.WriteLine("\nGames:");
        if (InstalledGames != null)
        {
            foreach (var game in InstalledGames)
            {
                Console.WriteLine($"- {game.Name}");
            }
        }
        else
        {
            Console.WriteLine("Keine Informationen zu Spielen verfügbar.");
        }
    }

}
