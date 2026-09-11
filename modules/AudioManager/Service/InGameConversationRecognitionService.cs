namespace SystemGameManager.AudioManager.Service;

using NAudio.Dsp;
using NAudio.Wave;

internal sealed class InGameConversationRecognitionService
{
    private const int FFT_SIZE = 1024;
    private const int MIN_SPEECH_FRAMES = 2;
    private const int MAX_SILENCE_FRAMES = 4;
    private const float MIN_RMS_LEVEL = 0.015f;
    private const float MIN_PEAK_LEVEL = 0.04f;
    private const float MIN_SPEECH_BAND_RATIO = 0.45f;
    private const float SPEECH_BAND_START_HZ = 300f;
    private const float SPEECH_BAND_END_HZ = 3400f;

    private int consecutiveSpeechFrames;
    private int consecutiveSilenceFrames;

    public bool IsCurrentlySpeech { get; private set; }

    public void ProcessAudioBuffer(byte[] buffer, int bytesRecorded, WaveFormat waveFormat)
    {
        if (bytesRecorded <= 0)
        {
            RegisterSilence();
            return;
        }

        float[] monoSamples = ConvertToMonoSamples(buffer, bytesRecorded, waveFormat);
        if (monoSamples.Length == 0)
        {
            RegisterSilence();
            return;
        }

        float rmsLevel = CalculateRmsLevel(monoSamples);
        float peakLevel = CalculatePeakLevel(monoSamples);
        float speechBandRatio = CalculateSpeechBandRatio(monoSamples, waveFormat.SampleRate);

        bool looksLikeSpeech = rmsLevel >= MIN_RMS_LEVEL
            && peakLevel >= MIN_PEAK_LEVEL
            && speechBandRatio >= MIN_SPEECH_BAND_RATIO;

        UpdateSpeechState(looksLikeSpeech);
    }

    public void Reset()
    {
        consecutiveSpeechFrames = 0;
        consecutiveSilenceFrames = 0;
        SetSpeechState(false);
    }

    private void UpdateSpeechState(bool looksLikeSpeech)
    {
        if (looksLikeSpeech)
        {
            consecutiveSpeechFrames++;
            consecutiveSilenceFrames = 0;

            if (consecutiveSpeechFrames >= MIN_SPEECH_FRAMES)
            {
                SetSpeechState(true);
            }

            return;
        }

        RegisterSilence();
    }

    private void RegisterSilence()
    {
        consecutiveSpeechFrames = 0;
        consecutiveSilenceFrames++;

        if (consecutiveSilenceFrames >= MAX_SILENCE_FRAMES)
        {
            SetSpeechState(false);
        }
    }

    private void SetSpeechState(bool isSpeech)
    {
        if (IsCurrentlySpeech == isSpeech)
        {
            return;
        }

        IsCurrentlySpeech = isSpeech;
        mlog($"Speech-VAD Status geändert: {IsCurrentlySpeech}");
    }

    private static float[] ConvertToMonoSamples(byte[] buffer, int bytesRecorded, WaveFormat waveFormat)
    {
        int channels = Math.Max(1, waveFormat.Channels);
        int frameCount;
        float[] monoSamples;

        if ((waveFormat.Encoding == WaveFormatEncoding.IeeeFloat || waveFormat.Encoding == WaveFormatEncoding.Extensible) && waveFormat.BitsPerSample == 32)
        {
            int sampleCount = bytesRecorded / sizeof(float);
            if (sampleCount == 0)
            {
                return [];
            }

            float[] interleavedSamples = new float[sampleCount];
            Buffer.BlockCopy(buffer, 0, interleavedSamples, 0, sampleCount * sizeof(float));
            frameCount = sampleCount / channels;
            monoSamples = new float[frameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float mixedSample = 0;
                int frameOffset = frameIndex * channels;
                for (int channelIndex = 0; channelIndex < channels; channelIndex++)
                {
                    mixedSample += interleavedSamples[frameOffset + channelIndex];
                }

                monoSamples[frameIndex] = mixedSample / channels;
            }

            return monoSamples;
        }

        if (waveFormat.BitsPerSample == 32)
        {
            int sampleCount = bytesRecorded / sizeof(int);
            frameCount = sampleCount / channels;
            monoSamples = new float[frameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float mixedSample = 0;
                int frameOffset = frameIndex * channels * sizeof(int);
                for (int channelIndex = 0; channelIndex < channels; channelIndex++)
                {
                    int sample = BitConverter.ToInt32(buffer, frameOffset + (channelIndex * sizeof(int)));
                    mixedSample += sample / (float)int.MaxValue;
                }

                monoSamples[frameIndex] = mixedSample / channels;
            }

            return monoSamples;
        }

        if (waveFormat.BitsPerSample == 16)
        {
            int sampleCount = bytesRecorded / sizeof(short);
            frameCount = sampleCount / channels;
            monoSamples = new float[frameCount];

            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                float mixedSample = 0;
                int frameOffset = frameIndex * channels * sizeof(short);
                for (int channelIndex = 0; channelIndex < channels; channelIndex++)
                {
                    short sample = BitConverter.ToInt16(buffer, frameOffset + (channelIndex * sizeof(short)));
                    mixedSample += sample / (float)short.MaxValue;
                }

                monoSamples[frameIndex] = mixedSample / channels;
            }

            return monoSamples;
        }

        return [];
    }

    private static float CalculateRmsLevel(float[] samples)
    {
        double sum = 0;

        foreach (float sample in samples)
        {
            sum += sample * sample;
        }

        return samples.Length == 0 ? 0 : (float)Math.Sqrt(sum / samples.Length);
    }

    private static float CalculatePeakLevel(float[] samples)
    {
        float peak = 0;

        foreach (float sample in samples)
        {
            float absoluteSample = Math.Abs(sample);
            if (absoluteSample > peak)
            {
                peak = absoluteSample;
            }
        }

        return peak;
    }

    private static float CalculateSpeechBandRatio(float[] samples, int sampleRate)
    {
        int fftSize = GetLargestPowerOfTwo(Math.Min(samples.Length, FFT_SIZE));
        if (fftSize < 256 || sampleRate <= 0)
        {
            return 0;
        }

        Complex[] fftBuffer = new Complex[fftSize];
        for (int index = 0; index < fftSize; index++)
        {
            float window = 0.54f - (0.46f * MathF.Cos((2 * MathF.PI * index) / (fftSize - 1)));
            fftBuffer[index].X = samples[index] * window;
            fftBuffer[index].Y = 0;
        }

        FastFourierTransform.FFT(true, (int)Math.Log2(fftSize), fftBuffer);

        double totalEnergy = 0;
        double speechBandEnergy = 0;
        double binWidth = (double)sampleRate / fftSize;

        for (int index = 1; index < fftSize / 2; index++)
        {
            double magnitude = (fftBuffer[index].X * fftBuffer[index].X) + (fftBuffer[index].Y * fftBuffer[index].Y);
            totalEnergy += magnitude;

            double frequency = index * binWidth;
            if (frequency >= SPEECH_BAND_START_HZ && frequency <= SPEECH_BAND_END_HZ)
            {
                speechBandEnergy += magnitude;
            }
        }

        if (totalEnergy <= double.Epsilon)
        {
            return 0;
        }

        return (float)(speechBandEnergy / totalEnergy);
    }

    private static int GetLargestPowerOfTwo(int value)
    {
        int power = 1;
        while (power * 2 <= value)
        {
            power *= 2;
        }

        return power;
    }
}
