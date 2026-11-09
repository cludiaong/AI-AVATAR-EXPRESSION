using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class AudioSourceEvent : MonoBehaviour
{
    public AudioSource audioSource;
    public UnityEvent onAudioPlayed, onAudioStop;
    IEnumerator Start()
    {
        if(audioSource == null) yield break;

        while (true)
        {
            yield return new WaitUntil(() => audioSource.isPlaying);
            onAudioPlayed.Invoke();
            yield return new WaitWhile(() => audioSource.isPlaying);
            onAudioStop.Invoke();
        }
    }
}
