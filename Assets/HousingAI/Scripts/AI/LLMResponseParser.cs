using System;
using UnityEngine;

public static class LLMResponseParser
{
    public static string ExtractText(string responseJson)
    {
        try
        {
            GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(responseJson);

            if (response == null ||
                response.candidates == null ||
                response.candidates.Length == 0 ||
                response.candidates[0].content == null ||
                response.candidates[0].content.parts == null ||
                response.candidates[0].content.parts.Length == 0)
            {
                Debug.LogError("Resposta Gemini inválida ou vazia.");
                return null;
            }

            return response.candidates[0].content.parts[0].text;
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao fazer parse da resposta Gemini:");
            Debug.LogError(e.Message);
            return null;
        }
    }
}

[Serializable]
public class GeminiResponse
{
    public GeminiCandidate[] candidates;
}

[Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
}

[Serializable]
public class GeminiContent
{
    public GeminiPart[] parts;
}

[Serializable]
public class GeminiPart
{
    public string text;
}