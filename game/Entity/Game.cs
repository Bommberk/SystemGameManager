using Krassheiten.SystemGameManager.Service;

namespace Krassheiten.SystemGameManager.Entity;

class Game
{
    public const int MUSIC_VOLUME_PERCENT = 50;
    public const int GAME_VOLUME_PERCENT = 100;
    public const string TABLE_NAME = "Game";
    public static Record[]? InstalledGames { get; set; }

    public Game()
    {
        // MUSIC_VOLUME_PERCENT = 50;
    }

    public class Record
    {
        public string Name { get; set; }
        public string InstallFolderPath { get; set; }
        public string ExePath { get; set; }
        public string ProzessName { get; set; }
        public int MusicVolumePercent { get; set; } = MUSIC_VOLUME_PERCENT;
        public int GameVolumePercent { get; set; } = GAME_VOLUME_PERCENT;

        public Record(string name, string installFolderPath, string exePath, string? prozessName = null, int musicVolumePercent = MUSIC_VOLUME_PERCENT, int gameVolumePercent = GAME_VOLUME_PERCENT)
        {
            Name = name;
            InstallFolderPath = installFolderPath;
            ExePath = exePath;
            ProzessName = prozessName ?? string.Empty;
            MusicVolumePercent = musicVolumePercent;
            GameVolumePercent = gameVolumePercent;
        }
    }

    public void WriteGamesFromDatabase()
    {
        var databaseController = new DatabaseController();
        databaseController.ShowTable(TABLE_NAME);
    }
}