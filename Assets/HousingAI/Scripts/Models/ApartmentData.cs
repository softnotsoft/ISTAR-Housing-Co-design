using System;
using System.Collections.Generic;

[Serializable]
public class ApartmentData
{
    public string apartmentId;
    public string buildingId;
    public string name;
    public int floor;

    public List<RoomData> rooms;
}