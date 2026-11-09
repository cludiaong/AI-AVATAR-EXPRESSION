using NaughtyAttributes;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class FX_PromptGet : MonoBehaviour
{
    public string URL;
    public bool bottomPrompt_use_File;
    [ShowIf("bottomPrompt_use_File")]
    public string bottomPromptFilename;
    [HideIf("bottomPrompt_use_File")]
    [TextArea(15, 20)]
    public string buttomPromptText;
    public UnityEvent<string> onGetNameConfig;
    public UnityEvent<string> onGetPromptConfig;
    public UnityEvent<string> onGetModelConfig;
    public UnityEvent<float> onGetTempConfig;
    public IEnumerator Start()
    {
        yield return StartCoroutine(GetToken());
        UnityWebRequest www = UnityWebRequest.Get(URL);
        www.SetRequestHeader("Authorization", $"Bearer: {token}");
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            string buttomPrompt = "";
            if (bottomPrompt_use_File)
            {
                string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, bottomPromptFilename);

                string contents;

                if (filePath.Contains("://") || filePath.Contains(":///"))
                {
                    WWW wwwBP = new WWW(filePath);
                    yield return wwwBP;
                    contents = wwwBP.text;
                }
                else
                {
                    contents = System.IO.File.ReadAllText(filePath);
                }
                buttomPrompt = contents;
            }
            else
            {
                buttomPrompt = buttomPromptText;
            }

            ChatConfig chatConfig = JsonUtility.FromJson<ChatConfig>(www.downloadHandler.text);
            onGetNameConfig.Invoke(chatConfig.data.name);
            onGetPromptConfig.Invoke($"{chatConfig.data.GetFormatedPrompt()}\n\n{buttomPrompt}");
            onGetModelConfig.Invoke(chatConfig.data.model);
            onGetTempConfig.Invoke(chatConfig.data.temperature);
        }
        else
        {
            Debug.LogWarning(www.downloadHandler.text);
        }
    }

    public static string BaseURL = "https://fxm-llm.fxwebapps.com";
    public static string LoginPath = "/api/auth/login";
    string token;
    IEnumerator GetToken()
    {
        var requestJson = "{\r\n    \"userName\": \"monika@fxmweb.com\",\r\n    \"password\": \"monika1234@\"\r\n}";
        UnityWebRequest www = UnityWebRequest.Post(BaseURL + LoginPath, requestJson, "application/json");
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            LoginJson response = JsonUtility.FromJson<LoginJson>(www.downloadHandler.text);
            token = response.data.authToken;
        }
        else
        {
            Debug.LogWarning(www.downloadHandler.text);
        }
        yield return null;
    }
    [Serializable]
    public class LoginJson
    {
        public LoginJsonData data;
    }
    [Serializable]
    public class LoginJsonData
    {
        public string authToken;
    }

    [Serializable]
    public class ChatConfig
    {
        public ChatConfigData data;
    }
    [Serializable]
    public class ChatConfigData
    {
        public string name;
        public string promptTemplate;
        public float temperature;
        public string model;
        public string GetFormatedPrompt()
        {
            return promptTemplate.Replace("{name}", name);
        }
    }
}
