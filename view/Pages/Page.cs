namespace SystemGameManager.View.Pages;

using SystemGameManager.View;
using SystemGameManager.View.Components;
using SystemGameManager.View.Elements;

class Page
{
    private Panel container;
    protected Panel page;
    private string? tabText;
    private string? tabIconPath;
    protected Navbar Navbar = new Navbar();
    protected Button navButton = new Button();
    
    public Page(string tabText, string tabIconPath, string navPosition)
    {
        container = MainForm.container;
        Navbar = MainForm.navbar;
        this.tabText = tabText;
        this.tabIconPath = tabIconPath;
        page = SetPagePanel();
        createNavTab(navPosition);
    }
    private void createNavTab(string navPosition)
    {
        switch (navPosition.ToLower())
        {
            case "center":
                CreateCenterNavTab();
                break;
            case "bottom":
                CreateBottomNavTab();
                break;
            default:
                throw new System.Exception("Invalid navigation position. Use 'center' or 'bottom'.");
        }
        this.navButton.Click += (sender, e) =>
        {
            this.page.BringToFront();
        };
    }

    protected Panel SetPagePanel()
    {
        var page = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20,20,20,20),
            BackColor = Color.Transparent
        };
        container.Controls.Add(page);
        return page;
    }

    public void SetNavbar(Navbar navbar)
    {
        this.Navbar = navbar;
    }

    protected void CreateCenterNavTab()
    {
        if(tabIconPath == null) throw new System.Exception("Tab icon path cannot be null");
        navButton = new NormalButton()
        {
            Dock = DockStyle.Top,
            Width = 30,
            Height = 30,
            Image = UIHelpers.LoadIcon(tabIconPath, new Size(30, 30)),
        };
        Panel itemContainer = UIHelpers.ItemContainer(new Padding(0,0,0,10),navButton);
        this.Navbar.centerNavigation.Controls.Add(itemContainer);
        this.Navbar.centerNavigation.BringToFront();
    }
    protected void CreateBottomNavTab()
    {
        if(tabIconPath == null) throw new System.Exception("Tab icon path cannot be null");
        navButton = new NormalButton()
        {
            Dock = DockStyle.Top,
            Width = 30,
            Height = 30,
            Image = UIHelpers.LoadIcon(tabIconPath, new Size(30, 30)),
        };
        Panel itemContainer = UIHelpers.ItemContainer(new Padding(0,0,0,10),navButton);
        itemContainer.Dock = DockStyle.Bottom;
        this.Navbar.bottomNavigation.Controls.Add(itemContainer);
        this.Navbar.bottomNavigation.BringToFront();
    }
}