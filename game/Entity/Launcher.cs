using System.Reflection.Metadata;

namespace Krassheiten.SystemGameManager.Entity;

class Launcher
{
    public const string DEFAULT_TABLE_NAME = "Launchers";
    public static readonly string[] RegistryUninstallPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    ];
    public static Record[]? InstalledLaunchers { get; set; }
    public static Record[]? KnownLaunchers { get; set; }

    public class Record
    {
        public string DisplayName { get; set; }
        public string SearchName { get; set; }
        public string StdInstallPath { get; set; }
        public string InstallPath { get; set; }
        public string StdGameFoldersPath { get; set; }
        public string GameFolderPath { get; set; }
        public string? StdLibraryFilePath { get; set; }
        public string? DirectRegistryKey { get; set; }

        public Record(string displayName, string searchName, string stdInstallPath, string installPath, string stdGameFoldersPath, string gameFolderPath, string? stdLibraryFilePath = null, string? directRegistryKey = null)
        {
            DisplayName = displayName;
            SearchName = searchName;
            StdInstallPath = stdInstallPath;
            InstallPath = installPath;
            StdGameFoldersPath = stdGameFoldersPath;
            GameFolderPath = gameFolderPath;
            StdLibraryFilePath = stdLibraryFilePath;
            DirectRegistryKey = directRegistryKey;
        }
    }
}