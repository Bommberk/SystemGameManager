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
    private static void Main(string[] args)
    {
        #if DEBUG
        new GlobalDevConfig();
        #endif

        try
        {
            VelopackApp.Build().Run();
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, ErrorSeverity.Fatal);
        }

        ErrorHandler.Register();

        try
        {
            // Update abschließen, ohne Main vom STA-Thread wegzuführen
            new Updater().AutoUpdate().GetAwaiter().GetResult();

            if (args.Length > 0 && args[0] == "--console")
            {
                RunConsole();
            }
            else
            {
                if (GlobalConfig.Settings.AppConfig.Environment != "dev")
                {
                    GetInfoAsync();
                }

                RunForm();
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.Handle(ex, ErrorSeverity.Fatal);
        }
    }

    private static void RunConsole()
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

    private static void RunForm()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static void WriteHeadline()
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
        var gameInfo = new GameInfoController();

        WriteHeadline();

        Game.WriteGamesFromDatabase();
    }
}