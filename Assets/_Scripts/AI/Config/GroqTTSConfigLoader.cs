using UnityEngine;

[RequireComponent(typeof(GroqTTS))]
public class GroqTTSConfigLoader : ConfigLoader
{
    private void Awake()
    {
        onGetConfigRaw.AddListener(
            (configRaw) =>
            {
                var Config = JsonUtility.FromJson<GroqTTS.GroqTTSRequest>(configRaw);
                var groq = GetComponent<GroqTTS>();
                groq.postData.voice = Config.voice;
                groq.postData.model = Config.model;
            });
    }
}
