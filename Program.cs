namespace Krassheiten.SystemGameManager;

using Krassheiten.SystemGameManager.Entity;
using Krassheiten.SystemGameManager.Controller;
using System;
using System.Windows.Forms;

internal static class Program
{

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
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