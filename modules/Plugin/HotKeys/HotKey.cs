namespace SystemGameManager.Plugin.HotKeys;

class HotKey
{
    public const string TABLE_NAME = "HotKey";

    public string Name { get; set; }
    public string? Description { get; set; }
    public int? MusicVolumePercent { get; set; }
    public int? GameVolumePercent { get; set; }
    public string? AudioOutputDevice { get; set; }
    /// <summary>Hardware key ids joined by "+" (see HotKeyListener), not characters, so macro keys work too.</summary>
    public string KeyCodes { get; set; }
    /// <summary>SerializedGameName of the game the hotkey is limited to; null/empty = global.</summary>
    public string? GameSerializedName { get; set; }

    public HotKey(string name, string keyCodes = "", string? description = null, int? musicVolumePercent = null, int? gameVolumePercent = null, string? audioOutputDevice = null, string? gameSerializedName = null)
    {
        Name = name;
        KeyCodes = keyCodes;
        Description = description;
        MusicVolumePercent = musicVolumePercent;
        GameVolumePercent = gameVolumePercent;
        AudioOutputDevice = audioOutputDevice;
        GameSerializedName = gameSerializedName;
    }

    public HashSet<int> GetKeySet()
    {
        var keys = new HashSet<int>();
        foreach (var part in (KeyCodes ?? string.Empty).Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var id)) keys.Add(id);
        }
        return keys;
    }

    private static HotKey[]? cache;

    public static HotKey[] GetHotKeys()
    {
        var databaseController = new DatabaseController();
        return databaseController.GetDatabaseService().GetTableRecords<HotKey>(TABLE_NAME);
    }

    /// <summary>In-memory copy used on key presses so the database is not queried for every key.</summary>
    public static HotKey[] GetCachedHotKeys() => cache ??= GetHotKeys();

    public static void Save(HotKey hotKey)
    {
        var hotKeys = GetHotKeys()
            .Where(existing => !string.Equals(existing.Name, hotKey.Name, StringComparison.OrdinalIgnoreCase))
            .Append(hotKey)
            .ToArray();
        new DatabaseController().GetDatabaseService().RecordManager(hotKeys);
        cache = null;
    }

    public static void Delete(string name)
    {
        var hotKeys = GetHotKeys()
            .Where(existing => !string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        new DatabaseController().GetDatabaseService().RecordManager(hotKeys);
        cache = null;
    }
}
