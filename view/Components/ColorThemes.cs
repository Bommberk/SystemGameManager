namespace SystemGameManager.View.Components;

using ViewService = SystemGameManager.View.Service.ViewService;

public class ColorThemes
{
    public static ITheme CurrentTheme {get;}
    static ColorThemes()
    {
        // bald auch in einstellungen einstellbar machen
        switch (ViewService.GetSystemTheme())
        {
            case "Dark":
                CurrentTheme = new StandardDarkTheme();
                break;
            case "Light":
                CurrentTheme = new StandardLightTheme();
                break;
            default:
                CurrentTheme = new StandardDarkTheme();
                break;
        }
    }
    public static Color GetPrimaryBackgroundColor()
    {
        return CurrentTheme.PrimaryBackgroundColor;
    }   
    public static Color GetSecondaryBackgroundColor()
    {
        return CurrentTheme.SecondaryBackgroundColor;
    }
    public static Color GetCardBackgroundColor()
    {
        return CurrentTheme.CardBackgroundColor;
    }
    public static Color GetPrimaryTextColor()
    {
        return CurrentTheme.PrimaryTextColor;
    }
    public static Color GetSecondaryTextColor()
    {
        return CurrentTheme.SecondaryTextColor;
    }
}

class StandardDarkTheme : ITheme
{
    public Color PrimaryBackgroundColor => Color.FromArgb(23, 26, 26);
    public Color SecondaryBackgroundColor => Color.FromArgb(59, 67, 49);
    public Color TertiaryBackgroundColor => Color.FromArgb(15, 15, 15);
    public Color CardBackgroundColor => Color.FromArgb(35, 36, 36);
    public Color PrimaryTextColor => Color.FromArgb(152, 177, 100);
    public Color SecondaryTextColor => Color.FromArgb(233, 235, 236);

    public Color GetHoveredColor(Color baseColor)
    {
        return UIHelpers.Lighter(baseColor, 0.05f);
    }
}

class StandardLightTheme : ITheme
{
    public Color PrimaryBackgroundColor => Color.FromArgb(252, 251, 251);
    public Color SecondaryBackgroundColor => Color.FromArgb(113, 123, 89);
    public Color TertiaryBackgroundColor => Color.FromArgb(255, 255, 255);
    public Color CardBackgroundColor => Color.FromArgb(242, 242, 242);
    public Color PrimaryTextColor => Color.FromArgb(152, 177, 100);
    public Color SecondaryTextColor => Color.FromArgb(30, 30, 30);

    public Color GetHoveredColor(Color baseColor)
    {
        return UIHelpers.Lighter(baseColor, 0.1f);
    }
}

public interface ITheme
{
    Color PrimaryBackgroundColor { get; }
    Color SecondaryBackgroundColor { get; }
    Color TertiaryBackgroundColor { get; }
    Color CardBackgroundColor { get; }
    Color PrimaryTextColor { get; }
    Color SecondaryTextColor { get; }
    Color GetHoveredColor(Color baseColor);
}