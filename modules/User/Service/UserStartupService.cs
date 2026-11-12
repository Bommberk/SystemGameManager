namespace SystemGameManager.User.Service;

using Microsoft.Win32;
using SystemGameManager.User.Entity;

public static class UserStartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "SystemGameManager";

    public static void Update(User user)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (runKey is null)
        {
            return;
        }

        if (!user.StartWithWindows)
        {
            runKey.DeleteValue(RunValueName, throwOnMissingValue: false);
            return;
        }

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return;
        }

        var arguments = user.EnableBackgroundRunning && user.StartInBackground ? " --background" : string.Empty;
        runKey.SetValue(RunValueName, $"\"{executablePath}\"{arguments}");
    }
}
