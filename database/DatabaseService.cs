namespace Krassheiten.SystemGameManager.Service;

using Microsoft.Data.Sqlite;
using System.Collections;
using System.Reflection;

class DatabaseService
: DatabaseController
{
    private object? currentRecord;
    private PropertyInfo[] currentRecordProperties = [];
    private string currentRecordName = string.Empty;

    public void SaveNewRecord(object? record)
    {
        RecordManager(record);
    }

    public void RecordManager(object? record)
    {
        if (record is null) return;

        var tableName = GetTableName(record);
        var recordType = GetRecordType(record);
        if (string.IsNullOrWhiteSpace(tableName) || recordType is null) return;

        var properties = GetPersistedProperties(recordType);
        if (properties.Length == 0) return;

        CreateTable(tableName, BuildColumnDefinitions(properties));

        var incomingRecords = NormalizeToRecordList(record);
        var incomingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingRecords = GetExistingRecords(tableName);

        foreach (var entry in incomingRecords)
        {
            var name = GetRecordName(entry);
            if (string.IsNullOrWhiteSpace(name)) continue;

            currentRecord = entry;
            currentRecordProperties = properties;
            currentRecordName = name;
            incomingNames.Add(name);

            if (!existingRecords.TryGetValue(name, out var existingRecord))
            {
                CreateRecord(tableName);
                continue;
            }

            if (HasDifferentValues(existingRecord, entry, properties))
            {
                UpdateRecord(tableName);
            }
        }

        foreach (var dbName in existingRecords.Keys.Where(dbName => !incomingNames.Contains(dbName)))
        {
            currentRecord = null;
            currentRecordProperties = properties;
            currentRecordName = dbName;
            DeleteRecord(tableName);
        }
    }

    private void CreateRecord(string tableName)
    {
        if (currentRecord is null || currentRecordProperties.Length == 0) return;

        using var command = dbConnection.CreateCommand();
        var columnNames = string.Join(", ", currentRecordProperties.Select(property => $"[{property.Name}]"));
        var parameterNames = string.Join(", ", currentRecordProperties.Select(property => $"@{property.Name}"));

        command.CommandText = $"INSERT INTO [{tableName}] ({columnNames}) VALUES ({parameterNames});";
        AddParametersFromCurrentRecord(command);
        command.ExecuteNonQuery();
    }

    private void UpdateRecord(string tableName)
    {
        if (currentRecord is null || currentRecordProperties.Length == 0 || string.IsNullOrWhiteSpace(currentRecordName)) return;

        using var command = dbConnection.CreateCommand();
        var setClause = string.Join(", ", currentRecordProperties
            .Where(property => !property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
            .Select(property => $"[{property.Name}] = @{property.Name}"));

        if (string.IsNullOrWhiteSpace(setClause)) return;

        command.CommandText = $"UPDATE [{tableName}] SET {setClause} WHERE [Name] = @OriginalName;";
        AddParametersFromCurrentRecord(command, skipName: true);
        command.Parameters.AddWithValue("@OriginalName", currentRecordName);
        command.ExecuteNonQuery();
    }

    private void DeleteRecord(string tableName)
    {
        if (string.IsNullOrWhiteSpace(currentRecordName)) return;

        using var command = dbConnection.CreateCommand();
        command.CommandText = $"DELETE FROM [{tableName}] WHERE [Name] = @Name;";
        command.Parameters.AddWithValue("@Name", currentRecordName);
        command.ExecuteNonQuery();
    }

    public void CreateTable(string tableName, string[] columns)
    {
        using var command = dbConnection.CreateCommand();
        var columnDefinitions = string.Join(",\r\n", columns);
        command.CommandText = $@"
            CREATE TABLE IF NOT EXISTS [{tableName}] (
                {columnDefinitions}
            );";
        command.ExecuteNonQuery();
    }

    private void AddParametersFromCurrentRecord(SqliteCommand command, bool skipName = false)
    {
        if (currentRecord is null) return;

        foreach (var property in currentRecordProperties)
        {
            if (skipName && property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
                continue;

            command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(currentRecord) ?? DBNull.Value);
        }
    }

    private Dictionary<string, Dictionary<string, object?>> GetExistingRecords(string tableName)
    {
        var existingRecords = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

        using var command = dbConnection.CreateCommand();
        command.CommandText = $"SELECT * FROM [{tableName}];";
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                values[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            var name = values.TryGetValue("Name", out var rawName) ? rawName?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(name))
            {
                existingRecords[name] = values;
            }
        }

        return existingRecords;
    }

    private static bool HasDifferentValues(IReadOnlyDictionary<string, object?> existingRecord, object newRecord, PropertyInfo[] properties)
    {
        foreach (var property in properties)
        {
            existingRecord.TryGetValue(property.Name, out var databaseValue);
            var recordValue = property.GetValue(newRecord);

            if (!Equals(NormalizeValue(databaseValue), NormalizeValue(recordValue)))
            {
                return true;
            }
        }

        return false;
    }

    private static object? NormalizeValue(object? value)
    {
        return value switch
        {
            null => null,
            DBNull => null,
            bool boolValue => boolValue ? 1L : 0L,
            byte or sbyte or short or ushort or int or uint or long or ulong => Convert.ToInt64(value),
            float or double or decimal => Convert.ToDouble(value),
            _ => value.ToString()
        };
    }

    private static string[] BuildColumnDefinitions(PropertyInfo[] properties)
    {
        var columns = new List<string>
        {
            "Id INTEGER PRIMARY KEY AUTOINCREMENT"
        };

        foreach (var property in properties)
        {
            var columnType = GetSqlType(property.PropertyType);
            var columnDefinition = property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase)
                ? $"[{property.Name}] {columnType} NOT NULL UNIQUE"
                : $"[{property.Name}] {columnType}";

            columns.Add(columnDefinition);
        }

        return [.. columns];
    }

    private static string GetSqlType(Type propertyType)
    {
        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(bool) ||
            type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(long) ||
            type == typeof(ulong))
        {
            return "INTEGER";
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            return "REAL";
        }

        return "TEXT";
    }

    private static PropertyInfo[] GetPersistedProperties(Type recordType)
    {
        return [.. recordType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite)
            .OrderBy(property => property.MetadataToken)];
    }

    private static List<object> NormalizeToRecordList(object record)
    {
        if (record is IEnumerable enumerable && record is not string)
        {
            var records = new List<object>();
            foreach (var entry in enumerable)
            {
                if (entry is not null)
                {
                    records.Add(entry);
                }
            }

            return records;
        }

        return [record];
    }

    private static Type? GetRecordType(object record)
    {
        var recordType = record.GetType();

        if (recordType.IsArray)
        {
            return recordType.GetElementType();
        }

        if (record is IEnumerable && record is not string && recordType.IsGenericType)
        {
            return recordType.GetGenericArguments().FirstOrDefault();
        }

        return recordType;
    }

    private static string? GetRecordName(object record)
    {
        return record.GetType()
            .GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(record)
            ?.ToString();
    }

    private string? GetTableName(object record)
    {
        var recordType = GetRecordType(record);
        var tableOwnerType = recordType?.DeclaringType ?? recordType;

        if (tableOwnerType is null) return null;

        var tableNameField = tableOwnerType.GetField("DEFAULT_TABLE_NAME", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (tableNameField is null || tableNameField.FieldType != typeof(string) || !tableNameField.IsLiteral) return null;

        var tableNameValue = tableNameField.GetRawConstantValue() as string;
        if (string.IsNullOrWhiteSpace(tableNameValue)) return null;

        return tableNameValue;
    }
}