using UnityEngine;

[RequireComponent (typeof(GroqChat))]
public class GroqChatConfigLoader : ConfigLoader
{
    private void Awake()
    {
        onGetConfigRaw.AddListener(
            (configRaw) =>
            {
                var Config = JsonUtility.FromJson<GroqChat.GroqRequest>(configRaw);
                var chat = GetComponent<GroqChat>();
                //chat.SetupChat(Config.Prompt);
                chat.SetModel(Config.model);
                //chat.SetTemp(Config.temperature);
            });
    }
}
