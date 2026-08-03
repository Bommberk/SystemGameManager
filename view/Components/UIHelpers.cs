namespace SystemGameManager.View.Components;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Svg;

internal static class UIHelpers
{
    public static string ResolveRuntimePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        if (Path.IsPathRooted(path))
        {
            return path;
        }

        var normalized = path
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        return Path.Combine(AppContext.BaseDirectory, normalized);
    }

    public static Image LoadIcon(string path, Size? targetSize = null)
    {
        var resolvedPath = ResolveRuntimePath(path);
        var extension = Path.GetExtension(path);

        if (string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase))
        {
            var size = targetSize ?? new Size(24, 24);
            var svgDocument = SvgDocument.Open(resolvedPath);
            ChangeNavbarMenuIconColor(svgDocument);
            return svgDocument.Draw(size.Width, size.Height);
        }

        using var image = Image.FromFile(resolvedPath);
        if (targetSize is null)
        {
            return new Bitmap(image);
        }

        return new Bitmap(image, targetSize.Value);
    }

    public static Image LoadImage(string path)
    {
        var resolvedPath = ResolveRuntimePath(path);
        var image = Image.FromFile(ResolveRuntimePath("assets/bild.jpg"));
        if(File.Exists(resolvedPath))
        {
            image = Image.FromFile(resolvedPath);
        }
        return new Bitmap(image);
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
    public static Color Transparentize(Color color, float factor)
    {
        factor = Math.Clamp(factor, 0f, 1f);

        return Color.FromArgb(
            (int)(color.A * (1 - factor)),
            color.R,
            color.G,
            color.B
        );
    }
    public static Color HoverColor(Color baseColor, float factor)
    {
        if (factor < 0)
        {
            return Darker(baseColor, -factor);
        }
        else
        {
            return Lighter(baseColor, factor);
        }
    }
    public static void RoundPictureBox(
        PictureBox pictureBox,
        int topLeft = 0,
        int topRight = 0,
        int bottomRight = 0,
        int bottomLeft = 0)
    {
        void UpdateRegion()
        {
            var path = new GraphicsPath();

            // Oben links
            if (topLeft > 0)
                path.AddArc(0, 0, topLeft * 2, topLeft * 2, 180, 90);
            else
                path.AddLine(0, 0, 0, 0);

            // Oben rechts
            if (topRight > 0)
                path.AddArc(pictureBox.Width - topRight * 2, 0, topRight * 2, topRight * 2, 270, 90);
            else
                path.AddLine(pictureBox.Width, 0, pictureBox.Width, 0);

            // Unten rechts
            if (bottomRight > 0)
                path.AddArc(
                    pictureBox.Width - bottomRight * 2,
                    pictureBox.Height - bottomRight * 2,
                    bottomRight * 2,
                    bottomRight * 2,
                    0,
                    90);
            else
                path.AddLine(pictureBox.Width, pictureBox.Height, pictureBox.Width, pictureBox.Height);

            // Unten links
            if (bottomLeft > 0)
                path.AddArc(
                    0,
                    pictureBox.Height - bottomLeft * 2,
                    bottomLeft * 2,
                    bottomLeft * 2,
                    90,
                    90);
            else
                path.AddLine(0, pictureBox.Height, 0, pictureBox.Height);

            path.CloseFigure();

            pictureBox.Region?.Dispose();
            pictureBox.Region = new Region(path);
        }

        pictureBox.Resize += (_, _) => UpdateRegion();
        UpdateRegion();
    }
    public static void SetSvgBoxShadow(SvgDocument svgDocument, Color shadowColor, float blurRadius, float offsetX, float offsetY)
    {
        
    }
}
