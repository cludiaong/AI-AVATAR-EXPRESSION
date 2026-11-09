using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GroqChat : ChatAI_Abstract
{
    public GroqRequest groqRequest = new();
    [TextArea(15, 20)]
    public string m_Prompt = "";
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
    public void SetModel(string model) => groqRequest.model = model;
    //public void SetTemp(float temp) => groqRequest.temperature = temp;

    public override void AskChat(string message)
    {
        if (message == "" || string.IsNullOrEmpty(message)) return;
        chatMessage.Add(new ChatMessage() { Role = "user", Content = message });
        AskChat();
    }

    public override void AskChat()
    {
        StartCoroutine(RequestChat());
    }

    public IEnumerator RequestChat()
    {
        string url = "https://api.groq.com/openai/v1/chat/completions";

        Debug.Log("Requesting Groq Chat");
        // Convert the whole conversation to JSON

        GroqRequest groqRequest = this.groqRequest;
        groqRequest.messages = GroqRequest.ConvertToGroqMessage(chatMessage);

        string jsonPayload = JsonUtility.ToJson(groqRequest);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        Debug.Log(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + Credentials.GROQ_API_KEY);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request error: " + request.error + "\n" + request.downloadHandler.text);
            Debug.LogError($"Request Body: {jsonPayload}");
        }
        else
        {
            string rawResponse = request.downloadHandler.text;
            GroqResponse response = JsonUtility.FromJson<GroqResponse>(rawResponse);
            Debug.Log(JsonUtility.ToJson(response));
            var responseContent = response.choices.First().message.content.Replace("```json","").Replace("```","");
            chatMessage.Add(new ChatMessage() { Role = response.choices.First().message.role, Content = responseContent });
            onReceiveRawResponse.Invoke(responseContent);
        }
    }
    [Serializable]
    public class GroqRequest
    {
        public string model;
        //public float temperature;
        public List<GroqChatMessage> messages;

        public static List<GroqChatMessage> ConvertToGroqMessage(List<ChatMessage> chatMessages)
        {
            List<GroqChatMessage> message = new();
            foreach (ChatMessage chatMessage in chatMessages)
            {
                message.Add(GroqChatMessage.ConvertToGroqChatMessage(chatMessage));
            }
            return message;
        }
    }
    [Serializable]
    public class GroqResponse
    {
        public List<GroqChoices> choices;

        [Serializable]
        public class GroqChoices
        {
            public GroqChatMessage message;
        }
    }
    [Serializable]
    public class GroqChatMessage
    {
        public string role;
        public string content;

        public static GroqChatMessage ConvertToGroqChatMessage(ChatMessage chatMessage)
        {
            var GChatMsg = new GroqChatMessage();
            GChatMsg.role = chatMessage.Role;
            GChatMsg.content = chatMessage.Content;
            return GChatMsg;
        }
    }
}
