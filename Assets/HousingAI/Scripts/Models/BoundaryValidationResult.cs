using System;

[Serializable]
/// <summary>
/// Indica se todos os pontos das divisoes geradas permanecem dentro do
/// contorno do apartamento e lista os pontos que violam esse limite.
/// </summary>
public class BoundaryValidationResult
{
    public bool isValid;
    public string[] errors;
}
