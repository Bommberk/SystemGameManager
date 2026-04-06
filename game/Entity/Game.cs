namespace Krassheiten.SystemGameManager.Entity;

class Game
{
    public static Record[]? InstalledGames { get; set; }

    public record Record
    (
        string Name,
        string InstallFolderPath,
        string ExePath,
        int GameVolume = 100,
        int MusicVolume = 50
    );
}