using System;

[Serializable]
public class BaseApartmentData
{
    public string apartmentId;
    public string buildingId;
    public string name;
    public int floor;

    public string unit; // meters

    public PointData[] boundary;
    public OpeningData entranceDoor;
    public OpeningData[] windows;
}