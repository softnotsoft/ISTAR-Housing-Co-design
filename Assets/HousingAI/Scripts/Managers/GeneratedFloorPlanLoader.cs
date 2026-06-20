using UnityEngine;

public class GeneratedFloorPlanLoader : MonoBehaviour
{
    [Header("JSON File")]
    public TextAsset generatedFloorPlanJson;

    private GeneratedFloorPlanData loadedFloorPlan;

    /// <summary>
    /// Carrega uma planta gerada guardada como TextAsset. Este loader serve o
    /// fluxo de teste antigo e nao e usado pela geracao atual via Gemini.
    /// </summary>
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

    /// <summary>Devolve a planta estatica carregada para testes.</summary>
    public GeneratedFloorPlanData GetLoadedFloorPlan()
    {
        return loadedFloorPlan;
    }
}
