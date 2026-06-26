namespace SystemGameManager.Games.Entity;

class Launcher
{
    public const string TABLE_NAME = "Launcher";
    public static readonly string[] RegistryUninstallPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    ];
    public static Launcher[]? InstalledLaunchers { get; set; }
    public static Launcher[]? KnownLaunchers { get; set; }

    public string Name { get; set; }
    public string SearchName { get; set; }
    public string StdInstallPath { get; set; }
    public string InstallPath { get; set; }
    public string StdGameFoldersPath { get; set; }
    public string[]? GameFolderPath { get; set; }
    public string? StdLibraryFilePath { get; set; }
    public string? DirectRegistryKey { get; set; }

    public Launcher(string name, string searchName, string stdInstallPath, string installPath, string stdGameFoldersPath, string[]? gameFolderPath = null, string? stdLibraryFilePath = null, string? directRegistryKey = null)
    {
        Name = name;
        SearchName = searchName;
        StdInstallPath = stdInstallPath;
        InstallPath = installPath;
        StdGameFoldersPath = stdGameFoldersPath;
        GameFolderPath = gameFolderPath;
        StdLibraryFilePath = stdLibraryFilePath;
        DirectRegistryKey = directRegistryKey;
    }

    public static void WriteLaunchersFromDatabase()
    {
        var databaseController = new DatabaseController();
        databaseController.ShowTable(TABLE_NAME);
    }
    public static Launcher[] GetLaunchers()
    {
        var databaseController = new DatabaseController();
        var launchers = databaseController.GetDatabaseService().GetTableRecords<Launcher>(TABLE_NAME);
        return launchers ?? Array.Empty<Launcher>();
    }
}