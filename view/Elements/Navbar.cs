namespace SystemGameManager.View.Elements;

using SystemGameManager.View.Components;
using System.Windows.Forms;

class Navbar
{
    public void RenderNavbar(Panel container)
    {
        var navbar = new Panel()
        {
            Dock = DockStyle.Left,
            Width = 60,
            Padding = new Padding(12),
            BackColor = ColorThemes.GetSecondaryBackgroundColor()
        };
        container.Controls.Add(navbar);
    }
}