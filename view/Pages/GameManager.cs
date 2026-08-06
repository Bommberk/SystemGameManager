namespace SystemGameManager.View.Pages;

using SystemGameManager.View.Components;
using SystemGameManager.Games.Service;
using SystemGameManager.View.Service;
using SystemGameManager.Games.Entity;

class GameManager : Page
{
    private const string TAB_ICON_PATH = "assets/icons/gamepad-solid-full.svg";
    private const string TAB_TEXT = "Game Manager";
    private const string PAGE_TITLE = "Game Library";
    public int LauncherSectionHeight { get; private set; } = 0;
    private readonly GameManagerViewService GameManagerViewService;
    private readonly SystemAudioService systemAudioService = new SystemAudioService();

    public readonly List<GameSelectionBinding> gameBindings = new();
    public NormalButton? selectAllGamesButton;
    public NormalButton? reverseSelectionButton;
    public ComboBox? audioDeviceSelection;
    public TrackBar? gameVolumeTrackBar;
    public TrackBar? musicVolumeTrackBar;
    public Label? gameVolumeValueLabel;
    public Label? musicVolumeValueLabel;

    public sealed record GameSelectionBinding(Game Game, CheckBox CheckBox, Label VolumeLabel, Label AudioOutputDeviceLabel);


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
        heroTitleContainer.Controls.Add(heroTitle);
        heroTitleContainer.Controls.Add(refreshButton);
        GameManagerViewService.RefreshGameAndLauncherInfos(refreshButton); // Attach event handlers for user activities

        // More hero infos
        var heroGameInfo = new Label()
        {
            Text = $"{Game.GetGames().Length} Spiele - {Launcher.GetLaunchers().Length} Launcher",
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
        var section = ViewService.GetNewSection("Launcher");
        // Launcher hinzufügen
        var launchers = Launcher.GetLaunchers();
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
        var section = ViewService.GetNewSection("Audio Einstellungen");
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
        audioDeviceSelection = new ComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200,
        };
        audioDeviceSelection.Items.Add("(Standard-Gerät)");
        // foreach(var device in systemAudioService.GetAudioOutputDeviceNames())
        // {
        //     audioDeviceSelection.Items.Add(device);
        // }
        audioDeviceSelection.SelectedIndex = 0;
        selectAllGamesButton = new NormalButton()
        {
            Text = "Alle auswählen",
            AutoSize = true,
        };
        reverseSelectionButton = new NormalButton()
        {
            Text = "Auswahl umkehren",
            AutoSize = true,
        };
        GameManagerViewService.SelectAllGames(selectAllGamesButton);
        GameManagerViewService.ReverseSelection(reverseSelectionButton);
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
        gameVolumeTrackBar = new TrackBar()
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
        gameVolumeValueLabel = new Label()
        {
            Text = $"{gameVolumeTrackBar.Value}%",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(5, 0, 0, 0),
        };
        GameManagerViewService.SetAudioTrackbar(gameVolumeTrackBar, gameVolumeValueLabel);

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
        musicVolumeTrackBar = new TrackBar()
        {
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            TickFrequency = 10,
            TickStyle = TickStyle.None,
            SmallChange = 5,
            LargeChange = 10,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            Width = 200
        };
        musicVolumeValueLabel = new Label()
        {
            Text = $"{musicVolumeTrackBar.Value}%",
            AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(5, 0, 0, 0),
        };
        GameManagerViewService.SetAudioTrackbar(musicVolumeTrackBar, musicVolumeValueLabel);
        musicVolumeContainer.Controls.Add(musicVolumeLabel);
        musicVolumeContainer.Controls.Add(musicVolumeTrackBar);
        musicVolumeContainer.Controls.Add(musicVolumeValueLabel);

        // Save
        var saveButton = new NormalButton()
        {
            Text = "Speichern",
            AutoSize = false,
            Margin = new Padding(0, 10, 0, 0),
        };
        GameManagerViewService.SaveAudioForGame(saveButton, audioDeviceSelection, gameVolumeTrackBar, musicVolumeTrackBar, gameBindings);
        
