namespace SystemGameManager.View.Elements;

using SystemGameManager.View.Components;
using System.Windows.Forms;
using Svg;

class Navbar
{
    public Panel navbar = new Panel();
    private Panel topNavigation = new Panel();
    public Panel centerNavigation = new Panel();
    public Panel bottomNavigation = new Panel();
    private const string NAVBAR_MENU_ICON_PATH = "assets/icons/bars-solid-full.svg";
    private const string NAVBAR_MENU_TEXT = "Menu";

    public void RenderNavbar(Panel container)
    {
        navbar = new Panel()
        {
            Dock = DockStyle.Left,
            Width = 60,
            Padding = new Padding(10),
            BackColor = ColorThemes.GetSecondaryBackgroundColor()
        };
        container.Controls.Add(navbar);

        CreateNavbarItems();
        CreateNavbarMenuButton();
    }
    public void ToggleNavbarWidth()
    {
        navbar.Width = navbar.Width == 60 ? 200 : 60;

    }

    private void CreateNavbarItems()
    {
        // Top Section
        topNavigation = new Panel()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0,0,0,20),
            BackColor = Color.Transparent
        };
        this.navbar.Controls.Add(topNavigation);

        // Center Section
        centerNavigation = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };
        this.navbar.Controls.Add(centerNavigation);

        // Bottom Section
        bottomNavigation = new Panel()
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.Blue
        };
        this.navbar.Controls.Add(bottomNavigation);
    }
    private void CreateNavbarMenuButton()
    {
        var menuButton = new NormalButton()
        {
            Width = 30,
            Height = 30,
            Image = UIHelpers.LoadIcon(NAVBAR_MENU_ICON_PATH, new Size(30, 30)),
        };
        topNavigation.Controls.Add(menuButton);

        menuButton.Click += (sender, e) =>
        {
            ToggleNavbarWidth();
        };
    }
}