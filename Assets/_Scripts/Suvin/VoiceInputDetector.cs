using UnityEngine;

public class VoiceInputDetector : MonoBehaviour
{
    [Header("References")]
    public AzureSTT stt;
    public GroqTTS tts;

    [Header("Mic Detection Settings")]
    public string microphoneDevice;
    public float detectionThreshold = 0.02f; // adjust based on environment
    public float checkInterval = 0.1f;       // seconds between checks
    public bool isListening = true;

    private AudioClip micClip;
    private int sampleWindow = 128;
    private bool isUserTalking = false;

    private void Start()
    {
        // Pick default mic
        if (Microphone.devices.Length > 0)
            microphoneDevice = Microphone.devices[0];

        // Start a looping mic capture for volume detection only
        micClip = Microphone.Start(microphoneDevice, true, 1, 16000);
        InvokeRepeating(nameof(CheckMicVolume), 0f, checkInterval);
    }

    private void CheckMicVolume()
    {
        if (!isListening || micClip == null) return;

        float level = GetMicLoudness();

        if (!isUserTalking && level > detectionThreshold)
        {
            Debug.Log($"🎙️ Voice detected! Volume={level}");

            // If AI is currently speaking → stop playback
            if (tts != null && tts.IsPlaying())
            {
                Debug.Log("🛑 Interrupting TTS playback...");
                tts.StopPlayback();
            }

            // Trigger AzureSTT to start recording
            if (stt != null && !stt.IsRecording())
            {
                Debug.Log("🎤 Starting speech recognition...");
                stt.StartRecord();
            }

            isUserTalking = true;
        }
        else if (level < detectionThreshold * 0.5f)
        {
            // Reset state after user stops speaking
            isUserTalking = false;
        }
    }

    private float GetMicLoudness()
    {
        int micPos = Microphone.GetPosition(microphoneDevice) - sampleWindow;
        if (micPos < 0) return 0;

        float[] waveData = new float[sampleWindow];
        micClip.GetData(waveData, micPos);

        float sum = 0f;
        for (int i = 0; i < sampleWindow; i++)
            sum += waveData[i] * waveData[i];

        return Mathf.Sqrt(sum / sampleWindow);
    }

    private void OnDisable()
    {
        if (Microphone.IsRecording(microphoneDevice))
            Microphone.End(microphoneDevice);
    }
}
