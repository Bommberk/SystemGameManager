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
        var section = GetNewSection("Launcher");
        // Launcher hinzufügen
        var launchers = gameViewService.GetInstalledLauncher();
        var launcherSection = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
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
        section.Controls.Add(launcherSection);
        page.Controls.Add(section);
    }

    private void CreateAudioSettingsSection()
    {
        var section = GetNewSection("Audio Einstellungen");
        
    }
    private void CreateGameOverviewSection()
    {
        var section = GetNewSection("Spiele");
        // Spiele hinzufügen
        var games = gameViewService.GetInstalledGames();
        var gameSection = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
        foreach (var game in games)
        {
            var gameCard = CardControls.GetRoundedCardPanel(8);
            gameCard.AutoSize = false;
            gameCard.Size = new Size(301, 287);
            gameCard.Margin = new Padding(5);
            gameCard.FlowDirection = FlowDirection.TopDown;
            var gameWallpaper = new PictureBox()
            {
                Image = Image.FromFile(game.GameImage ?? "assets/bild.jpg"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(280, 158),
                BackColor = ColorThemes.GetSecondaryBackgroundColor(),
                Margin = new Padding(0, 0, 0, 10),
            };
            var gameName = new Label()
            {
                Text = game.Name,
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
                Margin = new Padding(0, 0, 0, 5),
            };
            var gamePlayTime = new Label()
            {
                Text = $"Playtime: Nicht erkannt", // Placeholder, da Playtime-erkennung kommt erst mit smarthome update/integration
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var gameInstallPathTitle = new Label()
            {
                Text = "Installpath:",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var gameInstallPath = new Label()
            {
                Text = game.ExePath,
                AutoSize = true,
                MaximumSize = new Size(280, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetSecondaryTextColor(),
            };
            var openGameDirectoryButton = new NormalButton()
            {
                Text = "Ordner öffnen",
                AutoSize = true,
            };
            gameCard.Controls.Add(gameWallpaper);
            gameCard.Controls.Add(gameName);
            gameCard.Controls.Add(gamePlayTime);
            gameCard.Controls.Add(gameInstallPathTitle);
            gameCard.Controls.Add(gameInstallPath);
            gameCard.Controls.Add(openGameDirectoryButton);
            gameSection.Controls.Add(gameCard);
        }
        section.Controls.Add(gameSection);
        page.Controls.Add(section);
    }
    private FlowLayoutPanel GetNewSection(string title)
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
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        sectionTitlePanel.Controls.Add(sectionTitle);
        section.Controls.Add(sectionTitlePanel);
        return section;
    }
}