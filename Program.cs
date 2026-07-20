namespace SystemGameManager;

using SystemGameManager.Database.Controller;
using SystemGameManager.Games.Controller;
using SystemGameManager.Games.Entity;
using SystemGameManager.Pc.Controller;
using SystemGameManager.Handler;
using GameEntity = SystemGameManager.Games.Entity.Game;
using System;
using System.Threading;
using System.Windows.Forms;
using Velopack;
using System.Threading.Tasks;
using SystemGameManager.Service;
using SystemGameManager.View;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        VelopackApp.Build().Run();

        ErrorHandler.Register();

        await new Updater().AutoUpdate();

        try
        {
            if (args.Length > 0 && args[0] == "--console")
            {
                runConsole();
            }
            else
            {
                // GetInfoAsync();
                runForm();
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, ErrorSeverity.Fatal);
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
        var dbController = new DatabaseController();
        // var pcInfo = new PcInfoController();
        var gameInfo = new GameInfoController();
        // var gameAudio = new GameAudioController();
        writeHeadline();
        // pcInfo.Write();
        // gameInfo.Write();
        Game.WriteGamesFromDatabase();
    }
}
