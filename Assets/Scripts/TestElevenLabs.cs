using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class TestElevenLabs : MonoBehaviour
{
    [SerializeField] private ElevenlabsAPI elevenlabs;
    [SerializeField] private TMP_InputField inputField;

    private AudioClip lastClip;

    public void SetTTS(ElevenlabsAPI tts)
    {
        elevenlabs = tts;
    }
    private void Start()
    {
        elevenlabs.AudioReceived.AddListener(PlayClip);
    }

    public void Generate(string text)
    {
        elevenlabs.GetAudio(text);
    }

    public void PlayClip(AudioClip clip)
    {
        lastClip = clip;
        if (TryGetComponent<AudioSource>(out var source))
        {
            source.clip = clip;
            source.Play();
        }
        else
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
    [Button]
    public void SaveLastClip()
    {
        SavWav.Save("elevenLabsVoice.wav", lastClip);
    }
    [Button]
    public void PlayClip()
    {
        if (TryGetComponent<AudioSource>(out var source))
        {
            source.Play();
        }
    }
}
