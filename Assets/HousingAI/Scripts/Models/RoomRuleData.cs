using System;

[Serializable]
/// <summary>
/// Restricoes minimas configuradas para um tipo de divisao e uma ocupacao.
/// </summary>
public class RoomRuleData
{
    public string roomType;
    public int people;

    public float minArea;
    public float minWidth;
}
