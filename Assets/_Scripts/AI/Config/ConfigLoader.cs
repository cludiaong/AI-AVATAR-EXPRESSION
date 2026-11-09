using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ConfigLoader : MonoBehaviour
{
    public string m_ConfigFileName;

    public UnityEvent<string> onGetConfigRaw;
    protected string m_RawConfig;
    private IEnumerator Start()
{
    string configFileDir = Path.Combine(Application.streamingAssetsPath, m_ConfigFileName);

    // Prepend file:// on desktop platforms
    string uri = configFileDir;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
    uri = "file://" + configFileDir;
#endif

    using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
    yield return webRequest.SendWebRequest();

    if (webRequest.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError($"Failed to load config file {uri}: {webRequest.error}");
        yield break;
    }

    string configFile = webRequest.downloadHandler.text;
    if (string.IsNullOrEmpty(configFile))
    {
        Debug.LogError($"Config file was empty: {uri}");
        yield break;
    }

    m_RawConfig = configFile;
    onGetConfigRaw.Invoke(m_RawConfig);

    }
}
