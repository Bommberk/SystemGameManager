using System.Drawing;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Entity;
using Krassheiten.SystemGameManager.Service;

namespace Krassheiten.SystemGameManager.View;

internal sealed class GameAudioView
{
    private readonly GameAudioService gameAudioService = new();
    private readonly TrackBar allGameSlider = CreateSlider(100);
    private readonly TrackBar allMusicSlider = CreateSlider(50);
    private readonly Label allGameValueLabel = CreateValueLabel(100);
    private readonly Label allMusicValueLabel = CreateValueLabel(50);
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

    private bool isUpdatingControls;

    public GameAudioView()
    {
        gameListTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        gameListHost.Controls.Add(gameListTable);

        allGameSlider.ValueChanged += (_, _) => ApplyGlobalVolumes();
        allMusicSlider.ValueChanged += (_, _) => ApplyGlobalVolumes();
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

        var title = new Label()
        {
            Text = "Audio-Steuerung für Spiele und Musik",
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 0, 0, 12)
        };

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
            Text = "Globale Werte für alle Spiele",
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

        var listTitle = new Label()
        {
            Text = "Pro Spiel",
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 41, 55),
            Margin = new Padding(0, 0, 0, 10)
        };

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(globalPanel, 0, 1);
        layout.Controls.Add(separator, 0, 2);
        layout.Controls.Add(listTitle, 0, 3);
        layout.Controls.Add(gameListHost, 0, 4);

        tab.Controls.Add(layout);
        return tab;
    }

    public void ShowLoadingState()
    {
        SetGlobalControlsEnabled(false);
        ShowMessageCard("Spiele werden geladen...", "Nach dem Laden kannst du hier globale und spielbezogene Audio-Werte anpassen.");
    }

    public void ShowErrorState(string message)
    {
        SetGlobalControlsEnabled(false);
        ShowMessageCard("Audio-Einstellungen konnten nicht geladen werden", message);
    }

    public void RefreshGames()
    {
        var games = (Game.InstalledGames ?? Array.Empty<Game.Record>())
            .OrderBy(game => game.Name)
            .ToArray();

        UpdateGlobalSliderSnapshot(games);
        SetGlobalControlsEnabled(games.Length > 0);

        gameListTable.SuspendLayout();

        try
        {
            gameListTable.Controls.Clear();
            gameListTable.RowStyles.Clear();
            gameListTable.RowCount = 0;

            if (games.Length == 0)
            {
                AddGameListControl(CreateMessageCard("Keine Spiele gefunden", "Lade zuerst die Spielebibliothek, damit hier Audio-Regler angezeigt werden."));
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

        if (isUpdatingControls)
        {
            return;
        }

        gameAudioService.SetAudioSettings(musicVolume: allMusicSlider.Value, gameVolume: allGameSlider.Value);

        isUpdatingControls = true;
        try
        {
            foreach (var control in gameListTable.Controls.OfType<Panel>())
            {
                if (control.Tag is not SliderBinding binding)
                {
                    continue;
                }

                binding.GameSlider.Value = allGameSlider.Value;
                binding.MusicSlider.Value = allMusicSlider.Value;
                UpdateValueLabel(binding.GameValueLabel, binding.GameSlider.Value);
                UpdateValueLabel(binding.MusicValueLabel, binding.MusicSlider.Value);
            }
        }
        finally
        {
            isUpdatingControls = false;
        }
    }

    private void ApplyGameVolumes(Game.Record game, TrackBar gameSlider, Label gameValueLabel, TrackBar musicSlider, Label musicValueLabel)
    {
        UpdateValueLabel(gameValueLabel, gameSlider.Value);
        UpdateValueLabel(musicValueLabel, musicSlider.Value);

        if (isUpdatingControls)
        {
            return;
        }

        gameAudioService.SetAudioSettings(
            game: game,
            gameVolume: gameSlider.Value,
            musicVolume: musicSlider.Value);
    }

    private void UpdateGlobalSliderSnapshot(IEnumerable<Game.Record> games)
    {
        var snapshot = games.ToArray();

        isUpdatingControls = true;
        try
        {
            allGameSlider.Value = GetAverageValue(snapshot.Select(game => game.GameVolumePercent), 100);
            allMusicSlider.Value = GetAverageValue(snapshot.Select(game => game.MusicVolumePercent), 50);
            UpdateValueLabel(allGameValueLabel, allGameSlider.Value);
            UpdateValueLabel(allMusicValueLabel, allMusicSlider.Value);
        }
        finally
        {
            isUpdatingControls = false;
        }
    }

    private Control CreateGameCard(Game.Record game)
    {
        var card = CreateSectionPanel();
        card.Margin = new Padding(0, 0, 0, 12);

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label()
        {
            Text = game.Name,
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 4)
        };

        var pathLabel = new Label()
        {
            Text = string.IsNullOrWhiteSpace(game.InstallFolderPath) ? "Pfad nicht verfügbar" : game.InstallFolderPath,
            AutoSize = true,
            ForeColor = Color.FromArgb(107, 114, 128),
            Margin = new Padding(0, 0, 0, 10)
        };

        var sliderLayout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        sliderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        sliderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sliderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));

        var gameSlider = CreateSlider(game.GameVolumePercent);
        var musicSlider = CreateSlider(game.MusicVolumePercent);
        var gameValueLabel = CreateValueLabel(game.GameVolumePercent);
        var musicValueLabel = CreateValueLabel(game.MusicVolumePercent);

        gameSlider.ValueChanged += (_, _) => ApplyGameVolumes(game, gameSlider, gameValueLabel, musicSlider, musicValueLabel);
        musicSlider.ValueChanged += (_, _) => ApplyGameVolumes(game, gameSlider, gameValueLabel, musicSlider, musicValueLabel);

        AddSliderRow(sliderLayout, 0, "Game", gameSlider, gameValueLabel);
        AddSliderRow(sliderLayout, 1, "Music", musicSlider, musicValueLabel);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(pathLabel, 0, 1);
        layout.Controls.Add(sliderLayout, 0, 2);

        card.Tag = new SliderBinding(gameSlider, gameValueLabel, musicSlider, musicValueLabel);
        card.Controls.Add(layout);
        return card;
    }

    private void ShowMessageCard(string title, string message)
    {
        UpdateGlobalSliderSnapshot(Array.Empty<Game.Record>());

        gameListTable.SuspendLayout();
        try
        {
            gameListTable.Controls.Clear();
            gameListTable.RowStyles.Clear();
            gameListTable.RowCount = 0;
            AddGameListControl(CreateMessageCard(title, message));
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

    private static Control CreateMessageCard(string title, string message)
    {
        var card = CreateSectionPanel();

        var layout = new FlowLayoutPanel()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };

        layout.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 8)
        });

        layout.Controls.Add(new Label
        {
            Text = message,
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            ForeColor = Color.FromArgb(107, 114, 128)
        });

        card.Controls.Add(layout);
        return card;
    }

    private void SetGlobalControlsEnabled(bool enabled)
    {
        allGameSlider.Enabled = enabled;
        allMusicSlider.Enabled = enabled;
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

    private sealed record SliderBinding(TrackBar GameSlider, Label GameValueLabel, TrackBar MusicSlider, Label MusicValueLabel);
}