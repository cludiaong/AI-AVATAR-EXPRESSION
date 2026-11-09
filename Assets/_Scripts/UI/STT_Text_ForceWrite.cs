using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class STT_Text_ForceWrite : MonoBehaviour
{
    public STT_Abstract stt;
    public TMP_Text m_text;
    public GameObject m_Overlay;

    public UnityEvent<string> onTextChange;
    private void Start()
    {
        if (m_Overlay)
        {
            stt.onSpeechRecognized.AddListener(() => m_Overlay.SetActive(false));
            stt.onSpeechRecognizeEnd.AddListener(() => m_Overlay.SetActive(true));
        }
    }
    public void ChangeTextTmp(string text)
    {
        m_text_tmp = text;
    }
    public string m_text_tmp;
    public void ChangeText()
    {
        m_text.text = m_text_tmp;
    }
    public void ChangeText(string text)
    {
        m_text.text = text;
    }
    private void Update()
    {
        if (stt.IsRecording())
        {
            if(m_text)
                m_text.text = stt.GetString();
            onTextChange.Invoke(stt.GetString());
        }
        else if (m_text)
        {
            m_text.text = m_text_tmp;
        }
    }
}
