using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ElevenLabs_TTS : TTS_Abstract
{
    public string m_VoiceID = "mbL34QDB5FptPamlgvX5";
    //public string m_VoiceModel = "eleven_monolingual_v1";
    //public float m_VoiceStyle = 0.5f;
    //public float m_VoiceSimilarityBoost = 0;
    //public float m_VoiceStability = 0;

    private string _apiUrl = "https://api.elevenlabs.io";
    public ElevenLabsRequest postData = new ElevenLabsRequest();
    public IEnumerator RequestAudio(string message)
    {
        Debug.Log($"PreRequest ElevenLabs Audio: \"{message}\"");
        postData.text = message;
        var json = JsonConvert.SerializeObject(postData);
        var uH = new UploadHandlerRaw(Encoding.ASCII.GetBytes(json));
        var url = $"{_apiUrl}/v1/text-to-speech/{m_VoiceID}?optimize_streaming_latency=0";
        var request = UnityWebRequest.PostWwwForm(url, json);
        var downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        
        request.uploadHandler = uH;
        request.downloadHandler = downloadHandler;
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", Credentials.ELEVENLAP_API_KEY);
        request.SetRequestHeader("Accept", "audio/mpeg");
        do
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading audio: " + request.error + "\n" + url);
                yield return new WaitForSeconds(5f);
                request = UnityWebRequest.PostWwwForm(url, json);
                downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

                request.uploadHandler = uH;
                request.downloadHandler = downloadHandler;
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("xi-api-key", Credentials.ELEVENLAP_API_KEY);
                request.SetRequestHeader("Accept", "audio/mpeg");
            }
        } while ((request.result != UnityWebRequest.Result.Success));
        Debug.Log("Request ElevenLabs Audio: Success");
        AudioClip audioClip = downloadHandler.audioClip;
        onReceiveClip.Invoke(audioClip);
        onReceiveClipText.Invoke(message);
        onReceiveResponse.Invoke(message, audioClip);
        request.Dispose();
        yield return null;
    }

    public override void Speak(string message)
    {
        StartCoroutine(RequestAudio(message));
    }
    [Serializable]
    public class ElevenLabsConfig
    {
        public string VoiceID;
        public ElevenLabsRequest ElevenLabs_Settings;
    }
    [Serializable]
    public class ElevenLabsRequest
    {
        public string text;
        public string model_id; // eleven_monolingual_v1
        public VoiceSettings voice_settings;
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
