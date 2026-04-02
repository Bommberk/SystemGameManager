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
            if (launcher.DisplayName.Contains("Steam", StringComparison.OrdinalIgnoreCase))
            {
                libraryFolderPath = launcher.LibraryFolderPath;
                break;
            }
        }

        return [];
    }
    
    public record Games
    (
        string Id,
        string Name,
        string InstallPath
    );
}