using UnityEngine;

public class FloorPlanLoader : MonoBehaviour
{
    [Header("JSON File")]
    public TextAsset floorPlanJson;

    [Header("Renderer")]
    public FloorPlanRenderer floorPlanRenderer;

    [Header("Camera")]
    public FloorPlanCameraController cameraController;

    private BaseApartmentData loadedApartment;

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

        BaseApartmentData apartment = JsonUtility.FromJson<BaseApartmentData>(floorPlanJson.text);

        loadedApartment = apartment;

        if (apartment == null || apartment.boundary == null)
        {
            Debug.LogError("Erro ao converter JSON da planta.");
            return;
        }

        floorPlanRenderer.Render(apartment);

        if (cameraController != null)
        {
            cameraController.FocusOnFloorPlan();
        }

        Debug.Log($"Planta carregada: {apartment.name}");
    }

    public BaseApartmentData GetLoadedApartment()
    {
        return loadedApartment;
    }
}