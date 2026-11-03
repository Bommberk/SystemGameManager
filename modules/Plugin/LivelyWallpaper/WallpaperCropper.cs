namespace SystemGameManager.Plugin;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Schneidet Wallpaper, die kein 16:9 haben (z.B. über mehrere Bildschirme gestreckte Bilder),
/// auf einen einzelnen Bildschirm zu und legt das Ergebnis im Cache ab.
/// </summary>
public static class WallpaperCropper
{
    public static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };

    private const double TargetAspect = 16.0 / 9.0;
    private const double AspectTolerance = 0.02;
    private const double SpanMatchTolerance = 0.05;

    public static bool IsSupportedImage(string? path)
    {
        return !string.IsNullOrWhiteSpace(path)
            && SupportedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gibt den Pfad zurück, der an Lively übergeben werden soll (Original oder zugeschnittene Kopie).
    /// </summary>
    public static string PrepareForMonitor(string path, int monitorNumber)
    {
        try
        {
            var isKnownExtension = IsSupportedImage(path);
            if (isKnownExtension && Path.GetExtension(path).Equals(".gif", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            using var image = Image.FromFile(path);
            var cropRect = GetCropRectangle(image.Width, image.Height, monitorNumber);
            if (cropRect is null && isKnownExtension)
            {
                return path;
            }

            var rect = cropRect ?? new Rectangle(0, 0, image.Width, image.Height);
            var cachePath = GetCachePath(path, monitorNumber, rect);
            if (File.Exists(cachePath))
            {
                return cachePath;
            }

            using var cropped = new Bitmap(rect.Width, rect.Height);
            using (var graphics = Graphics.FromImage(cropped))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
            }

            cropped.Save(cachePath, ImageFormat.Png);
            return cachePath;
        }
        catch (Exception ex)
        {
            // Z.B. webp, das System.Drawing nicht lesen kann: dann das Original verwenden.
            mlog($"Could not crop wallpaper '{path}': {ex.Message}");
            return path;
        }
    }

    private static Rectangle? GetCropRectangle(int width, int height, int monitorNumber)
    {
        double imageAspect = (double)width / height;
        if (Math.Abs(imageAspect - TargetAspect) / TargetAspect <= AspectTolerance)
        {
            return null;
        }

        var spanRect = GetSpanRectangle(width, height, imageAspect, monitorNumber);
        if (spanRect is not null)
        {
            return spanRect;
        }

        // Mittiger 16:9-Ausschnitt
        if (imageAspect > TargetAspect)
        {
            int cropWidth = (int)Math.Round(height * TargetAspect);
            return new Rectangle((width - cropWidth) / 2, 0, cropWidth, height);
        }

        int cropHeight = (int)Math.Round(width / TargetAspect);
        return new Rectangle(0, (height - cropHeight) / 2, width, cropHeight);
    }

    // Passt das Bild zur Gesamtfläche aller Bildschirme, ist es vermutlich darüber gestreckt:
    // dann wird der Bereich des gewünschten Bildschirms ausgeschnitten.
    private static Rectangle? GetSpanRectangle(int width, int height, double imageAspect, int monitorNumber)
    {
        var screens = Screen.AllScreens;
        if (screens.Length < 2)
        {
            return null;
        }

        var virtualScreen = SystemInformation.VirtualScreen;
        double virtualAspect = (double)virtualScreen.Width / virtualScreen.Height;
        if (Math.Abs(imageAspect - virtualAspect) / virtualAspect > SpanMatchTolerance)
        {
            return null;
        }

        var screen = FindScreen(screens, monitorNumber);
        double scaleX = (double)width / virtualScreen.Width;
        double scaleY = (double)height / virtualScreen.Height;
        var rect = new Rectangle(
            (int)Math.Round((screen.Bounds.Left - virtualScreen.Left) * scaleX),
            (int)Math.Round((screen.Bounds.Top - virtualScreen.Top) * scaleY),
            (int)Math.Round(screen.Bounds.Width * scaleX),
            (int)Math.Round(screen.Bounds.Height * scaleY));

        rect.Intersect(new Rectangle(0, 0, width, height));
        return rect.Width > 0 && rect.Height > 0 ? rect : null;
    }

    private static Screen FindScreen(Screen[] screens, int monitorNumber)
    {
        foreach (var screen in screens)
        {
            var digits = new string(screen.DeviceName.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var number) && number == monitorNumber)
            {
                return screen;
            }
        }

        return screens[Math.Clamp(monitorNumber - 1, 0, screens.Length - 1)];
    }

    private static string GetCachePath(string path, int monitorNumber, Rectangle rect)
    {
        var cacheFolder = Path.Combine(GetAppdataPath(), "wallpaper-cache");
        Directory.CreateDirectory(cacheFolder);

        var key = $"{path}|{File.GetLastWriteTimeUtc(path).Ticks}|{monitorNumber}|{rect}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..24];
        return Path.Combine(cacheFolder, $"{hash}.png");
    }
}
