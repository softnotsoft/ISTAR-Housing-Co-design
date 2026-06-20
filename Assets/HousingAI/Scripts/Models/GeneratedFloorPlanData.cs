using System;

[Serializable]
/// <summary>
/// Planta produzida pelo Gemini depois de a resposta JSON ser convertida para
/// os modelos internos da aplicacao.
/// </summary>
public class GeneratedFloorPlanData
{
    public string generatedPlanId;
    public string sourceBaseApartmentId;
    public string unit;

    public RoomData[] rooms;
}
