using System;

[Serializable]
public class AreaValidationResult
{
    public bool isValid;
    public float availableArea;
    public float requiredArea;
    public string message;
}