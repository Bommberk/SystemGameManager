namespace SystemGameManager.View;

using System.Drawing;
using SystemGameManager.View.Components;

class Menu
{
    public void RenderPage(Panel container)
    {
        var text = new Label()
        {
            Text = "Willkommen zum System & Game Manager!",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = ColorThemes.GetPrimaryTextColor(),
        };
        container.Controls.Add(text);
    }
}