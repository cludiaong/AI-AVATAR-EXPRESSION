using UnityEngine;

[RequireComponent (typeof(ElevenLabs_TTS))]
public class ElevenLabsConfigLoader : ConfigLoader
{
    private void Awake()
    {
        onGetConfigRaw.AddListener(
            (config) => {
                var elevenLabsConfig = JsonUtility.FromJson<ElevenLabs_TTS.ElevenLabsConfig>(config);
                var elevenLabs = GetComponent<ElevenLabs_TTS>();
                elevenLabs.postData = elevenLabsConfig.ElevenLabs_Settings;
                elevenLabs.m_VoiceID = elevenLabsConfig.VoiceID;
                });
    }
}
