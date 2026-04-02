namespace Krassheiten.Game.Service;

class InfoService
{
    public InfoService(string messasge)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("InfoService: " + messasge);
        Console.ResetColor();
        Environment.Exit(0);
    }
}