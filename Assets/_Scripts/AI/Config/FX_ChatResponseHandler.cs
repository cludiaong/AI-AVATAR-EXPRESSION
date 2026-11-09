using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows;

public class FX_ChatResponseHandler : MonoBehaviour
{
    public UnityEvent<string> onGetResponseMessage;
    public UnityEvent<string> onGetResponseMessageFilter;
    public void OnGetRawResponse(string response)
    {
        try
        {
            ChatResponseJson chatResponse = JsonUtility.FromJson<ChatResponseJson>(response);
            onGetResponseMessage.Invoke(chatResponse.Response);
            onGetResponseMessageFilter.Invoke(chatResponse.Response);
            //onGetResponseMessageFilter.Invoke(Regex.Replace(chatResponse.Response, @"\[.*?\]", "").Trim());
        }
        catch
        {
            onGetResponseMessage.Invoke(response);
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
