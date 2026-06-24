namespace SystemGameManager.View.Pages;

using SystemGameManager.View.Components;
using SystemGameManager.Games.Service;
using SystemGameManager.View.Service;

class GameManager : Page
{
    private const string TAB_ICON_PATH = "assets/icons/gamepad-solid-full.svg";
    private const string TAB_TEXT = "Game Manager";
    private const string PAGE_TITLE = "Game Library";
    public int LauncherSectionHeight { get; private set; } = 0;
    private readonly GameManagerViewService GameManagerViewService;

    public GameManager() : base(TAB_TEXT, TAB_ICON_PATH, "center")
    {
        GameManagerViewService = new GameManagerViewService(this);
        CreatePageInput();
    }
    public void RefreshGameAndLauncherInfo()
    {
        page.Controls.Clear();
        CreatePageInput();
    }

    private void CreatePageInput()
    {
        CreatePageHero();
        CreateLauncherSection();
        CreateAudioSettingsSection();
        CreateGameOverviewSection();
    }

    private void CreatePageHero()
    {
        var hero = CardControls.GetCardPanel(5);

        // hero Input
        var heroImage = new PictureBox()
        {
            Image = UIHelpers.LoadIcon("assets/icons/gamepad-solid-full.svg", new Size(64, 64)),
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = ColorThemes.GetSecondaryBackgroundColor(),
            Padding = new Padding(10),
        };

        var heroTextContainer = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(12, 0, 0, 0),
            Padding = new Padding(0),
        };

