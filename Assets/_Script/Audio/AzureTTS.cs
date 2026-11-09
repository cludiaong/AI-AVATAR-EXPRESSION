using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class AzureTTS : ElevenlabsAPI
{
    public override string ttsName 
    {
        get => "azure";
    }
    // Replace with your own subscription key and service region (e.g., "westus").
    [SerializeField] private const string SubscriptionKey = "41b2565015f84eff95f7640a822775bb";
    [SerializeField] private const string Region = "southeastasia";

    private const int SampleRate = 24000;
    public string azureVoice = "en-SG-WayneNeural";
    public string azureVoiceLocale = "en-SG";
    private bool waitingForSpeak;
    private bool audioSourceNeedStop;
    private string message;

    private SpeechConfig speechConfig;
    private SpeechSynthesizer synthesizer;

    public AzureTTS(string apiKey, string voiceId) : base(apiKey, voiceId)
    {
    }

    public override void GetAudio(string text)
    {
        // Starts speech synthesis, and returns once the synthesis is started.
        using (var result = synthesizer.StartSpeakingTextAsync(text).Result)
        {
            var audioDataStream = AudioDataStream.FromResult(result);

            // Read all audio data at once
            List<byte> allAudioBytes = new List<byte>();
            byte[] buffer = new byte[4096];
            uint bytesRead;

            do
            {
                bytesRead = audioDataStream.ReadData(buffer);
                for (int i = 0; i < bytesRead; i++)
                {
                    allAudioBytes.Add(buffer[i]);
                }
            } while (bytesRead > 0);

            Debug.Log($"Read {allAudioBytes.Count} bytes of audio data.");

            // Convert bytes to floats (16-bit PCM to float)
            int totalSamples = allAudioBytes.Count / 2;
            float[] audioData = new float[totalSamples];
            for (int i = 0; i < totalSamples; i++)
            {
                short sample = (short)(allAudioBytes[i * 2 + 1] << 8 | allAudioBytes[i * 2]);
                audioData[i] = sample / 32768.0f;
            }

            // Create AudioClip
            var audioClip = AudioClip.Create(
                "Speech",
                totalSamples,
                1,                // 1 channel (Mono)
                SampleRate,       // usually 16000 or 24000 for Azure
                false             // streaming: false because we have full data
            );
            audioClip.SetData(audioData, 0);


            _audioClip = audioClip;
            AudioReceived.Invoke(audioClip);
        }

    }

    public override void Setup()
    {
        speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);
        speechConfig.SpeechSynthesisVoiceName = azureVoice;
        speechConfig.SpeechSynthesisLanguage = azureVoiceLocale;
        // The default format is RIFF, which has a riff header.
        // We are playing the audio in memory as audio clip, which doesn't require riff header.
        // So we need to set the format to raw (24KHz for better quality).
        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

        // Creates a speech synthesizer.
        // Make sure to dispose the synthesizer after use!
        synthesizer = new SpeechSynthesizer(speechConfig, null);

        synthesizer.SynthesisCanceled += (s, e) =>
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
            message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
        };
    }

    void Update()
    {

    }

    void OnDestroy()
    {
        if (synthesizer != null)
        {
            synthesizer.Dispose();
        }
    }
}
