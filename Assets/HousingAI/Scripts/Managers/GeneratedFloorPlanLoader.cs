using UnityEngine;

public class GeneratedFloorPlanLoader : MonoBehaviour
{
    [Header("JSON File")]
    public TextAsset generatedFloorPlanJson;

    private GeneratedFloorPlanData loadedFloorPlan;

    public void LoadGeneratedFloorPlan()
    {
        if (generatedFloorPlanJson == null)
        {
            Debug.LogError("Nenhum ficheiro de planta gerada foi atribuído.");
            return;
        }

        loadedFloorPlan =
            JsonUtility.FromJson<GeneratedFloorPlanData>(
                generatedFloorPlanJson.text
            );

        if (loadedFloorPlan == null)
        {
            Debug.LogError("Erro ao carregar planta gerada.");
            return;
        }

        Debug.Log(
            $"Planta gerada carregada: {loadedFloorPlan.sourceBaseApartmentId}"
        );
    }

    public GeneratedFloorPlanData GetLoadedFloorPlan()
    {
        return loadedFloorPlan;
    }
}