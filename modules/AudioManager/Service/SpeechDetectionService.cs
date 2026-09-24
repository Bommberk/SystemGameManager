namespace SystemGameManager.AudioManager.Service;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Pv;

/// <summary>
/// Erkennt in Echtzeit per Voice-Activity-Detection (Modell "Cobra" von Picovoice), ob in einem
/// Audiostream gerade gesprochen wird. Einfache pegel-/frequenzbasierte VAD-Ansätze reichen dafür
/// nicht aus und erkennen Sprache zu unzuverlässig, deshalb wird das Audio hier framebasiert
/// (üblicherweise ~32ms pro Frame) direkt an Cobra übergeben, wodurch Sprache praktisch sofort
/// erkannt wird.
/// </summary>
class SpeechDetectionService : IDisposable
{
    // Ab dieser Sprachwahrscheinlichkeit (0..1) gilt ein Frame als "Sprache".
    private const float VOICE_PROBABILITY_THRESHOLD = 0.6f;

    // Kurze Nachlaufzeit, damit natürliche Sprechpausen (z.B. zwischen Wörtern) nicht sofort als
    // "keine Sprache mehr" gewertet werden. Das Erkennen von Sprache selbst (isCurrentlySpeech = true)
    // erfolgt weiterhin sofort mit dem ersten Frame über dem Schwellenwert.
    private const int SPEECH_HANGOVER_MS = 300;

    private const int SCRATCH_BUFFER_LENGTH = 2048;

    private readonly string? accessKey;
    private readonly object processingLock = new();
    private readonly List<short> pendingSamples = new();
    private readonly float[] scratchBuffer = new float[SCRATCH_BUFFER_LENGTH];

    private Cobra? cobra;
    private bool cobraInitializationFailed;
    private BufferedWaveProvider? captureBuffer;
    private ISampleProvider? resampledMonoProvider;
    private WaveFormat? pipelineSourceFormat;
    private DateTime lastSpeechDetectedAtUtc = DateTime.MinValue;
    private bool isCurrentlySpeech;

    public SpeechDetectionService(string? accessKey = null)
    {
        this.accessKey = string.IsNullOrWhiteSpace(accessKey)
            ? GlobalConfig.Settings.SpeechDetectionConfig.AccessKey
            : accessKey;
    }

    /// <summary>
    /// true, sobald im aktuell überwachten Audiostream Sprache erkannt wird, sonst false.
    /// </summary>
    public bool IsCurrentlySpeech
    {
        get => isCurrentlySpeech;
        private set
        {
            if (isCurrentlySpeech == value)
            {
                return;
            }

            isCurrentlySpeech = value;
            mlog($"isCurrentlySpeech: {value}");
        }
    }

    /// <summary>
    /// Verarbeitet einen neuen Block aufgenommener Audiodaten, z.B. direkt aus einem
    /// WasapiCapture.DataAvailable Event. Aktualisiert dabei fortlaufend IsCurrentlySpeech.
    /// </summary>
    public void ProcessAudio(byte[] buffer, int bytesRecorded, WaveFormat sourceFormat)
    {
        if (bytesRecorded <= 0 || !TryEnsureCobra())
        {
            return;
        }

        lock (processingLock)
        {
            EnsurePipeline(sourceFormat);
            captureBuffer!.AddSamples(buffer, 0, bytesRecorded);
            DrainResampledAudioIntoPendingSamples();
            ProcessBufferedFrames();
        }
    }

    /// <summary>
    /// Setzt die Verarbeitung zurück, z.B. wenn keine Aufnahme (mehr) aktiv ist. IsCurrentlySpeech
    /// wird dabei wieder auf false gesetzt.
    /// </summary>
    public void Reset()
    {
        lock (processingLock)
        {
            pendingSamples.Clear();
            captureBuffer?.ClearBuffer();
            IsCurrentlySpeech = false;
        }
    }

    private bool TryEnsureCobra()
    {
        if (cobra is not null)
        {
            return true;
        }

        if (cobraInitializationFailed || string.IsNullOrWhiteSpace(accessKey))
        {
            return false;
        }

        try
        {
            cobra = new Cobra(accessKey);
            return true;
        }
        catch (Exception ex)
        {
            // Ohne (gültigen) Picovoice AccessKey oder bei fehlender nativer Bibliothek soll die
            // Spracherkennung einfach deaktiviert bleiben, statt die restliche Aufnahme zu stören.
            cobraInitializationFailed = true;
            mlog($"Cobra konnte nicht initialisiert werden: {ex.Message}");
            return false;
        }
    }