        // Hero Title and Refresh Button
        var heroTitleContainer = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };
        var heroTitle = new Label()
        {
            Text = PAGE_TITLE,
            AutoSize = true,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        var refreshButton = new NormalButton()
        {
            Image = UIHelpers.LoadIcon("assets/icons/rotate-solid-full.svg", new Size(16, 16)),
            Size = new Size(30, 30),
            Margin = new Padding(10, 0, 0, 0),
            BackColor = Color.Transparent
        };
        refreshButton.Click += async (_, _) =>
        {
            refreshButton.Enabled = false;
            try
            {
                await GameManagerViewService.RefreshGameAndLauncherInfoAsync();
            }
            finally
            {
                refreshButton.Enabled = true;
            }
        };
        heroTitleContainer.Controls.Add(heroTitle);
        heroTitleContainer.Controls.Add(refreshButton);

        // More hero infos
        var heroGameInfo = new Label()
        {
            Text = $"{GameManagerViewService.GetInstalledGames().Length} Spiele - {GameManagerViewService.GetInstalledLauncher().Length} Launcher",
            AutoSize = true,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
        };
        var heroSubtitle = new Label()
        {
            Text = "Launcher, Spiele und schnelle Aktionen auf einen Blick.",
            AutoSize = true,
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };

        heroTextContainer.Controls.Add(heroTitleContainer);
        heroTextContainer.Controls.Add(heroGameInfo);
        heroTextContainer.Controls.Add(heroSubtitle);

        hero.Controls.Add(heroImage);
        hero.Controls.Add(heroTextContainer);
        page.Controls.Add(hero);
    }

    private void CreateLauncherSection()
    {
        var section = GameManagerViewService.GetNewSection("Launcher");
        // Launcher hinzufügen
        var launchers = GameManagerViewService.GetInstalledLauncher();
        var launcherSection = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
        foreach (var launcher in launchers)
        {
            var launcherCard = CardControls.GetCardPanel(5);
            launcherCard.AutoSize = false;
            launcherCard.Size = new Size(250, 60);
            launcherCard.Margin = new Padding(5);
            var launcherTextContentContainer = new FlowLayoutPanel()
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
            launcherTextContentContainer.Controls.Add(launcherName);
            launcherTextContentContainer.Controls.Add(launcherInstallPath);
            launcherCard.Controls.Add(launcherTextContentContainer);
            launcherSection.Controls.Add(launcherCard);
        }
        section.Controls.Add(launcherSection);
        page.Controls.Add(section);
    }

    private void CreateAudioSettingsSection()
    {
        var section = GameManagerViewService.GetNewSection("Audio Einstellungen");
        var gameAudioCard = CardControls.GetCardPanel(5);
        gameAudioCard.FlowDirection = FlowDirection.TopDown;

        // Audio Device Selection
        var gameSelectionLabel = new Label()
        {
            Text = "Spiele auswählen",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(0, 0, 0, 5),
        };
        var gameSelectionContainer = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
        var audioDeviceSelection = new ComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200,
        };
        audioDeviceSelection.Items.Add("(Standard-Gerät)");
        foreach(var device in GameManagerViewService.GetAudioOutputDeviceNames())
        {
            audioDeviceSelection.Items.Add(device);
        }
        audioDeviceSelection.SelectedIndex = 0;
        var selectAllGamesButton = new NormalButton()
        {
            Text = "Alle auswählen",
            AutoSize = true,
        };
        var reverseSelectionButton = new NormalButton()
        {
            Text = "Auswahl umkehren",
            AutoSize = true,
        };
        gameSelectionContainer.Controls.Add(audioDeviceSelection);
        gameSelectionContainer.Controls.Add(selectAllGamesButton);
        gameSelectionContainer.Controls.Add(reverseSelectionButton);

        // Volume Controlls
        var volumeControlLabel = new Label()
        {
            Text = "Lautstärke für ausgewählte Spiele",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(0, 0, 0, 5),
        };

        // Game Volume Control
        var gameVolumeContainer = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
        var gameVolumeLabel = new Label()
        {
            Text = "Game-Lautstärke:",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(0, 0, 5, 0),
        };
        var gameVolumeTrackBar = new TrackBar()
        {
            Minimum = 0,
            Maximum = 100,
            Value = 100,
            TickFrequency = 10,
            TickStyle = TickStyle.None,
            SmallChange = 5,
            LargeChange = 10,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        var gameVolumeValueLabel = new Label()
        {
            Text = $"{gameVolumeTrackBar.Value}%",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(5, 0, 0, 0),
        };
        gameVolumeContainer.Controls.Add(gameVolumeLabel);
        gameVolumeContainer.Controls.Add(gameVolumeTrackBar);
        gameVolumeContainer.Controls.Add(gameVolumeValueLabel);
        // Music Volume Control
        var musicVolumeContainer = new FlowLayoutPanel()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
        var musicVolumeLabel = new Label()
        {
            Text = "Musik-Lautstärke:",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(0, 0, 5, 0),
        };
        var musicVolumeTrackBar = new TrackBar()
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            TickFrequency = 10,
            TickStyle = TickStyle.None,
            SmallChange = 5,
            LargeChange = 10,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        var musicVolumeValueLabel = new Label()
        {
            Text = $"{musicVolumeTrackBar.Value}%",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(5, 0, 0, 0),
        };
        musicVolumeContainer.Controls.Add(musicVolumeLabel);
        musicVolumeContainer.Controls.Add(musicVolumeTrackBar);
        musicVolumeContainer.Controls.Add(musicVolumeValueLabel);

        // Save
        var saveButton = new NormalButton()
        {
            Text = "Speichern",
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 0),
        };
        
        // Add to Controlls
        gameAudioCard.Controls.Add(gameSelectionLabel);
        gameAudioCard.Controls.Add(gameSelectionContainer);
        gameAudioCard.Controls.Add(volumeControlLabel);
        gameAudioCard.Controls.Add(gameVolumeContainer);
        gameAudioCard.Controls.Add(musicVolumeContainer);
        section.Controls.Add(gameAudioCard);
        page.Controls.Add(section);
    }
    private void CreateGameOverviewSection()
    {
        var section = GameManagerViewService.GetNewSection("Spiele");
        // Spiele hinzufügen
        var games = GameManagerViewService.GetInstalledGames();
        var gameContainer = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,

        };
        foreach (var game in games)
        {
            var gameCard = CardControls.GetSectionCard(5);
            gameCard.AutoSize = true;
            gameCard.Margin = new Padding(5);
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
            var volumeLabel = new Label()
            {
                Text = $"Volume:",
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var volume = new Label()
            {
                Text = $"Game: {game.GameVolumePercent}% | Music: {game.MusicVolumePercent}%",
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetSecondaryTextColor(),
            };
            var audioOutputDeviceLabel = new Label()
            {
                Text = $"Audio Output:",
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var audioOutputDevice = new Label()
            {
                Text = $"{game.AudioOutputDevice ?? "(Standard-Gerät)"}",
                AutoSize = true,
                MaximumSize = new Size(280, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetSecondaryTextColor(),
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
                Dock = DockStyle.Bottom,
            };

            gameCard.Controls.Add(gameWallpaper);
            gameCard.Controls.Add(gameName);
            gameCard.Controls.Add(gamePlayTime);
            gameCard.Controls.Add(gameInstallPathTitle);
            gameCard.Controls.Add(gameInstallPath);
            gameCard.Controls.Add(volumeLabel);
            gameCard.Controls.Add(volume);
            gameCard.Controls.Add(audioOutputDeviceLabel);
            gameCard.Controls.Add(audioOutputDevice);
            gameCard.Controls.Add(openGameDirectoryButton);
            gameContainer.Controls.Add(gameCard);
        }
        section.Controls.Add(gameContainer);
        page.Controls.Add(section);
    }
}