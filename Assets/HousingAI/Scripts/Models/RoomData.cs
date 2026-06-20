using System;

[Serializable]
/// <summary>
/// Descreve uma divisao da planta gerada, incluindo o poligono, ocupacao,
/// area declarada pelo modelo e eventuais portas.
/// </summary>
public class RoomData
{
    public string id;
    public string name;
    public string type;
    public int people;

    public string color;

    public PointData[] points;
    public OpeningData[] doors;

    public float area;
}
