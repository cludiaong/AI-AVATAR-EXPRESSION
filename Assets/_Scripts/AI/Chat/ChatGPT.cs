using OpenAI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public class ChatGPT : ChatAI_Abstract
{
    public string m_Model = "gpt-4o";
    public float m_Temp = 0.7f;
    [TextArea(15, 20)]
    public string m_Prompt = "";
    OpenAIApi openAI = new(Credentials.OPENAI_API_KEY, Credentials.OPENAI_API_KEY_ORG);
    List<ChatMessage> chatMessage = new();
    public override void SetupChat(string prompt)
    {
        if (prompt == "" || string.IsNullOrEmpty(prompt)) return;
        m_Prompt = prompt;
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = prompt;
        newMessage.Role = "system";
        chatMessage.Add(newMessage);
    }
    public void SetModel(string model) => m_Model = model;
    public void SetTemp(float temp) => m_Temp = temp;
    public override async void AskChat(string message)
    {
        if (message == "" || string.IsNullOrEmpty(message)) return;
        Debug.Log("User: " + message);

        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = message;
        newMessage.Role = "user";
        chatMessage.Add(newMessage);

        AskChat();
    }
    public override async void AskChat()
    {
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = chatMessage;
        request.Model = m_Model;
        request.Temperature = m_Temp;
        string chatResponseContent = "";
        do
        {
            var response = await openAI.CreateChatCompletion(request);

            if (response.Choices != null && response.Choices.Count > 0)
            {
                var chatResponse = response.Choices[0].Message;
                chatResponse.Content = Regex.Replace(chatResponse.Content, @"\*.*?\*", "");
                chatMessage.Add(chatResponse);

                chatResponseContent = (chatResponse.Content.Replace("```json", "").Replace("```", ""));
                Debug.Log($"{chatResponse.Content}");
            }
        } while (string.IsNullOrEmpty(chatResponseContent));
        string debugStr = "";
        foreach (var msg in chatMessage)
        {
            debugStr += ($"Role: {msg.Role} | Msg: {msg.Content}\n");
        }
        Debug.Log(debugStr);
        onReceiveRawResponse.Invoke(chatResponseContent);
    }

    public class ChatGPT_Settings
    {
        public string Model = "gpt-4o";
        public float Temp = 0.7f;
        public string Prompt = "";
    }
}
