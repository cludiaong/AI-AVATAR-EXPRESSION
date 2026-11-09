using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using static Unity.VisualScripting.Icons;

public class AzureSTT : STT_Abstract
{
    private SpeechRecognizer recognizer;

    string recognizedString = "";
    string recognizedStringFinal = "";
    bool isRecording;
    bool isRecognizerProcessing;
    bool isCancel;
    IEnumerator Start()
    {
        CheckMicPermission();
        StartCoroutine(HandleMicrophone());
        while (true)
        {
            yield return new WaitUntil(() => !string.IsNullOrEmpty(recognizedString));
            onSpeechRecognized.Invoke();
            yield return new WaitUntil(() => string.IsNullOrEmpty(recognizedString));
        }
    }
    IEnumerator HandleMicrophone()
    {
        while (true)
        {
            yield return new WaitUntil(() => isRecording);
            StartingRecord();
            yield return new WaitUntil(() => recognizer != null);
            yield return new WaitUntil(() => !isRecognizerProcessing);
            yield return new WaitUntil(() => !isRecording);
            float stopTimer = 0f;
            if (isCancel)
            {
                CanclingRecord();
                isCancel = false;
            }
            else
            {
                do
                {
                    stopTimer += Time.deltaTime;
                    yield return null;
                } while (stopTimer < 1f && !isRecording);

                if (stopTimer >= 1f)
                {
                    StopingRecord();
                    onSpeechRecognizeEndText.Invoke(FormatRecognizeStr());
                }
            }
            yield return new WaitUntil(() => !isRecognizerProcessing);
        }
    }
    void CheckMicPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    public void SetMicrophone()
    {
        isRecording = !isRecording;

        if (isRecording)
        {
            StartRecord();
        }
        else
        {
            StopRecord();
        }
    }
    private void OnDisable()
    {
        StopRecognition();
    }
    public override void StartRecord()
    {
        isRecording = true;
    }

    public override void StopRecord()
    {
        isRecording = false;
    }

    public override void CancelRecord()
    {
        isRecording = false;
        isCancel = true;
    }
    public override string GetString()
    {
        return FormatRecognizeStr();
    }
    public void StartingRecord()
    {
        StartCoroutine(StartContinuousRecognition());

        Debug.Log("Starting Record");
    }
    public void StopingRecord() //SUVIN
{
    isRecording = false;  // just mark as stopped
    Debug.Log("Stopping Record (pause only, not disposing)");
}

    public void CanclingRecord()
    {
        StopRecognition();
        recognizedString = "";
    }
    public override bool IsRecording()
    {
        return isRecording;
    }

    void CreateSpeechRecognizer()
    {
        if (recognizer == null)
        {
            SpeechConfig config = SpeechConfig.FromSubscription(Credentials.SPEECH_API_KEY, Credentials.SPEECH_REGION);
            config.SpeechRecognitionLanguage = language;
            recognizer = new SpeechRecognizer(config);

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
    private IEnumerator StartContinuousRecognition()
    {
        //Debug.Log("Starting Continuous Speech Recognition.");
        isRecognizerProcessing = true;
        CreateSpeechRecognizer();
        if (recognizer != null)
        {
            //if (!(onTest) && ObjectiveHandler.handler != null)
            //{
            //    ObjectiveHandler.handler.SetObjective(ObjectiveHandler.objectiveEnum.TalkTo, true);
            //}
            //Debug.Log("Starting Speech Recognizer.");

            onSpeechRecognizeStart.Invoke();
            yield return recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            //Debug.Log("Speech Recognizer is now running.");
        }
        isRecognizerProcessing = false;
        //Debug.Log("Start Continuous Speech Recognition exit");
    }

    public async void StopRecognition()
    {
        isRecognizerProcessing = true;
        if (recognizer != null)
        {
            onSpeechRecognizeEnd.Invoke();
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
            //Debug.Log("Speech Recognizer is now stopped.");
        }
        isRecognizerProcessing = false;
    }

    #region Speech Recognition event handlers
    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"Session started event. Event: {e.ToString()}.");
    }

    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"\n    Session event. Event: {e.ToString()}.");
        recognizedString = "";
        Debug.Log($"Session Stop detected. Stop the recognition.");

