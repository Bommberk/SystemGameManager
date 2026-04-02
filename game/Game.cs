namespace Krassheiten.Game.Entity;

class Game
{
    public static Record[]? InstalledGames { get; set; }

    public record Record
    (
        string DisplayName,
        string SearchName,
        string StdPath,
        string InstallPath,
        string GameFoldersPath,
        string LibraryFolderPath,
        string? DirectRegistryKey = null
    );
}