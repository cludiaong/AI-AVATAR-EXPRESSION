using OpenAI;
using UnityEngine;
using UnityEngine.Events;

public class avatarResponseHandle : MonoBehaviour
{
    public UnityEvent<int> onUpdateExpression;
    public UnityEvent<float, string> onUpdatePersona1;
    public UnityEvent<int, int, int, int> onUpdatePersona2;
    public UnityEvent<string> onGroqTextToSpeech;
    public void OnReceiveResponse(string answer)
    {
        //if ((answer.StartsWith('{') && answer.EndsWith('}')) || answer.Contains("```json"))
        //{
        Debug.Log(answer);
        ChatResponseJson jsonResponse = JsonUtility.FromJson<ChatResponseJson>(answer);
        onUpdateExpression.Invoke(jsonResponse.ExpressionId);
        onUpdatePersona1.Invoke(jsonResponse.Sentiment_Score, jsonResponse.Expression);
        onUpdatePersona2.Invoke(jsonResponse.Personality, jsonResponse.Attitude, jsonResponse.EQ, jsonResponse.IQ);
        onGroqTextToSpeech.Invoke(jsonResponse.Response);
        //}
    }
}
