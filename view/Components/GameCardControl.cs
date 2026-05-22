using System.Drawing;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Service;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class GameCardControl
{
    public static Control Create(GameViewService.GameCardItem game, Image? artwork, Action<string> openGameDirectory)
    {
        var gameArtwork = TryLoadGameArtwork(game.ImagePath) ?? artwork;

        var shell = new HoverShadowPanel()
        {
            Width = 318,
            Height = 360,
            Margin = new Padding(0, 0, 18, 18)
        };

        var body = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.CardBackground,
            Padding = new Padding(14)
        };
        UIHelpers.SetRoundedRegion(body, 18);

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 164));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var imageHost = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.SurfaceBackground,
            Margin = new Padding(0, 0, 0, 10)
        };
        UIHelpers.SetRoundedRegion(imageHost, 14);

        var picture = new PictureBox()
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.StretchImage,
            Image = gameArtwork,
            BackColor = Color.Transparent
        };

        if (gameArtwork is not null && !ReferenceEquals(gameArtwork, artwork))
        {
            picture.Disposed += (_, _) => gameArtwork.Dispose();
        }

        imageHost.Controls.Add(picture);

        var badge = new Label()
        {
            Text = "✓ INSTALLIERT",
            AutoSize = true,
            BackColor = Color.FromArgb(68, 74, 49),
            ForeColor = UIHelpers.AccentColor,
            Padding = new Padding(10, 4, 10, 4),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Margin = new Padding(0, 2, 0, 10)
        };

        var title = new Label()
        {
            Text = game.Title,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Height = 48,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = UIHelpers.TextPrimaryColor,
            Margin = new Padding(0, 0, 0, 8)
        };

        var pathTitle = new Label()
        {
            Text = "Installationspfad",
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = UIHelpers.TextMutedColor,
            Margin = new Padding(0, 0, 0, 4)
        };

        var pathLabel = new Label()
        {
            Text = game.InstallPath,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Height = 44,
            AutoEllipsis = true,
            ForeColor = UIHelpers.TextSecondaryColor,
            Margin = new Padding(0, 0, 0, 8)
        };

        var openButton = UIHelpers.CreateSecondaryButton("Ordner öffnen", 130);
        openButton.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        openButton.Margin = new Padding(0);
        openButton.Click += (_, _) => openGameDirectory(game.InstallPath);

        layout.Controls.Add(imageHost, 0, 0);
        layout.Controls.Add(badge, 0, 1);
        layout.Controls.Add(title, 0, 2);
        layout.Controls.Add(pathTitle, 0, 3);
        layout.Controls.Add(pathLabel, 0, 4);
        layout.Controls.Add(openButton, 0, 5);

        body.Controls.Add(layout);
        shell.Controls.Add(body);

        HoverShadowPanel.AddHoverEffect(shell, body);
        return shell;
    }

    private static Image? TryLoadGameArtwork(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return null;
        }

        try
        {
            using var stream = File.OpenRead(imagePath);
            using var image = Image.FromStream(stream);
            return new Bitmap(image);
        }
        catch
        {
            return null;
        }
    }
}
