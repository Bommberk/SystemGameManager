namespace SystemGameManager.User.Entity;

public sealed class User
{
    public const string TABLE_NAME = "User";

    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Theme { get; set; }
}
