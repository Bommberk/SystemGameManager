namespace SystemGameManager.AudioManager.Service;

using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;

class AudioManagerService
{
    public bool IsProgramPlayingAudio(string programName)
    {
        Process[] processes = Process.GetProcessesByName(programName);
        if(processes.Length == 0)
            return false;

        using MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        // Windows/NAudio erlauben kein direktes Capture einzelner Prozesse, deshalb wird nur geprüft ob eine der Prozessinstanzen gerade aktiv Audio abspielt
        SessionCollection sessions = device.AudioSessionManager.Sessions;
        for (int i = 0; i < sessions.Count; i++)
        {
            AudioSessionControl session = sessions[i];
            uint sessionProcessId = session.GetProcessID;

            bool matchesProcess = false;
            foreach (Process process in processes)
            {
                if (sessionProcessId == (uint)process.Id)
                {
                    matchesProcess = true;
                    break;
                }
            }

            if (matchesProcess && session.State == AudioSessionState.AudioSessionStateActive)
            {
                return true;
            }
        }

        return false;
    }

    public WasapiLoopbackCapture CreateSystemLoopbackCapture()
    {
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return new WasapiLoopbackCapture(device);
    }

    public void StartCaptureProcessing(WasapiCapture capture, Action<byte[], int, WaveFormat> onDataAvailable)
    {
        if (capture == null)
            throw new ArgumentNullException(nameof(capture));
        if (onDataAvailable == null)
            throw new ArgumentNullException(nameof(onDataAvailable));

        capture.DataAvailable += (s, e) =>
        {
            onDataAvailable(e.Buffer, e.BytesRecorded, capture.WaveFormat);
        };
        capture.RecordingStopped += (s, e) =>
        {
            if (e.Exception is not null)
            {
                mlog($"Capture-Fehler beim Stoppen: {e.Exception}");
            }
            capture.Dispose();
        };
        capture.StartRecording();
    }

    public void SaveWavAudioFileFromCapture(WasapiCapture capture, string filePath, Action<byte[], int, WaveFormat>? onDataAvailable = null)
    {
        if(capture == null)
            throw new ArgumentNullException(nameof(capture));

        // WaveFileWriter darf erst nach RecordingStopped disposed werden, sonst wird die .wav vor dem Schreiben der Audiodaten finalisiert
        WaveFileWriter writer = new WaveFileWriter(filePath, capture.WaveFormat);
        capture.DataAvailable += (s, e) =>
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
            onDataAvailable?.Invoke(e.Buffer, e.BytesRecorded, capture.WaveFormat);
        };
        capture.RecordingStopped += (s, e) =>
        {
            if (e.Exception is not null)
            {
                mlog($"Capture-Fehler beim Speichern: {e.Exception}");
            }
            writer.Dispose();
            capture.Dispose();
        };
        capture.StartRecording();
    }
}