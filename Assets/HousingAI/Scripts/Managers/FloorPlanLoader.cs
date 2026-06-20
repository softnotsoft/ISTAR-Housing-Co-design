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

    //private void Start()
    //{
    //    LoadFloorPlan();
    //}

    /// <summary>
    /// Converte o TextAsset selecionado no Inspector para BaseApartmentData,
    /// desenha o contorno e ajusta a camara ao resultado.
    /// </summary>
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
        
        // Os ficheiros em Data/Floorplans seguem diretamente este modelo C#.
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

    /// <summary>Devolve o ultimo apartamento carregado com sucesso.</summary>
    public BaseApartmentData GetLoadedApartment()
    {
        return loadedApartment;
    }
}
