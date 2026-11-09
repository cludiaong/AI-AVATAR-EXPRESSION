using UnityEngine;
using System.Threading;

public class ConversationConnector : MonoBehaviour
{
    [SerializeField] private STT_Abstract stt;          // AzureSTT
    [SerializeField] private ChatAI_Abstract chatAI;    // GroqChat or ChatGPT

    private SynchronizationContext _unity;  // main-thread context

    private void Awake()
    {
        _unity = SynchronizationContext.Current; // captured on main thread
    }

    private void OnEnable()
    {
        stt.onSpeechRecognizeEndText.AddListener(OnUserFinishedTalking);
    }

    private void OnDisable()
    {
        stt.onSpeechRecognizeEndText.RemoveListener(OnUserFinishedTalking);
    }

    private void OnUserFinishedTalking(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript)) { Debug.Log("Ignored empty transcript."); return; }

        var text = transcript.Trim();

        // 🔑 Post back to Unity main thread before starting a coroutine
        _unity.Post(_ =>
        {
            Debug.Log("Auto-sending transcript (main thread): " + text);
            chatAI.AskChat(text);                 // now "Requesting Groq Chat" should appear
        }, null);
    }
}
