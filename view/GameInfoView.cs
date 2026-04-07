using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Service;

namespace Krassheiten.SystemGameManager.View;

internal sealed class GameInfoView
{
    private readonly Image? artwork;
    private readonly Action<string> openGameDirectory;
    private readonly FlowLayoutPanel launcherPanel = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        WrapContents = true,
        Margin = new Padding(0, 0, 0, 8),
        Padding = new Padding(0),
        BackColor = Color.Transparent
    };

    private readonly FlowLayoutPanel gameCardsPanel = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        WrapContents = true,
        Margin = new Padding(0),
        Padding = new Padding(0, 8, 12, 12),
        BackColor = Color.FromArgb(245, 247, 250)
    };

    private readonly Label gameManagerSummaryLabel = new()
    {
        Text = "Noch keine Daten geladen.",
        AutoSize = true,
        ForeColor = Color.FromArgb(224, 231, 255),
        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
        Margin = new Padding(0, 4, 0, 0)
    };

    public GameInfoView(Image? artwork, Action<string> openGameDirectory)
    {
        this.artwork = artwork;
        this.openGameDirectory = openGameDirectory;
    }

    public TabPage CreateTab()
    {
        var tab = new TabPage("Game-Manager")
        {
            BackColor = Color.FromArgb(245, 247, 250)
        };

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Color.FromArgb(245, 247, 250)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var heroPanel = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(30, 41, 59),
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 0, 12)
        };

        var heroTitle = new Label()
        {
            Text = "Game Library",
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            Margin = new Padding(0)
        };

        var heroSubtitle = new Label()
        {
            Text = "Launcher, Spiele und schnelle Aktionen auf einen Blick.",
            AutoSize = true,
            ForeColor = Color.FromArgb(209, 213, 219),
            Margin = new Padding(0, 6, 0, 0)
        };

        var heroTextLayout = new FlowLayoutPanel()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        heroTextLayout.Controls.Add(heroTitle);
        heroTextLayout.Controls.Add(gameManagerSummaryLabel);
        heroTextLayout.Controls.Add(heroSubtitle);
        heroPanel.Controls.Add(heroTextLayout);

        var launcherTitle = new Label()
        {
            Text = "Launcher",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 0, 0, 8)
        };

        var gamesTitle = new Label()
        {
            Text = "Installierte Spiele",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 6, 0, 6)
        };

        layout.Controls.Add(heroPanel, 0, 0);
        layout.Controls.Add(launcherTitle, 0, 1);
        layout.Controls.Add(launcherPanel, 0, 2);
        layout.Controls.Add(gamesTitle, 0, 3);
        layout.Controls.Add(gameCardsPanel, 0, 4);

        tab.Controls.Add(layout);
        return tab;
    }

    public void ShowLoadingState()
    {
        gameManagerSummaryLabel.Text = "Lade Spielebibliothek...";
        gameCardsPanel.Controls.Clear();
        launcherPanel.Controls.Clear();
        gameCardsPanel.Controls.Add(CreateStateCard("Spiele werden geladen...", "Die Bibliothek wird gerade aktualisiert."));
    }

    public void ShowErrorState(string message)
    {
        gameManagerSummaryLabel.Text = "Fehler beim Laden der Spieledaten";
        gameCardsPanel.Controls.Clear();
        launcherPanel.Controls.Clear();
        gameCardsPanel.Controls.Add(CreateStateCard("Laden fehlgeschlagen", message));
    }

    public void Populate(GameViewService.GameManagerViewData viewData)
    {
        launcherPanel.SuspendLayout();
        gameCardsPanel.SuspendLayout();

        try
        {
            launcherPanel.Controls.Clear();
            gameCardsPanel.Controls.Clear();

            gameManagerSummaryLabel.Text = viewData.SummaryText;

            if (viewData.Launchers.Count == 0)
            {
                launcherPanel.Controls.Add(CreateLauncherBadge("Keine Launcher gefunden", "Prüfe bekannte Installationspfade."));
            }
            else
            {
                foreach (var launcher in viewData.Launchers)
                {
                    launcherPanel.Controls.Add(CreateLauncherBadge(launcher.Title, launcher.Subtitle));
                }
            }

            if (viewData.Games.Count == 0)
            {
                gameCardsPanel.Controls.Add(CreateStateCard("Keine Spiele gefunden", "Sobald Spiele erkannt werden, erscheinen sie hier als Cards."));
            }
            else
            {
                foreach (var game in viewData.Games)
                {
                    gameCardsPanel.Controls.Add(CreateGameCard(game));
                }
            }
        }
        finally
        {
            launcherPanel.ResumeLayout();
            gameCardsPanel.ResumeLayout();
        }
    }

    private Control CreateGameCard(GameViewService.GameCardItem game)
    {
        var shell = new HoverShadowPanel()
        {
            Width = 290,
            Height = 390,
            Margin = new Padding(0, 0, 18, 18)
        };

        var body = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(14)
        };
        SetRoundedRegion(body, 18);

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var imageHost = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(236, 240, 248),
            Margin = new Padding(0, 0, 0, 10)
        };
        SetRoundedRegion(imageHost, 14);

        var picture = new PictureBox()
        {
            Size = new Size(120, 120),
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = artwork,
            BackColor = Color.Transparent
        };

        void CenterArtwork()
        {
            picture.Location = new Point(
                Math.Max(0, (imageHost.ClientSize.Width - picture.Width) / 2),
                Math.Max(0, (imageHost.ClientSize.Height - picture.Height) / 2));
        }

        imageHost.Controls.Add(picture);
        imageHost.Resize += (_, _) => CenterArtwork();
        CenterArtwork();

        var badge = new Label()
        {
            Text = "INSTALLIERT",
            AutoSize = true,
            BackColor = Color.FromArgb(224, 231, 255),
            ForeColor = Color.FromArgb(67, 56, 202),
            Padding = new Padding(8, 4, 8, 4),
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
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 8)
        };

        var pathTitle = new Label()
        {
            Text = "Installationspfad",
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(107, 114, 128),
            Margin = new Padding(0, 0, 0, 4)
        };

        var pathLabel = new Label()
        {
            Text = game.InstallPath,
            Dock = DockStyle.Fill,
            AutoSize = false,
            Height = 44,
            AutoEllipsis = true,
            ForeColor = Color.FromArgb(75, 85, 99),
            Margin = new Padding(0, 0, 0, 8)
        };

        var openButton = CreatePrimaryButton("Ordner öffnen", 120);
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

        AddHoverEffect(shell, body);
        return shell;
    }

    private static Panel CreateLauncherBadge(string title, string subtitle)
    {
        var shell = new Panel()
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 10, 10),
            Padding = new Padding(1),
            BackColor = Color.FromArgb(221, 227, 237)
        };

        var body = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(12),
            Margin = new Padding(0)
        };

        body.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55)
        });

        body.Controls.Add(new Label
        {
            Text = subtitle,
            AutoSize = true,
            MaximumSize = new Size(280, 0),
            ForeColor = Color.FromArgb(107, 114, 128)
        });

        shell.Controls.Add(body);
        return shell;
    }

    private static Control CreateStateCard(string title, string message)
    {
        var shell = new HoverShadowPanel()
        {
            Width = 420,
            Height = 160,
            Margin = new Padding(0, 0, 14, 14)
        };

        var body = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18)
        };
        SetRoundedRegion(body, 18);

        var textLayout = new FlowLayoutPanel()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        textLayout.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 8)
        });

        textLayout.Controls.Add(new Label
        {
            Text = message,
            AutoSize = true,
            MaximumSize = new Size(360, 0),
            ForeColor = Color.FromArgb(107, 114, 128)
        });

        body.Controls.Add(textLayout);
        shell.Controls.Add(body);
        return shell;
    }

    private static void AddHoverEffect(HoverShadowPanel shell, Panel body)
    {
        void SetState(bool hovered)
        {
            shell.IsHovered = hovered;
            body.BackColor = hovered ? Color.FromArgb(245, 247, 255) : Color.White;
        }

        void EnterHandler(object? sender, EventArgs e) => SetState(true);

        void LeaveHandler(object? sender, EventArgs e)
        {
            var cursor = shell.PointToClient(Cursor.Position);
            if (!shell.ClientRectangle.Contains(cursor))
            {
                SetState(false);
            }
        }

        WireHoverEvents(shell, EnterHandler, LeaveHandler);
    }

    private static void WireHoverEvents(Control control, EventHandler enterHandler, EventHandler leaveHandler)
    {
        control.MouseEnter += enterHandler;
        control.MouseLeave += leaveHandler;

        foreach (Control child in control.Controls)
        {
            WireHoverEvents(child, enterHandler, leaveHandler);
        }
    }

    private static Button CreatePrimaryButton(string text, int width)
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

    private static void SetRoundedRegion(Control control, int radius)
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

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
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

    private sealed class HoverShadowPanel : Panel
    {
        private bool isHovered;

        public bool IsHovered
        {
            get => isHovered;
            set
            {
                if (isHovered == value)
                {
                    return;
                }

                isHovered = value;
                Invalidate();
            }
        }

        public HoverShadowPanel()
        {
            BackColor = Color.Transparent;
            Padding = new Padding(8, 8, 14, 14);
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var cardBounds = new Rectangle(6, 6, Width - 22, Height - 22);
            var layers = IsHovered ? 6 : 3;
            var baseAlpha = IsHovered ? 18 : 8;

            for (var layer = layers; layer >= 1; layer--)
            {
                var shadowBounds = new Rectangle(
                    cardBounds.X + layer,
                    cardBounds.Y + layer + 1,
                    Math.Max(1, cardBounds.Width),
                    Math.Max(1, cardBounds.Height));

                using var path = CreateRoundedRectanglePath(shadowBounds, 18);
                using var brush = new SolidBrush(Color.FromArgb(baseAlpha + (layer * 5), 15, 23, 42));
                e.Graphics.FillPath(brush, path);
            }
        }
    }
}