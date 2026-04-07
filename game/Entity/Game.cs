using Krassheiten.SystemGameManager.Service;

namespace Krassheiten.SystemGameManager.Entity;

class Game
{
    public const int DEFAULT_MUSIC_VOLUME_PERCENT = 50;
    public const int DEFAULT_GAME_VOLUME_PERCENT = 100;
    public const string DEFAULT_TABLE_NAME = "Games";
    public static Record[]? InstalledGames { get; set; }

    public class Record
    {
        public string Name { get; set; }
        public string InstallFolderPath { get; set; }
        public string ExePath { get; set; }
        public string ProzessName { get; set; }
        public int MusicVolumePercent { get; set; } = DEFAULT_MUSIC_VOLUME_PERCENT;
        public int GameVolumePercent { get; set; } = DEFAULT_GAME_VOLUME_PERCENT;

        public Record(string name, string installFolderPath, string exePath, string? prozessName = null, int musicVolumePercent = DEFAULT_MUSIC_VOLUME_PERCENT, int gameVolumePercent = DEFAULT_GAME_VOLUME_PERCENT)
        {
            Name = name;
            InstallFolderPath = installFolderPath;
            ExePath = exePath;
            ProzessName = prozessName ?? string.Empty;
            MusicVolumePercent = musicVolumePercent;
            GameVolumePercent = gameVolumePercent;
        }
    }
}