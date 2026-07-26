namespace SystemGameManager.View;

using System.Drawing;
using System.Windows.Forms;
using SystemGameManager.Games.Controller;
using SystemGameManager.Games.Service;
using SystemGameManager.View.Service;
using SystemGameManager.View.Components;
using SystemGameManager.View.Elements;
using SystemGameManager.View.Pages;

public class MainForm : Form
{
    private readonly Button btnLoadInfo = new Button();
    private readonly Label statusLabel = new Label();
    // private readonly GameViewService gameViewService = new GameViewService();
    // private readonly GameInfoView gameInfoView;
    // private readonly GameAudioView gameAudioView = new GameAudioView();
    private GameAudioController? gameAudioController;
    private readonly ViewService viewService = new ViewService();
    public static Navbar navbar = new Navbar(); 
    private readonly Header header = new Header();
    public static Panel container = new Panel();

    public MainForm()
    {
        Text = $"System & Game Manager (v{ViewService.GetVersionFromConfig()})";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 640);
        Width = 1180;
        Height = 760;
        BackColor = ColorThemes.GetPrimaryBackgroundColor(); 
        DoubleBuffered = true;

        var body = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0),
            BackColor = Color.Transparent
        };
        navbar.RenderNavbar(body);
        header.RenderHeader(body);
        this.Controls.Add(body);

        container = new Panel()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(60, 0, 0, 0),
            BackColor = Color.Transparent
        };
        body.Controls.Add(container);

        // Initialize pages
        var menuPage = new GameManager();
        new MenuPage();
        new Settings();
        new Info();

    }
}