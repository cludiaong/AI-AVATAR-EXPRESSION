using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class TTSManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField; // For TTS text input
    [SerializeField] private TMP_InputField voiceIdInputField; // For TTS voice ID
    [SerializeField] private Slider volumeSlider; // For TTS volume
    [SerializeField] private Slider speedSlider;  // For TTS speed
    [SerializeField] private Button speakButton; // For TTS
    [SerializeField] private AudioSource audioSource; // For TTS playback
    [SerializeField] private HumeTTS humeTTS; // For TTS

    [Header("Speech-to-Text (STT) Elements")]
    [SerializeField] private Button recordButton; // For STT recording
    [SerializeField] private Button stopRecordButton; // For stopping STT recording
    [SerializeField] private TMP_Text transcribedTextDisplay; // To display transcribed text
    [SerializeField] private TMP_Text emotionAnalysisDisplay; // To display emotion analysis
    [SerializeField] private HumeSTT humeSTT; // For STT

    private AudioClip microphoneClip; // To store microphone audio
    private string microphoneDevice; // To store the selected microphone device

    void Start()
    {
        // TTS setup
        if (inputField == null) Debug.LogError("Input Field not assigned.");
        if (voiceIdInputField == null) Debug.LogError("Voice ID Input Field not assigned.");
        if (volumeSlider == null) Debug.LogError("Volume Slider not assigned.");
        if (speedSlider == null) Debug.LogError("Speed Slider not assigned.");
        if (speakButton == null) Debug.LogError("Speak Button not assigned.");
        if (audioSource == null) Debug.LogError("Audio Source not assigned.");
        if (humeTTS == null) Debug.LogError("HumeTTS script not assigned.");

        speakButton.onClick.AddListener(OnSpeakButtonClicked);

        volumeSlider.value = audioSource.volume;
        speedSlider.value = 1.0f;

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // STT setup
        if (recordButton == null) Debug.LogError("Record Button not assigned.");
        if (stopRecordButton == null) Debug.LogError("Stop Record Button not assigned.");
        if (transcribedTextDisplay == null) Debug.LogError("Transcribed Text Display not assigned.");
        if (emotionAnalysisDisplay == null) Debug.LogError("Emotion Analysis Display not assigned.");
        if (humeSTT == null) Debug.LogError("HumeSTT script not assigned.");

        recordButton.onClick.AddListener(OnRecordButtonClicked);
        stopRecordButton.onClick.AddListener(OnStopRecordButtonClicked);
        stopRecordButton.interactable = false; // Disable stop button initially

        // Get microphone device
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            Debug.Log($"Using microphone: {microphoneDevice}");
        }
        else
        {
            Debug.LogError("No microphone devices found!");
            recordButton.interactable = false; // Disable record button if no mic
        }
    }

    void OnVolumeChanged(float value)
    {
        audioSource.volume = value;
    }

    void OnSpeakButtonClicked()
    {
        string textToSynthesize = inputField.text;
        string voiceId = voiceIdInputField.text;
        float speed = speedSlider.value;

        speakButton.interactable = false;
        StartCoroutine(humeTTS.SynthesizeSpeech(textToSynthesize, voiceId, speed, OnSpeechSynthesized, OnError));
    }

    void OnSpeechSynthesized(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
        speakButton.interactable = true;
    }

    void OnError(string errorMessage)
    {
        Debug.LogError($"TTS Error: {errorMessage}");
        speakButton.interactable = true;
    }

    void OnRecordButtonClicked()
    {
        if (string.IsNullOrEmpty(microphoneDevice))
        {
            Debug.LogError("No microphone device selected.");
            return;
        }

        // Start recording
        microphoneClip = Microphone.Start(microphoneDevice, false, 30, 16000); // 30 seconds, 16kHz sample rate  recordButton.interactable = false;
        stopRecordButton.interactable = true;
        transcribedTextDisplay.text = "Recording...";
        emotionAnalysisDisplay.text = "";
    }

    void OnStopRecordButtonClicked()
    {
        if (microphoneClip == null) return;

        Microphone.End(microphoneDevice);
        recordButton.interactable = true;
        stopRecordButton.interactable = false;
        transcribedTextDisplay.text = "Processing...";

        // Convert AudioClip to byte array for Hume STT
        float[] samples = new float[microphoneClip.samples * microphoneClip.channels];
        microphoneClip.GetData(samples, 0);

        byte[] audioBytes = WavUtility.FromAudioClip(microphoneClip);

        StartCoroutine(humeSTT.SendAudioForAnalysis(audioBytes, OnSTTAnalysisComplete, OnSTTError));
    }

    void OnSTTAnalysisComplete(string transcribedText, string emotionAnalysis)
    {
        transcribedTextDisplay.text = "Transcribed: " + (string.IsNullOrEmpty(transcribedText) ? "No transcription available." : transcribedText);
        emotionAnalysisDisplay.text = "Emotion: " + (string.IsNullOrEmpty(emotionAnalysis) ? "No emotion analysis available." : emotionAnalysis);
    }

    void OnSTTError(string errorMessage)
    {
        Debug.LogError($"STT Error: {errorMessage}");
        transcribedTextDisplay.text = "STT Error: " + errorMessage;
        emotionAnalysisDisplay.text = "";
    }
}


