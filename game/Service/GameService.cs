namespace Krassheiten.Game.Service;

using Krassheiten.Game.Entity;

class GameService
{
    public static Games[]? GetInstalledGames()
    {
        return GetSteamGames() ?? [];
    }

    private static Games[]? GetSteamGames()
    {
        string libraryFolderPath = string.Empty;
        foreach (var launcher in Launcher.InstalledLaunchers ?? [])
        {
            Console.WriteLine($"Checking launcher: {launcher.DisplayName}");
            if (launcher.DisplayName.Contains("steam", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Steam launcher found!");
                libraryFolderPath = launcher.LibraryFolderPath;
                break;
            }
        }
        
        Console.WriteLine($"Steam Library Folder Path: {libraryFolderPath}");

        return [];
    }
    
    public record Games
    (
        string Id,
        string Name,
        string InstallPath
    );
}