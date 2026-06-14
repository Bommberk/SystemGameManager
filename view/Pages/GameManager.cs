using SystemGameManager.View.Components;
using SystemGameManager.View.Pages;

namespace SystemGameManager.View;

class GameManager : Page
{
    private const string TAB_ICON_PATH = "assets/icons/gamepad-solid-full.svg";
    private const string TAB_TEXT = "Game Manager";
    private const string PAGE_TITLE = "Hier finden Sie Spiele und Einstellungen";

    public GameManager() : base(TAB_TEXT, TAB_ICON_PATH, "center")
    {
        CreatePageInput();
    }

    private void CreatePageInput()
    {
        var text = new Label()
        {
            Text = PAGE_TITLE,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        page.Controls.Add(text);
    }
}