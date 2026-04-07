namespace Krassheiten.SystemGameManager.Controller;

using Microsoft.Data.Sqlite;
using Krassheiten.SystemGameManager.Service;

class DatabaseController
{
    protected SqliteConnection dbConnection;

    public DatabaseController()
    {
        dbConnection = GetSqlConnection();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = GetSqlConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Games (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                InstallFolderPath TEXT NOT NULL UNIQUE,
                ExePath TEXT NOT NULL,
                GameVolumePercent INTEGER NOT NULL,
                MusicVolumePercent INTEGER NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
    
    protected static SqliteConnection GetSqlConnection()
    {
        string dbPath = "Data Source=systemgamemanager.db";
        var connection = new SqliteConnection(dbPath);
        connection.Open();
        return connection;
    }

    public DatabaseService GetDatabaseService()
    {
        return new DatabaseService();
    }
}