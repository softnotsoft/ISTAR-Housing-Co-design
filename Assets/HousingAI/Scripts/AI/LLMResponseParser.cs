using System;
using UnityEngine;

public static class LLMResponseParser
{
    /// <summary>
    /// Navega no envelope da resposta Gemini e devolve apenas o texto da
    /// primeira parte do primeiro candidato. Esse texto contem a planta JSON.
    /// </summary>
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
/// <summary>Envelope minimo necessario para desserializar a resposta Gemini.</summary>
public class GeminiResponse
{
    public GeminiCandidate[] candidates;
}

[Serializable]
/// <summary>Candidato de resposta devolvido pelo modelo.</summary>
public class GeminiCandidate
{
    public GeminiContent content;
}

[Serializable]
/// <summary>Conteudo textual associado a um candidato.</summary>
public class GeminiContent
{
    public GeminiPart[] parts;
}

[Serializable]
/// <summary>Parte individual de uma resposta Gemini.</summary>
public class GeminiPart
{
    public string text;
}
