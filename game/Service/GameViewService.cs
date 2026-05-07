namespace Krassheiten.SystemGameManager.Service;

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Krassheiten.SystemGameManager.Entity;

internal sealed class GameViewService : IDisposable
{
    private readonly Image? artwork = LoadGameArtwork();

    public Image? Artwork => artwork;

    public GameManagerViewData BuildViewData()
    {
        var launchers = (Launcher.InstalledLaunchers ?? Array.Empty<Launcher.Record>())
            .OrderBy(launcher => launcher.Name)
            .Select(launcher => new LauncherBadgeItem(
                launcher.Name,
                string.IsNullOrWhiteSpace(launcher.InstallPath) || launcher.InstallPath == "nothing found"
                    ? "Installationspfad nicht verfügbar"
                    : launcher.InstallPath))
            .ToArray();

        var games = (Game.InstalledGames ?? Array.Empty<Game.Record>())
            .OrderBy(game => game.Name)
            .Select(game => new GameCardItem(
                game.Name,
                string.IsNullOrWhiteSpace(game.InstallFolderPath)
                    ? "Pfad nicht verfügbar"
                    : game.InstallFolderPath))
            .ToArray();

        var summaryText = $"{games.Length} Spiele • {launchers.Length} Launcher erkannt";
        return new GameManagerViewData(summaryText, launchers, games);
    }

    public bool TryOpenDirectory(string? path, out string errorMessage)
    {
        errorMessage = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                errorMessage = "Kein Pfad verfügbar.";
                return false;
            }

            var targetPath = Directory.Exists(path)
                ? path
                : Path.GetDirectoryName(path);

            if (string.IsNullOrWhiteSpace(targetPath) || !Directory.Exists(targetPath))
            {
                errorMessage = "Der Ordner konnte nicht gefunden werden.";
                return false;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = targetPath,
                UseShellExecute = true
            });

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Ordner konnte nicht geöffnet werden:\r\n{ex.Message}";
            return false;
        }
    }

    public void Dispose()
    {
        artwork?.Dispose();
    }

    private static Image? LoadGameArtwork()
    {
        var possiblePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "assets", "bild.jpg"),
            Path.Combine(Environment.CurrentDirectory, "assets", "bild.jpg"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "assets", "bild.jpg")),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "assets", "bild.jpg"))
        };

        var imagePath = possiblePaths.FirstOrDefault(File.Exists);
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        using var stream = File.OpenRead(imagePath);
        using var image = Image.FromStream(stream);
        return CropToSquare(image);
    }

    private static Bitmap CropToSquare(Image image)
    {
        var squareSize = Math.Min(image.Width, image.Height);
        var sourceX = (image.Width - squareSize) / 2;
        var sourceY = (image.Height - squareSize) / 2;

        var bitmap = new Bitmap(squareSize, squareSize);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.DrawImage(
            image,
            new Rectangle(0, 0, squareSize, squareSize),
            new Rectangle(sourceX, sourceY, squareSize, squareSize),
            GraphicsUnit.Pixel);

        return bitmap;
    }

    internal sealed record GameManagerViewData(
        string SummaryText,
        IReadOnlyList<LauncherBadgeItem> Launchers,
        IReadOnlyList<GameCardItem> Games);

    internal sealed record LauncherBadgeItem(string Title, string Subtitle);

    internal sealed record GameCardItem(string Title, string InstallPath);
}
