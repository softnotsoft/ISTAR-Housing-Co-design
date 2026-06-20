using System;

[Serializable]
/// <summary>
/// Resultado da validacao da ocupacao global da planta gerada.
/// </summary>
public class GeneratedPlanValidationResult
{
    public bool isValid;

    public float availableArea;
    public float usedArea;
    public float usagePercentage;

    public string[] errors;
}
