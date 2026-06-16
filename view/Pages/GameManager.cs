namespace SystemGameManager.View.Pages;

using System.Drawing;
using System.Windows.Forms;
using SystemGameManager.Games.Controller;
using SystemGameManager.Games.Entity;
using SystemGameManager.Games.Service;
using SystemGameManager.View.Components;
using SystemGameManager.View.Service;

internal sealed class GameManager : Page
{
    private const string TAB_ICON_PATH = "assets/icons/gamepad-solid-full.svg";
    private const string TAB_TEXT = "Game Manager";

    // Hero
    private readonly Label summaryLabel = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        Margin = new Padding(0, 2, 0, 0)
    };

    // Launchers
    private readonly FlowLayoutPanel launcherBadgesPanel = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        WrapContents = true,
        Padding = new Padding(0, 8, 0, 4),
        BackColor = Color.Transparent
    };

    // Audio controls
    private readonly TrackBar gameSlider = CreateSlider(100);
    private readonly TrackBar musicSlider = CreateSlider(50);
    private readonly Label gameValueLabel = CreateValueLabel(100);
    private readonly Label musicValueLabel = CreateValueLabel(50);
    private readonly Button saveButton = CreateSaveButton();
    private readonly Button selectAllButton = CreateActionButton("Alle auswählen");
    private readonly Button toggleSelectionButton = CreateActionButton("Auswahl umkehren");
    private readonly ComboBox outputDeviceComboBox = CreateOutputDeviceComboBox();
    private readonly TableLayoutPanel audioGameListTable = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 1,
        Margin = new Padding(0),
        Padding = new Padding(0),
        BackColor = Color.Transparent
    };

    // Game cards
    private readonly FlowLayoutPanel gameCardsPanel = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        WrapContents = true,
        Padding = new Padding(0, 8, 0, 8),
        BackColor = Color.Transparent
    };

    // State
    private bool isUpdatingSliders;
    private bool hasPendingChanges;
    private bool dataLoaded;
    private readonly GameViewService gameViewService = new();
    private GameAudioController? gameAudioController;

    public GameManager() : base(TAB_TEXT, TAB_ICON_PATH, "center")
    {
        page.Padding = new Padding(0);
        audioGameListTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        WireEvents();
        BuildLayout();
        ShowLoadingState();

        navButton.Click += async (_, _) => await EnsureDataLoadedAsync();
    }

    // ──────────────────────────── Layout ────────────────────────────

    private void BuildLayout()
    {
        var scrollHost = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ColorThemes.GetPrimaryBackgroundColor()
        };

        var outerFlow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(12),
            BackColor = ColorThemes.GetPrimaryBackgroundColor()
        };

        outerFlow.Controls.Add(BuildHeroSection());

        // Launcher section (collapsed by default so audio is visible first)
        var (launcherHeader, launcherContent) = BuildCollapsibleSection(
            "Launcher", launcherBadgesPanel, initiallyExpanded: false);
        outerFlow.Controls.Add(launcherHeader);
        outerFlow.Controls.Add(launcherContent);

        // Audio section
        var (audioHeader, audioContent) = BuildCollapsibleSection(
            "Audio-Steuerung für Spiele und Musik", BuildAudioSectionContent(), initiallyExpanded: true,
            rightControl: saveButton);
        outerFlow.Controls.Add(audioHeader);
        outerFlow.Controls.Add(audioContent);

        // Game selection row (Spiele auswählen + combo + buttons)
        outerFlow.Controls.Add(BuildGameSelectionRow());

        // Installed games section
        var (gamesHeader, gamesContent) = BuildCollapsibleSection(
            "Installierte Spiele", gameCardsPanel, initiallyExpanded: true);
        outerFlow.Controls.Add(gamesHeader);
        outerFlow.Controls.Add(gamesContent);

        scrollHost.Controls.Add(outerFlow);
        page.Controls.Add(scrollHost);

        // Keep all section panels as wide as the scroll host
        scrollHost.Resize += (_, _) =>
        {
            outerFlow.Width = scrollHost.ClientSize.Width;
            int sectionWidth = Math.Max(0, outerFlow.ClientSize.Width);
            foreach (Control c in outerFlow.Controls)
                c.Width = sectionWidth;
        };
    }

    private Panel BuildHeroSection()
    {
        var panel = new Panel
        {
            BackColor = ColorThemes.GetCardBackgroundColor(),
            Padding = new Padding(18),
            Height = 110,
            Margin = new Padding(0, 0, 0, 12)
        };

        var titleLabel = new Label
        {
            Text = "Game Library",
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(0)
        };

        summaryLabel.ForeColor = ColorThemes.GetPrimaryTextColor();

        var subtitleLabel = new Label
        {
            Text = "Launcher, Spiele und schnelle Aktionen auf einen Blick.",
            AutoSize = true,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Margin = new Padding(0, 4, 0, 0)
        };

        var textFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        textFlow.Controls.Add(titleLabel);
        textFlow.Controls.Add(summaryLabel);
        textFlow.Controls.Add(subtitleLabel);

        panel.Controls.Add(textFlow);
        return panel;
    }

    private Panel BuildAudioSectionContent()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(14),
            BackColor = ColorThemes.GetCardBackgroundColor(),
            Margin = new Padding(0)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 3,
            RowCount = 3,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

        var sectionTitle = new Label
        {
            Text = "Lautstärke für ausgewählte Spiele",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Margin = new Padding(0, 0, 0, 8)
        };
        layout.Controls.Add(sectionTitle, 0, 0);
        layout.SetColumnSpan(sectionTitle, 3);

        AddSliderRow(layout, 1, "Game-Lautstärke", gameSlider, gameValueLabel);
        AddSliderRow(layout, 2, "Musik-Lautstärke", musicSlider, musicValueLabel);

        panel.Controls.Add(layout);

        // Audio game list (checkboxes)
        var listHost = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 0),
            BackColor = ColorThemes.GetPrimaryBackgroundColor()
        };
        listHost.Controls.Add(audioGameListTable);
        panel.Controls.Add(listHost);

        return panel;
    }

    private Panel BuildGameSelectionRow()
    {
        var panel = new Panel
        {
            BackColor = Color.Transparent,
            Height = 46,
            Margin = new Padding(0, 8, 0, 4)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var label = new Label
        {
            Text = "Spiele auswählen",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(0, 6, 0, 0)
        };

        layout.Controls.Add(label, 0, 0);
        layout.Controls.Add(outputDeviceComboBox, 1, 0);
        layout.Controls.Add(selectAllButton, 2, 0);
        layout.Controls.Add(toggleSelectionButton, 3, 0);

        panel.Controls.Add(layout);
        return panel;
    }

    private static (Panel header, Panel contentWrapper) BuildCollapsibleSection(
        string title, Control contentControl, bool initiallyExpanded, Control? rightControl = null)
    {
        bool expanded = initiallyExpanded;

        // Content wrapper
        var contentWrapper = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Visible = initiallyExpanded,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 0, 4)
        };
        contentControl.Dock = DockStyle.Top;
        contentWrapper.Controls.Add(contentControl);

        // Section header
        var header = new Panel
        {
            BackColor = ColorThemes.GetSecondaryBackgroundColor(),
            Height = 40,
            Margin = new Padding(0, 0, 0, 0),
            Padding = new Padding(14, 0, 8, 0),
            Cursor = Cursors.Hand
        };

        var titleLabel = new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Hand
        };

        var chevronLabel = new Label
        {
            Text = initiallyExpanded ? "∧" : "∨",
            Width = 28,
            Dock = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Cursor = Cursors.Hand
        };

        void Toggle(object? s, EventArgs e)
        {
            expanded = !expanded;
            contentWrapper.Visible = expanded;
            chevronLabel.Text = expanded ? "∧" : "∨";
        }

        header.Click += Toggle;
        titleLabel.Click += Toggle;
        chevronLabel.Click += Toggle;

        if (rightControl != null)
        {
            rightControl.Dock = DockStyle.Right;
            rightControl.Margin = new Padding(0, 5, 0, 5);
            header.Controls.Add(rightControl);
        }

        header.Controls.Add(chevronLabel);
        header.Controls.Add(titleLabel);

        return (header, contentWrapper);
    }

    // ──────────────────────────── Data loading ────────────────────────────

    private async Task EnsureDataLoadedAsync()
    {
        if (dataLoaded)
        {
            return;
        }

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        ShowLoadingState();
        try
        {
            var viewData = await Task.Run(BuildViewData);
            PopulateGameInfo(viewData);
            PopulateAudio();
            gameAudioController ??= new GameAudioController();
            dataLoaded = true;
        }
        catch (Exception ex)
        {
            ShowErrorState(ex.Message);
        }
    }

    private static GameViewService.GameManagerViewData BuildViewData()
    {
        _ = new GameInfoController();
        return new GameViewService().BuildViewData();
    }

    // ──────────────────────────── State ────────────────────────────

    private void ShowLoadingState()
    {
        summaryLabel.Text = "Lade Spielebibliothek...";
        summaryLabel.ForeColor = ColorThemes.GetSecondaryTextColor();

        launcherBadgesPanel.Controls.Clear();
        gameCardsPanel.Controls.Clear();
        gameCardsPanel.Controls.Add(StateCardControl.Create(
            "Spiele werden geladen...", "Die Bibliothek wird gerade aktualisiert."));

        SetAudioControlsEnabled(false);
        SetHasPendingChanges(false);
        ShowAudioMessageCard(
            "Spiele werden geladen...",
            "Nach dem Laden kannst du hier globale und spielbezogene Audio-Werte anpassen.");
    }

    private void ShowErrorState(string message)
    {
        summaryLabel.Text = "Fehler beim Laden";
        summaryLabel.ForeColor = Color.OrangeRed;

        launcherBadgesPanel.Controls.Clear();
        gameCardsPanel.Controls.Clear();
        gameCardsPanel.Controls.Add(StateCardControl.Create("Laden fehlgeschlagen", message));

        SetAudioControlsEnabled(false);
        SetHasPendingChanges(false);
        ShowAudioMessageCard("Audio-Einstellungen konnten nicht geladen werden", message);
    }

    private void PopulateGameInfo(GameViewService.GameManagerViewData viewData)
    {
        if (page.InvokeRequired)
        {
            page.Invoke(() => PopulateGameInfo(viewData));
            return;
        }

        summaryLabel.Text = viewData.SummaryText;
        summaryLabel.ForeColor = ColorThemes.GetPrimaryTextColor();

        launcherBadgesPanel.SuspendLayout();
        gameCardsPanel.SuspendLayout();
        try
        {
            launcherBadgesPanel.Controls.Clear();
            gameCardsPanel.Controls.Clear();

            if (viewData.Launchers.Count == 0)
            {
                launcherBadgesPanel.Controls.Add(LauncherBadgeControl.Create(
                    "Keine Launcher gefunden", "Prüfe bekannte Installationspfade."));
            }
            else
            {
                foreach (var launcher in viewData.Launchers)
                    launcherBadgesPanel.Controls.Add(LauncherBadgeControl.Create(launcher.Title, launcher.Subtitle));
            }

            if (viewData.Games.Count == 0)
            {
                gameCardsPanel.Controls.Add(StateCardControl.Create(
                    "Keine Spiele gefunden",
                    "Sobald Spiele erkannt werden, erscheinen sie hier als Cards."));
            }
            else
            {
                foreach (var game in viewData.Games)
                    gameCardsPanel.Controls.Add(
                        GameCardControl.Create(game, gameViewService.Artwork, ViewService.OpenGameDirectory));
            }
        }
        finally
        {
            launcherBadgesPanel.ResumeLayout();
            gameCardsPanel.ResumeLayout();
        }
    }

    private void PopulateAudio()
    {
        if (page.InvokeRequired)
        {
            page.Invoke(PopulateAudio);
            return;
        }

        var games = Game.InstalledGames ?? Array.Empty<Game.Record>();
        UpdateGlobalSliderSnapshot(games);
        SetAudioControlsEnabled(games.Length > 0);
        SetHasPendingChanges(false);

        audioGameListTable.SuspendLayout();
        try
        {
            audioGameListTable.Controls.Clear();
            audioGameListTable.RowStyles.Clear();
            audioGameListTable.RowCount = 0;

            if (games.Length == 0)
            {
                AddAudioListControl(MessageCardControl.Create(
                    "Keine Spiele gefunden",
                    "Lade zuerst die Spielebibliothek, damit hier Audio-Regler angezeigt werden."));
                return;
            }

            foreach (var game in games)
                AddAudioListControl(CreateAudioGameCard(game));
        }
        finally
        {
            audioGameListTable.ResumeLayout();
        }
    }

    // ──────────────────────────── Audio logic ────────────────────────────

    private void WireEvents()
    {
        gameSlider.ValueChanged += (_, _) => OnSliderChanged();
        musicSlider.ValueChanged += (_, _) => OnSliderChanged();
        saveButton.Click += (_, _) => SaveAudioChanges();
        selectAllButton.Click += (_, _) => SelectAllGames();
        toggleSelectionButton.Click += (_, _) => ToggleGameSelection();
        outputDeviceComboBox.SelectedIndexChanged += (_, _) => ApplyGlobalOutputDevice();
    }

    private void OnSliderChanged()
    {
        UpdateValueLabel(gameValueLabel, gameSlider.Value);
        UpdateValueLabel(musicValueLabel, musicSlider.Value);

        foreach (var binding in GetGameBindings())
        {
            if (binding.CheckBox.Checked)
                binding.VolumeLabel.Text = $"Game: {gameSlider.Value}%  |  Music: {musicSlider.Value}%";
        }

        if (!isUpdatingSliders)
            SetHasPendingChanges(true);
    }

    private void UpdateGlobalSliderSnapshot(IEnumerable<Game.Record> games)
    {
        var snapshot = games.ToArray();
        isUpdatingSliders = true;
        try
        {
            gameSlider.Value = GetAverageValue(snapshot.Select(g => g.GameVolumePercent ?? Game.GAME_VOLUME_PERCENT), 100);
            musicSlider.Value = GetAverageValue(snapshot.Select(g => g.MusicVolumePercent ?? Game.MUSIC_VOLUME_PERCENT), 50);
            UpdateValueLabel(gameValueLabel, gameSlider.Value);
            UpdateValueLabel(musicValueLabel, musicSlider.Value);
        }
        finally
        {
            isUpdatingSliders = false;
        }
    }

    private Control CreateAudioGameCard(Game.Record game)
    {
        var card = GameAudioCardControl.Create(game, out var checkBox, out var volumeLabel, out var outputDeviceLabel);
        card.Tag = new GameCheckBinding(game, checkBox, volumeLabel, outputDeviceLabel);
        checkBox.CheckedChanged += (_, _) => UpdateSelectAllButtonText();
        return card;
    }

    private void SaveAudioChanges()
    {
        if (!hasPendingChanges || Game.InstalledGames is null)
            return;

        foreach (var binding in GetGameBindings())
        {
            if (!binding.CheckBox.Checked)
                continue;

            binding.Game.GameVolumePercent = gameSlider.Value;
            binding.Game.MusicVolumePercent = musicSlider.Value;
            binding.VolumeLabel.Text = $"Game: {gameSlider.Value}%  |  Music: {musicSlider.Value}%";
        }

        Game.SaveGames();
        SetHasPendingChanges(false);
    }

    private void SelectAllGames()
    {
        var bindings = GetGameBindings().ToArray();
        bool allChecked = bindings.Length > 0 && bindings.All(b => b.CheckBox.Checked);
        foreach (var b in bindings)
            b.CheckBox.Checked = !allChecked;
    }

    private void UpdateSelectAllButtonText()
    {
        var bindings = GetGameBindings().ToArray();
        bool allChecked = bindings.Length > 0 && bindings.All(b => b.CheckBox.Checked);
        selectAllButton.Text = allChecked ? "Alle abwählen" : "Alle auswählen";
    }

    private void ApplyGlobalOutputDevice()
    {
        var selected = outputDeviceComboBox.SelectedItem?.ToString() ?? "(Standard-Gerät)";
        var deviceToSave = selected == "(Standard-Gerät)" ? null : selected;

        foreach (var binding in GetGameBindings())
        {
            if (!binding.CheckBox.Checked)
                continue;

            binding.Game.AudioOutputDevice = deviceToSave;
            binding.OutputDeviceLabel.Text = selected;
        }

        SetHasPendingChanges(true);
    }

    private void ToggleGameSelection()
    {
        foreach (var b in GetGameBindings())
            b.CheckBox.Checked = !b.CheckBox.Checked;
    }

    private IEnumerable<GameCheckBinding> GetGameBindings()
        => audioGameListTable.Controls.OfType<Panel>().Select(c => c.Tag).OfType<GameCheckBinding>();

    private void ShowAudioMessageCard(string title, string message)
    {
        UpdateGlobalSliderSnapshot(Array.Empty<Game.Record>());
        audioGameListTable.SuspendLayout();
        try
        {
            audioGameListTable.Controls.Clear();
            audioGameListTable.RowStyles.Clear();
            audioGameListTable.RowCount = 0;
            AddAudioListControl(MessageCardControl.Create(title, message));
        }
        finally
        {
            audioGameListTable.ResumeLayout();
        }
    }

    private void AddAudioListControl(Control control)
    {
        var rowIndex = audioGameListTable.RowCount++;
        audioGameListTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        audioGameListTable.Controls.Add(control, 0, rowIndex);
    }

    private void SetAudioControlsEnabled(bool enabled)
    {
        gameSlider.Enabled = enabled;
        musicSlider.Enabled = enabled;
        selectAllButton.Enabled = enabled;
        toggleSelectionButton.Enabled = enabled;
        outputDeviceComboBox.Enabled = enabled;
        saveButton.Enabled = enabled && hasPendingChanges;
    }

    private void SetHasPendingChanges(bool value)
    {
        hasPendingChanges = value;
        saveButton.Enabled = value && gameSlider.Enabled;
        saveButton.Text = value ? "Save*" : "Save";
    }

    // ──────────────────────────── Static helpers ────────────────────────────

    private static void AddSliderRow(TableLayoutPanel layout, int rowIndex, string title, TrackBar slider, Label valueLabel)
    {
        layout.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Margin = new Padding(0, 6, 0, 0)
        }, 0, rowIndex);
        layout.Controls.Add(slider, 1, rowIndex);
        layout.Controls.Add(valueLabel, 2, rowIndex);
    }

    private static void UpdateValueLabel(Label label, int value) => label.Text = $"{value}%";

    private static int GetAverageValue(IEnumerable<int> values, int fallback)
    {
        var arr = values.ToArray();
        return arr.Length == 0 ? fallback : (int)Math.Round(arr.Average());
    }

    private static TrackBar CreateSlider(int value) => new()
    {
        Minimum = 0, Maximum = 100,
        Value = Math.Clamp(value, 0, 100),
        TickFrequency = 10, TickStyle = TickStyle.None,
        SmallChange = 5, LargeChange = 10,
        Dock = DockStyle.Fill, Margin = new Padding(0)
    };

    private static Label CreateValueLabel(int value) => new()
    {
        Text = $"{value}%",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        ForeColor = ColorThemes.GetSecondaryTextColor(),
        Margin = new Padding(8, 6, 0, 0)
    };

    private static Button CreateSaveButton()
    {
        var b = new Button
        {
            Text = "Save",
            Width = 80,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorThemes.GetSecondaryBackgroundColor(),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Cursor = Cursors.Hand,
            Enabled = false,
            Margin = new Padding(8, 0, 0, 0)
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseDownBackColor = ColorThemes.CurrentTheme.GetHoveredColor(b.BackColor);
        b.FlatAppearance.MouseOverBackColor = ColorThemes.CurrentTheme.GetHoveredColor(b.BackColor);
        return b;
    }

    private static Button CreateActionButton(string text)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = ColorThemes.GetSecondaryBackgroundColor(),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
            Cursor = Cursors.Hand,
            Enabled = false,
            Margin = new Padding(6, 0, 0, 0),
            Padding = new Padding(10, 0, 10, 0)
        };
        b.FlatAppearance.BorderColor = ColorThemes.GetSecondaryBackgroundColor();
        b.FlatAppearance.BorderSize = 1;
        b.FlatAppearance.MouseDownBackColor = ColorThemes.CurrentTheme.GetHoveredColor(b.BackColor);
        b.FlatAppearance.MouseOverBackColor = ColorThemes.CurrentTheme.GetHoveredColor(b.BackColor);
        return b;
    }

    private static ComboBox CreateOutputDeviceComboBox()
    {
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220,
            FlatStyle = FlatStyle.Flat,
            Enabled = false,
            Margin = new Padding(6, 0, 0, 0)
        };
        combo.BackColor = ColorThemes.GetSecondaryBackgroundColor();
        combo.ForeColor = ColorThemes.GetPrimaryTextColor();
        combo.Items.Add("(Standard-Gerät)");
        foreach (var device in GameAudioCardControl.GetAudioOutputDeviceNames())
            combo.Items.Add(device);
        combo.SelectedIndex = 0;
        return combo;
    }

    private sealed record GameCheckBinding(
        Game.Record Game, CheckBox CheckBox, Label VolumeLabel, Label OutputDeviceLabel);
}