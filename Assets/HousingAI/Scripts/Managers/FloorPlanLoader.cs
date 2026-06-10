using UnityEngine;

public class FloorPlanLoader : MonoBehaviour
{
    [Header("JSON File")]
    public TextAsset floorPlanJson;

    [Header("Renderer")]
    public FloorPlanRenderer floorPlanRenderer;

    private void Start()
    {
        LoadFloorPlan();
    }

    public void LoadFloorPlan()
    {
        if (floorPlanJson == null)
        {
            Debug.LogError("Nenhum ficheiro JSON foi atribuído.");
            return;
        }

        if (floorPlanRenderer == null)
        {
            Debug.LogError("FloorPlanRenderer não foi atribuído.");
            return;
        }

        ApartmentData apartment = JsonUtility.FromJson<ApartmentData>(floorPlanJson.text);

        if (apartment == null || apartment.rooms == null)
        {
            Debug.LogError("Erro ao converter JSON da planta.");
            return;
        }

        floorPlanRenderer.Render(apartment);

        Debug.Log($"Planta carregada: {apartment.name}");
    }
}