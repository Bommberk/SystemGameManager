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
    private const int EXTENDED_FLAG = 0x100;
    private const int VIRTUAL_KEY_FLAG = 0x10000;
    private const int HID_USAGE_FLAG = 0x20000;
    private const int WM_INPUT = 0x00FF;
    private const uint RID_INPUT = 0x10000003;
    private const uint RIM_TYPEHID = 2;
    private const uint RIDEV_INPUTSINK = 0x100;
    private const uint RIDEV_PAGEONLY = 0x20;
    private const uint RIDI_DEVICEINFO = 0x2000000B;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetRawInputDeviceList(IntPtr list, ref uint count, uint size);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetRawInputDeviceInfo(IntPtr device, uint command, IntPtr data, ref uint size);

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

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public IntPtr hwndTarget;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] devices, uint count, uint size);
    [DllImport("user32.dll")]
    private static extern uint GetRawInputData(IntPtr hRawInput, uint command, IntPtr data, ref uint size, uint headerSize);

    /// <summary>
    /// Receives WM_INPUT for HID devices (consumer control / media keys), which macro keys often use
    /// and which never show up in the low-level keyboard hook.
    /// </summary>
    private sealed class RawInputWindow : NativeWindow
    {
        private readonly Dictionary<IntPtr, int> lastUsage = new();

        public RawInputWindow()
        {
            CreateHandle(new CreateParams());
            // Consumer control, system control and the vendor-defined pages (0xFF00+) used by many macro keyboards
            var devices = new List<RAWINPUTDEVICE>
            {
                new() { usUsagePage = 0x0C, usUsage = 0, dwFlags = RIDEV_INPUTSINK | RIDEV_PAGEONLY, hwndTarget = Handle },
                new() { usUsagePage = 0x01, usUsage = 0x06, dwFlags = RIDEV_INPUTSINK, hwndTarget = Handle },
                new() { usUsagePage = 0x01, usUsage = 0x80, dwFlags = RIDEV_INPUTSINK, hwndTarget = Handle }
            };
            for (ushort page = 0xFF00; page <= 0xFF0F; page++)
            {
                devices.Add(new RAWINPUTDEVICE { usUsagePage = page, usUsage = 0, dwFlags = RIDEV_INPUTSINK | RIDEV_PAGEONLY, hwndTarget = Handle });
            }
            AddInstalledDevices(devices);

            var size = (uint)Marshal.SizeOf<RAWINPUTDEVICE>();
            if (!RegisterRawInputDevices(devices.ToArray(), (uint)devices.Count, size))
            {
                mlog("HotKeys: raw input could not be registered, registering individually.");
                foreach (var device in devices)
                {
                    if (!RegisterRawInputDevices(new[] { device }, 1, size))
                        mlog($"HotKeys: page 0x{device.usUsagePage:X} usage 0x{device.usUsage:X} failed.");
                }
            }
        }

        /// <summary>Registers every usage page/usage the connected devices announce (e.g. Razer macro interfaces).</summary>
        private void AddInstalledDevices(List<RAWINPUTDEVICE> devices)
        {
            try
            {
                uint count = 0;
                var listSize = (uint)(IntPtr.Size * 2);
                GetRawInputDeviceList(IntPtr.Zero, ref count, listSize);
                if (count == 0) return;

                var list = Marshal.AllocHGlobal((int)(count * listSize));
                try
                {
                    var found = GetRawInputDeviceList(list, ref count, listSize);
                    for (var i = 0; i < found; i++)
                    {
                        var entry = list + (int)(i * listSize);
                        var handle = Marshal.ReadIntPtr(entry);
                        var type = Marshal.ReadInt32(entry, IntPtr.Size);
                        if (type != 2) continue;

                        uint infoSize = 32;
                        var info = Marshal.AllocHGlobal((int)infoSize);
                        try
                        {
                            Marshal.WriteInt32(info, 32);
                            if (GetRawInputDeviceInfo(handle, RIDI_DEVICEINFO, info, ref infoSize) == uint.MaxValue) continue;

                            var page = (ushort)Marshal.ReadInt16(info, 20);
                            var usage = (ushort)Marshal.ReadInt16(info, 22);

                            // Mouse (page 1, usage 2) would only flood the window with input
                            if (page == 0x01 && usage == 0x02) continue;
                            if (devices.Any(d => d.usUsagePage == page && (d.usUsage == usage || (d.dwFlags & RIDEV_PAGEONLY) != 0))) continue;
                            devices.Add(new RAWINPUTDEVICE
                            {
                                usUsagePage = page,
                                usUsage = usage,
                                dwFlags = RIDEV_INPUTSINK | (usage == 0 ? RIDEV_PAGEONLY : 0),
                                hwndTarget = Handle
                            });
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(info);
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(list);
                }
            }
            catch (Exception ex)
            {
                mlog("HotKeys: device enumeration failed: " + ex.Message);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT)
            {
                try { HandleRawInput(m.LParam); }
                catch (Exception ex) { mlog("HotKeys: " + ex.Message); }
            }
            base.WndProc(ref m);
        }

        private void HandleRawInput(IntPtr rawInputHandle)
        {
            uint size = 0;
            var headerSize = (uint)Marshal.SizeOf<RAWINPUTHEADER>();
            GetRawInputData(rawInputHandle, RID_INPUT, IntPtr.Zero, ref size, headerSize);
            if (size == 0) return;

            var buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                if (GetRawInputData(rawInputHandle, RID_INPUT, buffer, ref size, headerSize) != size) return;

                var header = Marshal.PtrToStructure<RAWINPUTHEADER>(buffer);
                if (header.dwType == 1)
                {
                    var makeCode = Marshal.ReadInt16(buffer, (int)headerSize) & 0xFFFF;
                    var keyFlags = Marshal.ReadInt16(buffer, (int)headerSize + 2) & 0xFFFF;
                    var vKey = Marshal.ReadInt16(buffer, (int)headerSize + 6) & 0xFFFF;

                    // Keys the low-level hook can't see (no scan code or the 0xFF placeholder vk) are treated as momentary presses
                    if ((keyFlags & 1) == 0 && (makeCode == 0 || vKey is 0xFF or 0))
                    {
                        OnMomentaryPress(VIRTUAL_KEY_FLAG | 0x8000 | makeCode);
                    }
                    return;
                }
                if (header.dwType != RIM_TYPEHID) return;

                var hidSize = Marshal.ReadInt32(buffer, (int)headerSize);
                var count = Marshal.ReadInt32(buffer, (int)headerSize + 4);
                if (hidSize < 2 || count < 1) return;

                var dataOffset = (int)headerSize + 8;
                var report = new byte[hidSize];
                Marshal.Copy(buffer + dataOffset, report, 0, hidSize);

                // consumer report = [report id, usage low, usage high]; otherwise the payload is hashed into a stable id.
                // An all-zero payload means released.
                int usage;
                if (hidSize == 3)
                {
                    usage = report[1] | (report[2] << 8);
                }
                else
                {
                    var hash = 17;
                    var hasData = false;
                    for (var i = 1; i < report.Length; i++)
                    {
                        if (report[i] != 0) hasData = true;
                        hash = unchecked(hash * 31 + report[i]);
                    }
                    usage = hasData ? ((hash ^ (report[0] << 8)) & 0xFFFF) | 1 : 0;
                }

                if (usage != 0)
                {
                    if (lastUsage.TryGetValue(header.hDevice, out var held) && held == usage) return;
                    lastUsage[header.hDevice] = usage;
                    OnMomentaryPress(HID_USAGE_FLAG | usage);
                }
                else
                {
                    lastUsage.Remove(header.hDevice);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }

    private static RawInputWindow? rawInputWindow;
    private static readonly LowLevelKeyboardProc hookProc = HookCallback;
    private static readonly HashSet<int> pressedKeys = new();
    private static readonly HashSet<int> capturedKeys = new();
    private static IntPtr hookHandle = IntPtr.Zero;
    private static bool isCapturing;
    private static System.Threading.Timer? captureTimeout;
    private static SynchronizationContext uiContext = new();

    /// <summary>Raised on the hook thread once a captured combination was released.</summary>
    public static event Action<string>? KeysCaptured;

    public static void Start()
    {
        if (hookHandle != IntPtr.Zero) return;

        uiContext = SynchronizationContext.Current ?? uiContext;
        Task.Run(() => HotKey.GetCachedHotKeys());
        rawInputWindow ??= new RawInputWindow();
        hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, GetModuleHandle(null), 0);
        if (hookHandle == IntPtr.Zero)
        {
            mlog("HotKeys: keyboard hook could not be installed.");
        }
    }

    public static void Stop()
    {
        rawInputWindow?.DestroyHandle();
        rawInputWindow = null;
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
        if ((id & HID_USAGE_FLAG) != 0)
        {
            return $"Media/HID 0x{id & 0xFFFF:X}";
        }

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
                                // Macro keys are usually sent as injected events by the keyboard driver, so they must not be filtered out
                var message = wParam.ToInt32();
                var id = GetKeyId(info);
                if (message is WM_KEYDOWN or WM_SYSKEYDOWN) OnKeyDown(id);
                else if (message is WM_KEYUP or WM_SYSKEYUP) OnKeyUp(id);
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
            RestartCaptureTimeout();
            return;
        }

        var pressed = pressedKeys.ToHashSet();
        if (!IsRegisteredKey(id)) return;
        Task.Run(() => TriggerMatchingHotKeys(pressed));
    }

    /// <summary>Quick in-memory check so ordinary typing never triggers any database or audio work.</summary>
    private static bool IsRegisteredKey(int id) =>
        HotKey.GetCachedHotKeys().Any(hotKey => hotKey.GetKeySet().Contains(id));

    private static void OnKeyUp(int id)
    {
        pressedKeys.Remove(id);

        if (isCapturing && pressedKeys.Count == 0)
        {
            FinishCapture();
        }
    }

    /// <summary>
    /// HID/macro keys often report a press without a matching release, so they are handled as momentary presses
    /// and never stay in the pressed-key set.
    /// </summary>
    private static void OnMomentaryPress(int id)
    {
        if (isCapturing)
        {
            capturedKeys.Add(id);
            RestartCaptureTimeout();
            return;
        }

        var pressed = new HashSet<int>(pressedKeys) { id };
        if (!IsRegisteredKey(id)) return;
        Task.Run(() => TriggerMatchingHotKeys(pressed));
    }

    private static void RestartCaptureTimeout()
    {
        captureTimeout?.Dispose();
        captureTimeout = new System.Threading.Timer(_ => uiContext.Post(__ =>
        {
            if (isCapturing) FinishCapture();
        }, null), null, 1200, Timeout.Infinite);
    }

    private static void FinishCapture()
    {
        captureTimeout?.Dispose();
        captureTimeout = null;
        if (capturedKeys.Count == 0) return;

        isCapturing = false;
        var keyCodes = string.Join("+", capturedKeys.OrderBy(key => key));
        capturedKeys.Clear();
        KeysCaptured?.Invoke(keyCodes);
    }

    private static void TriggerMatchingHotKeys(HashSet<int> pressed)
    {
        try
        {
            var matches = HotKey.GetCachedHotKeys()
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

    private sealed class AudioSnapshot
    {
        public int? MusicVolume;
        public int? GameVolume;
        public string? GameProcess;
        public string? DeviceId;
    }

    // Original state from before the first hotkey; kept until the active hotkey is pressed again
    private static AudioSnapshot? baseline;
    private static string? activeHotKey;
    private static readonly object toggleLock = new();
    private static readonly Dictionary<string, long> lastActivation = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Pressing a hotkey applies it. Pressing the currently active hotkey again restores the state
    /// from before the first hotkey, even if other hotkeys were used in between.
    /// </summary>
    private static void Activate(HotKey hotKey, Game? game)
    {
        lock (toggleLock)
        {
            // The same physical key can be reported by several HID interfaces at once
            var now = Environment.TickCount64;
            if (lastActivation.TryGetValue(hotKey.Name, out var last) && now - last < 400) return;
            lastActivation[hotKey.Name] = now;

            var audio = new SystemAudioService();

            if (baseline is not null && string.Equals(activeHotKey, hotKey.Name, StringComparison.OrdinalIgnoreCase))
            {
                Restore(audio, baseline);
                baseline = null;
                activeHotKey = null;
                return;
            }

            // Only remember values that were not already saved by an earlier hotkey
            var musicApp = GameAudioService.DEFAULT_MUSIC_APP_NAME;
            baseline ??= new AudioSnapshot();
            if (hotKey.MusicVolumePercent is not null && baseline.MusicVolume is null)
                baseline.MusicVolume = audio.GetMusicAppVolume(musicApp);
            if (hotKey.GameVolumePercent is not null && game is not null && baseline.GameVolume is null)
            {
                baseline.GameVolume = audio.GetMusicAppVolume(game.ProzessName);
                baseline.GameProcess = game.ProzessName;
            }
            if (!string.IsNullOrWhiteSpace(hotKey.AudioOutputDevice) && baseline.DeviceId is null)
                baseline.DeviceId = audio.GetDefaultAudioOutputDeviceId();
            activeHotKey = hotKey.Name;

            if (hotKey.MusicVolumePercent is int musicVolume)
            {
                audio.SetMusicAudio(musicApp, musicVolume);
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

    private static void Restore(SystemAudioService audio, AudioSnapshot snapshot)
    {
        if (snapshot.MusicVolume is int musicVolume)
        {
            audio.SetMusicAudio(GameAudioService.DEFAULT_MUSIC_APP_NAME, musicVolume);
        }

        if (snapshot.GameVolume is int gameVolume && snapshot.GameProcess is not null)
        {
            audio.SetGameAudio(snapshot.GameProcess, gameVolume);
        }

        if (snapshot.DeviceId is not null)
        {
            audio.SetDefaultAudioOutputDevice(snapshot.DeviceId);
        }
    }
}
