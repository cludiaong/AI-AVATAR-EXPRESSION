using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class HumeTTS : MonoBehaviour
{
    private const string API_ENDPOINT = "https://api.hume.ai/v0/tts";
    [SerializeField] private string humeApiKey;

    public IEnumerator SynthesizeSpeech(string text, string voiceId, float speed, Action<AudioClip> onSpeechSynthesized, Action<string> onError)
    {
        if (string.IsNullOrEmpty(humeApiKey))
        {
            onError?.Invoke("Hume AI API Key is not set.");
            yield break;
        }

        if (string.IsNullOrEmpty(text))
        {
            onError?.Invoke("Text cannot be empty.");
            yield break;
        }

        string voiceJson = string.IsNullOrEmpty(voiceId) ? "" : ", \"voice\":{\"id\":\"" + voiceId + "\"}";
        string speedJson = ", \"speed\":" + speed.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string requestBody = "{\"utterances\":[{\"text\":\"" + text + "\"" + voiceJson + speedJson + "}], \"format\":{\"type\":\"wav\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(API_ENDPOINT, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-Hume-Api-Key", humeApiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Error: {request.error}\n{request.downloadHandler.text}");
            }
            else
            {
                try
                {
                    HumeTTSResponse response = JsonUtility.FromJson<HumeTTSResponse>(request.downloadHandler.text);
                    if (response != null && response.generations != null && response.generations.Length > 0)
                    {
                        string base64Audio = response.generations[0].audio;
                        if (!string.IsNullOrEmpty(base64Audio))
                        {
                            // Remove the data URI prefix if present
                            if (base64Audio.Contains(","))
                            {
                                base64Audio = base64Audio.Substring(base64Audio.IndexOf(",") + 1);
                            }

                            byte[] audioBytes = Convert.FromBase64String(base64Audio);
                            AudioClip audioClip = WavUtility.FromWav(audioBytes);
                            onSpeechSynthesized?.Invoke(audioClip);
                        }
                        else
                        {
                            onError?.Invoke("No audio data received from Hume AI.");
                        }
                    }
                    else
                    {
                        onError?.Invoke("Invalid response format from Hume AI.");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Failed to parse Hume AI response: {e.Message}\nResponse: {request.downloadHandler.text}");
                }
            }
        }
    }
}


