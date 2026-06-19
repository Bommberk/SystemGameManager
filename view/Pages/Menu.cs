namespace SystemGameManager.View.Pages;

using System.Drawing;
using SystemGameManager.View.Components;
using SystemGameManager.View.Elements;

class MenuPage : Page
{
    private const string TAB_ICON_PATH = "assets/icons/house-solid-full.svg";
    private const string TAB_TEXT = "Home";
    private const string PAGE_TITLE = "Willkommen zum SystemGameManager";

    public MenuPage() : base(TAB_TEXT, TAB_ICON_PATH, "center")
    {
        CreatePageInput();
    }

    public void CreatePageInput()
    {
        var text = new Label()
        {
            Text = PAGE_TITLE,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = ColorThemes.GetSecondaryTextColor(),
        };
        page.Controls.Add(text);
    }
}