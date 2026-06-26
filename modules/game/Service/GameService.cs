namespace SystemGameManager.Games.Service;

using SystemGameManager.Games.Entity;
using Microsoft.Win32;

class GameService
{
    public void SetInstalledGames()
    {
        var gamesFromFolders = GetGamesWithGameFolder();
        var gamesFromRegistry = GetGamesWithRegistry();
        
        var allGames = gamesFromFolders.Concat(gamesFromRegistry)
                                       .DistinctBy(game => game.InstallFolderPath)
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
                    string exePath = GetGameExe(gameDir);
                    games.Add(new Game(gameName, gameDir, exePath));
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
                    games.Add(new Game(resolvedGameName, installPath, exePath, processName));
                }
            }
        }
        return games;
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
        if (!Directory.Exists(imagesFolder))
            return;

        // Search recursively for image files in the Pictures folder
        foreach (Game game in Game.InstalledGames ?? Array.Empty<Game>())
        {
            // Entfernt alle Sonderzeichen aus einem String (behält nur Buchstaben, Ziffern, Leerzeichen, Bindestrich, Unterstrich)
            static string StripSpecialChars(string s) =>
                new string(s.Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_').ToArray());

            // Erzeuge verschiedene Varianten des Spielnamens (ohne Sonderzeichen)
            var cleanName = StripSpecialChars(game.Name);
            var nameVariants = new[]
            {
                cleanName,
                cleanName.Replace(" ", "-"),
                cleanName.Replace(" ", "_")
            };

            // Suche alle Bilddateien im Pictures-Ordner (rekursiv)
            var imageFiles = Directory.GetFiles(imagesFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                            || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));

            // Vergleiche Dateinamen (ohne Erweiterung, ohne Sonderzeichen) mit den Varianten, case-insensitive
            var foundImage = imageFiles.FirstOrDefault(file =>
                nameVariants.Any(variant =>
                    string.Equals(
                        StripSpecialChars(Path.GetFileNameWithoutExtension(file)),
                        variant,
                        StringComparison.OrdinalIgnoreCase)));

            if (foundImage != null)
            {
                game.GameImage = foundImage;
            }
        }
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