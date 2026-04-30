using System.Diagnostics;
using NAudio.CoreAudioApi;

namespace Krassheiten.SystemGameManager.Service;

public class SystemAudioService
{
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
}
