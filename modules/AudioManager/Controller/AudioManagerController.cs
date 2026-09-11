namespace SystemGameManager.AudioManager.Controller;

using NAudio.CoreAudioApi;
using NAudio.Wave;
using SystemGameManager.AudioManager.Service;

class AudioManagerController
{
    private readonly AudioManagerService audioManagerService = new AudioManagerService();
    private readonly InGameConversationRecognitionService conversationRecognitionService = new();
    private WasapiCapture? activeCapture;
    private int captureSessionId;
    private string? activeProgramName;

    public bool IsCurrentlySpeech => conversationRecognitionService.IsCurrentlySpeech;

    public AudioManagerController()
    {
        
    }

    public void StartCaptureMonitoring(string? programName)
    {
        if (string.IsNullOrWhiteSpace(programName))
        {
            StopCaptureMonitoring();
            return;
        }

        bool activeProgramChanged = !string.Equals(activeProgramName, programName, StringComparison.OrdinalIgnoreCase);
        if (activeProgramChanged)
        {
            StopActiveCapture();
            activeProgramName = programName;
            mlog($"Capture-Überwachung auf aktives Programm gesetzt: {activeProgramName}");
        }

        bool isPlayingAudio = audioManagerService.IsProgramPlayingAudio(programName);

        if (isPlayingAudio && activeCapture == null)
        {
            activeCapture = audioManagerService.CreateSystemLoopbackCapture();
            WasapiCapture capture = activeCapture;
            int sessionId = Interlocked.Increment(ref captureSessionId);
            audioManagerService.StartCaptureProcessing(capture, (buffer, bytesRecorded, waveFormat) =>
            {
                if (sessionId != Volatile.Read(ref captureSessionId) || !ReferenceEquals(activeCapture, capture))
                {
                    return;
                }

                conversationRecognitionService.ProcessAudioBuffer(buffer, bytesRecorded, waveFormat);
            });
            mlog($"Capture für aktives Programm gestartet: {programName}");
        }
        else if (!isPlayingAudio && activeCapture != null)
        {
            mlog($"Kein Audio mehr aktiv für {programName}, Aufnahme wird beendet und gespeichert");
            StopActiveCapture();
        }
        else if (!isPlayingAudio)
        {
            conversationRecognitionService.Reset();
        }
    }

    private void StopActiveCapture()
    {
        // StopRecording löst RecordingStopped aus, wodurch Writer/Capture erst dort finalisiert und disposed werden
        Interlocked.Increment(ref captureSessionId);
        activeCapture?.StopRecording();
        activeCapture = null;
        conversationRecognitionService.Reset();
    }

    private void StopCaptureMonitoring()
    {
        StopActiveCapture();
        activeProgramName = null;
    }

    public void Dispose()
    {
        mlog("Disposing AudioManagerController");
        StopCaptureMonitoring();
    }
}