using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace SystemGameManager.Games.Service;

public class SystemAudioService
{
    // NOTE: IPolicyConfig is an undocumented Windows COM interface used to programmatically
    // change the system default audio endpoint. This is a well-known workaround used by
    // many audio management tools (e.g. EarTrumpet, AudioSwitch). Compatibility with
    // future Windows versions is not guaranteed by Microsoft.
    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    private class PolicyConfigClientImpl { }

    [ComImport, Guid("f8679f50-850a-41cf-9c72-430f290290c8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPolicyConfig
    {
        [PreserveSig] int GetMixFormat(string pszDeviceName, IntPtr ppFormat);
        [PreserveSig] int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);
        [PreserveSig] int ResetDeviceFormat(string pszDeviceName);
        [PreserveSig] int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);
        [PreserveSig] int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);
        [PreserveSig] int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);
        [PreserveSig] int GetShareMode(string pszDeviceName, IntPtr pMode);
        [PreserveSig] int SetShareMode(string pszDeviceName, int mode);
        [PreserveSig] int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
        [PreserveSig] int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
        [PreserveSig] int SetDefaultEndpoint(string pszDeviceName, uint dwRole);
        [PreserveSig] int SetEndpointVisibility(string pszDeviceName, bool bVisible);
    }

    private enum ERole : uint
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    public void SetMusicAudio(string musicAppName, int musicVolumePercent)
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (var device in devices)
        {
            var sessions = device.AudioSessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                var processName = GetProcessName(session.GetProcessID);
                if (!string.IsNullOrWhiteSpace(processName) && processName.Equals(musicAppName, StringComparison.OrdinalIgnoreCase))
                {
                    session.SimpleAudioVolume.Volume = musicVolumePercent / 100f;
                }
            }
        }
    }

    public void SetGameAudio(string gameName, int gameVolumePercent)
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (var device in devices)
        {
            var sessions = device.AudioSessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                var processName = GetProcessName(session.GetProcessID);
                if (!string.IsNullOrWhiteSpace(processName) && processName.Equals(gameName, StringComparison.OrdinalIgnoreCase))
                {
                    session.SimpleAudioVolume.Volume = gameVolumePercent / 100f;
                }
            }
        }
    }

    public int? GetMusicAppVolume(string musicAppName)
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (var device in devices)
        {
            var sessions = device.AudioSessionManager.Sessions;
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                var processName = GetProcessName(session.GetProcessID);
                if (!string.IsNullOrWhiteSpace(processName) && processName.Equals(musicAppName, StringComparison.OrdinalIgnoreCase))
                {
                    return (int)Math.Round(session.SimpleAudioVolume.Volume * 100f);
                }
            }
        }

        return null;
    }

    public string? GetProcessName(uint processId)
    {
        try
        {
            if (processId == 0)
            {
                return null;
            }

            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return null;
        }
    }

    public string? GetDefaultAudioOutputDeviceId()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device?.ID;
        }
        catch
        {
            return null;
        }
    }

    public string? GetAudioOutputDeviceIdByName(string friendlyName)
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator
                .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .FirstOrDefault(d => d.FriendlyName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase))
                ?.ID;
        }
        catch
        {
            return null;
        }
    }

    public bool SetDefaultAudioOutputDevice(string deviceId)
    {
        try
        {
            var policyConfig = (IPolicyConfig)new PolicyConfigClientImpl();
            policyConfig.SetDefaultEndpoint(deviceId, (uint)ERole.eConsole);
            policyConfig.SetDefaultEndpoint(deviceId, (uint)ERole.eMultimedia);
            policyConfig.SetDefaultEndpoint(deviceId, (uint)ERole.eCommunications);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static List<string> GetAudioOutputDeviceNames()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator
                .EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                .Select(device => device.FriendlyName)
                .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}
