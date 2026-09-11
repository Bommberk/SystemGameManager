namespace SystemGameManager.AudioManager.Controller;

using NAudio.CoreAudioApi;
using NAudio.Wave;
using SystemGameManager.AudioManager.Service;

class AudioManagerController
{
    private const string PROGRAM_NAME = "Chrome";
    private const string OUTPUT_FILE_PATH = "capture.wav";

    private readonly AudioManagerService audioManagerService = new AudioManagerService();
    private WasapiCapture? activeCapture;

    public AudioManagerController()
    {
        
    }

    public void StartCaptureMonitoring()
    {
        bool isPlayingAudio = audioManagerService.IsProgramPlayingAudio(PROGRAM_NAME);

        if (isPlayingAudio && activeCapture == null)
        {
            activeCapture = audioManagerService.CreateSystemLoopbackCapture();
            audioManagerService.SaveWavAudioFileFromCapture(activeCapture, OUTPUT_FILE_PATH);
            mlog("Capture gestartet");
        }
        else if (!isPlayingAudio && activeCapture != null)
        {
            mlog("Kein Audio mehr aktiv, Aufnahme wird beendet und gespeichert");
            StopActiveCapture();
        }
    }

    private void StopActiveCapture()
    {
        // StopRecording löst RecordingStopped aus, wodurch Writer/Capture erst dort finalisiert und disposed werden
        activeCapture?.StopRecording();
        activeCapture = null;
    }

    public void Dispose()
    {
        mlog("Disposing AudioManagerController");
        StopActiveCapture();
    }
}