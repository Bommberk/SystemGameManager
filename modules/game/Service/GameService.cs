namespace SystemGameManager.Games.Service;

using SystemGameManager.Games.Entity;
using Microsoft.Win32;
using System.Text.Json;
using System.Drawing;
using System.Text.RegularExpressions;
using Windows.Management.Deployment;

class GameService
{
    private static readonly Regex SteamInstallDirectoryPattern = new(
        "\"installdir\"\\s+\"(?<installDirectory>[^\"]+)\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public void SetInstalledGames()
    {
        var gamesFromFolders = GetGamesWithGameFolder();
        var gamesFromRegistry = GetGamesWithRegistry();
        var minecraftGames = GetMinecraftGames();
        
        var allGames = gamesFromFolders.Concat(gamesFromRegistry)
                                       .Concat(minecraftGames)
                                       .DistinctBy(game => game.InstallFolderPath, StringComparer.OrdinalIgnoreCase)
                                       .ToArray();
                                       
        Game.InstalledGames = allGames;
        SetGamesImage();
        ApplySavedSettings();
        Game.SaveGames();
    }

    private IEnumerable<Game> GetGamesWithGameFolder()
    {
        List<Game> games = new();
        if(Launcher.InstalledLaunchers is null) return games;
        
        foreach (var launcher in Launcher.InstalledLaunchers)
        {
            if (launcher.GameFolderPath is null || launcher.GameFolderPath.Length == 0)
                continue;

            foreach (var gameFolder in launcher.GameFolderPath.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(gameFolder))
                    continue;

                string[] gameDirs = Directory.GetDirectories(gameFolder);
                foreach (var gameDir in gameDirs)
                {
                    bool hasExe;
                    try
                    {
                        hasExe = Directory.EnumerateFiles(gameDir, "*.exe", SearchOption.AllDirectories).Any();
                    }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (IOException) { continue; }

                    if (!hasExe)
                        continue;

                    string gameName = Path.GetFileName(gameDir);
                    string serializedGameName = new string(gameName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray()).ToLower();
                    string exePath = GetGameExe(gameDir);
                    games.Add(new Game(gameName, serializedGameName, gameDir, exePath));
                }
            }
        }
        return games;
    }

    private IEnumerable<Game> GetGamesWithRegistry()
    {
        List<Game> games = new();
        if(Launcher.InstalledLaunchers is null) return games;
        
        foreach(var launcher in Launcher.InstalledLaunchers)
        {
            string? registryKeyPath = launcher.DirectRegistryKey;
            if (string.IsNullOrEmpty(registryKeyPath))
                continue;

            using var key = Registry.LocalMachine.OpenSubKey(registryKeyPath);
            if (key == null) continue;

            string[] gameNames = key.GetSubKeyNames();
            foreach (var gameName in gameNames)
            {
                using var subKey = key.OpenSubKey(gameName);
                if (subKey == null) continue;

                string? installPath = null;
                foreach (var keyName in RegistryInstallKeyNames)
                {
                    if (!string.IsNullOrEmpty(installPath)) break;
                    installPath = subKey.GetValue(keyName) as string;
                }

                if (!string.IsNullOrEmpty(installPath))
                {
                    string pathName = Path.GetFileName(installPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    var resolvedGameName = int.TryParse(gameName, out _)
                        ? pathName
                        : gameName;

                    string exePath = GetGameExe(installPath);
                    string processName = GetProcessName(resolvedGameName, exePath);
                    string serializedGameName = new string(resolvedGameName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray()).ToLower();   
                    games.Add(new Game(resolvedGameName, serializedGameName, installPath, exePath, processName));
                }
            }
        }
        return games;
    }

    private static readonly Dictionary<string, string> MinecraftStoreTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Microsoft.MinecraftUWP"] = "Minecraft: Bedrock Edition",
        ["Microsoft.Lovika"] = "Minecraft Dungeons",
        ["Microsoft.Studios.Odin"] = "Minecraft Legends",
        ["Microsoft.MinecraftEducationEdition"] = "Minecraft Education"
    };

