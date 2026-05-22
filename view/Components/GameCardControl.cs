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
            Width = 348,
            Height = 306,
            Margin = new Padding(0, 0, 18, 18)
        };

        var body = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.CardBackground,
            Padding = new Padding(10)
        };
        UIHelpers.SetRoundedRegion(body, 18);

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        var imageHost = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.SurfaceBackground,
            Margin = new Padding(0, 0, 0, 8)
        };
        UIHelpers.SetRoundedRegion(imageHost, 12);

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

        var footer = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var badge = new Label()
        {
            Text = "✓  INSTALLIERT",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            BackColor = UIHelpers.AccentBadgeBackground,
            ForeColor = UIHelpers.AccentColor,
            Padding = new Padding(12, 5, 12, 5),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 0)
        };
        UIHelpers.SetRoundedRegion(badge, 10);

        var optionsButton = new Button()
        {
            Text = "⋮",
            Width = 32,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = UIHelpers.TextSecondaryColor,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0)
        };
        optionsButton.FlatAppearance.BorderSize = 0;
        optionsButton.FlatAppearance.MouseOverBackColor = UIHelpers.CardHoverBackground;
        optionsButton.FlatAppearance.MouseDownBackColor = UIHelpers.CardHoverBackground;
        optionsButton.Click += (_, _) => openGameDirectory(game.InstallPath);
        optionsButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
        optionsButton.AccessibleDescription = game.Title;

        layout.Controls.Add(imageHost, 0, 0);
        footer.Controls.Add(badge, 0, 0);
        footer.Controls.Add(optionsButton, 1, 0);
        layout.Controls.Add(footer, 0, 1);

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
