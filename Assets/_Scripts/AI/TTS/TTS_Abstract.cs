using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

public abstract class TTS_Abstract : MonoBehaviour
{
    public UnityEvent<AudioClip> onReceiveClip = new();
    public UnityEvent<string> onReceiveClipText = new();
    public UnityEvent<string, AudioClip> onReceiveResponse = new();
    public abstract void Speak(string message);

    public string TTS_Test_String = "- Ah, hi, officer... I-I'm doing all right, thanks for asking. - A bit... uneasy, I suppose. Been having some trouble with break-ins in the neighborhood lately. (nervously chuckles) -";
    [Button]
    public void TestTTS()
    {
        Speak(TTS_Test_String);
    }
}
