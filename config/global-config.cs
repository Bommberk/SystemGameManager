namespace SystemGameManager.Config;

using System;
using System.IO;
using System.Text.Json;

public class AppConfig
{
    public string AppName { get; set; } = "SystemGameManager";
    public string Version { get; set; } = "1.0.0";
    public string Environment { get; set; } = "Production";
    public string RepositoryUrl { get; set; } = "https://github.com/Bommberk/SystemGameManager";
    public string Author { get; set; } = "Bommberk";
    public string LogLevel { get; set; } = "Info";
    public string Language { get; set; } = "de-DE";
}

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = "Data Source=modules/database/systemgamemanager.db";
    public bool BackupEnabled { get; set; } = true;
}

public class GameManagerConfig
{
    public string[] LibraryPaths { get; set; } = Array.Empty<string>();
    public bool AutoUpdate { get; set; } = true;
    public bool ScanOnStartup { get; set; } = true;
}

public class AppSettings
{
    public AppConfig AppConfig { get; set; } = new();
    public DatabaseConfig DatabaseConfig { get; set; } = new();
    public GameManagerConfig GameManagerConfig { get; set; } = new();
}

public static class GlobalConfig
{
    private static AppSettings? _settings;
    
    // Pfad zur appsettings.json Datei
    private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "appsettings.json");

    // Eigenschaft zum Abrufen der Einstellungen
    public static AppSettings Settings
    {
        get
        {
            if (_settings == null)
            {
                Load();
            }
            return _settings!;
        }
    }

    // Explizite Funktion zum Abrufen der Einstellungen (wie von dir gewünscht)
    public static AppSettings GetSettings()
    {
        if (_settings == null)
        {
            Load();
        }
        return _settings!;
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string jsonString = File.ReadAllText(ConfigPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(jsonString) ?? new AppSettings();
                mlog("Konfiguration erfolgreich geladen.");
            }
            else
            {
                // Erstelle eine Standard-Konfiguration, falls keine existiert
                mlog("Keine Konfigurationsdatei gefunden. Standard-Konfiguration wird erstellt...");
                _settings = new AppSettings();
                Save();
            }
        }
        catch (Exception ex)
        {
            ConsoleError($"Fehler beim Laden der Konfiguration: {ex.Message}");
            _settings = new AppSettings(); // Fallback auf Standardwerte
        }
    }

    public static void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(ConfigPath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(ConfigPath, jsonString);
            mlog("Konfiguration erfolgreich gespeichert.");
        }
        catch (Exception ex)
        {
            ConsoleError($"Fehler beim Speichern der Konfiguration: {ex.Message}");
        }
    }
}
