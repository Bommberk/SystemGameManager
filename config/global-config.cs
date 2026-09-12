namespace SystemGameManager.Config;

using System;
using System.IO;

public class AppConfig
{
    public string AppName { get; set; } = "SystemGameManager";
    public string Version { get; set; } = "0.6.5";
    public string Environment { get; set; } = "production";
    public string RepositoryUrl { get; set; } = "https://github.com/Bommberk/SystemGameManager";
    public string Author { get; set; } = "Krassheiten";
    public string LogLevel { get; set; } = "Info";
    public string Language { get; set; } = "de-DE";
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = "Data Source=modules/database/systemgamemanager.db";
    public string DatabaseFile { get; set; } = Path.Combine(GetAppdataPath(),"systemgamemanager.db");
    public bool BackupEnabled { get; set; } = true;
}

public class GameManagerConfig
{
    public string[] LibraryPaths { get; set; } = Array.Empty<string>();
    public bool AutoUpdate { get; set; } = true;
    public bool ScanOnStartup { get; set; } = true;
}

public class SmarthomeApiConfig
{
    public string ApiUrl { get; set; } = "https://api.smarthome.krassheiten.de";
    public string ApiKey { get; set; } = "testtoken123";
}

public class SpeechDetectionConfig
{
    // AccessKey wird kostenlos über die Picovoice Console (https://console.picovoice.ai/) erstellt
    // und wird von Cobra zur Initialisierung benötigt. Ohne gültigen AccessKey bleibt die
    // Spracherkennung deaktiviert (isCurrentlySpeech bleibt dauerhaft false).
    public string AccessKey { get; set; } = "";
}

public class AppSettings
{
    public AppConfig AppConfig { get; set; } = new();
    public DatabaseConfig DatabaseConfig { get; set; } = new();
    public GameManagerConfig GameManagerConfig { get; set; } = new();
    public SmarthomeApiConfig SmarthomeApiConfig { get; set; } = new();
    public SpeechDetectionConfig SpeechDetectionConfig { get; set; } = new();
}

public static class GlobalConfig
{
    public static AppSettings Settings { get; } = new();
}
