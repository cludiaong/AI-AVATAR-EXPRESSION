using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GameDataHandle : MonoBehaviour
{
    [SerializeField] string configFileName = "avatarConfig.json";

    public AzureSpeechRecognitionModule speechRecognitionModule;

    public UnityEvent<ElevenlabsAPI> onSelectedTTS = new();
    private IEnumerator Start()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, configFileName);

        string contents;

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            contents = www.text;
        }
        else
        {
            contents = System.IO.File.ReadAllText(filePath);
        }

        Debug.Log("Loaded file: " + contents);

        AvatarConfig config = JsonUtility.FromJson<AvatarConfig>(contents);
        Debug.Log(JsonUtility.ToJson(config));

        speechRecognitionModule.language = config.speechLang;

        var allTTS = FindObjectsOfType<ElevenlabsAPI>();
        ElevenlabsAPI selectedTTS = allTTS.Where((e) => e.ttsName.ToLower() == config.voiceMode.ToLower()).FirstOrDefault();
        SetupTTS(selectedTTS, config);

        onSelectedTTS.Invoke(selectedTTS);
    }
    public void SetupTTS(ElevenlabsAPI selectedTTS, AvatarConfig config)
    {
        if (selectedTTS.ttsName == "elevenlabs")
        {
            selectedTTS._voiceId = config.voiceConfig.elevenLabsConfig.voiceID;
        }
        else if (selectedTTS.ttsName == "azure")
        {
            (selectedTTS as AzureTTS).azureVoice = config.voiceConfig.azureConfig.azureVoice;
            (selectedTTS as AzureTTS).azureVoiceLocale = config.voiceConfig.azureConfig.azureVoiceLocale;
        }
        else if (selectedTTS.ttsName == "google")
        {
            if (Enum.TryParse(typeof(Enumerators.SsmlVoiceGender), config.voiceConfig.googleConfig.voiceGender, true, out object genderObj))
            {
                (selectedTTS as GoogleTTS).voiceGender = (Enumerators.SsmlVoiceGender)genderObj;
            }
            if (Enum.TryParse(typeof(Enumerators.LanguageCode), config.voiceConfig.googleConfig.voiceLanguageCode, true, out object langObj))
            {
                (selectedTTS as GoogleTTS).voiceLanguageCode = (Enumerators.LanguageCode)langObj;
            }
            (selectedTTS as GoogleTTS).voiceName = config.voiceConfig.googleConfig.voiceName;
            (selectedTTS as GoogleTTS).ssml = config.voiceConfig.googleConfig.ssml;
            (selectedTTS as GoogleTTS).pitch = config.voiceConfig.googleConfig.pitch;
            (selectedTTS as GoogleTTS).speakingRate = config.voiceConfig.googleConfig.speakingRate;
        }
        //else if (selectedTTS.ttsName == "hume")
        //{
        //    //(selectedTTS as HumeTTS).humeRequest = config.voiceConfig.humeConfig;
        //    //(selectedTTS as HumeTTS).utterancesPreset = config.voiceConfig.humeConfig.utterances[0];
        //    //(selectedTTS as HumeTTS).humeRequest.utterances = new();
        //}
        Debug.Log($"Selected TTS {selectedTTS.ttsName}");
        selectedTTS.Setup();
    }
}
[Serializable]
public class AvatarConfig
{
    public string speechLang;
    public string voiceMode;
    public VoiceConfig voiceConfig;

    [Serializable]
    public class VoiceConfig
    {
        public ElevenLabsTTSConfig elevenLabsConfig;
        public AzureTTSConfig azureConfig;
        public GoogleTTSConfig googleConfig;
        //public HumeTTS.HumeRequest humeConfig;

        [Serializable]
        public class ElevenLabsTTSConfig
        {
            public string voiceID;
        }
        [Serializable]
        public class AzureTTSConfig
        {
            public string azureVoice;
            public string azureVoiceLocale;
        }
        [Serializable]
        public class GoogleTTSConfig
        {
            public string voiceGender;
            public string voiceLanguageCode;
            public string voiceName;
            public bool ssml;
            public double pitch;
            public double speakingRate;
        }
    }
}