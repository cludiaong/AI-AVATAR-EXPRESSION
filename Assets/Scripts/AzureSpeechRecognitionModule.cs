using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TMPro;
using UnityEngine.UI;
using NAudio.CoreAudioApi;
using UnityEngine.Events;

public class AzureSpeechRecognitionModule : MonoBehaviour
{
    private DictationRecognizer m_DictationRecognizer;

    public string recognizedString = "";

    private SpeechRecognizer recognizer;

    [SerializeField] public string language = "en-us";
    public TextMeshProUGUI speechText;
    public TextMeshProUGUI answerText;
    public InputField inFieldChat;

    [SerializeField] private AudioClip clip;
    public bool isRecording;
    public float speakingDelay;

    public bool isDoneTalking;

    //[SerializeField] GeminiAPIModule geminiAPI;

    public TMP_Dropdown sourceDropdown;
    public int choosenDeviceIndex = 0;
    public List<string> micName = new List<string>();
    public List<string> familiarName = new List<string>();

    [SerializeField] private Button sendButton, speechButton;
    [SerializeField] private Sprite micSprite, recSprite;

    public UnityEvent STT_Start, STT_Stop;

    // Start is called before the first frame update
    void Start()
    {
        PopulateSourceDropDown();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDoneTalking)
        {
            isDoneTalking = false;
            sendButton.interactable = true;
        }

        if (isRecording)
        {
            inFieldChat.text = recognizedString;
            speechText.text = recognizedString;
            if (speechText.text != "")
            {
                sendButton.interactable = true;
            }
            else
            {
                sendButton.interactable = false;
            }
        }

    }


    private void PopulateSourceDropDown()
    {
        var options = new List<TMP_Dropdown.OptionData>();

        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        Debug.Log(enumerator.ToString());

        foreach (var endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(endpoint.DeviceFriendlyName, null);

            options.Add(optionData);
            options.Reverse();

            micName.Add(endpoint.ID);
            micName.Reverse();

            familiarName.Add(endpoint.DeviceFriendlyName);
            familiarName.Reverse();
        }

        sourceDropdown.options = options;
        choosenDeviceIndex = 0;
    }

    public void ChooseMicrophone(int optionIndex)
    {
        choosenDeviceIndex = optionIndex;
        Debug.Log(Microphone.devices[choosenDeviceIndex]);
        if (isRecording)
        {
            StartCoroutine(SwitchMic());
        }
    }

    IEnumerator SwitchMic()
    {
        isRecording = false;
        StopRecognition();
        speechButton.GetComponent<Image>().sprite = micSprite;
        yield return new WaitForSeconds(0.5f);
        isRecording = true;
        recognizedString = "";
        speechButton.GetComponent<Image>().sprite = recSprite;
        StartContinuousRecognition();
    }

    public void SetRecord()
    {
        isRecording = !isRecording;

        if (isRecording)
        {
            StartRecord();
            STT_Start.Invoke();
        }
        else
        {
            StopRecord();
            STT_Stop.Invoke();
        }
    }

    public void StartRecord()
    {
        answerText.text = "";
        isRecording = true;
        //isDoneTalking = false;
        speakingDelay = 2.5f;
        recognizedString = "";
        StartContinuousRecognition();
        sendButton.interactable = true;
        speechButton.GetComponent<Image>().sprite = recSprite;
    }

    public void StopRecord()
    {
        isRecording = false;
        StopRecognition();
        speechButton.GetComponent<Image>().sprite = micSprite;
        sendButton.interactable = false;
    }

    public void SubmitText()
    {
        //speechButton.interactable = false;
        //sendButton.interactable = false;
        //if (speechText.text != "")
        //{
        //geminiAPI.AskAI();
        //}
    }

    public void DictationCallbacks()
    {
        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            Debug.LogFormat("Dictation result: {0}", text);
            speechText.text = text;
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };
    }


    void CreateSpeechRecognizer()
    {
        if (recognizer == null)
        {
            SpeechConfig config = SpeechConfig.FromSubscription(Credentials.SPEECH_API_KEY, Credentials.SPEECH_REGION);
            config.SpeechRecognitionLanguage = language;
            AudioConfig audioConfig = AudioConfig.FromMicrophoneInput(micName[choosenDeviceIndex]);
            Debug.Log(familiarName[choosenDeviceIndex]);
            recognizer = new SpeechRecognizer(config, audioConfig);

            if (recognizer != null)
            {
                // Subscribes to speech events.
                recognizer.Recognizing += RecognizingHandler;
                recognizer.Recognized += RecognizedHandler;
                recognizer.SpeechStartDetected += SpeechStartDetectedHandler;
                recognizer.SpeechEndDetected += SpeechEndDetectedHandler;
                recognizer.Canceled += CanceledHandler;
                recognizer.SessionStarted += SessionStartedHandler;
                recognizer.SessionStopped += SessionStoppedHandler;
            }
        }
    }

    private async void StartContinuousRecognition()
    {
        Debug.Log("Starting Continuous Speech Recognition.");
        CreateSpeechRecognizer();
        if (recognizer != null)
        {
            Debug.Log("Starting Speech Recognizer.");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

            Debug.Log("Speech Recognizer is now running.");
        }
        Debug.Log("Start Continuous Speech Recognition exit");
    }

    public async void StopRecognition()
    {
        if (recognizer != null)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            recognizer.Recognizing -= RecognizingHandler;
            recognizer.Recognized -= RecognizedHandler;
            recognizer.SpeechStartDetected -= SpeechStartDetectedHandler;
            recognizer.SpeechEndDetected -= SpeechEndDetectedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.SessionStarted -= SessionStartedHandler;
            recognizer.SessionStopped -= SessionStoppedHandler;
            recognizer.Dispose();
            recognizer = null;

            Debug.Log("Speech Recognizer is now stopped.");
        }
    }

    #region Speech Recognition event handlers
    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"\n    Session started event. Event: {e.ToString()}.");
    }

    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"\n    Session event. Event: {e.ToString()}.");
        Debug.Log($"Session Stop detected. Stop the recognition.");
    }

    private void SpeechStartDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechStartDetected received: offset: {e.Offset}.");
    }

    private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechEndDetected received: offset: {e.Offset}.");
        Debug.Log($"Speech end detected.");
    }

    // "Recognizing" events are fired every time we receive interim results during recognition (i.e. hypotheses)
    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizingSpeech)
        {
            Debug.Log($"HYPOTHESIS: Text={e.Result.Text}");
            recognizedString = $"{e.Result.Text}";
        }
    }

    // "Recognized" events are fired when the utterance end was detected by the server
    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
            recognizedString = $"{e.Result.Text}";
        }
        else if (e.Result.Reason == ResultReason.NoMatch)
        {
            Debug.Log($"NOMATCH: Speech could not be recognized.");
        }
    }

    // "Canceled" events are fired if the server encounters some kind of error.
    // This is often caused by invalid subscription credentials.
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        Debug.Log($"CANCELED: Reason={e.Reason}");

        if (e.Reason == CancellationReason.Error)
        {
            Debug.LogError($"CANCELED: ErrorDetails={e.ErrorDetails}");
            Debug.LogError("CANCELED: Did you update the subscription info?");
        }
    }
    #endregion
}
