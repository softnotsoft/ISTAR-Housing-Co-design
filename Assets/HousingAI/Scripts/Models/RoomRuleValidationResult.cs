using System;

[Serializable]
/// <summary>
/// Resultado da comparacao das divisoes geradas com as regras minimas.
/// </summary>
public class RoomRuleValidationResult
{
    public bool isValid;
    public string[] errors;
}
