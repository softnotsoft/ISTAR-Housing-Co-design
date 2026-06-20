using System;

[Serializable]
/// <summary>
/// Segmento que representa uma porta ou janela entre dois pontos 2D.
/// </summary>
public class OpeningData
{
    public string id;
    public string type;

    public PointData start;
    public PointData end;
}
