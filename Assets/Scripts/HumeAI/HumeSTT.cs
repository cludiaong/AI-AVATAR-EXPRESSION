using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class HumeSTT : MonoBehaviour
{
    private const string BATCH_API_ENDPOINT = "https://api.hume.ai/v0/batch/jobs";
    [SerializeField] private string humeApiKey;

    public IEnumerator SendAudioForAnalysis(byte[] audioBytes, Action<string, string> onAnalysisComplete, Action<string> onError)
    {
        Debug.Log("HumeSTT: Starting SendAudioForAnalysis.");
        if (string.IsNullOrEmpty(humeApiKey))
        {
            onError?.Invoke("Hume AI API Key is not set.");
            Debug.LogError("HumeSTT: Hume AI API Key is not set.");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioBytes, "audio.wav", "audio/wav");
        string jsonPayload = "{\"models\":{\"prosody\":{}}, \"transcription\":{\"identify_speakers\":false}}";
        form.AddField("json", jsonPayload);

        Debug.Log($"HumeSTT: Sending request to {BATCH_API_ENDPOINT} with payload: {jsonPayload}");
        using (UnityWebRequest request = UnityWebRequest.Post(BATCH_API_ENDPOINT, form))
        {
            request.SetRequestHeader("X-Hume-Api-Key", humeApiKey);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = $"Error: {request.error}\n{request.downloadHandler.text}";
                onError?.Invoke(errorMessage);
                Debug.LogError($"HumeSTT: API Request Error: {errorMessage}");
            }
            else
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"HumeSTT: API Response: {responseText}");
                    JobCreationResponse jobResponse = JsonUtility.FromJson<JobCreationResponse>(responseText);
                    if (jobResponse != null && !string.IsNullOrEmpty(jobResponse.job_id))
                    {
                        Debug.Log($"HumeSTT: Job ID received: {jobResponse.job_id}. Starting polling.");
                        StartCoroutine(PollForJobResults(jobResponse.job_id, onAnalysisComplete, onError));
                    }
                    else
                    {
                        onError?.Invoke("Failed to get Job ID from Hume AI.");
                        Debug.LogError($"HumeSTT: Failed to get Job ID from Hume AI. Response: {responseText}");
                    }
                }
                catch (Exception e)
                {
                    string errorMessage = $"Failed to process Hume AI STT response: {e.Message}\nResponse: {request.downloadHandler.text}";
                    onError?.Invoke(errorMessage);
                    Debug.LogError($"HumeSTT: JSON Parsing Error: {errorMessage}");
                }
            }
        }
    }

    private IEnumerator PollForJobResults(string jobId, Action<string, string> onAnalysisComplete, Action<string> onError)
    {
        string jobDetailsEndpoint = $"{BATCH_API_ENDPOINT}/{jobId}";
        string predictionsEndpoint = $"{BATCH_API_ENDPOINT}/{jobId}/predictions";
        float pollingInterval = 5.0f; // Poll every 5 seconds
        float timeout = 300.0f; // 5 minutes timeout
        float timeElapsed = 0;

        Debug.Log($"HumeSTT: Polling for job {jobId} results from {jobDetailsEndpoint}");
        while (timeElapsed < timeout)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(jobDetailsEndpoint))
            {
                request.SetRequestHeader("X-Hume-Api-Key", humeApiKey);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    // If it\"s a 400 Bad Request with "Job is in progress", continue polling
                    if (request.responseCode == 400 && request.downloadHandler.text.Contains("Job is in progress"))
                    {
                        Debug.Log($"HumeSTT: Job {jobId} still in progress. Polling again in {pollingInterval} seconds.");
                    }
                    else
                    {
                        string errorMessage = $"Error polling for job status: {request.error}\n{request.downloadHandler.text}";
                        onError?.Invoke(errorMessage);
                        Debug.LogError($"HumeSTT: Polling Error: {errorMessage}");
                        yield break;
                    }
                }
                else
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"HumeSTT: Job Status Response for {jobId}: {responseText}");
                    BatchJobStatusResponseWrapper statusResponseWrapper = JsonUtility.FromJson<BatchJobStatusResponseWrapper>(responseText);

                    if (statusResponseWrapper != null && statusResponseWrapper.state != null)
                    {
                        Debug.Log($"HumeSTT: Job {jobId} status: {statusResponseWrapper.state.status}");
                        if (statusResponseWrapper.state.status == "COMPLETED")
                        {
                            Debug.Log($"HumeSTT: Job {jobId} completed. Getting predictions from {predictionsEndpoint}");
                            StartCoroutine(GetJobPredictions(predictionsEndpoint, onAnalysisComplete, onError));
                            yield break; // Exit the polling loop
                        }
                        else if (statusResponseWrapper.state.status == "FAILED")
                        {
                            onError?.Invoke("Hume AI job failed.");
                            Debug.LogError($"HumeSTT: Job {jobId} failed.");
                            yield break;
                        }
                        // If still in progress, continue polling
                    }
                    else
                    {
                        Debug.LogError($"HumeSTT: Failed to parse job status response for {jobId}. Response: {responseText}");
                    }
                }
            }

            yield return new WaitForSeconds(pollingInterval);
            timeElapsed += pollingInterval;
        }

        if (timeElapsed >= timeout)
        {
            onError?.Invoke("Polling for results timed out.");
            Debug.LogError($"HumeSTT: Polling for job {jobId} timed out after {timeout} seconds.");
        }
    }

    private IEnumerator GetJobPredictions(string predictionsEndpoint, Action<string, string> onAnalysisComplete, Action<string> onError)
    {
        Debug.Log($"HumeSTT: Getting predictions from {predictionsEndpoint}");
        using (UnityWebRequest request = UnityWebRequest.Get(predictionsEndpoint))
        {
            request.SetRequestHeader("X-Hume-Api-Key", humeApiKey);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = $"Error getting job predictions: {request.error}\n{request.downloadHandler.text}";
                onError?.Invoke(errorMessage);
                Debug.LogError($"HumeSTT: Predictions API Error: {errorMessage}");
            }
            else
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"HumeSTT: Predictions API Response: {responseText}");
                    // The response is an array of prediction results
                    PredictionResultWrapper[] predictionResultsWrapper = JsonUtility.FromJson<PredictionResultWrapperArray>("{\"items\": " + responseText + "}").items;

                    string transcribedText = "No transcription available.";
                    string emotionAnalysis = "No emotion analysis available.";

                    if (predictionResultsWrapper != null && predictionResultsWrapper.Length > 0)
                    {
                        Debug.Log($"HumeSTT: Found {predictionResultsWrapper.Length} prediction results.");
                        // Assuming we are interested in the first result for transcription and prosody
                        if (predictionResultsWrapper[0].results != null && predictionResultsWrapper[0].results.predictions != null && predictionResultsWrapper[0].results.predictions.Length > 0)
                        {
                            Debug.Log("HumeSTT: Processing first prediction result.");
                            // Accessing the grouped_predictions for transcription and prosody
                            if (predictionResultsWrapper[0].results.predictions[0].models != null)
                            {
                                // Try to get transcription from the transcription model first
                                if (predictionResultsWrapper[0].results.predictions[0].models.transcription != null && 
                                    predictionResultsWrapper[0].results.predictions[0].models.transcription.results != null && 
                                    predictionResultsWrapper[0].results.predictions[0].models.transcription.results.Length > 0)
                                {
                                    StringBuilder fullTranscription = new StringBuilder();
                                    foreach (var result in predictionResultsWrapper[0].results.predictions[0].models.transcription.results)
                                    {
                                        fullTranscription.Append(result.text).Append(" ");
                                    }
                                    transcribedText = fullTranscription.ToString().Trim();
                                    Debug.Log($"HumeSTT: Transcribed Text (from transcription model): {transcribedText}");
                                }
                                // Fallback to prosody grouped_predictions if transcription model doesn\'t yield results
                                else if (predictionResultsWrapper[0].results.predictions[0].models.prosody != null && 
                                         predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions != null && 
                                         predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions.Length > 0 &&
                                         predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions != null &&
                                         predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions.Length > 0)
                                {
                                    StringBuilder fullTranscription = new StringBuilder();
                                    foreach (var prediction in predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions)
                                    {
                                        fullTranscription.Append(prediction.text).Append(" ");
                                    }
                                    transcribedText = fullTranscription.ToString().Trim();
                                    Debug.Log($"HumeSTT: Transcribed Text (from prosody model fallback): {transcribedText}");
                                }

                                if (predictionResultsWrapper[0].results.predictions[0].models.prosody != null && 
                                    predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions != null && 
                                    predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions.Length > 0 &&
                                    predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions != null &&
                                    predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions.Length > 0 &&
                                    predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions[0].emotions != null && 
                                    predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions[0].emotions.Length > 0)
                                {
                                    List<Emotion> sortedEmotions = new List<Emotion>(predictionResultsWrapper[0].results.predictions[0].models.prosody.grouped_predictions[0].predictions[0].emotions);
                                    sortedEmotions.Sort((e1, e2) => e2.score.CompareTo(e1.score));

                                    StringBuilder emotionAnalysisBuilder = new StringBuilder();
                                    emotionAnalysisBuilder.AppendLine("Top Emotions:");
                                    for (int i = 0; i < Mathf.Min(3, sortedEmotions.Count); i++)
                                    {
                                        Emotion emotion = sortedEmotions[i];
                                        emotionAnalysisBuilder.AppendLine($"- {emotion.name}: {emotion.score:F2}");
                                    }
                                    emotionAnalysis = emotionAnalysisBuilder.ToString();
                                    Debug.Log($"HumeSTT: Emotion Analysis: {emotionAnalysis}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("HumeSTT: No prediction results found or predictionResults is null.");
                    }

                    onAnalysisComplete?.Invoke(transcribedText, emotionAnalysis);
                }
                catch (Exception e)
                {
                    string errorMessage = $"Failed to parse prediction results: {e.Message}\nResponse: {request.downloadHandler.text}";
                    onError?.Invoke(errorMessage);
                    Debug.LogError($"HumeSTT: Prediction Parsing Error: {errorMessage}");
                }
            }
        }
    }
}

