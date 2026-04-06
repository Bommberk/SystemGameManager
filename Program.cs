namespace Krassheiten.SystemGameManager;

using Krassheiten.SystemGameManager.Entity;
using Krassheiten.SystemGameManager.Controller;
using System;
using System.Threading;
using System.Windows.Forms;

internal static class Program
{

    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--console")
        {
            runConsole();
        }
        else
        {
            runForm();
        }
    }

    private static void runConsole()
    {
        GetInfoAsync();

        using var shutdownSignal = new ManualResetEventSlim(false);
        Console.WriteLine("Audio-Monitoring läuft. Mit Strg+C beenden.");

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            shutdownSignal.Set();
        };

        shutdownSignal.Wait();
    }
    private static void runForm()
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
        var gameAudio = new GameAudioController();
        writeHeadline();
        // pcInfo.Write();
        // gameInfo.Write();
    }
}