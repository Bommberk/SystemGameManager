namespace SystemGameManager.View.Service;

using System.Threading.Tasks;
using SystemGameManager.Games.Entity;
using SystemGameManager.View.Components;
using SystemGameManager.View.Pages;

class GameManagerViewService
{
    private GameManager page;

    public GameManagerViewService(GameManager gameManagerPage)
    {
        this.page = gameManagerPage;
    }

    public void RefreshGameAndLauncherInfos(NormalButton refreshButton)
    {
        refreshButton.Click += async (_, _) =>
        {
            refreshButton.Enabled = false;
            await Task.Run(() => new GameInfoController());
            page.RefreshGameAndLauncherInfo();
            refreshButton.Enabled = true;
        };
    }
    public void SelectAllGames(NormalButton selectAllGamesButton)
    {
        selectAllGamesButton.Click += (sender, e) =>
        {
            if (page.gameBindings.Count == 0) return;
            bool allChecked = page.gameBindings.All(b => b.CheckBox.Checked);
            foreach (var binding in page.gameBindings)
            {
                binding.CheckBox.Checked = !allChecked;
            }
            UpdateSelectAllButtonText(selectAllGamesButton);
        };
    }
    public void ReverseSelection(NormalButton reverseSelectionButton)
    {
        if(reverseSelectionButton == null) return;
        reverseSelectionButton.Click += (sender, e) =>
        {
            foreach (var binding in page.gameBindings)
            {
                binding.CheckBox.Checked = !binding.CheckBox.Checked;
            }
            UpdateSelectAllButtonText(reverseSelectionButton);
        };
    }
    public void SelectGame(PictureBox gameWallpaper, CheckBox gameSelectCheckBox, NormalButton selectAllGamesButton)
    {
        gameWallpaper.Click += (sender, e) =>
        {
            gameSelectCheckBox.Checked = !gameSelectCheckBox.Checked;
        };

        gameSelectCheckBox.CheckedChanged += (sender, e) =>
        {
            UpdateSelectAllButtonText(selectAllGamesButton);
        };
    }
    public void SetAudioTrackbar(TrackBar audioTrackbar, Label audioTrackbarValueLabel)
    {
        audioTrackbar.ValueChanged += (sender, e) =>
        {
            if (audioTrackbarValueLabel != null && audioTrackbar != null)
                audioTrackbarValueLabel.Text = $"{audioTrackbar.Value}%";
        };
    }
    public void SaveAudioForGame(NormalButton saveButton, ComboBox audioDeviceSelection, TrackBar gameVolumeTrackBar, TrackBar musicVolumeTrackBar, List<GameManager.GameSelectionBinding> gameBindings)
    {
        saveButton.Click += (sender, e) =>
        {
            if (audioDeviceSelection == null || gameVolumeTrackBar == null || musicVolumeTrackBar == null)
                return;

            var selectedDevice = audioDeviceSelection.SelectedItem?.ToString() ?? "(Standard-Gerät)";
            var deviceToSave = selectedDevice == "(Standard-Gerät)" ? null : selectedDevice;

            bool anySaved = false;
            foreach (var binding in gameBindings)
            {
                if (binding.CheckBox.Checked)
                {
                    binding.Game.GameVolumePercent = gameVolumeTrackBar.Value;
                    binding.Game.MusicVolumePercent = musicVolumeTrackBar.Value;
                    binding.Game.AudioOutputDevice = deviceToSave;

                    binding.VolumeLabel.Text = $"Game: {gameVolumeTrackBar.Value}% | Music: {musicVolumeTrackBar.Value}%";
                    binding.AudioOutputDeviceLabel.Text = selectedDevice;
                    anySaved = true;
                }
            }

            if (anySaved)
            {
                Game.InstalledGames = gameBindings.Select(b => b.Game).ToArray();
                Game.SaveGames();
                MessageBox.Show("Einstellungen für die ausgewählten Spiele wurden gespeichert.", "Speichern erfolgreich", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Bitte wähle mindestens ein Spiel aus.", "Keine Auswahl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        };
    }
    private void UpdateSelectAllButtonText(NormalButton selectAllGamesButton)
    {
        if (selectAllGamesButton == null) return;
        bool allChecked = page.gameBindings.Count > 0 && page.gameBindings.All(b => b.CheckBox.Checked);
        selectAllGamesButton.Text = allChecked ? "Alle abwählen" : "Alle auswählen";
    }
}