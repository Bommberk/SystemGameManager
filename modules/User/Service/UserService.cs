namespace SystemGameManager.User.Service;

using SystemGameManager.Database.Controller;
using UserEntity = SystemGameManager.User.Entity.User;

internal sealed class UserService : DatabaseController
{
    private static readonly HashSet<string> SupportedThemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "dark",
        "light",
        "red",
        "pink",
        "yellow"
    };

    public UserEntity GetUser()
    {
        EnsureUserTable();

        using var command = dbConnection.CreateCommand();
        command.CommandText = $"SELECT [UserId], [Name], [Theme] FROM [{UserEntity.TABLE_NAME}] ORDER BY rowid LIMIT 1;";
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new UserEntity
            {
                UserId = Guid.Parse(reader.GetString(0)),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                Theme = reader.IsDBNull(2) ? null : reader.GetString(2)
            };
        }

        reader.Close();
        var user = new UserEntity { UserId = Guid.NewGuid() };
        using var insert = dbConnection.CreateCommand();
        insert.CommandText = $"INSERT INTO [{UserEntity.TABLE_NAME}] ([UserId], [Name], [Theme]) VALUES (@UserId, NULL, NULL);";
        insert.Parameters.AddWithValue("@UserId", user.UserId.ToString());
        insert.ExecuteNonQuery();
        return user;
    }

    public UserEntity UpdateName(string? name)
    {
        var user = GetUser();
        user.Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        Save(user, updateName: true);
        return user;
    }

    public UserEntity UpdateTheme(string? theme)
    {
        if (theme is null || !SupportedThemes.Contains(theme))
        {
            return GetUser();
        }

        var user = GetUser();
        user.Theme = SupportedThemes.First(supportedTheme =>
            string.Equals(supportedTheme, theme, StringComparison.OrdinalIgnoreCase));
        Save(user, updateName: false);
        return user;
    }

    private void Save(UserEntity user, bool updateName)
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = updateName
            ? $"UPDATE [{UserEntity.TABLE_NAME}] SET [Name] = @Value WHERE [UserId] = @UserId;"
            : $"UPDATE [{UserEntity.TABLE_NAME}] SET [Theme] = @Value WHERE [UserId] = @UserId;";
        command.Parameters.AddWithValue("@Value", updateName ? (object?)user.Name ?? DBNull.Value : user.Theme!);
        command.Parameters.AddWithValue("@UserId", user.UserId.ToString());
        command.ExecuteNonQuery();
    }

    private void EnsureUserTable()
    {
        using var command = dbConnection.CreateCommand();
        command.CommandText = $"""
            CREATE TABLE IF NOT EXISTS [{UserEntity.TABLE_NAME}] (
                [UserId] TEXT PRIMARY KEY NOT NULL,
                [Name] TEXT NULL,
                [Theme] TEXT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
}
