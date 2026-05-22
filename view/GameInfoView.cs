using System.Drawing;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Service;
using Krassheiten.SystemGameManager.View.Components;

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
        Padding = new Padding(0, 8, 6, 12),
        BackColor = Color.Transparent
    };

    private readonly Label gameManagerSummaryLabel = new()
    {
        Text = "Noch keine Daten geladen.",
        AutoSize = true,
        ForeColor = UIHelpers.AccentColor,
        Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
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
            BackColor = UIHelpers.WindowBackground
        };

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 16, 18, 18),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = UIHelpers.WindowBackground
        };

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var heroPanel = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = UIHelpers.SurfaceBackground,
            Padding = new Padding(24),
            Margin = new Padding(0, 0, 0, 18)
        };
        UIHelpers.SetRoundedRegion(heroPanel, 22);

        var heroTitle = new Label()
        {
            Text = "Game Library",
            AutoSize = true,
            Font = new Font("Segoe UI", 19F, FontStyle.Bold),
            ForeColor = UIHelpers.TextPrimaryColor,
            Margin = new Padding(0)
        };

        var heroSubtitle = new Label()
        {
            Text = "Launcher, Spiele und schnelle Aktionen auf einen Blick.",
            AutoSize = true,
            ForeColor = UIHelpers.TextSecondaryColor,
            Margin = new Padding(0, 8, 0, 0)
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
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
            ForeColor = UIHelpers.TextPrimaryColor,
            Margin = new Padding(0, 0, 0, 10)
        };

        var gamesTitle = new Label()
        {
            Text = "Installierte Spiele",
            AutoSize = true,
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold),
            ForeColor = UIHelpers.TextPrimaryColor,
            Margin = new Padding(0, 10, 0, 8)
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
        gameCardsPanel.Controls.Add(StateCardControl.Create("Spiele werden geladen...", "Die Bibliothek wird gerade aktualisiert."));
    }

    public void ShowErrorState(string message)
    {
        gameManagerSummaryLabel.Text = "Fehler beim Laden der Spieledaten";
        gameCardsPanel.Controls.Clear();
        launcherPanel.Controls.Clear();
        gameCardsPanel.Controls.Add(StateCardControl.Create("Laden fehlgeschlagen", message));
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
                launcherPanel.Controls.Add(LauncherBadgeControl.Create("Keine Launcher gefunden", "Prüfe bekannte Installationspfade."));
            }
            else
            {
                foreach (var launcher in viewData.Launchers)
                {
                    launcherPanel.Controls.Add(LauncherBadgeControl.Create(launcher.Title, launcher.Subtitle));
                }
            }

            if (viewData.Games.Count == 0)
            {
                gameCardsPanel.Controls.Add(StateCardControl.Create("Keine Spiele gefunden", "Sobald Spiele erkannt werden, erscheinen sie hier als Cards."));
            }
            else
            {
                foreach (var game in viewData.Games)
                {
                    gameCardsPanel.Controls.Add(GameCardControl.Create(game, artwork, openGameDirectory));
                }
            }
        }
        finally
        {
            launcherPanel.ResumeLayout();
            gameCardsPanel.ResumeLayout();
        }
    }
}
