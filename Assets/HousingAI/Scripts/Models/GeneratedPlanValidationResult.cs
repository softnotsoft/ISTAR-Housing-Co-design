using System;

[Serializable]
public class GeneratedPlanValidationResult
{
    public bool isValid;

    public float availableArea;
    public float usedArea;
    public float usagePercentage;

    public string[] errors;
}