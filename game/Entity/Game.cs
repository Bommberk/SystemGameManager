namespace Krassheiten.SystemGameManager.Entity;

using Krassheiten.SystemGameManager.Service;
using System.Reflection;

class Game
{
    public const int MUSIC_VOLUME_PERCENT = 50;
    public const int GAME_VOLUME_PERCENT = 100;
    public const string TABLE_NAME = "Game";
    public static Record[]? InstalledGames { get; set; }

    public class Record
    {
        public string Name { get; set; }
        public string InstallFolderPath { get; set; }
        public string ExePath { get; set; }
        public string ProzessName { get; set; } 
        public int? MusicVolumePercent { get; set; } = MUSIC_VOLUME_PERCENT;
        public int? GameVolumePercent { get; set; } = GAME_VOLUME_PERCENT;
        public string? AudioOutputDevice { get; set; }
        public string? GameImage { get; set; }

        public Record(string name, string installFolderPath, string exePath, string prozessName = "nothing found", int? musicVolumePercent = null, int? gameVolumePercent = null, string? audioOutputDevice = null, string? gameImage = null)
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

        // public Dictionary<string, string>[] GetProperties()
        // {
        //     var properties = typeof(Record).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //     var result = new Dictionary<string, string>[properties.Length];
        //     for (int i = 0; i < properties.Length; i++)
        //     {
        //         var prop = properties[i];
        //         result[i] = new Dictionary<string, string>
        //         {
        //             { "Name", prop.Name },
        //             { "Value", prop.GetValue(Activator.CreateInstance(typeof(Record), "","",""))?.ToString() ?? "null" },
        //             { "isNullable", Nullable.GetUnderlyingType(prop.PropertyType) != null ? "Yes" : "No" }
        //         };
        //     }
        //     return result;
        // }
    }

    public void WriteGamesFromDatabase()
    {
        var databaseController = new DatabaseController();
        // databaseController.ShowTable(TABLE_NAME);
    }

    public static void SaveGames()
    {
        var databaseController = new DatabaseController();
        databaseController.GetDatabaseService().RecordManager(InstalledGames);
    }
    public static Record[]? GetGames()
    {
        var databaseController = new DatabaseController();
        return databaseController.GetDatabaseService().GetTableRecords<Record>(TABLE_NAME);
    }
}