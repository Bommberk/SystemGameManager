namespace SystemGameManager.View.Pages;

using System.Drawing;
using SystemGameManager.View.Components;
using SystemGameManager.View.Elements;

class MenuPage : Page
{
    private const string MENUPAGE_ICON_PATH = "assets/icons/house-solid-full.svg";
    private const string MENUPAGE_TEXT = "Home";

    public MenuPage(Navbar navbar)
    {
        SetTabTitle(MENUPAGE_TEXT);
        SetTabIconPath(MENUPAGE_ICON_PATH);
        SetNavbar(navbar);
        CreateNavTab();
        CreatePageInput();
    }

    public void CreatePageInput()
    {
        var text = new Label()
        {
            Text = "Willkommen zum System & Game Manager!",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        page.Controls.Add(text);
    }
}