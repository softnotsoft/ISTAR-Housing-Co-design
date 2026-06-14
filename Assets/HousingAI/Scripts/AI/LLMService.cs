using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LLMService : MonoBehaviour
{
    [Header("Gemini API")]
    [SerializeField] private string apiKey;
    [SerializeField] private string model = "gemini-2.5-flash";

    public async Task<string> GenerateAsync(string prompt)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

        string requestBody = BuildRequestBody(prompt);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("X-goog-api-key", apiKey);

        await SendRequestAsync(request);

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"LLM request failed: {request.error}");
            string errorBody = request.downloadHandler.text;

            Debug.LogError(
                $"HTTP {request.responseCode}\n" +
                errorBody
            );
            return null;
        }

        string responseJson = request.downloadHandler.text;
        Debug.Log("LLM raw response:");
        Debug.Log(responseJson);

        return responseJson;
    }

    private string BuildRequestBody(string prompt)
    {
        string escapedPrompt = EscapeJson(prompt);

        return $@"
{{
    ""contents"": [
        {{
            ""parts"": [
                {{
                    ""text"": ""{escapedPrompt}""
                }}
            ]
        }}
    ]
}}";
    }

    private async Task SendRequestAsync(UnityWebRequest request)
    {
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }
    }

    private string EscapeJson(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }
}