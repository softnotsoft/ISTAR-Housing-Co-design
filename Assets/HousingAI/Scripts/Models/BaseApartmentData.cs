using System;

[Serializable]
/// <summary>
/// Representa a geometria fixa de um apartamento carregada de um ficheiro JSON.
/// O contorno usa coordenadas 2D em metros e as aberturas pertencem ao perimetro.
/// </summary>
public class BaseApartmentData
{
    public string apartmentId;
    public string buildingId;
    public string name;
    public int floor;

    public string unit; // Unidade usada nas coordenadas, atualmente "meters".

    public PointData[] boundary;
    public OpeningData entranceDoor;
    public OpeningData[] windows;
}
