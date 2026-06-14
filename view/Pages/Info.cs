using SystemGameManager.View.Elements;

namespace SystemGameManager.View.Pages;

class Info : Page
{
    private const string TAB_ICON_PATH = "assets/icons/circle-info-solid-full.svg";
    private const string TAB_TEXT = "Info";
    private const string PAGE_TITLE = "Hier finden Sie Informationen";

    public Info() : base(TAB_TEXT, TAB_ICON_PATH, "bottom")
    {
    }
}