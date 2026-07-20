namespace SystemGameManager.Handler;

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

public enum ErrorSeverity
{
    Warning,
    Error,
    Fatal
}

public static class ErrorHandler
{
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SystemGameManager", "logs");

    // Evaluated on every access so the filename always reflects the current date,
    // even if the application runs past midnight.
    private static string LogFile =>
        Path.Combine(LogDirectory, $"error_{DateTime.Now:yyyy-MM-dd}.log");

    private static readonly object _logLock = new();

    static ErrorHandler()
    {
        Directory.CreateDirectory(LogDirectory);
    }

    /// <summary>
    /// Handles an exception by logging it and, when running as a GUI application,
    /// showing a user-friendly message box.
    /// </summary>
    public static void Handle(Exception ex, ErrorSeverity severity = ErrorSeverity.Error, bool showDialog = true)
    {
        var logPath = Log(ex, severity);

        if (showDialog && IsGuiApplication())
        {
            ShowDialog(ex, severity, logPath);
        }
        else
        {
            ConsoleError($"[{severity}] {ex.Message}");
        }

        if (severity == ErrorSeverity.Fatal)
        {
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Writes the exception details to the daily log file in a thread-safe manner.
    /// Returns the path of the log file that was written to.
    /// </summary>
    public static string Log(Exception ex, ErrorSeverity severity = ErrorSeverity.Error)
    {
        string logPath;
        try
        {
            var entry = BuildLogEntry(ex, severity);
            lock (_logLock)
            {
                // Capture the log path inside the lock so that concurrent calls near
                // midnight always write to the same file within a single operation.
                logPath = LogFile;
                File.AppendAllText(logPath, entry);
            }
        }
        catch
        {
            // If logging itself fails, silently swallow to avoid recursive errors.
            logPath = LogFile;
        }
        return logPath;
    }

    /// <summary>
    /// Registers global unhandled-exception hooks for WinForms applications.
    /// Call once from Program.Main, before Application.Run().
    /// </summary>
    public static void Register()
    {
        // Unhandled exceptions on the UI thread
        Application.ThreadException += (_, args) =>
            Handle(args.Exception, ErrorSeverity.Fatal);

        // Unhandled exceptions on non-UI threads
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception
                     ?? new Exception(args.ExceptionObject?.ToString() ?? "Unknown error");
            Handle(ex, ErrorSeverity.Fatal, showDialog: false);
        };

        // Unobserved task exceptions are treated as fatal to avoid silent failures.
        // Note: Environment.Exit(1) is called inside Handle() for Fatal, so SetObserved()
        // is intentionally omitted here – the process is about to terminate.
        TaskScheduler.UnobservedTaskException += (_, args) =>
            Handle(args.Exception, ErrorSeverity.Fatal);

        // Treat WinForms threading errors as fatal
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
    }

    private static string BuildLogEntry(Exception ex, ErrorSeverity severity)
    {
        var sb = new StringBuilder();
        sb.AppendLine("================================================================================");
        sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{severity.ToString().ToUpper()}]");
        sb.AppendLine($"Message   : {ex.Message}");
        sb.AppendLine($"Type      : {ex.GetType().FullName}");
        sb.AppendLine($"Source    : {ex.Source}");
        sb.AppendLine("StackTrace:");
        sb.AppendLine(ex.StackTrace);
        AppendInnerExceptions(sb, ex.InnerException);
        sb.AppendLine("================================================================================");
        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>
    /// Recursively appends all levels of inner exceptions.
    /// </summary>
    private static void AppendInnerExceptions(StringBuilder sb, Exception? inner, int depth = 1)
    {
        if (inner == null) return;

        var prefix = new string(' ', depth * 2);
        sb.AppendLine($"{prefix}InnerException (depth {depth}):");
        sb.AppendLine($"{prefix}  Message   : {inner.Message}");
        sb.AppendLine($"{prefix}  Type      : {inner.GetType().FullName}");
        sb.AppendLine($"{prefix}  StackTrace:");
        sb.AppendLine($"{prefix}  {inner.StackTrace}");
        AppendInnerExceptions(sb, inner.InnerException, depth + 1);
    }

    private static bool IsGuiApplication()
    {
        return SystemInformation.UserInteractive;
    }

    private static void ShowDialog(Exception ex, ErrorSeverity severity, string logPath)
    {
        var (title, icon) = severity switch
        {
            ErrorSeverity.Warning => ("Warning", MessageBoxIcon.Warning),
            ErrorSeverity.Error   => ("An error occurred", MessageBoxIcon.Error),
            ErrorSeverity.Fatal   => ("Fatal Error – Application will exit", MessageBoxIcon.Error),
            _                     => ("An error occurred", MessageBoxIcon.Error)
        };

        var message = $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Details have been saved to:{Environment.NewLine}{logPath}";

        MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
    }
}