// 🔑 Restart listening automatically // SUVIN
    if (!isCancel)
    {
        StartCoroutine(RestartSTT(true));
    }

    }

    private void SpeechStartDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechStartDetected received: offset: {e.Offset}.");
    }

    private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechEndDetected received: offset: {e.Offset}.");
        Debug.Log($"Speech end detected.");
        
        StartCoroutine(DelayedSendTranscript(2f)); // 2s delay


        // ✅ Send transcript only at end
    string finalTranscript = FormatRecognizeStr();
    if (!string.IsNullOrWhiteSpace(finalTranscript))
    {
        Debug.Log("Sending final transcript after speech end: " + finalTranscript);
        onSpeechRecognizeEndText.Invoke(finalTranscript);
    }

    // ✅ Clear buffer so TTS voice doesn’t get re-sent
    recognizedString = "";
    }

    private IEnumerator DelayedSendTranscript(float delay) //Suvin
{
    yield return new WaitForSeconds(delay);

    var text = FormatRecognizeStr().Trim();
    if (!string.IsNullOrEmpty(text))
    {
        Debug.Log("Auto-sending transcript after delay: " + text);
        onSpeechRecognizeEndText.Invoke(text);
    }
} //SUVIN

    // "Recognizing" events are fired every time we receive interim results during recognition (i.e. hypotheses)
    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizingSpeech)
        {
            Debug.Log($"HYPOTHESIS: Text={e.Result.Text}");
            onTranscriptRecieve(e.Result.Text);
        }
    }

    // "Recognized" events are fired when the utterance end was detected by the server
    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e) //suvin
{
    if (e.Result.Reason == ResultReason.RecognizedSpeech)
    {
        Debug.Log($"RECOGNIZED: Text={e.Result.Text}");

        onSpeechRecognizedFinishHypnosis.Invoke(e.Result.Text);
        onTranscriptRecieveFinal(e.Result.Text);

        // ✅ Send transcript immediately when recognized
        if (!string.IsNullOrWhiteSpace(e.Result.Text))
        {
            onSpeechRecognizeEndText.Invoke(e.Result.Text.Trim());
        }
    }
    else if (e.Result.Reason == ResultReason.NoMatch)
    {
        Debug.Log("NOMATCH: Speech could not be recognized.");
    }
}

    // "Canceled" events are fired if the server encounters some kind of error.
    // This is often caused by invalid subscription credentials.
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        onSpeechRecognizeCancel.Invoke();
        Debug.LogError($"CANCELED: ErrorDetails={e.ErrorDetails}");
        Debug.LogError("CANCELED: Did you update the subscription info?");

        StartCoroutine(RestartSTT(isRecording));
    }
    #endregion

    public IEnumerator RestartSTT(bool isRecording)
    {
        this.isRecording = !isRecording;
        yield return new WaitUntil(() => isRecognizerProcessing);
        yield return new WaitUntil(() => !isRecognizerProcessing);
        this.isRecording = isRecording;
    }
    public void onTranscriptRecieveFinal(string content)
    {
        recognizedString = $"{recognizedString[..(recognizedString.IndexOf("<i>"))]}{content}";
    }
    public void onTranscriptRecieve(string content)
    {
        var interim = $"<i>{content}</i>";
        var index = recognizedString.IndexOf("<i>");
        if (index >= 0)
            recognizedString = $"{recognizedString[..index]}{interim}";
        else
            recognizedString = $"{recognizedString} {interim}";
        onSpeechRecognizedHypnosis.Invoke(FormatRecognizeStr());
    }
    public string FormatRecognizeStr()
    {
        return recognizedString.Replace("<i>", "").Replace("</i>", "");
    }

    [Serializable]
    public class AzureSettings
    {
        public string Language;
    }
}