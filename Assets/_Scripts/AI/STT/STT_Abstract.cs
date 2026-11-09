using UnityEngine;
using UnityEngine.Events;

public abstract class STT_Abstract : MonoBehaviour
{
    public string language = "en-us";
    public UnityEvent onSpeechRecognizeStart = new();
    public UnityEvent onSpeechRecognized = new();
    public UnityEvent<string> onSpeechRecognizedHypnosis = new();
    public UnityEvent<string> onSpeechRecognizedFinishHypnosis = new();
    public UnityEvent onSpeechRecognizeEnd = new();
    public UnityEvent<string> onSpeechRecognizeEndText = new();
    public UnityEvent onSpeechRecognizeCancel = new();
    public abstract void StartRecord();
    public abstract void StopRecord();
    public abstract void CancelRecord();
    public abstract string GetString();
    public virtual bool IsRecording()
    {
        return false;
    }
}
