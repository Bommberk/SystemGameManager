using System.Drawing;
using System.Windows.Forms;
using Krassheiten.SystemGameManager.Entity;
using NAudio.CoreAudioApi;

namespace Krassheiten.SystemGameManager.View.Components;

internal static class GameAudioCardControl
{
    public static Panel Create(Game.Record game, out CheckBox selectionCheckBox, out Label volumeLabel, out ComboBox outputDeviceComboBox)
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
            ForeColor = Color.FromArgb(17, 24, 39),
            Margin = new Padding(0, 0, 0, 4)
        };

        var pathLabel = new Label()
        {
            Text = string.IsNullOrWhiteSpace(game.InstallFolderPath) ? "Pfad nicht verfügbar" : game.InstallFolderPath,
            AutoSize = true,
            ForeColor = Color.FromArgb(107, 114, 128),
            Margin = new Padding(0, 0, 0, 4)
        };

        volumeLabel = new Label()
        {
            Text = $"Game: {game.GameVolumePercent ?? Game.GAME_VOLUME_PERCENT}%  |  Music: {game.MusicVolumePercent ?? Game.MUSIC_VOLUME_PERCENT}%",
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(67, 56, 202),
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

        var outputDeviceLabel = new Label()
        {
            Text = "Audioausgabe",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = Color.FromArgb(55, 65, 81),
            Margin = new Padding(0, 4, 8, 0)
        };

        outputDeviceComboBox = new ComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 250,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 0, 0)
        };

        outputDeviceComboBox.Items.Add("(Standard-Gerät)");
        foreach (var deviceName in GetAudioOutputDeviceNames())
        {
            outputDeviceComboBox.Items.Add(deviceName);
        }
        outputDeviceComboBox.SelectedIndex = 0;

        outputDeviceRow.Controls.Add(outputDeviceLabel);
        outputDeviceRow.Controls.Add(outputDeviceComboBox);

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
