namespace Krassheiten.SystemGameManager.Controller;

using Microsoft.Data.Sqlite;
using Krassheiten.SystemGameManager.Service;
using Krassheiten.SystemGameManager.Entity;

class DatabaseController
{
    protected SqliteConnection dbConnection;

    public DatabaseController()
    {
        dbConnection = GetSqlConnection();
    }
    public void ShowAllDatabases()
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
        using var reader = command.ExecuteReader();
        Console.WriteLine("Tables in the database:");
        while (reader.Read())        {
            Console.WriteLine($"- {reader.GetString(0)}");
        }
    }

    public void ShowTable(string tableName)
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = $"SELECT * FROM [{tableName}];";
        using var reader = command.ExecuteReader();

        var rows = new List<Dictionary<string, object?>>();
        while (reader.Read())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            rows.Add(row);
        }

        Console.WriteLine($"{tableName} in the database:");
        dump(rows);
    }
    
    protected static SqliteConnection GetSqlConnection()
    {
        string dbPath = "Data Source=database/systemgamemanager.db";
        var connection = new SqliteConnection(dbPath);
        connection.Open();
        return connection;
    }

    public DatabaseService GetDatabaseService()
    {
        return new DatabaseService();
    }
}