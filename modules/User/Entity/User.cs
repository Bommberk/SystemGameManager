namespace SystemGameManager.User.Entity;

using System.ComponentModel.DataAnnotations;
using SystemGameManager.Database.Controller;

public sealed class User
{
    public const string TABLE_NAME = "User";

    [Key]
    public Guid UserId { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public string? Theme { get; set; }

    public static User GetUser()
    {
        var databaseController = new DatabaseController();
        var user = databaseController.GetDatabaseService().GetTableRecords<User>(TABLE_NAME).FirstOrDefault();
        if (user is not null)
        {
            return user;
        }

        user = new User();
        SaveUser(user);
        return user;
    }

    public static void SaveUser(User user)
    {
        new DatabaseController().GetDatabaseService().RecordManager(user);
    }

    public static User UpdateName(string? name)
    {
        var user = GetUser();
        user.Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        SaveUser(user);
        return user;
    }

    public static User UpdateTheme(string? theme)
    {
        if (theme is null || !SupportedThemes.Contains(theme))
        {
            return GetUser();
        }

        var user = GetUser();
        user.Theme = SupportedThemes.First(supportedTheme =>
            string.Equals(supportedTheme, theme, StringComparison.OrdinalIgnoreCase));
        SaveUser(user);
        return user;
    }

    private static readonly HashSet<string> SupportedThemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "dark",
        "light",
        "red",
        "pink",
        "yellow"
    };
}
