namespace SystemGameManager.AudioManager.Controller;

using NAudio.CoreAudioApi;
using NAudio.Wave;
using SystemGameManager.AudioManager.Service;

class AudioManagerController
{
    private const string OUTPUT_FILE_PATH = "capture.wav";

    private readonly AudioManagerService audioManagerService = new AudioManagerService();
    private readonly SpeechDetectionService speechDetectionService = new SpeechDetectionService();
    private WasapiCapture? activeCapture;

    public AudioManagerController()
    {
        
    }

    /// <summary>
    /// true, sobald im Audio des aktuell überwachten Spiels Sprache erkannt wird (siehe SpeechDetectionService).
    /// </summary>
    public bool IsCurrentlySpeech => speechDetectionService.IsCurrentlySpeech;

    /// <summary>
    /// Prüft, ob das aktuell aktive Spiel (statt eines fest hinterlegten Programms) gerade Audio
    /// ausgibt, und startet/stoppt darauf basierend die Aufnahme samt Sprecherkennung.
    /// </summary>
    public void StartCaptureMonitoring(string? activeGameProcessName)
    {
        bool isPlayingAudio = !string.IsNullOrWhiteSpace(activeGameProcessName)
            && audioManagerService.IsProgramPlayingAudio(activeGameProcessName);

        if (isPlayingAudio && activeCapture == null)
        {
            activeCapture = audioManagerService.CreateSystemLoopbackCapture();
            audioManagerService.SaveWavAudioFileFromCapture(activeCapture, OUTPUT_FILE_PATH);
            activeCapture.DataAvailable += OnCaptureDataAvailable;
            mlog("Capture gestartet");
        }
        else if (!isPlayingAudio && activeCapture != null)
        {
            mlog("Kein Audio mehr aktiv, Aufnahme wird beendet und gespeichert");
            StopActiveCapture();
        }
    }

    private void OnCaptureDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (sender is WasapiCapture capture)
        {
            speechDetectionService.ProcessAudio(e.Buffer, e.BytesRecorded, capture.WaveFormat);
        }
    }

    private void StopActiveCapture()
    {
        // StopRecording löst RecordingStopped aus, wodurch Writer/Capture erst dort finalisiert und disposed werden
        activeCapture?.StopRecording();
        activeCapture = null;
        speechDetectionService.Reset();
    }

    public void Dispose()
    {
        mlog("Disposing AudioManagerController");
        StopActiveCapture();
        speechDetectionService.Dispose();
    }
}