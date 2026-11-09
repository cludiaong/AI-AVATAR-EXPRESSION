// using Newtonsoft.Json;
// using System;
// using System.Collections;
// using System.Text;
// using UnityEngine;
// using UnityEngine.Networking;
// using UnityEngine.UI;
// using UnityEngine.Events; //SUVIN


// public class GroqTTS : TTS_Abstract
// {

//     public GroqTTSRequest postData;

//      public UnityEvent<string> onAudioPlay = new UnityEvent<string>(); //SUVIN
//     public UnityEvent onAudioComplete = new UnityEvent(); //SUVIN

//         private AudioSource audioSource;

//         private void Awake()
//     {
//         // Attach or get existing AudioSource
//         audioSource = GetComponent<AudioSource>();
//         if (audioSource == null)
//         {
//             audioSource = gameObject.AddComponent<AudioSource>();
//         }
//     }



//     public override void Speak(string message)
//     {
//         StartCoroutine(RequestAudio(message.Trim()));
//     }
//     public IEnumerator RequestAudio(string message)
//     {
//         if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message) || message == "")
//         {
//             Debug.LogWarning("PreRequest Groq no input");
//             onReceiveClip.Invoke(null);
//             onReceiveClipText.Invoke(message);
//             onReceiveResponse.Invoke(message, null);
//             yield break;
//         }
//         Debug.Log($"PreRequest Groq TTS Audio: \"{message}\"");
//         string apiUrl = "https://api.groq.com/openai/v1/audio/speech";
//         postData.input = message;
//         var json = JsonConvert.SerializeObject(postData);
//         Debug.Log(json);
//         byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
//         var url = $"{apiUrl}";
//         var request = UnityWebRequest.PostWwwForm(url, json);
//         var downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

//         request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//         request.downloadHandler = downloadHandler;
//         request.SetRequestHeader("Content-Type", "application/json");
//         request.SetRequestHeader("Authorization", $"Bearer {Credentials.GROQ_API_KEY}");

//         yield return request.SendWebRequest();
//         AudioClip audioClip = null;
//         if (request.result != UnityWebRequest.Result.Success)
//         {
//             Debug.LogError("Error downloading audio: " + request.error + "\n" + url);
//         }
//         else
//         {
//             Debug.Log("Request GroqTTS Audio: Success");
//             audioClip = downloadHandler.audioClip;

//             // 🔊 Fire "play started". SUVIN UNTIL
//     onAudioPlay.Invoke(message);

//     // Play audio (if you’re using AudioSource in TTS_Abstract)
//     if (audioSource != null && audioClip != null)
//     {
//         audioSource.clip = audioClip;
//         audioSource.Play();

//         // wait for it to finish
//         StartCoroutine(WaitForAudioEnd(audioClip.length));
//     } //UNTIL SUVIN

//             request.Dispose();
//         }
//         onReceiveClip.Invoke(audioClip);
//         onReceiveClipText.Invoke(message);
//         onReceiveResponse.Invoke(message, audioClip);
//         yield return null;
//     }
// private IEnumerator WaitForAudioEnd(float duration) //SUVIN
// {
//     yield return new WaitForSeconds(duration);

//     Debug.Log("GroqTTS finished playback.");
//     onAudioComplete.Invoke();
// }

//     [Serializable]
//     public class GroqTTSRequest
//     {
//         public string input;
//         public string model;
//         public string voice;
//         public string response_format;
//     }
// }


using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class GroqTTS : TTS_Abstract
{
    public GroqTTSRequest postData;

    public UnityEvent<string> onAudioPlay = new UnityEvent<string>();
    public UnityEvent onAudioComplete = new UnityEvent();

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public override void Speak(string message)
    {
        StartCoroutine(RequestAudio(message.Trim()));
    }

    public bool IsPlaying()
{
    return audioSource != null && audioSource.isPlaying;
}

public void StopPlayback()
{
    if (audioSource != null && audioSource.isPlaying)
    {
        audioSource.Stop();
        Debug.Log("GroqTTS: Stopped playback manually.");
    }
}

    public IEnumerator RequestAudio(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("PreRequest Groq no input");
            onReceiveClip.Invoke(null);
            onReceiveClipText.Invoke(message);
            onReceiveResponse.Invoke(message, null);
            yield break;
        }

        Debug.Log($"PreRequest Groq TTS Audio: \"{message}\"");
        string apiUrl = "https://api.groq.com/openai/v1/audio/speech";
        postData.input = message;
        var json = JsonConvert.SerializeObject(postData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        var request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerAudioClip(apiUrl, AudioType.MPEG);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {Credentials.GROQ_API_KEY}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading audio: " + request.error);
            yield break;
        }

        Debug.Log("Request GroqTTS Audio: Success");
        var audioClip = DownloadHandlerAudioClip.GetContent(request);
        request.Dispose();

        // 🔊 Fire play event
        onAudioPlay.Invoke(message);

        if (audioSource != null && audioClip != null)
{
    audioSource.clip = audioClip;
    audioSource.Play();

    // 🚫 Pause STT while TTS is speaking
    var azure = FindObjectOfType<AzureSTT>();
    if (azure != null)
    {
        Debug.Log("Pausing STT while TTS speaks...");
        azure.StopRecognition(); // pause only, not dispose
    }

    StartCoroutine(WaitForAudioEnd(audioClip.length));
}

       

        onReceiveClip.Invoke(audioClip);
        onReceiveClipText.Invoke(message);
        onReceiveResponse.Invoke(message, audioClip);
    }

    private IEnumerator WaitForAudioEnd(float duration)
{
    yield return new WaitForSeconds(duration + 1f); // add 1 sec safety buffer

    Debug.Log("GroqTTS finished playback. Resuming STT...");
    var azure = FindObjectOfType<AzureSTT>();
    if (azure != null)
    {
        azure.StartRecord(); // re-enable mic listening
    }

    onAudioComplete.Invoke();
}

    [Serializable]
    public class GroqTTSRequest
    {
        public string input;
        public string model;
        public string voice;
        public string response_format;
    }
}