    private void EnsurePipeline(WaveFormat sourceFormat)
    {
        if (pipelineSourceFormat is not null && pipelineSourceFormat.Equals(sourceFormat))
        {
            return;
        }

        captureBuffer = new BufferedWaveProvider(sourceFormat)
        {
            // Nur tatsächlich vorhandene Samples liefern (keine Stille auffüllen), damit die
            // Resampling-/VAD-Pipeline ausschließlich echtes Audio auswertet.
            ReadFully = false,
            DiscardOnBufferOverflow = true,
        };

        ISampleProvider monoProvider = new DownmixToMonoSampleProvider(captureBuffer.ToSampleProvider());
        resampledMonoProvider = new WdlResamplingSampleProvider(monoProvider, cobra!.SampleRate);
        pipelineSourceFormat = sourceFormat;
        pendingSamples.Clear();
    }

    private void DrainResampledAudioIntoPendingSamples()
    {
        int samplesRead;
        while ((samplesRead = resampledMonoProvider!.Read(scratchBuffer, 0, scratchBuffer.Length)) > 0)
        {
            for (int i = 0; i < samplesRead; i++)
            {
                pendingSamples.Add(FloatSampleToPcm16(scratchBuffer[i]));
            }

            if (samplesRead < scratchBuffer.Length)
            {
                break;
            }
        }
    }

    private void ProcessBufferedFrames()
    {
        int frameLength = cobra!.FrameLength;
        while (pendingSamples.Count >= frameLength)
        {
            short[] frame = pendingSamples.GetRange(0, frameLength).ToArray();
            pendingSamples.RemoveRange(0, frameLength);

            try
            {
                float voiceProbability = cobra.Process(frame);
                EvaluateVoiceProbability(voiceProbability);
            }
            catch (Exception ex)
            {
                mlog($"Cobra Verarbeitung fehlgeschlagen: {ex.Message}");
            }
        }
    }

    private void EvaluateVoiceProbability(float voiceProbability)
    {
        DateTime now = DateTime.UtcNow;
        if (voiceProbability >= VOICE_PROBABILITY_THRESHOLD)
        {
            lastSpeechDetectedAtUtc = now;
            IsCurrentlySpeech = true;
            return;
        }

        if (IsCurrentlySpeech && (now - lastSpeechDetectedAtUtc).TotalMilliseconds > SPEECH_HANGOVER_MS)
        {
            IsCurrentlySpeech = false;
        }
    }

    private static short FloatSampleToPcm16(float sample)
    {
        float clamped = Math.Clamp(sample, -1f, 1f);
        return (short)(clamped * short.MaxValue);
    }

    public void Dispose()
    {
        cobra?.Dispose();
        cobra = null;
    }

    /// <summary>
    /// Mischt einen mehrkanaligen Audiostream (z.B. Stereo oder Surround) auf einen Mono-Kanal
    /// herunter, da Cobra ausschließlich Mono-Audio verarbeitet.
    /// </summary>
    private sealed class DownmixToMonoSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly int channels;
        private float[]? sourceBuffer;

        public DownmixToMonoSampleProvider(ISampleProvider source)
        {
            this.source = source;
            channels = Math.Max(1, source.WaveFormat.Channels);
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(source.WaveFormat.SampleRate, 1);
        }

        public WaveFormat WaveFormat { get; }

        public int Read(float[] buffer, int offset, int count)
        {
            if (channels == 1)
            {
                return source.Read(buffer, offset, count);
            }

            int sourceSamplesRequired = count * channels;
            if (sourceBuffer == null || sourceBuffer.Length < sourceSamplesRequired)
            {
                sourceBuffer = new float[sourceSamplesRequired];
            }

            int sourceSamplesRead = source.Read(sourceBuffer, 0, sourceSamplesRequired);
            int framesRead = sourceSamplesRead / channels;
            for (int frame = 0; frame < framesRead; frame++)
            {
                float sum = 0f;
                int baseIndex = frame * channels;
                for (int channel = 0; channel < channels; channel++)
                {
                    sum += sourceBuffer[baseIndex + channel];
                }

                buffer[offset + frame] = sum / channels;
            }

            return framesRead;
        }
    }
}
