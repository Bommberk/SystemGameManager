using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Krassheiten.Game;
using Krassheiten.PC;

namespace Krassheiten;

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
        var pcInfo = new PcInfo();
        var gameInfo = new GameInfo();
        writeHeadline();
        // pcInfo.Write();
        gameInfo.Write();
    }

}