        // Add to Controlls
        gameAudioCard.Controls.Add(gameSelectionLabel);
        gameAudioCard.Controls.Add(gameSelectionContainer);
        gameAudioCard.Controls.Add(volumeControlLabel);
        gameAudioCard.Controls.Add(gameVolumeContainer);
        gameAudioCard.Controls.Add(musicVolumeContainer);
        gameAudioCard.Controls.Add(saveButton);
        section.Controls.Add(gameAudioCard);
        page.Controls.Add(section);
    }
    private void CreateGameOverviewSection()
    {
        var section = ViewService.GetNewSection("Spiele");
        gameBindings.Clear();

        // Spiele hinzufügen
        var games = Game.GetGames();
        var gameContainer = new FlowLayoutPanel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
        };
        int gameCardWidth = 300;
        foreach (var game in games)
        {
            var gameCard = CardControls.GetSectionCard(5);
            gameCard.AutoSize = true;
            gameCard.Margin = new Padding(5);
            gameCard.MaximumSize = new Size(gameCardWidth, 0);
            gameCard.Width = gameCardWidth;
            gameCard.MinimumSize = new Size(gameCardWidth, 0);
            gameCard.AutoSizeMode  = AutoSizeMode.GrowAndShrink;
            var gameWallpaper = new PictureBox()
            {
                Image = UIHelpers.LoadImage(game.GameImage ?? "assets/bild.jpg"),
                SizeMode = PictureBoxSizeMode.Zoom,
                Width = gameCardWidth - 20,
                Height = (int)((gameCardWidth - 20) * 9.0 / 16.0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                BackColor = ColorThemes.GetSecondaryCardBackgroundColor(),
                Margin = new Padding(0, 0, 0, 10),
            };
            UIHelpers.SetRoundedRegion(gameWallpaper, 5);

            // Checkbox oben links auf dem Bild hinzufügen
            var gameSelectCheckBox = new CheckBox()
            {
                Checked = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(10, 10),
                Text = string.Empty,
                Cursor = Cursors.Hand,
            };
            gameWallpaper.Controls.Add(gameSelectCheckBox);

            // Bild klickbar machen, um Checkbox zu toggeln
            gameWallpaper.Cursor = Cursors.Hand;
            if(selectAllGamesButton != null)
                GameManagerViewService.SelectGame(gameWallpaper, gameSelectCheckBox, selectAllGamesButton);
            

            // Search and set Game Image
            var gameMenuButton = new NormalButton()
            {
                Image = UIHelpers.LoadIcon("assets/icons/ellipsis-vertical-solid-full.svg", new Size(25, 25)),
                Size = new Size(33, 33),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            gameMenuButton.SetHoverColor(ColorThemes.GetCardBackgroundColor());
            gameMenuButton.Location = new Point(gameWallpaper.Width - gameMenuButton.Width - 5, 5);
            gameWallpaper.Controls.Add(gameMenuButton);

            // Create Game Menu
            var gameMenu = new FlowLayoutPanel()
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0,0,15,0),
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            gameMenu.Location = new Point(gameWallpaper.Width - gameMenu.Width - 5, 10);
            // Game Menu Items
            var openGameDirectoryButton = new NormalButton()
            {
                Text = "Spielordner öffnen",
                AutoSize = true,
                Size = new Size(gameMenu.Width, 0),
                BackColor = ColorThemes.GetQuaternaryBackgroundColor(),
                Margin = new Padding(0),
            };
            openGameDirectoryButton.SetHoverColor(UIHelpers.Darker(ColorThemes.GetQuaternaryBackgroundColor(), 0.85f));
            var changeGameImageButton = new NormalButton()
            {
                Text = "Spielbild ändern",
                AutoSize = true,
                Size = new Size(gameMenu.Width, 0),
                Margin = new Padding(0),
                BackColor = ColorThemes.GetQuaternaryBackgroundColor(),
            };
            changeGameImageButton.SetHoverColor(UIHelpers.Darker(ColorThemes.GetQuaternaryBackgroundColor(), 0.85f));
            var removeGameButton = new NormalButton()
            {
                Text = "Spiel entfernen",
                AutoSize = true,
                Size = new Size(gameMenu.Width, 0),
                Margin = new Padding(0),
                BackColor = ColorThemes.GetErrorBackgroundColor(),
            };
            removeGameButton.SetHoverColor(Color.FromArgb(163, 26, 44));

            // Attach event handlers for user activities
            GameManagerViewService.OpenGameDirectory(openGameDirectoryButton, game);
            GameManagerViewService.ChangeGameImage(changeGameImageButton, game, gameCard, gameWallpaper);
            GameManagerViewService.RemoveGame(removeGameButton, game, gameCard);

            // Add gamemenu to GameCard
            gameMenu.Controls.Add(openGameDirectoryButton);
            gameMenu.Controls.Add(changeGameImageButton);
            gameMenu.Controls.Add(removeGameButton);
            gameWallpaper.Controls.Add(gameMenu);
            GameManagerViewService.ToggleGameMenu(gameMenuButton, gameMenu);

            // Game Infos
            var gameName = new Label()
            {
                Text = game.Name,
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 0),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UIHelpers.Lighter(ColorThemes.GetPrimaryTextColor(), 0.1f),
                Margin = new Padding(0, 0, 0, 5),
            };
            var gamePlayTime = new Label()
            {
                Text = $"Playtime: Nicht erkannt", // Placeholder, da Playtime-erkennung kommt erst mit smarthome update/integration
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var volumeLabel = new Label()
            {
                Text = $"Volume:",
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var volume = new Label()
            {
                Text = $"Game: {game.GameVolumePercent}% | Music: {game.MusicVolumePercent}%",
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetSecondaryTextColor(),
            };
            var audioOutputDeviceLabel = new Label()
            {
                Text = $"Audio Output:",
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var audioOutputDevice = new Label()
            {
                Text = $"{game.AudioOutputDevice ?? "(Standard-Gerät)"}",
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetSecondaryTextColor(),
            };
            var gameInstallPathTitle = new Label()
            {
                Text = "Installpath:",
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 0),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetPrimaryTextColor(),
            };
            var gameInstallPath = new Label()
            {
                Text = game.ExePath,
                AutoSize = true,
                MaximumSize = new Size(gameCardWidth - 20, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ColorThemes.GetSecondaryTextColor(),
            };

            gameBindings.Add(new GameSelectionBinding(game, gameSelectCheckBox, volume, audioOutputDevice));

            gameCard.Controls.Add(gameWallpaper);
            gameCard.Controls.Add(gameName);
            gameCard.Controls.Add(gamePlayTime);
            gameCard.Controls.Add(gameInstallPathTitle);
            gameCard.Controls.Add(gameInstallPath);
            gameCard.Controls.Add(volumeLabel);
            gameCard.Controls.Add(volume);
            gameCard.Controls.Add(audioOutputDeviceLabel);
            gameCard.Controls.Add(audioOutputDevice);
            gameContainer.Controls.Add(gameCard);
        }
        section.Controls.Add(gameContainer);
        page.Controls.Add(section);
    }
}