namespace SystemGameManager.View.Pages;

using SystemGameManager.View;
using SystemGameManager.View.Components;
using SystemGameManager.View.Elements;

class Page
{
    private Panel container;
    protected Panel page;
    private string? tabTitle;
    private string? tabIconPath;
    protected Navbar Navbar = new Navbar();

    public Page()
    {
        container = MainForm.container;
        page = SetPagePanel();
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
    protected void SetTabTitle(string text)
    {
        tabTitle = text;
    }
    protected void SetTabIconPath(string path)
    {
        tabIconPath = path;
    }

    public void SetNavbar(Navbar navbar)
    {
        this.Navbar = navbar;
    }

    protected void CreateNavTab()
    {
        var TabButton = new NormalButton()
        {
            Dock = DockStyle.Top,
            Width = 30,
            Height = 30,
            Image = UIHelpers.LoadIcon(tabIconPath!, new Size(30, 30)),
        };
        this.Navbar.centerNavigation.Controls.Add(TabButton);
    }
}