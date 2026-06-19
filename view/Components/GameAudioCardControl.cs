namespace SystemGameManager.View.Components;

using System.Drawing;
using System.Windows.Forms;
using SystemGameManager.Games.Entity;
using NAudio.CoreAudioApi;

internal static class GameAudioCardControl
{
    public static Panel Create(Game.Record game, out CheckBox selectionCheckBox, out Label volumeLabel, out Label outputDeviceLabel)
    {
        var card = new Panel()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(14),
            BackColor = ColorThemes.GetCardBackgroundColor(),
            Margin = new Padding(0, 0, 0, 12)
        };

        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        selectionCheckBox = new CheckBox()
        {
            Checked = false,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Margin = new Padding(0, 4, 12, 0)
        };

        var title = new Label()
        {
            Text = game.Name,
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Margin = new Padding(0, 0, 0, 4)
        };

        var pathLabel = new Label()
        {
            Text = string.IsNullOrWhiteSpace(game.InstallFolderPath) ? "Pfad nicht verfügbar" : game.InstallFolderPath,
            AutoSize = true,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Margin = new Padding(0, 0, 0, 4)
        };

        volumeLabel = new Label()
        {
            Text = $"Game: {game.GameVolumePercent ?? Game.GAME_VOLUME_PERCENT}%  |  Music: {game.MusicVolumePercent ?? Game.MUSIC_VOLUME_PERCENT}%",
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Margin = new Padding(0, 0, 0, 0)
        };

        var outputDeviceRow = new FlowLayoutPanel()
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 4, 0, 0),
            Padding = new Padding(0)
        };

        var outputDeviceCaption = new Label()
        {
            Text = "Audioausgabe:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Margin = new Padding(0, 4, 8, 0)
        };

        outputDeviceLabel = new Label()
        {
            Text = game.AudioOutputDevice ?? "(Standard-Gerät)",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = ColorThemes.GetSecondaryTextColor(),
            Margin = new Padding(0, 4, 0, 0)
        };

        outputDeviceRow.Controls.Add(outputDeviceCaption);
        outputDeviceRow.Controls.Add(outputDeviceLabel);

        layout.Controls.Add(selectionCheckBox, 0, 0);
        layout.SetRowSpan(selectionCheckBox, 4);
        layout.Controls.Add(title, 1, 0);
        layout.Controls.Add(pathLabel, 1, 1);
        layout.Controls.Add(volumeLabel, 1, 2);
        layout.Controls.Add(outputDeviceRow, 1, 3);

        card.Controls.Add(layout);

        card.Cursor = Cursors.Hand;
        layout.Cursor = Cursors.Hand;

        var checkBox = selectionCheckBox;
        EventHandler toggle = (_, _) => checkBox.Checked = !checkBox.Checked;
        card.Click += toggle;
        layout.Click += toggle;
        title.Click += toggle;
        pathLabel.Click += toggle;
        volumeLabel.Click += toggle;
        outputDeviceCaption.Click += toggle;
        outputDeviceLabel.Click += toggle;

        return card;
    }

    internal static IEnumerable<string> GetAudioOutputDeviceNames()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator
                .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(device => device.FriendlyName)
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}
