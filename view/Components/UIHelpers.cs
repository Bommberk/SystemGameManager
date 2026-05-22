using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class UIHelpers
{
    public static Color WindowBackground => Color.FromArgb(22, 24, 23);
    public static Color WindowGradientEnd => Color.FromArgb(18, 21, 24);
    public static Color SurfaceBackground => Color.FromArgb(30, 33, 31);
    public static Color CardBackground => Color.FromArgb(36, 39, 37);
    public static Color CardHoverBackground => Color.FromArgb(46, 51, 47);
    public static Color BorderColor => Color.FromArgb(64, 69, 64);
    public static Color SidebarBackground => Color.FromArgb(47, 63, 43);
    public static Color SidebarActiveBackground => Color.FromArgb(99, 119, 80);
    public static Color AccentColor => Color.FromArgb(163, 176, 94);
    public static Color AccentPressedColor => Color.FromArgb(128, 140, 69);
    public static Color AccentHoverColor => Color.FromArgb(182, 195, 108);
    public static Color AccentBadgeBackground => Color.FromArgb(68, 74, 49);
    public static Color TextPrimaryColor => Color.FromArgb(242, 243, 237);
    public static Color TextSecondaryColor => Color.FromArgb(184, 189, 176);
    public static Color TextMutedColor => Color.FromArgb(136, 142, 131);

    public static Button CreatePrimaryButton(string text, int width)
    {
        var button = new Button()
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        ApplyPrimaryButtonStyle(button);
        return button;
    }

    public static Button CreateSecondaryButton(string text, int width)
    {
        var button = new Button()
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        ApplySecondaryButtonStyle(button);
        return button;
    }

    public static void ApplyPrimaryButtonStyle(Button button)
    {
        button.BackColor = AccentColor;
        button.ForeColor = Color.FromArgb(35, 40, 24);
        button.FlatAppearance.BorderColor = AccentColor;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = AccentPressedColor;
        button.FlatAppearance.MouseOverBackColor = AccentHoverColor;
    }

    public static void ApplySecondaryButtonStyle(Button button)
    {
        button.BackColor = SurfaceBackground;
        button.ForeColor = TextPrimaryColor;
        button.FlatAppearance.BorderColor = BorderColor;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseDownBackColor = CardBackground;
        button.FlatAppearance.MouseOverBackColor = CardHoverBackground;
    }

    public static void SetRoundedRegion(Control control, int radius)
    {
        void ApplyRegion()
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            using var path = CreateRoundedRectanglePath(new Rectangle(0, 0, control.Width - 1, control.Height - 1), radius);
            control.Region?.Dispose();
            control.Region = new Region(path);
        }

        control.SizeChanged += (_, _) => ApplyRegion();
        ApplyRegion();
    }

    public static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
