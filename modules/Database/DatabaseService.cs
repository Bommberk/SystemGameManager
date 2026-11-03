namespace SystemGameManager.Database.Service;

using SystemGameManager.Database.Controller;
using Microsoft.Data.Sqlite;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

class DatabaseService
: DatabaseController
{
    private object? currentRecord;
    private PropertyInfo[] currentRecordProperties = [];
    private string currentRecordKeyColumn = "Name";
    private string currentRecordKey = string.Empty;

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
        var keyProperty = GetRecordKeyProperty(recordType);
        if (keyProperty is null) return;

        CreateTable(tableName, BuildColumnDefinitions(properties, keyProperty));
        AddMissingColumns(tableName, properties);

        var incomingRecords = NormalizeToRecordList(record);
        var incomingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingRecords = GetExistingRecords(tableName, keyProperty.Name);

        foreach (var entry in incomingRecords)
        {
            var key = GetRecordKey(entry, keyProperty);
            if (string.IsNullOrWhiteSpace(key)) continue;

            currentRecord = entry;
            currentRecordProperties = properties;
            currentRecordKeyColumn = keyProperty.Name;
            currentRecordKey = key;
            incomingKeys.Add(key);

            if (!existingRecords.TryGetValue(key, out var existingRecord))
            {
                CreateRecord(tableName);
                continue;
            }

            if (existingRecord.TryGetValue(keyProperty.Name, out var existingKey))
            {
                currentRecordKey = existingKey?.ToString() ?? currentRecordKey;
            }

            if (HasDifferentValues(existingRecord, entry, properties))
            {
                UpdateRecord(tableName);
            }
        }

        foreach (var dbKey in existingRecords.Keys.Where(dbKey => !incomingKeys.Contains(dbKey)))
        {
            currentRecord = null;
            currentRecordProperties = properties;
            currentRecordKeyColumn = keyProperty.Name;
            currentRecordKey = dbKey;
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
        if (currentRecord is null || currentRecordProperties.Length == 0 || string.IsNullOrWhiteSpace(currentRecordKey)) return;

        using var command = dbConnection.CreateCommand();
        var setClause = string.Join(", ", currentRecordProperties
            .Where(property => !property.Name.Equals(currentRecordKeyColumn, StringComparison.OrdinalIgnoreCase))
            .Select(property => $"[{property.Name}] = @{property.Name}"));

        if (string.IsNullOrWhiteSpace(setClause)) return;

        command.CommandText = $"UPDATE [{tableName}] SET {setClause} WHERE [{currentRecordKeyColumn}] = @OriginalKey;";
        AddParametersFromCurrentRecord(command, skipKey: true);
        command.Parameters.AddWithValue("@OriginalKey", currentRecordKey);
        command.ExecuteNonQuery();
    }

    public void UpdateRecordByName(string tableName, string recordName, object updatedRecord)
    {
        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(recordName) || updatedRecord is null)
            return;

        var recordType = GetRecordType(updatedRecord);
        if (recordType is null) return;

        var properties = GetPersistedProperties(recordType);
        if (properties.Length == 0) return;

        currentRecord = updatedRecord;
        currentRecordProperties = properties;
        currentRecordKeyColumn = "Name";
        currentRecordKey = recordName;

        UpdateRecord(tableName);
    }

    private void DeleteRecord(string tableName)
    {
        if (string.IsNullOrWhiteSpace(currentRecordKey)) return;

        using var command = dbConnection.CreateCommand();
        command.CommandText = $"DELETE FROM [{tableName}] WHERE [{currentRecordKeyColumn}] = @Key;";
        command.Parameters.AddWithValue("@Key", currentRecordKey);
        command.ExecuteNonQuery();
    }

    public void CreateTable(string tableName, string[] columns)
    {
        dump($"Creating table '{tableName}'");
        using var command = dbConnection.CreateCommand();
        var columnDefinitions = string.Join(",\r\n", columns);
        command.CommandText = $@"
            CREATE TABLE IF NOT EXISTS [{tableName}] (
                {columnDefinitions}
            );";
        command.ExecuteNonQuery();
    }

    private void AddMissingColumns(string tableName, PropertyInfo[] properties)
    {
        var existingColumns = new HashSet<string>(GetTableColumns<object>(tableName), StringComparer.OrdinalIgnoreCase);

        foreach (var property in properties)
        {
            if (existingColumns.Contains(property.Name))
            {
                continue;
            }

            var columnType = GetSqlType(property.PropertyType);
            using var command = dbConnection.CreateCommand();
            command.CommandText = $"ALTER TABLE [{tableName}] ADD COLUMN [{property.Name}] {columnType};";
            command.ExecuteNonQuery();
        }
    }

    public string[] GetTableColumns<T>(string tableName)
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = $"PRAGMA table_info([{tableName}]);";
        using var reader = command.ExecuteReader();

        var columns = new List<string>();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1)); // Column name is in the second column (index 1)
        }

        return columns.ToArray();
    }

    public T[] GetTableRecords<T>(string tableName)
    {
        if (!TableExists(tableName)) return [];

        using var command = dbConnection.CreateCommand();
        command.CommandText = $"SELECT * FROM [{tableName}];";
        using var reader = command.ExecuteReader();

        var records = new List<T>();
        while (reader.Read())
        {
            var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                values[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            var record = CreateRecord<T>(values);
            if (record is not null)
            {
                records.Add(record);
            }
        }

        return [.. records];
    }

    private void AddParametersFromCurrentRecord(SqliteCommand command, bool skipKey = false)
    {
        if (currentRecord is null) return;

        foreach (var property in currentRecordProperties)
        {
            if (skipKey && property.Name.Equals(currentRecordKeyColumn, StringComparison.OrdinalIgnoreCase))
                continue;

            command.Parameters.AddWithValue($"@{property.Name}", SerializeForStorage(property.GetValue(currentRecord)));
        }
    }

    private static object SerializeForStorage(object? value)
    {
        if (value is null)
        {
            return DBNull.Value;
        }

        return value.GetType().IsArray
            ? JsonSerializer.Serialize(value)
            : value;
    }

    private Dictionary<string, Dictionary<string, object?>> GetExistingRecords(string tableName, string keyColumn)
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

            var key = values.TryGetValue(keyColumn, out var rawKey) ? rawKey?.ToString() : null;
            if (!string.IsNullOrWhiteSpace(key))
            {
                existingRecords[key] = values;
            }
        }

        return existingRecords;
    }

    private bool TableExists(string tableName)
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = @TableName LIMIT 1;";
        command.Parameters.AddWithValue("@TableName", tableName);
        return command.ExecuteScalar() is not null;
    }

    private static T? CreateRecord<T>(IReadOnlyDictionary<string, object?> values)
    {
        var recordType = typeof(T);
        var constructors = recordType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(constructor => constructor.GetParameters().Length)
            .ToArray();

        object? instance = null;
        foreach (var constructor in constructors)
        {
            try
            {
                var parameters = constructor.GetParameters()
                    .Select(parameter =>
                    {
                        values.TryGetValue(parameter.Name ?? string.Empty, out var value);
                        return ConvertValue(value, parameter.ParameterType);
                    })
                    .ToArray();

                instance = constructor.Invoke(parameters);
                break;
            }
            catch
            {
            }
        }

        instance ??= Activator.CreateInstance(recordType, nonPublic: true);
        if (instance is not T record)
        {
            return default;
        }

        foreach (var property in GetPersistedProperties(recordType))
        {
            if (!values.TryGetValue(property.Name, out var value))
            {
                continue;
            }

            property.SetValue(record, ConvertValue(value, property.PropertyType));
        }

        return record;
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        var effectiveType = underlyingType ?? targetType;

        if (value is null or DBNull)
        {
            if (effectiveType.IsArray)
            {
                return CreateEmptyArray(effectiveType);
            }

            return underlyingType is not null || !effectiveType.IsValueType
                ? null
                : Activator.CreateInstance(effectiveType);
        }

        if (effectiveType.IsArray)
        {
            if (value is string json)
            {
                try
                {
                    return JsonSerializer.Deserialize(json, effectiveType) ?? CreateEmptyArray(effectiveType);
                }
                catch
                {
                    return CreateEmptyArray(effectiveType);
                }
            }

            return CreateEmptyArray(effectiveType);
        }

        if (effectiveType == typeof(string))
        {
            return value.ToString() ?? string.Empty;
        }

        if (effectiveType == typeof(Guid))
        {
            return Guid.Parse(value.ToString()!);
        }

        if (effectiveType == typeof(bool))
        {
            return value switch
            {
                bool boolValue => boolValue,
                string text when bool.TryParse(text, out var parsedBool) => parsedBool,
                _ => Convert.ToInt64(value) != 0
            };
        }

        if (effectiveType.IsEnum)
        {
            return value is string text
                ? Enum.Parse(effectiveType, text, ignoreCase: true)
                : Enum.ToObject(effectiveType, value);
        }

        return Convert.ChangeType(value, effectiveType);
    }

    private static Array CreateEmptyArray(Type arrayType)
    {
        return Array.CreateInstance(arrayType.GetElementType()!, 0);
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
            Array arrayValue => JsonSerializer.Serialize(arrayValue),
            bool boolValue => boolValue ? 1L : 0L,
            byte or sbyte or short or ushort or int or uint or long or ulong => Convert.ToInt64(value),
            float or double or decimal => Convert.ToDouble(value),
            _ => value.ToString()
        };
    }

    private static string[] BuildColumnDefinitions(PropertyInfo[] properties, PropertyInfo keyProperty)
    {
        var columns = new List<string>
        {
            "Id INTEGER PRIMARY KEY AUTOINCREMENT"
        };

        foreach (var property in properties)
        {
            var columnType = GetSqlType(property.PropertyType);
            var columnDefinition = property.Name.Equals(keyProperty.Name, StringComparison.OrdinalIgnoreCase)
                ? $"[{property.Name}] {columnType} NOT NULL UNIQUE"
                : property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) && !IsNullable(property)
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

    private static PropertyInfo? GetRecordKeyProperty(Type recordType)
    {
        return recordType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(property => property.GetCustomAttribute<KeyAttribute>() is not null)
            ?? recordType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
    }

    private static string? GetRecordKey(object record, PropertyInfo keyProperty)
    {
        return keyProperty.GetValue(record) switch
        {
            Guid guid => guid.ToString("D").ToUpperInvariant(),
            var value => value?.ToString()
        };
    }

    private static bool IsNullable(PropertyInfo property)
    {
        return Nullable.GetUnderlyingType(property.PropertyType) is not null
            || new NullabilityInfoContext().Create(property).WriteState == NullabilityState.Nullable;
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

    private string? GetTableName(object record)
    {
        var recordType = GetRecordType(record);
        var tableOwnerType = recordType?.DeclaringType ?? recordType;

        if (tableOwnerType is null) return null;

        var fieldNames = new[] { "TABLE_NAME", "DEFAULT_TABLE_NAME" };
        foreach (var fieldName in fieldNames)
        {
            var tableNameField = tableOwnerType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (tableNameField is null || tableNameField.FieldType != typeof(string) || !tableNameField.IsLiteral)
            {
                continue;
            }

            var tableNameValue = tableNameField.GetRawConstantValue() as string;
            if (!string.IsNullOrWhiteSpace(tableNameValue))
            {
                return tableNameValue;
            }
        }

        return null;
    }
}