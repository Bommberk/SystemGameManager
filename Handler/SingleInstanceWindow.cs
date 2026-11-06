using System.Runtime.InteropServices;
using SystemGameManager.View2;

namespace SystemGameManager.Handler;

internal sealed class SingleInstanceWindow : NativeWindow, IDisposable
{
    private static readonly uint ActivationMessage = RegisterWindowMessage("SystemGameManager.ActivateExistingInstance");
    private static readonly IntPtr Broadcast = new(0xffff);

    public SingleInstanceWindow()
    {
        CreateHandle(new CreateParams());
    }

    public static void ActivateExisting()
    {
        AllowSetForegroundWindow(-1);
        PostMessage(Broadcast, ActivationMessage, IntPtr.Zero, IntPtr.Zero);
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == ActivationMessage)
        {
            var form = Application.OpenForms.OfType<MainForm>().FirstOrDefault(f => f.Opacity > 0)
                ?? Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Visible);
            if (form is MainForm mainForm)
            {
                mainForm.RestoreFromTray();
            }
            else if (form is not null)
            {
                if (form.WindowState == FormWindowState.Minimized)
                    form.WindowState = FormWindowState.Normal;
                form.Show();
                form.Activate();
            }
            else
            {
                var console = GetConsoleWindow();
                if (console != IntPtr.Zero)
                {
                    ShowWindow(console, 9);
                    SetForegroundWindow(console);
                }
            }
        }

        base.WndProc(ref message);
    }

    public void Dispose() => DestroyHandle();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint RegisterWindowMessage(string message);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(int processId);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr window, int command);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
}
