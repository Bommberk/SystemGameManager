namespace Krassheiten.Game.Entity;

class Launcher
{
    public static readonly string[] RegistryUninstallPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    ];
    public static Record[]? InstalledLaunchers { get; set; }
    public static Record[]? KnownLaunchers { get; set; }

    public record Record
    (
        string DisplayName,
        string SearchName,
        string StdInstallPath,
        string InstallPath,
        string StdGameFoldersPath,
        string StdLibraryFilePath,
        string GameFolderPath,
        string? DirectRegistryKey = null
    );
}