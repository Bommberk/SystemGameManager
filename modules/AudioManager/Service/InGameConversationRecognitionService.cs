namespace SystemGameManager.AudioManager.Service;

using NAudio.Dsp;
using NAudio.Wave;

internal sealed class InGameConversationRecognitionService
{
    private const int FFT_SIZE = 1024;
    private const double MIN_SPEECH_CANDIDATE_MS = 450;
    private const double MIN_SILENCE_MS = 1400;
    private const double SPEECH_CONFIDENCE_BUILD_DIVIDER = 650d;
    private const double SPEECH_CONFIDENCE_TRIGGER = 0.55d;
    private const float MIN_RMS_LEVEL = 0.02f;
    private const float MIN_PEAK_LEVEL = 0.06f;
    private const float MIN_SPEECH_BAND_RATIO = 0.52f;
    private const float MAX_LOW_BAND_RATIO = 0.40f;
    private const float MAX_HIGH_BAND_RATIO = 0.34f;
    private const float MIN_ZERO_CROSSING_RATE = 0.015f;
    private const float MAX_ZERO_CROSSING_RATE = 0.22f;
    private const float MAX_CREST_FACTOR = 5.5f;
    private const float SPEECH_BAND_START_HZ = 300f;
    private const float SPEECH_BAND_END_HZ = 3400f;
    private const float LOW_BAND_END_HZ = 250f;
    private const float HIGH_BAND_START_HZ = 4000f;

    private double speechCandidateDurationMs;
    private double silenceDurationMs;
    private double speechConfidence;

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
        float zeroCrossingRate = CalculateZeroCrossingRate(monoSamples);
        float crestFactor = rmsLevel <= float.Epsilon ? float.MaxValue : peakLevel / rmsLevel;
        FrequencyBandProfile bandProfile = CalculateFrequencyBandProfile(monoSamples, waveFormat.SampleRate);
        double bufferDurationMs = GetBufferDurationMs(monoSamples.Length, waveFormat.SampleRate);

        int speechScore = 0;
        if (rmsLevel >= MIN_RMS_LEVEL) speechScore++;
        if (peakLevel >= MIN_PEAK_LEVEL) speechScore++;
        if (bandProfile.SpeechBandRatio >= MIN_SPEECH_BAND_RATIO) speechScore += 2;
        if (bandProfile.LowBandRatio <= MAX_LOW_BAND_RATIO) speechScore++;
        if (bandProfile.HighBandRatio <= MAX_HIGH_BAND_RATIO) speechScore++;
        if (zeroCrossingRate >= MIN_ZERO_CROSSING_RATE && zeroCrossingRate <= MAX_ZERO_CROSSING_RATE) speechScore++;
        if (crestFactor <= MAX_CREST_FACTOR) speechScore++;

        bool looksLikeSpeech = speechScore >= 7
            && bandProfile.SpeechBandRatio > bandProfile.LowBandRatio
            && bandProfile.SpeechBandRatio > bandProfile.HighBandRatio;

        UpdateSpeechState(looksLikeSpeech, bufferDurationMs);
    }

    public void Reset()
    {
        speechCandidateDurationMs = 0;
        silenceDurationMs = 0;
        speechConfidence = 0;
        SetSpeechState(false);
    }

    private void UpdateSpeechState(bool looksLikeSpeech, double bufferDurationMs)
    {
        if (looksLikeSpeech)
        {
            speechCandidateDurationMs += bufferDurationMs;
            silenceDurationMs = 0;
            speechConfidence = Math.Min(1, speechConfidence + (bufferDurationMs / SPEECH_CONFIDENCE_BUILD_DIVIDER));

            if (speechCandidateDurationMs >= MIN_SPEECH_CANDIDATE_MS && speechConfidence >= SPEECH_CONFIDENCE_TRIGGER)
            {
                SetSpeechState(true);
            }

            return;
        }

        RegisterSilence(bufferDurationMs);
    }

    private void RegisterSilence(double bufferDurationMs = 0)
    {
        speechCandidateDurationMs = 0;
        silenceDurationMs += bufferDurationMs;
        double decayDivider = IsCurrentlySpeech ? 2200d : 1100d;
        speechConfidence = Math.Max(0, speechConfidence - (bufferDurationMs / decayDivider));

        if (silenceDurationMs >= MIN_SILENCE_MS || speechConfidence <= 0.15d)
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

    private static float CalculateZeroCrossingRate(float[] samples)
    {
        if (samples.Length < 2)
        {
            return 0;
        }

        int zeroCrossings = 0;
        for (int index = 1; index < samples.Length; index++)
        {
            bool previousNegative = samples[index - 1] < 0;
            bool currentNegative = samples[index] < 0;
            if (previousNegative != currentNegative)
            {
                zeroCrossings++;
            }
        }

        return zeroCrossings / (float)(samples.Length - 1);
    }

    private static FrequencyBandProfile CalculateFrequencyBandProfile(float[] samples, int sampleRate)
    {
        int fftSize = GetLargestPowerOfTwo(Math.Min(samples.Length, FFT_SIZE));
        if (fftSize < 256 || sampleRate <= 0)
        {
            return default;
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
        double lowBandEnergy = 0;
        double highBandEnergy = 0;
        double binWidth = (double)sampleRate / fftSize;

        for (int index = 1; index < fftSize / 2; index++)
        {
            double magnitude = (fftBuffer[index].X * fftBuffer[index].X) + (fftBuffer[index].Y * fftBuffer[index].Y);
            totalEnergy += magnitude;

            double frequency = index * binWidth;
            if (frequency <= LOW_BAND_END_HZ)
            {
                lowBandEnergy += magnitude;
            }

            if (frequency >= SPEECH_BAND_START_HZ && frequency <= SPEECH_BAND_END_HZ)
            {
                speechBandEnergy += magnitude;
            }

            if (frequency >= HIGH_BAND_START_HZ)
            {
                highBandEnergy += magnitude;
            }
        }

        if (totalEnergy <= double.Epsilon)
        {
            return default;
        }

        return new FrequencyBandProfile(
            (float)(speechBandEnergy / totalEnergy),
            (float)(lowBandEnergy / totalEnergy),
            (float)(highBandEnergy / totalEnergy));
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

    private static double GetBufferDurationMs(int sampleCount, int sampleRate)
    {
        if (sampleCount <= 0 || sampleRate <= 0)
        {
            return 0;
        }

        return sampleCount * 1000d / sampleRate;
    }

    private readonly record struct FrequencyBandProfile(float SpeechBandRatio, float LowBandRatio, float HighBandRatio);
}
