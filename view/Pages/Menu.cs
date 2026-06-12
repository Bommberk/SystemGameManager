namespace SystemGameManager.View.Pages;

using System.Drawing;
using SystemGameManager.View.Components;

class MenuPage : Page
{
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