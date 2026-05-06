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
    
    private static readonly object _dbInitLock = new();

    protected static SqliteConnection GetSqlConnection()
    {
        const string dbFile = "database/systemgamemanager.db";
        const string templateFile = "database/template-systemgamemanager.db";

        lock (_dbInitLock)
        {
            if (!File.Exists(dbFile) && File.Exists(templateFile))
            {
                File.Copy(templateFile, dbFile);
            }
            else if (File.Exists(dbFile) && File.Exists(templateFile))
            {
                SyncSchemaFromTemplate(dbFile, templateFile);
            }
        }

        var connection = new SqliteConnection($"Data Source={dbFile}");
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=DELETE;";
        pragma.ExecuteNonQuery();

        return connection;
    }

    private static void SyncSchemaFromTemplate(string dbFile, string templateFile)
    {
        var templateTables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using (var templateConn = new SqliteConnection($"Data Source={templateFile};Mode=ReadOnly"))
        {
            templateConn.Open();
            using var cmd = templateConn.CreateCommand();
            cmd.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                templateTables[reader.GetString(0)] = reader.GetString(1);
            }
        }

        if (templateTables.Count == 0) return;

        using var mainConn = new SqliteConnection($"Data Source={dbFile}");
        mainConn.Open();

        var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = mainConn.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                existingTables.Add(reader.GetString(0));
            }
        }

        foreach (var (tableName, createSql) in templateTables)
        {
            if (existingTables.Contains(tableName)) continue;

            using var cmd = mainConn.CreateCommand();
            cmd.CommandText = createSql;
            cmd.ExecuteNonQuery();
        }
    }

    public DatabaseService GetDatabaseService()
    {
        return new DatabaseService();
    }
}