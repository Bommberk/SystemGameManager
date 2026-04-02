namespace Krassheiten.Game;

using Krassheiten.Game.Service;
using Krassheiten.Game.Entity;

class GameInfoController
{
    private readonly LauncherService launcherService = new();

    public GameInfoController()
    {
        launcherService.SetKnownLaunchers();
        launcherService.SetInstalledLaunchers();
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
                Console.WriteLine($"- {launcher.DisplayName} -> Installationspfad: {launcher.InstallPath}");
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
                Console.WriteLine($"- {game}");
            }
        }
        else
        {
            Console.WriteLine("Keine Informationen zu Spielen verfügbar.");
        }
    }

}
