using System;

[Serializable]
/// <summary>
/// Resultado da verificacao preliminar que compara a area disponivel no
/// apartamento com a soma das areas minimas pedidas pelo utilizador.
/// </summary>
public class AreaValidationResult
{
    public bool isValid;
    public float availableArea;
    public float requiredArea;
    public string message;
}
