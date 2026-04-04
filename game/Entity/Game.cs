namespace Krassheiten.SystemGameManager.Entity;

class Game
{
    public static Record[]? InstalledGames { get; set; }

    public record Record
    (
        string Name,
        string InstallPath
    );
}