[Serializable]
public class JobCreationResponse
{
    public string job_id;
}

[Serializable]
public class BatchJobStatusResponseWrapper
{
    public string job_id;
    public JobState state;
}

[Serializable]
public class JobState
{
    public string status;
}

[Serializable]
public class PredictionResultWrapperArray
{
    public PredictionResultWrapper[] items;
}

[Serializable]
public class PredictionResultWrapper
{
    public SourceData source;
    public ResultData results;
}

[Serializable]
public class SourceData
{
    public string type;
    public string filename;
    public string content_type;
    public string md5sum;
}

[Serializable]
public class ResultData
{
    public PredictionItem[] predictions;
}

[Serializable]
public class PredictionItem
{
    public ModelsData models;
}

[Serializable]
public class ModelsData
{
    public TranscriptionData transcription;
    public ProsodyModel prosody;
}

[Serializable]
public class TranscriptionData
{
    public TranscriptionResult[] results;
}

[Serializable]
public class TranscriptionResult
{
    public string text;
}

[Serializable]
public class ProsodyData
{
    public ProsodyPrediction[] predictions;
}

[Serializable]
public class ProsodyPrediction
{
    public Emotion[] emotions;
}

[Serializable]
public class Emotion
{
    public string name;
    public float score;
}

[Serializable]
public class ProsodyModel
{
    public Metadata metadata;
    public GroupedPrediction[] grouped_predictions;
}

[Serializable]
public class Metadata
{
    public float confidence;
    public string detected_language;
}

[Serializable]
public class GroupedPrediction
{
    public string id;
    public InnerPrediction[] predictions;
}

[Serializable]
public class InnerPrediction
{
    public string text;
    public TimeData time;
    public float confidence;
    public Emotion[] emotions;
}

[Serializable]
public class TimeData
{
    public float begin;
    public float end;
}


