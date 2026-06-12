//Pedido completo que será enviado à IA.

using System;

[Serializable]
public class FloorPlanRequestData
{
    public string apartmentId;
    public int totalResidents;

    public RoomRequirementData[] roomRequirements;
}