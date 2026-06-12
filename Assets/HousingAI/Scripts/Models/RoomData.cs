using System;

[Serializable]
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