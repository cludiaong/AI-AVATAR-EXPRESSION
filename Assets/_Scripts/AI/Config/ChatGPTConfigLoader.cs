using UnityEngine;
[RequireComponent(typeof(ChatGPT))]
public class ChatGPTConfigLoader : ConfigLoader
{
    private void Awake()
    {
        onGetConfigRaw.AddListener(
            (configRaw) =>
            {
                var Config = JsonUtility.FromJson<ChatGPT.ChatGPT_Settings>(configRaw);
                var chatgpt = GetComponent<ChatGPT>();
                chatgpt.SetupChat(Config.Prompt);
                chatgpt.m_Model = Config.Model;
                chatgpt.m_Temp = Config.Temp;
            });
    }
}
