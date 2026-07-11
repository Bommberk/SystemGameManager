namespace SystemGameManager.Database.Controller;

using System.Reflection;
using Microsoft.Data.Sqlite;
using SystemGameManager.Database.Service;
using SystemGameManager.Games.Entity;

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
        var dbFile = Path.Combine(AppContext.BaseDirectory, "modules", "database", "systemgamemanager.db");
        var templateFile = Path.Combine(AppContext.BaseDirectory, "modules", "database", "template-systemgamemanager.db");

        var dbDirectory = Path.GetDirectoryName(dbFile);
        if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
        {
            Directory.CreateDirectory(dbDirectory);
        }

        lock (_dbInitLock)
        {
            if (File.Exists(templateFile))
            {
                SyncEntitiesToTemplate(templateFile);
            }

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

    private static void SyncEntitiesToTemplate(string templateFile)
    {
        var entityTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.Name == "Record" && t.IsClass && t.DeclaringType != null)
            .Select(t => new
            {
                RecordType = t,
                TableName = t.DeclaringType!
                    .GetField("TABLE_NAME", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    ?.GetRawConstantValue() as string
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.TableName))
            .ToList();

        if (entityTypes.Count == 0) return;

        using var conn = new SqliteConnection($"Data Source={templateFile}");
        conn.Open();

        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=DELETE;";
        pragma.ExecuteNonQuery();

        foreach (var entity in entityTypes)
        {
            var properties = entity.RecordType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .OrderBy(p => p.MetadataToken)
                .ToArray();

            if (properties.Length == 0) continue;

            var columns = new List<string> { "Id INTEGER PRIMARY KEY AUTOINCREMENT" };
            foreach (var property in properties)
            {
                var colType = GetEntitySqlType(property.PropertyType);
                var colDef = property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)
                    ? $"[{property.Name}] {colType} NOT NULL UNIQUE"
                    : $"[{property.Name}] {colType}";
                columns.Add(colDef);
            }

            using (var cmd = conn.CreateCommand())
            {
                var columnDefinitions = string.Join(", ", columns);
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS [{entity.TableName}] ({columnDefinitions});";
                cmd.ExecuteNonQuery();
            }

            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info([{entity.TableName}]);";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    existingColumns.Add(reader.GetString(1));
                }
            }

            foreach (var property in properties)
            {
                if (existingColumns.Contains(property.Name)) continue;

                var colType = GetEntitySqlType(property.PropertyType);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"ALTER TABLE [{entity.TableName}] ADD COLUMN [{property.Name}] {colType};";
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static string GetEntitySqlType(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(bool) || type == typeof(byte) || type == typeof(sbyte) ||
            type == typeof(short) || type == typeof(ushort) || type == typeof(int) ||
            type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
        {
            return "INTEGER";
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return "REAL";
        }

        return "TEXT";
    }

    private static void SyncSchemaFromTemplate(string dbFile, string templateFile)
    {
        var templateSchema = new Dictionary<string, (string CreateSql, List<(string Name, string Type)> Columns)>(StringComparer.OrdinalIgnoreCase);

        using (var templateConn = new SqliteConnection($"Data Source={templateFile};Mode=ReadOnly"))
        {
            templateConn.Open();

            var tableNames = new List<(string Name, string Sql)>();
            using (var cmd = templateConn.CreateCommand())
            {
                cmd.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    tableNames.Add((reader.GetString(0), reader.GetString(1)));
                }
            }

            foreach (var (name, sql) in tableNames)
            {
                var columns = new List<(string Name, string Type)>();
                using var colCmd = templateConn.CreateCommand();
                colCmd.CommandText = $"PRAGMA table_info([{name}]);";
                using var colReader = colCmd.ExecuteReader();
                while (colReader.Read())
                {
                    columns.Add((colReader.GetString(1), colReader.GetString(2)));
                }
                templateSchema[name] = (sql, columns);
            }
        }

        if (templateSchema.Count == 0) return;

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

        foreach (var (tableName, (createSql, templateColumns)) in templateSchema)
        {
            if (!existingTables.Contains(tableName))
            {
                using var cmd = mainConn.CreateCommand();
                cmd.CommandText = createSql;
                cmd.ExecuteNonQuery();
            }
            else
            {
                var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = mainConn.CreateCommand())
                {
                    cmd.CommandText = $"PRAGMA table_info([{tableName}]);";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        existingColumns.Add(reader.GetString(1));
                    }
                }

                foreach (var (colName, colType) in templateColumns)
                {
                    if (existingColumns.Contains(colName)) continue;

                    using var cmd = mainConn.CreateCommand();
                    cmd.CommandText = $"ALTER TABLE [{tableName}] ADD COLUMN [{colName}] {colType};";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public DatabaseService GetDatabaseService()
    {
        return new DatabaseService();
    }
}