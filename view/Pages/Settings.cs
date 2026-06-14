using SystemGameManager.View.Components;
using SystemGameManager.View.Elements;

namespace SystemGameManager.View.Pages;

class Settings : Page
{
    private const string TAB_ICON_PATH = "assets/icons/gear-solid-full.svg";
    private const string TAB_TEXT = "Settings";
    private const string PAGE_TITLE = "Hier finden Sie Einstellungen";
    
    public Settings() : base(TAB_TEXT, TAB_ICON_PATH, "bottom")
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