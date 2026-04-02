namespace Krassheiten;

using Krassheiten.Game.Controller;
using Krassheiten.PC.Controller;

internal static class Program
{
    private static void Main(string[] args)
    {
        GetInfoAsync();
    }

    private static void writeHeadline()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("==============================");
        Console.WriteLine("        INFORMATIONEN!        ");
        Console.WriteLine("==============================");
        Console.ResetColor();
    }

    private static void GetInfoAsync()
    {
        var pcInfo = new PcInfoController();
        var gameInfo = new GameInfoController();
        writeHeadline();
        // pcInfo.Write();
        gameInfo.Write();
    }

}
