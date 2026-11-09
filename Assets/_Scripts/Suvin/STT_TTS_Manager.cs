using UnityEngine;

public class STT_TTS_Manager : MonoBehaviour
{
    [SerializeField] private STT_Abstract stt;   // AzureSTT
    [SerializeField] private GroqTTS tts;        // GroqTTS instance

    private void OnEnable()
    {
        tts.onAudioPlay.AddListener(PauseSTT);
        tts.onAudioComplete.AddListener(ResumeSTT);
    }

    private void OnDisable()
    {
        tts.onAudioPlay.RemoveListener(PauseSTT);
        tts.onAudioComplete.RemoveListener(ResumeSTT);
    }

    private void PauseSTT(string text)
    {
        Debug.Log("🔇 Pausing STT while TTS speaks...");
        stt.StopRecord();
    }

    private void ResumeSTT()
    {
        Debug.Log("🎤 Resuming STT after TTS finished.");
        stt.StartRecord();
    }
}
