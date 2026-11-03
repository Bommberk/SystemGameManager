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
using SystemGameManager.View2;
using System.Net.Http;
using SystemGameManager.Plugin;
using SystemGameManager.Plugin.HotKeys;
using SystemGameManager.AudioManager.Controller;
using UserEntity = SystemGameManager.User.Entity.User;

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
            var isConsole = args.Length > 0 && args[0] == "--console";
            var startInBackground = args.Any(arg => string.Equals(arg, "--background", StringComparison.OrdinalIgnoreCase));
            if (!isConsole)
            {
                ApplicationConfiguration.Initialize();
            }

            if (!isConsole && !startInBackground)
            {
                RunWithStartupWindow(args);
                return;
            }

            new Updater().AutoUpdate().GetAwaiter().GetResult();

            if (isConsole)
            {
                RunConsole();
            }
            else
            {
                PrepareApplication(args);
                RunForm(startInBackground);
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

        Console.WriteLine("Audio-Monitoring läuft. Mit Strg+C beenden.");
        var gameAudioController = new GameAudioController();

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            Application.Exit();
        };

        // The hotkey hook and raw input window need a message loop on this thread
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
        HotKeyListener.Start();
        Application.Run();
        HotKeyListener.Stop();
        gameAudioController.Dispose();
    }

    private static void RunForm(bool startInBackground)
    {
        Application.Run(new MainForm(startInBackground));
    }

    private static void RunWithStartupWindow(string[] args)
    {
        var startupForm = CreateStartupForm();
        using var context = new ApplicationContext(startupForm);

        startupForm.Shown += async (_, _) =>
        {
            try
            {
                await new Updater().AutoUpdate();
                await Task.Run(() => PrepareApplication(args));

                var userSettings = await Task.Run(UserEntity.GetUser);
                var mainForm = new MainForm(userSettings: userSettings);
                mainForm.Opacity = 0;
                mainForm.Show();
                await mainForm.Initialization;

                mainForm.Opacity = 1;
                context.MainForm = mainForm;
                startupForm.Close();
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, ErrorSeverity.Fatal);
            }
        };

        Application.Run(context);
    }

    private static void PrepareApplication(string[] args)
    {
        if (GlobalConfig.Settings.AppConfig.Environment != "dev")
        {
            GetInfoAsync();
            try
            {
                var smarthomeApi = new SmarthomeApi();
                smarthomeApi.SendDeviceInfoAsync();
            }
            catch (Exception ex)
            {
                ErrorHandler.Handle(ex, ErrorSeverity.Warning);
            }
        }
        else if (args.Length > 0 && args[0] == "--infos")
        {
            GetInfoAsync();
        }
    }

    private static Form CreateStartupForm()
    {
        var form = new Form
        {
            Text = "System Game Manager",
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ControlBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(320, 80)
        };
        form.Controls.Add(new Label
        {
            Text = "Starting System Game Manager...",
            AutoSize = true,
            Location = new Point(20, 30)
        });
        return form;
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