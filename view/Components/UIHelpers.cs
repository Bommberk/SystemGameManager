namespace SystemGameManager.View.Components;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

internal static class UIHelpers
{
    public static Button CreatePrimaryButton(string text, int width)
    {
        var button = new Button()
        {
            Text = text,
            Width = width,
            Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(67, 56, 202);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(99, 102, 241);
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
}
