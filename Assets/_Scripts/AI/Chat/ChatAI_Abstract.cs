using UnityEngine;
using UnityEngine.Events;

public abstract class ChatAI_Abstract : MonoBehaviour
{
    public UnityEvent<string> onReceiveRawResponse;
    public virtual void SetupChat(string prompt)
    {

    }
    public abstract void AskChat(string message);
    public abstract void AskChat();
}
