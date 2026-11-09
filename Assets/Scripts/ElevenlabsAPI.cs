using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NaughtyAttributes;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Utilities.Async;
using Random = UnityEngine.Random;

public class ElevenlabsAPI : MonoBehaviour
{
    public virtual string ttsName
    {
        get => "elevenlabs";
    }
    [SerializeField]
    public string m_ForceAccent = "[strong Singapore accent]";
    [SerializeField]
    public string m_model_ID = "eleven_multilingual_v2";
    [SerializeField]
    public string _voiceId;
    [SerializeField]
    protected string _apiKey;
    [SerializeField]
    protected string _apiUrl = "https://api.elevenlabs.io";

    [SerializeField]
    protected int timeout = 10;

    public OpenAI.ChatGPT chatGPTModule;
    public AzureSpeechRecognitionModule azureSpeech;
    public Animator animTalk;

    protected AudioClip _audioClip;

    // If true, the audio will be streamed instead of downloaded
    // Unfortunately, Unity has some problems with streaming audio
    // but I left this option here in case you want to try it.
    public bool Streaming;

    [Range(0, 4)]
    public int LatencyOptimization;

    // This event is used to broadcast the received AudioClip
    public UnityEvent<AudioClip> AudioReceived;
    public string debugTestTTS;

    public ElevenlabsAPI(string apiKey, string voiceId)
    {
        _apiKey = apiKey;
        _voiceId = voiceId;
    }
    public virtual void Setup()
    {

    }
    [Button]
    public void TestTTS()
    {
        GetAudio(debugTestTTS);
    }

    public virtual void GetAudio(string text)
    {
        StartCoroutine(DoRequestFile($"{m_ForceAccent}{(text)}"));
    }
    public virtual string AddBreak(string text)
    {
        return text.Replace(".", $".<break time=\"{Random.Range(0.4f, 0.5f):0.0}s\" />")
            .Replace("!", $"!<break time=\"{Random.Range(0.5f, 0.7f):0.0}s\" />")
            .Replace("?", $"?<break time=\"{Random.Range(0.5f, 0.7f):0.0}s\" />")
            .Replace(",", $",<break time=\"{Random.Range(0.2f, 0.3f):0.0}s\" />");
    }
    public virtual IEnumerator DoRequestFile(string message)
    {
        message = message.Trim();
        var postData = new TextToSpeechRequest
        {
            text = message,
            model_id = m_model_ID
        };
        Debug.Log($"Eleven TTS: {message}");

        // TODO: This could be easily exposed in the Unity inspector,
        // but I had no use for it in my work demo.
        var voiceSetting = new VoiceSettings
        {
            stability = 0.5f,
            similarity_boost = 0.8f,
            style = 0.0f,
            use_speaker_boost = true
        };
        //postData.voice_settings = voiceSetting;
        var json = JsonConvert.SerializeObject(postData);
        var uH = new UploadHandlerRaw(Encoding.ASCII.GetBytes(json));
        var stream = (Streaming) ? "/stream" : "";
        var url = $"{_apiUrl}/v1/text-to-speech/{_voiceId}";
        //var url = $"{_apiUrl}/v1/text-to-speech/{_voiceId}{stream}?optimize_streaming_latency={LatencyOptimization}";
        var request = UnityWebRequest.PostWwwForm(url, json);
        var downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        if (Streaming)
        {
            downloadHandler.streamAudio = true;
        }
        request.uploadHandler = uH;
        request.downloadHandler = downloadHandler;
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", _apiKey);
        request.SetRequestHeader("Accept", "audio/mpeg");
        yield return request.SendWebRequest();
        Debug.Log($"{url}\n{json}");

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading audio: " + request.error);
            yield break;
        }
        AudioClip audioClip = downloadHandler.audioClip;
        _audioClip = audioClip;
        AudioReceived.Invoke(audioClip);
        request.Dispose();

        animTalk.SetBool("isTalking", true);
        yield return new WaitForSeconds(audioClip.length);
        animTalk.SetBool("isTalking", false);
    }
    public async Task<AudioClip> RequestAudio(string message)
    {
        var postData = new TextToSpeechRequest
        {
            text = message,
            model_id = "eleven_multilingual_v2"
        };

        // TODO: This could be easily exposed in the Unity inspector,
        // but I had no use for it in my work demo.
        var voiceSetting = new VoiceSettings
        {
            stability = 0.5f,
            similarity_boost = 0.8f,
            style = 0.0f,
            use_speaker_boost = true
        };
        //postData.voice_settings = voiceSetting;
        var json = JsonConvert.SerializeObject(postData);
        var uH = new UploadHandlerRaw(Encoding.ASCII.GetBytes(json));
        var stream = (Streaming) ? "/stream" : "";
        var url = $"{_apiUrl}/v1/text-to-speech/{_voiceId}{stream}?optimize_streaming_latency={LatencyOptimization}";
        var request = UnityWebRequest.PostWwwForm(url, json);
        var downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        if (Streaming)
        {
            downloadHandler.streamAudio = true;
        }
        request.uploadHandler = uH;
        request.downloadHandler = downloadHandler;
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", _apiKey);
        request.SetRequestHeader("Accept", "audio/mpeg");
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading audio: " + request.error);
            return null;
        }
        AudioClip audioClip = downloadHandler.audioClip;
        _audioClip = audioClip;
        AudioReceived.Invoke(audioClip);
        request.Dispose();
        return audioClip;
    }

    [Serializable]
    public class TextToSpeechRequest
    {
        public string text;
        public string model_id; // eleven_monolingual_v1
        //public VoiceSettings voice_settings;
    }

    [Serializable]
    public class VoiceSettings
    {
        public float stability; // 0
        public float similarity_boost; // 0
        public float style; // 0.5
        public bool use_speaker_boost; // true
    }
}