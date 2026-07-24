namespace SystemGameManager.Config;

class GlobalDevConfig
{
    public GlobalDevConfig()
    {
        GlobalConfig.Settings.AppConfig.Environment = "dev";
        GlobalConfig.Settings.DatabaseConfig.DatabaseFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)?.FullName ?? "systemgamemanager.db", "Datenbank", "systemgamemanager.db");
    }
}