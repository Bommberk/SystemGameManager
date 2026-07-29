namespace SystemGameManager.Games.Entity;

using SystemGameManager.Games.Service;
using System.Reflection;

class Game
{
    public const int MUSIC_VOLUME_PERCENT = 50;
    public const int GAME_VOLUME_PERCENT = 100;
    public const string TABLE_NAME = "Game";
    public static Game[]? InstalledGames { get; set; }
    public string Name { get; set; }
    public string InstallFolderPath { get; set; }
    public string ExePath { get; set; }
    public string ProzessName { get; set; } 
    public int? MusicVolumePercent { get; set; } = MUSIC_VOLUME_PERCENT;
    public int? GameVolumePercent { get; set; } = GAME_VOLUME_PERCENT;
    public string? AudioOutputDevice { get; set; }
    public string? GameImage { get; set; }

    public Game(string name, string installFolderPath, string exePath, string prozessName = "nothing found", int? musicVolumePercent = null, int? gameVolumePercent = null, string? audioOutputDevice = null, string? gameImage = null)
    {
        Name = name;
        InstallFolderPath = installFolderPath;
        ExePath = exePath;
        ProzessName = prozessName;
        MusicVolumePercent = musicVolumePercent;
        GameVolumePercent = gameVolumePercent;
        AudioOutputDevice = audioOutputDevice;
        GameImage = gameImage;
    }

    public static void WriteGamesFromDatabase()
    {
        var databaseController = new DatabaseController();
        // databaseController.ShowTable(TABLE_NAME);
    }

    public static void SaveGames()
    {
        var databaseController = new DatabaseController();
        databaseController.GetDatabaseService().RecordManager(InstalledGames);
    }
    public static void UpdateGame(Game game)
    {
        var databaseController = new DatabaseController();
        databaseController.GetDatabaseService().UpdateRecordByName(TABLE_NAME, game.Name, game);
    }
    public static Game[] GetGames()
    {
        var databaseController = new DatabaseController();
        var games = databaseController.GetDatabaseService().GetTableRecords<Game>(TABLE_NAME);
        return games ?? Array.Empty<Game>();
    }
}   