namespace SystemGameManager.View.Components;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Svg;

internal static class UIHelpers
{
    public static Image LoadIcon(string path, Size? targetSize = null)
    {
        var extension = Path.GetExtension(path);

        if (string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase))
        {
            var size = targetSize ?? new Size(24, 24);
            var svgDocument = SvgDocument.Open(path);
            ChangeNavbarMenuIconColor(svgDocument);
            return svgDocument.Draw(size.Width, size.Height);
        }

        using var image = Image.FromFile(path);
        if (targetSize is null)
        {
            return new Bitmap(image);
        }

        return new Bitmap(image, targetSize.Value);
    }
    private static void ChangeNavbarMenuIconColor(SvgDocument svg)
    {
        foreach (var element in svg.Descendants().OfType<SvgVisualElement>())
        {
            element.Fill = new SvgColourServer(ColorThemes.GetPrimaryTextColor());
        }
    }

    public static Panel ItemContainer(Padding padding, Control child)
    {
        var panel = new Panel()
        {
            Dock = DockStyle.Top,
            Height = child.Height + padding.Vertical,
            Padding = padding,
            BackColor = Color.Transparent
        };
        panel.Controls.Add(child);
        return panel;
    }

    public static Button CreatePrimaryButton(string text, int width)
    {
        var button = new Button()
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorThemes.GetSecondaryBackgroundColor(),
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = ColorThemes.CurrentTheme.GetHoveredColor(button.BackColor);
        button.FlatAppearance.MouseOverBackColor = ColorThemes.CurrentTheme.GetHoveredColor(button.BackColor);
        return button;
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

    public static Color Darker(Color color, float factor)
    {
        factor = Math.Clamp(factor, 0f, 1f);

        return Color.FromArgb(
            color.A,
            (int)(color.R * factor),
            (int)(color.G * factor),
            (int)(color.B * factor)
        );
    }
    public static Color Lighter(Color color, float factor)
    {
        factor = Math.Clamp(factor, 0f, 1f);

        return Color.FromArgb(
            color.A,
            (int)(color.R + (255 - color.R) * factor),
            (int)(color.G + (255 - color.G) * factor),
            (int)(color.B + (255 - color.B) * factor)
        );
    }
}
