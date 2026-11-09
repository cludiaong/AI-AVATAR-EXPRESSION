using UnityEngine;
using System;
using System.IO;
using System.Text;

public static class WavUtility
{
    private const int HEADER_SIZE = 44;

    public static AudioClip FromWav(byte[] wavBytes)
    {
        if (wavBytes == null || wavBytes.Length < HEADER_SIZE)
        {
            Debug.LogError("Invalid WAV data: too short or null.");
            return null;
        }

        // Read WAV header to get audio properties
        int sampleRate = BitConverter.ToInt32(wavBytes, 24); // Sample Rate
        int channels = BitConverter.ToInt16(wavBytes, 22);   // Number of Channels
        int bitsPerSample = BitConverter.ToInt16(wavBytes, 34); // Bits per sample

        // Calculate number of samples
        int dataSize = BitConverter.ToInt32(wavBytes, 40); // Subchunk2Size (data size)
        int bytesPerSample = bitsPerSample / 8;
        int sampleCount = dataSize / (channels * bytesPerSample);

        // Convert byte array to float array
        float[] samples = new float[sampleCount * channels];
        int byteOffset = HEADER_SIZE;

        for (int i = 0; i < sampleCount * channels; i++)
        {
            if (byteOffset + bytesPerSample > wavBytes.Length)
            {
                Debug.LogError("WAV data truncated.");
                return null;
            }

            long sampleValue = 0;
            switch (bitsPerSample)
            {
                case 8:
                    sampleValue = wavBytes[byteOffset] - 128; // 8-bit unsigned to signed
                    break;
                case 16:
                    sampleValue = BitConverter.ToInt16(wavBytes, byteOffset);
                    break;
                case 24:
                    // 24-bit audio is stored as 3 bytes. Need to handle endianness.
                    sampleValue = (sbyte)wavBytes[byteOffset + 2] << 16 | wavBytes[byteOffset + 1] << 8 | wavBytes[byteOffset];
                    break;
                case 32:
                    sampleValue = BitConverter.ToInt32(wavBytes, byteOffset);
                    break;
                default:
                    Debug.LogError($"Unsupported WAV format: {bitsPerSample}-bit. Only 8, 16, 24, or 32-bit WAV is supported.");
                    return null;
            }

            samples[i] = sampleValue / (float)Math.Pow(2, bitsPerSample - 1); // Normalize to -1.0 to 1.0
            byteOffset += bytesPerSample;
        }

        // Create AudioClip
        AudioClip audioClip = AudioClip.Create("SynthesizedSpeech", sampleCount, channels, sampleRate, false);
        audioClip.SetData(samples, 0);

        return audioClip;
    }

    public static byte[] FromAudioClip(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogError("AudioClip is null.");
            return null;
        }

        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // WAV header
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + samples.Length * 2);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1); // Audio format (1 for PCM)
                writer.Write((short)audioClip.channels);
                writer.Write(audioClip.frequency);
                writer.Write(audioClip.frequency * audioClip.channels * 2);
                writer.Write((short)(audioClip.channels * 2));
                writer.Write((short)16); // Bits per sample

                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(samples.Length * 2);

                // Convert samples to 16-bit PCM
                foreach (float sample in samples)
                {
                    writer.Write((short)(sample * 32767.0f));
                }
            }
            return stream.ToArray();
        }
    }
}


