namespace SystemGameManager.View.Pages;

using SystemGameManager.View.Components;
using SystemGameManager.Games.Service;

class GameManager : Page
{
    private const string TAB_ICON_PATH = "assets/icons/gamepad-solid-full.svg";
    private const string TAB_TEXT = "Game Manager";
    private const string PAGE_TITLE = "Game Library";
    private readonly GameViewService gameViewService = new GameViewService();
    public int LauncherSectionHeight { get; private set; } = 0;

    public GameManager() : base(TAB_TEXT, TAB_ICON_PATH, "center")
    {
        CreatePageInput();
    }

    private void CreatePageInput()
    {
        CreatePageHero();
        CreateLauncherSection();
        CreateGameOverviewSection();
        CreateAudioSettingsSection();
    }

    private void CreatePageHero()
    {

        var hero = CardControls.GetRoundedCardPanel(5);

        // hero Input
        var heroImage = new PictureBox()
        {
            Image = UIHelpers.LoadIcon("assets/icons/gamepad-solid-full.svg", new Size(64, 64)),
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = ColorThemes.GetSecondaryBackgroundColor(),
            Padding = new Padding(10),
        };

        var heroTextPanel = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(12, 0, 0, 0),
            Padding = new Padding(0),
        };

        var heroTitle = new Label()
        {
            Text = PAGE_TITLE,
            AutoSize = true,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        var heroGameInfo = new Label()
        {
            Text = $"{gameViewService.GetInstalledGames().Length} Spiele - {gameViewService.GetInstalledLauncher().Length} Launcher",
            AutoSize = true,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
        };
        var heroSubtitle = new Label()
        {
            Text = "Launcher, Spiele und schnelle Aktionen auf einen Blick.",
            AutoSize = true,
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        heroTextPanel.Controls.Add(heroTitle);
        heroTextPanel.Controls.Add(heroGameInfo);
        heroTextPanel.Controls.Add(heroSubtitle);

        hero.Controls.Add(heroImage);
        hero.Controls.Add(heroTextPanel);
        page.Controls.Add(hero);
    }

    private void CreateLauncherSection()
    {
        var section = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 12),
        };
        var sectionTitlePanel = new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
        };
        var sectionTitle = new Label()
        {
            Text = "Launcher",
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        sectionTitlePanel.Controls.Add(sectionTitle);
        // Sectiontitlepanel als dropdown deklarieren
        section.Controls.Add(sectionTitlePanel);
        // Launcher hinzufügen
        var launchers = gameViewService.GetInstalledLauncher();
        var launcherSection = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
        section.Controls.Add(launcherSection);
        foreach (var launcher in launchers)
        {
            var launcherCard = CardControls.GetRoundedCardPanel(8);
            launcherCard.AutoSize = false;
            launcherCard.Size = new Size(250, 60);
            launcherCard.Margin = new Padding(5);
            var launcherTextContentPanel = new FlowLayoutPanel()
            {
                // AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0,0,5,0),
            };
            var launcherName = new Label()
            {
                Text = launcher.Name,
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var launcherInstallPath = new Label()
            {
                Text = launcher.InstallPath,
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = ColorThemes.GetSecondaryTextColor(),
            };
            launcherTextContentPanel.Controls.Add(launcherName);
            launcherTextContentPanel.Controls.Add(launcherInstallPath);
            launcherCard.Controls.Add(launcherTextContentPanel);
            launcherSection.Controls.Add(launcherCard);
        }
        CardControls.GetDropDownCard(section);
        page.Controls.Add(section);
    }

    private void CreateAudioSettingsSection()
    {
        var section = CardControls.GetRoundedCardPanel(5);
        section = CardControls.GetDropDownCard(section);
    }
    private void CreateGameOverviewSection()
    {
        
    }
}