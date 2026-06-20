using System;

[Serializable]
/// <summary>
/// Resultado consolidado das validacoes. Quando a planta e invalida, os erros
/// sao reutilizados pelo pipeline para construir o prompt de correcao.
/// </summary>
public class ValidationFeedbackData
{
    public bool isValid;
    public string[] errors;
}
