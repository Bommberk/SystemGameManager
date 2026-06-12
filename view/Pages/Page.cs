namespace SystemGameManager.View.Pages;

using SystemGameManager.View;

class Page
{
    private Panel container = MainForm.container;
    protected Panel page;

    public Page()
    {
        RenderPage();
    }

    protected void RenderPage()
    {
        if(container == null) return;
        page = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20,20,20,20),
            BackColor = Color.Transparent
        };
        container.Controls.Add(page);
    }
}