    private static string DefaultMinecraftDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ".minecraft");

    private IEnumerable<Game> GetMinecraftGames()
    {
        return GetMinecraftJavaGames().Concat(GetMinecraftStoreGames());
    }

    private static IEnumerable<Game> GetMinecraftJavaGames()
    {
        var gameDirectories = new[] { DefaultMinecraftDirectory }
            .Concat(GetMinecraftProfileDirectories())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var gameDirectory in gameDirectories)
        {
            if (!HasMinecraftVersions(gameDirectory))
                continue;

            bool isDefaultDirectory = string.Equals(gameDirectory, DefaultMinecraftDirectory, StringComparison.OrdinalIgnoreCase);
            string gameName = isDefaultDirectory
                ? "Minecraft: Java Edition"
                : $"Minecraft: Java Edition ({Path.GetFileName(gameDirectory)})";

            yield return new Game(gameName, SerializeGameName(gameName), gameDirectory, string.Empty, "javaw");
        }
    }

    // Launcher profiles can point to their own game folder via "gameDir"
    private static IEnumerable<string> GetMinecraftProfileDirectories()
    {
        string launcherProfilesPath = Path.Combine(DefaultMinecraftDirectory, "launcher_profiles.json");
        if (!File.Exists(launcherProfilesPath))
            return [];

        var directories = new List<string>();
        try
        {
            using var profilesDocument = JsonDocument.Parse(File.ReadAllText(launcherProfilesPath));
            if (!profilesDocument.RootElement.TryGetProperty("profiles", out var profiles)
                || profiles.ValueKind != JsonValueKind.Object)
            {
                return directories;
            }

            foreach (var profile in profiles.EnumerateObject())
            {
                if (profile.Value.ValueKind != JsonValueKind.Object
                    || !profile.Value.TryGetProperty("gameDir", out var gameDirectory)
                    || gameDirectory.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                string? directory = gameDirectory.GetString();
                if (string.IsNullOrWhiteSpace(directory))
                    continue;

                directories.Add(Path.IsPathRooted(directory)
                    ? directory
                    : Path.Combine(DefaultMinecraftDirectory, directory));
            }
        }
        catch (JsonException exception)
        {
            mlog($"Minecraft launcher profile file could not be read: {exception.Message}");
        }

        return directories;
    }

    private static bool HasMinecraftVersions(string gameDirectory)
    {
        string versionsDirectory = Path.Combine(gameDirectory, "versions");
        return Directory.Exists(versionsDirectory)
            && Directory.EnumerateDirectories(versionsDirectory).Any();
    }

    // Bedrock, Dungeons, Legends and Education are Microsoft Store packages
    private static IEnumerable<Game> GetMinecraftStoreGames()
    {
        var packageManager = new PackageManager();
        foreach (var package in packageManager.FindPackagesForUser(string.Empty))
        {
            if (!MinecraftStoreTitles.TryGetValue(package.Id.Name, out string? gameName))
                continue;

            string installPath = package.InstalledLocation.Path;
            if (string.IsNullOrWhiteSpace(installPath) || !Directory.Exists(installPath))
                continue;

            yield return new Game(gameName, SerializeGameName(gameName), installPath, string.Empty, package.Id.Name);
        }
    }

    private static string SerializeGameName(string gameName)
    {
        return new string(gameName.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray()).ToLowerInvariant();
    }
    private string GetGameExe(string installPath)
    {
        if (!Directory.Exists(installPath))
            return string.Empty;

        var exeFiles = Directory.GetFiles(installPath, "*.exe", SearchOption.TopDirectoryOnly);
        if(exeFiles.Length == 0)
        {
            var firstSubDir = Directory.GetDirectories(installPath).FirstOrDefault();
            if (firstSubDir != null)
                exeFiles = Directory.GetFiles(firstSubDir, "*.exe", SearchOption.TopDirectoryOnly);
        }
        return exeFiles.FirstOrDefault() ?? string.Empty;
    }

    private static string GetProcessName(string fallbackName, string exePath)
    {
        if (!string.IsNullOrWhiteSpace(exePath))
        {
            string processName = Path.GetFileNameWithoutExtension(exePath);
            if (!string.IsNullOrWhiteSpace(processName))
                return processName;
        }

        return fallbackName;
    }

    private static void SetGamesImage()
    {
        string userName = Environment.UserName;
        string imagesFolder = Path.Combine(@"C:\Users", userName, "Pictures");

        foreach (Game game in Game.InstalledGames ?? Array.Empty<Game>())
        {
            if (!string.IsNullOrEmpty(game.GameImage) && File.Exists(game.GameImage))
                continue;

            static string StripSpecialChars(string s) =>
                new string(s.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_').ToArray());

            var cleanName = StripSpecialChars(game.Name);
            var nameVariants = new[]
            {
                cleanName,
                cleanName.Replace(" ", "-"),
                cleanName.Replace(" ", "_")
            };

            string? foundImage = FindPictureImage(imagesFolder, nameVariants, StripSpecialChars);
            if (foundImage is not null)
                game.GameImage = foundImage;

            if (string.IsNullOrEmpty(game.GameImage))
                game.GameImage = FindGameFolderImage(game.InstallFolderPath);

            if (string.IsNullOrEmpty(game.GameImage))
                game.GameImage = FindSteamLibraryImage(game);
        }
    }

    private static string? FindPictureImage(string imagesFolder, string[] nameVariants, Func<string, string> stripSpecialChars)
    {
        if (!Directory.Exists(imagesFolder))
            return null;

        try
        {
            return Directory.EnumerateFiles(imagesFolder, "*.*", SearchOption.AllDirectories)
                .Where(IsSupportedImage)
                .FirstOrDefault(file => nameVariants.Any(variant =>
                    string.Equals(
                        stripSpecialChars(Path.GetFileNameWithoutExtension(file)),
                        variant,
                        StringComparison.OrdinalIgnoreCase)));
        }
        catch (UnauthorizedAccessException) { return null; }
        catch (IOException) { return null; }
    }

    private static string? FindGameFolderImage(string gameFolder)
    {
        if (!Directory.Exists(gameFolder))
            return null;

        try
        {
            return Directory.EnumerateFiles(gameFolder, "*.*", SearchOption.AllDirectories)
                .Where(IsSupportedImage)
                .FirstOrDefault(IsApproximately16By9);
        }
        catch (UnauthorizedAccessException) { return null; }
        catch (IOException) { return null; }
    }

    private static string? FindSteamLibraryImage(Game game)
    {
        var steam = Launcher.InstalledLaunchers?.FirstOrDefault(launcher =>
            string.Equals(launcher.Name, "Steam", StringComparison.OrdinalIgnoreCase)
            && Directory.Exists(launcher.InstallPath));
        if (steam?.GameFolderPath is null)
            return null;

        string gameFolderName = Path.GetFileName(game.InstallFolderPath);
        try
        {
            foreach (string steamGamesFolder in steam.GameFolderPath)
            {
                string normalizedSteamGamesFolder = Path.TrimEndingDirectorySeparator(steamGamesFolder);
                if (!game.InstallFolderPath.StartsWith(normalizedSteamGamesFolder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    continue;

                string? steamAppsFolder = Directory.GetParent(steamGamesFolder)?.FullName;
                if (steamAppsFolder is null)
                    continue;

                foreach (string manifestPath in Directory.EnumerateFiles(steamAppsFolder, "appmanifest_*.acf"))
                {
                    string manifest = File.ReadAllText(manifestPath);
                    var manifestMatch = SteamInstallDirectoryPattern.Match(manifest);
                    if (!manifestMatch.Success
                        || !string.Equals(manifestMatch.Groups["installDirectory"].Value, gameFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string appId = Path.GetFileNameWithoutExtension(manifestPath)["appmanifest_".Length..];
                    string libraryCache = Path.Combine(steam.InstallPath, "appcache", "librarycache");
                    foreach (string imageName in new[] { $"{appId}_library_hero.jpg", $"{appId}_header.jpg", $"{appId}_library_600x900.jpg" })
                    {
                        string imagePath = Path.Combine(libraryCache, imageName);
                        if (File.Exists(imagePath))
                            return imagePath;
                    }
                }
            }
        }
        catch (UnauthorizedAccessException) { return null; }
        catch (IOException) { return null; }

        return null;
    }

    private static bool IsSupportedImage(string file) =>
        file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
        || file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
        || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);

    private static bool IsApproximately16By9(string imagePath)
    {
        try
        {
            using var image = Image.FromFile(imagePath);
            return Math.Abs((double)image.Width / image.Height - 16d / 9d) < 0.05;
        }
        catch (ArgumentException) { return false; }
        catch (OutOfMemoryException) { return false; }
    }

    
    /// <summary>
    /// Wendet gespeicherte Einstellungen auf die installierten Spiele an.
    /// Liest die Spieleinstellungen aus der Datenbank und übernimmt die benutzerdefinierten
    /// Werte für Musiklautstärke, Spiellautstärke und Audioausgabegerät in die aktuellen Spieldaten.
    /// </summary>
    private static void ApplySavedSettings()
    {
        var gamesInDb = Game.GetGames();
        foreach(var gameInDb in gamesInDb ?? [])
        {
            var game = Game.InstalledGames?
                .FirstOrDefault(g => g.InstallFolderPath == gameInDb.InstallFolderPath);
            if (game == null)
                continue;

            // Übernehme benutzerdefinierte Einstellungen, falls vorhanden
            if(gameInDb.MusicVolumePercent != null)
                game.MusicVolumePercent = gameInDb.MusicVolumePercent;

            if(gameInDb.GameVolumePercent != null)
                game.GameVolumePercent = gameInDb.GameVolumePercent;

            if(gameInDb.AudioOutputDevice != null)
                game.AudioOutputDevice = gameInDb.AudioOutputDevice;

            if(gameInDb.GameImage != null)
                game.GameImage = gameInDb.GameImage;

            if(gameInDb.IsRemovedFromView != false)
                game.IsRemovedFromView = true;
                
            if(gameInDb.LivelyWallpaperScreen != null)
                game.LivelyWallpaperScreen = gameInDb.LivelyWallpaperScreen;
        }
    }

    private static readonly string[] RegistryInstallKeyNames =
    [
        "InstallLocation",
        "InstallFolder",
        "InstallPath",
        "InstallDir",
        "Install Dir",
        "UninstallString"
    ];
}