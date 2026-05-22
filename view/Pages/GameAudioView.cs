namespace SystemGameManager.View;

using System.Drawing;
using System.Windows.Forms;
using SystemGameManager.Games.Entity;
using SystemGameManager.Games.Service;
using SystemGameManager.View.Components;

internal sealed class GameAudioView
{
    private readonly TrackBar allGameSlider = CreateSlider(100);
    private readonly TrackBar allMusicSlider = CreateSlider(50);
    private readonly Label allGameValueLabel = CreateValueLabel(100);
    private readonly Label allMusicValueLabel = CreateValueLabel(50);
    private readonly Button saveButton = CreateSaveButton();
    private readonly Button selectAllButton = CreateActionButton("Alle auswählen");
    private readonly Button toggleSelectionButton = CreateActionButton("Auswahl umkehren");
    private readonly ComboBox globalOutputDeviceComboBox = CreateGlobalOutputDeviceComboBox();
    private readonly TableLayoutPanel gameListTable = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 1,
        Margin = new Padding(0),
        Padding = new Padding(0),
        BackColor = Color.Transparent
    };

    private readonly Panel gameListHost = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        BackColor = Color.FromArgb(245, 247, 250)
    };

    private bool isUpdatingSliders;
    private bool hasPendingChanges;

    public GameAudioView()
    {
        gameListTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        gameListHost.Controls.Add(gameListTable);

        allGameSlider.ValueChanged += (_, _) => ApplyGlobalVolumes();
        allMusicSlider.ValueChanged += (_, _) => ApplyGlobalVolumes();
        saveButton.Click += (_, _) => SaveChanges();
        selectAllButton.Click += (_, _) => SelectAll();
        toggleSelectionButton.Click += (_, _) => ToggleSelection();
        globalOutputDeviceComboBox.SelectedIndexChanged += (_, _) => ApplyGlobalOutputDevice();
    }

    public TabPage CreateTab()
    {
        var tab = new TabPage("Game-Audio-Manager")
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

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(0)
        };

        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label()
        {
            Text = "Audio-Steuerung für Spiele und Musik",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 0, 0)
        };

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(saveButton, 1, 0);

        var globalPanel = CreateSectionPanel();
        var globalLayout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        globalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        globalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        globalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

        var globalTitle = new Label()
        {
            Text = "Lautstärke für ausgewählte Spiele",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 8)
        };

        globalLayout.Controls.Add(globalTitle, 0, 0);
        globalLayout.SetColumnSpan(globalTitle, 3);
        AddSliderRow(globalLayout, 1, "Game-Lautstärke", allGameSlider, allGameValueLabel);
        AddSliderRow(globalLayout, 2, "Musik-Lautstärke", allMusicSlider, allMusicValueLabel);
        globalPanel.Controls.Add(globalLayout);

        var separator = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = Color.FromArgb(221, 227, 237),
            Margin = new Padding(0, 6, 0, 10)
        };

        var listHeader = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(0)
        };

        listHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        listHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        listHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        listHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var listTitle = new Label()
        {
            Text = "Spiele auswählen",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(0, 6, 0, 0)
        };

        listHeader.Controls.Add(listTitle, 0, 0);
        listHeader.Controls.Add(globalOutputDeviceComboBox, 1, 0);
        listHeader.Controls.Add(selectAllButton, 2, 0);
        listHeader.Controls.Add(toggleSelectionButton, 3, 0);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(globalPanel, 0, 1);
        layout.Controls.Add(separator, 0, 2);
        layout.Controls.Add(listHeader, 0, 3);
        layout.Controls.Add(gameListHost, 0, 4);

        tab.Controls.Add(layout);
        return tab;
    }

    public void ShowLoadingState()
    {
        SetControlsEnabled(false);
        SetHasPendingChanges(false);
        ShowMessageCard("Spiele werden geladen...", "Nach dem Laden kannst du hier globale und spielbezogene Audio-Werte anpassen.");
    }

    public void ShowErrorState(string message)
    {
        SetControlsEnabled(false);
        SetHasPendingChanges(false);
        ShowMessageCard("Audio-Einstellungen konnten nicht geladen werden", message);
    }

    public void RefreshGames()
    {
        Game.InstalledGames = Game.GetGames();
        var games = Game.InstalledGames ?? Array.Empty<Game.Record>();

        UpdateGlobalSliderSnapshot(games);
        SetControlsEnabled(games.Length > 0);
        SetHasPendingChanges(false);

        gameListTable.SuspendLayout();

        try
        {
            gameListTable.Controls.Clear();
            gameListTable.RowStyles.Clear();
            gameListTable.RowCount = 0;

            if (games.Length == 0)
            {
                AddGameListControl(MessageCardControl.Create("Keine Spiele gefunden", "Lade zuerst die Spielebibliothek, damit hier Audio-Regler angezeigt werden."));
                return;
            }

            foreach (var game in games)
            {
                AddGameListControl(CreateGameCard(game));
            }
        }
        finally
        {
            gameListTable.ResumeLayout();
        }
    }

    private void ApplyGlobalVolumes()
    {
        UpdateValueLabel(allGameValueLabel, allGameSlider.Value);
        UpdateValueLabel(allMusicValueLabel, allMusicSlider.Value);

        foreach (var binding in GetGameBindings())
        {
            if (binding.CheckBox.Checked)
            {
                binding.VolumeLabel.Text = $"Game: {allGameSlider.Value}%  |  Music: {allMusicSlider.Value}%";
            }
        }

        if (isUpdatingSliders)
        {
            return;
        }

        SetHasPendingChanges(true);
    }

    private void UpdateGlobalSliderSnapshot(IEnumerable<Game.Record> games)
    {
        var snapshot = games.ToArray();

        isUpdatingSliders = true;
        try
        {
            allGameSlider.Value = GetAverageValue(snapshot.Select(game => game.GameVolumePercent ?? Game.GAME_VOLUME_PERCENT), 100);
            allMusicSlider.Value = GetAverageValue(snapshot.Select(game => game.MusicVolumePercent ?? Game.MUSIC_VOLUME_PERCENT), 50);
            UpdateValueLabel(allGameValueLabel, allGameSlider.Value);
            UpdateValueLabel(allMusicValueLabel, allMusicSlider.Value);
        }
        finally
        {
            isUpdatingSliders = false;
        }
    }

    private Control CreateGameCard(Game.Record game)
    {
        var card = GameAudioCardControl.Create(game, out var checkBox, out var volumeLabel, out var outputDeviceLabel);
        card.Tag = new GameCheckBinding(game, checkBox, volumeLabel, outputDeviceLabel);
        checkBox.CheckedChanged += (_, _) => UpdateSelectAllButton();
        return card;
    }

    private void SaveChanges()
    {
        if (!hasPendingChanges || Game.InstalledGames is null)
        {
            return;
        }

        foreach (var binding in GetGameBindings())
        {
            if (!binding.CheckBox.Checked)
            {
                continue;
            }

            binding.Game.GameVolumePercent = allGameSlider.Value;
            binding.Game.MusicVolumePercent = allMusicSlider.Value;
            binding.VolumeLabel.Text = $"Game: {allGameSlider.Value}%  |  Music: {allMusicSlider.Value}%";
        }

        Game.SaveGames();
        SetHasPendingChanges(false);
    }

    private void SelectAll()
    {
        var bindings = GetGameBindings().ToArray();
        bool allChecked = bindings.Length > 0 && bindings.All(b => b.CheckBox.Checked);
        foreach (var binding in bindings)
        {
            binding.CheckBox.Checked = !allChecked;
        }
    }

    private void UpdateSelectAllButton()
    {
        var bindings = GetGameBindings().ToArray();
        bool allChecked = bindings.Length > 0 && bindings.All(b => b.CheckBox.Checked);
        selectAllButton.Text = allChecked ? "Alle abwählen" : "Alle auswählen";
    }

    private void ApplyGlobalOutputDevice()
    {
        var selectedDevice = globalOutputDeviceComboBox.SelectedItem?.ToString() ?? "(Standard-Gerät)";
        var deviceToSave = selectedDevice == "(Standard-Gerät)" ? null : selectedDevice;

        foreach (var binding in GetGameBindings())
        {
            if (!binding.CheckBox.Checked)
            {
                continue;
            }

            binding.Game.AudioOutputDevice = deviceToSave;
            binding.OutputDeviceLabel.Text = selectedDevice;
        }

        SetHasPendingChanges(true);
    }

    private void ToggleSelection()
    {
        foreach (var binding in GetGameBindings())
        {
            binding.CheckBox.Checked = !binding.CheckBox.Checked;
        }
    }

    private IEnumerable<GameCheckBinding> GetGameBindings()
        => gameListTable.Controls.OfType<Panel>().Select(c => c.Tag).OfType<GameCheckBinding>();

    private void ShowMessageCard(string title, string message)
    {
        UpdateGlobalSliderSnapshot(Array.Empty<Game.Record>());

        gameListTable.SuspendLayout();
        try
        {
            gameListTable.Controls.Clear();
            gameListTable.RowStyles.Clear();
            gameListTable.RowCount = 0;
            AddGameListControl(MessageCardControl.Create(title, message));
        }
        finally
        {
            gameListTable.ResumeLayout();
        }
    }

    private void AddGameListControl(Control control)
    {
        var rowIndex = gameListTable.RowCount++;
        gameListTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        gameListTable.Controls.Add(control, 0, rowIndex);
    }

    private static void AddSliderRow(TableLayoutPanel layout, int rowIndex, string title, TrackBar slider, Label valueLabel)
    {
        layout.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Color.FromArgb(55, 65, 81),
            Margin = new Padding(0, 6, 0, 0)
        }, 0, rowIndex);

        layout.Controls.Add(slider, 1, rowIndex);
        layout.Controls.Add(valueLabel, 2, rowIndex);
    }

    private static Panel CreateSectionPanel()
    {
        return new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(14),
            BackColor = Color.White
        };
    }

    private void SetControlsEnabled(bool enabled)
    {
        allGameSlider.Enabled = enabled;
        allMusicSlider.Enabled = enabled;
        selectAllButton.Enabled = enabled;
        toggleSelectionButton.Enabled = enabled;
        globalOutputDeviceComboBox.Enabled = enabled;
        saveButton.Enabled = enabled && hasPendingChanges;
    }

    private void SetHasPendingChanges(bool value)
    {
        hasPendingChanges = value;
        saveButton.Enabled = hasPendingChanges && allGameSlider.Enabled;
        saveButton.Text = hasPendingChanges ? "Save*" : "Save";
    }

    private static TrackBar CreateSlider(int value)
    {
        return new TrackBar()
        {
            Minimum = 0,
            Maximum = 100,
            Value = Math.Clamp(value, 0, 100),
            TickFrequency = 10,
            TickStyle = TickStyle.None,
            SmallChange = 5,
            LargeChange = 10,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
    }

    private static Label CreateValueLabel(int value)
    {
        return new Label()
        {
            Text = $"{value}%",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(67, 56, 202),
            Margin = new Padding(8, 6, 0, 0)
        };
    }

    private static Button CreateSaveButton()
    {
        var button = new Button()
        {
            Text = "Save",
            Width = 100,
            Height = 34,
            Anchor = AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(37, 99, 235),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Enabled = false,
            Margin = new Padding(12, 0, 0, 0)
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(29, 78, 216);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(59, 130, 246);
        return button;
    }

    private static Button CreateActionButton(string text)
    {
        var button = new Button()
        {
            Text = text,
            AutoSize = true,
            Height = 30,
            Anchor = AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(243, 244, 246),
            ForeColor = Color.FromArgb(31, 41, 55),
            Cursor = Cursors.Hand,
            Enabled = false,
            Margin = new Padding(6, 0, 0, 0),
            Padding = new Padding(10, 0, 10, 0)
        };

        button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(229, 231, 235);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(249, 250, 251);
        return button;
    }

    private static ComboBox CreateGlobalOutputDeviceComboBox()
    {
        var combo = new ComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220,
            FlatStyle = FlatStyle.Flat,
            Enabled = false,
            Margin = new Padding(6, 0, 0, 0)
        };

        combo.Items.Add("(Standard-Gerät)");
        foreach (var deviceName in GameAudioCardControl.GetAudioOutputDeviceNames())
        {
            combo.Items.Add(deviceName);
        }
        combo.SelectedIndex = 0;
        return combo;
    }

    private static void UpdateValueLabel(Label label, int value)
    {
        label.Text = $"{value}%";
    }

    private static int GetAverageValue(IEnumerable<int> values, int fallback)
    {
        var snapshot = values.ToArray();
        return snapshot.Length == 0
            ? fallback
            : (int)Math.Round(snapshot.Average());
    }

    private sealed record GameCheckBinding(Game.Record Game, CheckBox CheckBox, Label VolumeLabel, Label OutputDeviceLabel);
}