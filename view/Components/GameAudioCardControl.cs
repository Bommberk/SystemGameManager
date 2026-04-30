using System.Drawing;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Entity;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class GameAudioCardControl
{
    public static Panel Create(Game.Record game, out TrackBar gameSlider, out TrackBar musicSlider, out Label gameValueLabel, out Label musicValueLabel)
    {
        var card = new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(14),
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 12)
        };

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

        gameSlider = CreateSlider(game.GameVolumePercent ?? Game.GAME_VOLUME_PERCENT);
        musicSlider = CreateSlider(game.MusicVolumePercent ?? Game.MUSIC_VOLUME_PERCENT);
        gameValueLabel = CreateValueLabel(game.GameVolumePercent ?? Game.GAME_VOLUME_PERCENT);
        musicValueLabel = CreateValueLabel(game.MusicVolumePercent ?? Game.MUSIC_VOLUME_PERCENT);

        AddSliderRow(sliderLayout, 0, "Game", gameSlider, gameValueLabel);
        AddSliderRow(sliderLayout, 1, "Music", musicSlider, musicValueLabel);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(pathLabel, 0, 1);
        layout.Controls.Add(sliderLayout, 0, 2);

        card.Controls.Add(layout);
        return card;
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
}
