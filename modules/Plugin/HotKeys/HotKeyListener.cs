namespace SystemGameManager.Plugin.HotKeys;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using SystemGameManager.AudioManager.Service;
using SystemGameManager.Games.Entity;
using SystemGameManager.Games.Service;

/// <summary>
/// Global low-level keyboard hook. Keys are identified by their hardware scan code (plus extended flag),
/// falling back to the virtual key code for keys without scan code (e.g. keyboard macro keys).
/// Must be started on a thread with a message loop (the UI thread).
/// </summary>
static class HotKeyListener
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;
    private const uint LLKHF_EXTENDED = 0x01;
    private const uint LLKHF_INJECTED = 0x10;
    private const int EXTENDED_FLAG = 0x100;
    private const int VIRTUAL_KEY_FLAG = 0x10000;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetKeyNameText(int lParam, StringBuilder lpString, int nSize);

    private static readonly LowLevelKeyboardProc hookProc = HookCallback;
    private static readonly HashSet<int> pressedKeys = new();
    private static readonly HashSet<int> capturedKeys = new();
    private static IntPtr hookHandle = IntPtr.Zero;
    private static bool isCapturing;

    /// <summary>Raised on the hook thread once a captured combination was released.</summary>
    public static event Action<string>? KeysCaptured;

    public static void Start()
    {
        if (hookHandle != IntPtr.Zero) return;

        hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, GetModuleHandle(null), 0);
        if (hookHandle == IntPtr.Zero)
        {
            mlog("HotKeys: keyboard hook could not be installed.");
        }
    }

    public static void Stop()
    {
        if (hookHandle == IntPtr.Zero) return;
        UnhookWindowsHookEx(hookHandle);
        hookHandle = IntPtr.Zero;
    }

    public static void StartCapture()
    {
        pressedKeys.Clear();
        capturedKeys.Clear();
        isCapturing = true;
    }

    public static string GetKeyDisplayName(string keyCodes)
    {
        var names = new HotKey("", keyCodes).GetKeySet().Select(GetKeyName);
        return string.Join(" + ", names);
    }

    private static string GetKeyName(int id)
    {
        if ((id & VIRTUAL_KEY_FLAG) != 0)
        {
            return $"Key 0x{id & 0xFFFF:X2}";
        }

        var lParam = ((id & 0xFF) << 16) | ((id & EXTENDED_FLAG) != 0 ? 1 << 24 : 0);
        var builder = new StringBuilder(64);
        return GetKeyNameText(lParam, builder, builder.Capacity) > 0 ? builder.ToString() : $"Scan 0x{id:X}";
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            try
            {
                var info = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                if ((info.flags & LLKHF_INJECTED) == 0)
                {
                    var message = wParam.ToInt32();
                    var id = GetKeyId(info);
                    if (message is WM_KEYDOWN or WM_SYSKEYDOWN) OnKeyDown(id);
                    else if (message is WM_KEYUP or WM_SYSKEYUP) OnKeyUp(id);
                }
            }
            catch (Exception ex)
            {
                mlog("HotKeys: " + ex.Message);
            }
        }

        return CallNextHookEx(hookHandle, nCode, wParam, lParam);
    }

    private static int GetKeyId(KBDLLHOOKSTRUCT info)
    {
        if (info.scanCode == 0)
        {
            return VIRTUAL_KEY_FLAG | (int)info.vkCode;
        }

        return (int)info.scanCode | ((info.flags & LLKHF_EXTENDED) != 0 ? EXTENDED_FLAG : 0);
    }

    private static void OnKeyDown(int id)
    {
        if (!pressedKeys.Add(id)) return; // auto-repeat

        if (isCapturing)
        {
            capturedKeys.Add(id);
            return;
        }

        var pressed = pressedKeys.ToHashSet();
        Task.Run(() => TriggerMatchingHotKeys(pressed));
    }

    private static void OnKeyUp(int id)
    {
        pressedKeys.Remove(id);

        if (isCapturing && pressedKeys.Count == 0 && capturedKeys.Count > 0)
        {
            isCapturing = false;
            var keyCodes = string.Join("+", capturedKeys.OrderBy(key => key));
            capturedKeys.Clear();
            KeysCaptured?.Invoke(keyCodes);
        }
    }

    private static void TriggerMatchingHotKeys(HashSet<int> pressed)
    {
        try
        {
            var matches = HotKey.GetHotKeys()
                .Where(hotKey => hotKey.GetKeySet() is { Count: > 0 } keys && keys.SetEquals(pressed))
                .ToArray();
            if (matches.Length == 0) return;

            var game = matches.Any(hotKey => !string.IsNullOrWhiteSpace(hotKey.GameSerializedName))
                ? GetGameProcess.GetRunningOpenGame()
                : null;

            var specific = matches.Where(hotKey => game is not null
                && string.Equals(hotKey.GameSerializedName, game.SerializedGameName, StringComparison.OrdinalIgnoreCase)).ToArray();
            var toRun = specific.Length > 0
                ? specific
                : matches.Where(hotKey => string.IsNullOrWhiteSpace(hotKey.GameSerializedName)).ToArray();

            foreach (var hotKey in toRun)
            {
                Activate(hotKey, game ?? GetGameProcess.GetRunningOpenGame());
            }
        }
        catch (Exception ex)
        {
            mlog("HotKeys: " + ex.Message);
        }
    }

    private static void Activate(HotKey hotKey, Game? game)
    {
        mlog($"HotKey activated: {hotKey.Name}");
        var audio = new SystemAudioService();

        if (hotKey.MusicVolumePercent is int musicVolume)
        {
            audio.SetMusicAudio(GameAudioService.DEFAULT_MUSIC_APP_NAME, musicVolume);
        }

        if (hotKey.GameVolumePercent is int gameVolume && game is not null)
        {
            audio.SetGameAudio(game.ProzessName, gameVolume);
        }

        if (!string.IsNullOrWhiteSpace(hotKey.AudioOutputDevice))
        {
            var deviceId = audio.GetAudioOutputDeviceIdByName(hotKey.AudioOutputDevice);
            if (deviceId is not null) audio.SetDefaultAudioOutputDevice(deviceId);
        }
    }
}
