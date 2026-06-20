using System;

[Serializable]
/// <summary>
/// Pedido funcional construido a partir das escolhas do utilizador.
/// Identifica o apartamento, o agregado e as divisoes pretendidas.
/// </summary>
public class FloorPlanRequestData
{
    public string apartmentId;
    public int totalResidents;

    public RoomRequirementData[] roomRequirements;
}
