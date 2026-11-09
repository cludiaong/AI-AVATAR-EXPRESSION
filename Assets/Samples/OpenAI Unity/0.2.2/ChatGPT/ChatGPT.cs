using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.Events;
using NaughtyAttributes;
using System.Linq;
using UnityEditor.VersionControl;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        public AzureSpeechRecognitionModule azureSpeech;
        public TestElevenLabs elevenlabsModule;
        public TextMeshProUGUI questionText;
        public TextMeshProUGUI answerText;
        public GameObject objLoading;

        public string chatModel = "gpt-4o";
        public float chatTemp = 1;
        [TextArea(15, 20)]
        public string initialPrompt;
        public string initialPromptFileName;

        private float height;
        private OpenAIApi openai = new OpenAIApi("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", "org-JzX5GWGorVtD8DRGhhA7VTUn");

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Act as a random stranger in a chat room and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";
        public bool GUIDebug;

        public UnityEvent<string> OnReceiveResponse;
        public string debugSendMessage;
        private void Start()
        {
            GetInitialPrompt();
            button.onClick.AddListener(SendReply);
        }
        public int GetMessagesCount()
        {
            return messages.Count;
        }
        public int GetMessagesCount(string role)
        {
            return messages.Where((x) => x.Role == role).ToList().Count;
        }
        public void GetInitialPrompt()
        {
            string initialPromptFilePath = Application.streamingAssetsPath + "/" + initialPromptFileName;
            string contents = File.ReadAllText(initialPromptFilePath);
            initialPrompt = contents;
            prompt = contents;
            var newMessagePrompt = new ChatMessage()
            {
                Role = "system",
                Content = prompt
            };
            messages.Add(newMessagePrompt);
        }
        public void InitialPrompt(string contents)
        {
            messages.Clear();
            initialPrompt = contents;
            prompt = contents;
            var newMessagePrompt = new ChatMessage()
            {
                Role = "system",
                Content = prompt
            };
            messages.Add(newMessagePrompt);
        }
        public void SetTemp(float temp)
        {
            chatTemp = temp;
        }
        public void SetModel(string model)
        {
            chatModel = model;
        }

        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }
        [Button]
        private async void SendReply()
        {
            if (inputField.text == "")
            {
                if(string.IsNullOrEmpty(debugSendMessage))
                    return;
                inputField.text = debugSendMessage;
            }

            objLoading.SetActive(true);

            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };

            //AppendMessage(newMessage);

            //if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text;
            if (messages.Count == 0) newMessage.Content = inputField.text;

            messages.Add(newMessage);

            foreach(var message in messages)
            {
                //Debug.Log($"{message.Role}: {message.Content}");
            }

            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            var ChatCompletion = new CreateChatCompletionRequest()
            {
                Model = chatModel,
                Messages = messages,
                Temperature = chatTemp
            };
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(ChatCompletion);

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                string answer = message.Content.Replace("```json", "").Replace("```", "");
                answer = answer[answer.IndexOf("{")..(answer.IndexOf("}")+1)];
                answer = answer.Trim();
                
                messages.Add(message);
                AppendMessage(message);
                OnReceiveResponse.Invoke(answer);
                Debug.Log(answer);
                //if ((answer.StartsWith('{') && answer.EndsWith('}')) || answer.Contains("```json"))
                //{
                ChatResponseJson jsonResponse = JsonUtility.FromJson<ChatResponseJson>(answer);
                this.jsonResponse = JsonUtility.ToJson(jsonResponse);
                answer = jsonResponse.Response;
                //}
                answerText.text = answer;
                elevenlabsModule.Generate(answer);
                questionText.text = "";
                azureSpeech.StopRecord();
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
            objLoading.SetActive(false);
        }
        string jsonResponse = "";
        private void OnGUI()
        {
            if (!GUIDebug) return;
            GUI.Label(new Rect(10, 10, 500, 500), jsonResponse);
        }
    }
    [SerializeField]
    public class ChatResponseJson
    {
        public string Response;
        public float Sentiment_Score;
        public string Description;
        public int ExpressionId;
        public string Expression;
        public int Personality;
        public int IQ;
        public int EQ;
        public int Attitude;

    }
}
