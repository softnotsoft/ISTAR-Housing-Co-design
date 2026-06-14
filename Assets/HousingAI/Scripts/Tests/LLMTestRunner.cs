using UnityEngine;

public class LLMTestRunner : MonoBehaviour
{
    [Header("Pipeline")]
    [SerializeField] private FloorPlanGenerationPipeline generationPipeline;

    [Header("Data")]
    [SerializeField] private FloorPlanLoader floorPlanLoader;
    [SerializeField] private RoomRulesLoader roomRulesLoader;
    [SerializeField] private RequestTestBuilder requestTestBuilder;

    private async void Start()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        floorPlanLoader.LoadFloorPlan();
        roomRulesLoader.LoadRules();

        BaseApartmentData apartment = floorPlanLoader.GetLoadedApartment();
        RoomRulesData rules = roomRulesLoader.GetRules();
        FloorPlanRequestData request = requestTestBuilder.CreateTestRequest();

        if (apartment == null || rules == null || request == null)
        {
            Debug.LogError("Nao foi possivel preparar os dados de teste para o pipeline.");
            return;
        }

        GeneratedFloorPlanData generatedPlan =
            await generationPipeline.GenerateAndRenderAsync(
                apartment,
                request,
                rules
            );

        if (generatedPlan == null)
        {
            Debug.LogError("O teste terminou sem uma planta gerada valida.");
            return;
        }

        Debug.Log("LLMTestRunner terminou com uma planta valida renderizada.");
    }

    private bool HasRequiredReferences()
    {
        if (
            generationPipeline == null ||
            floorPlanLoader == null ||
            roomRulesLoader == null ||
            requestTestBuilder == null
        )
        {
            Debug.LogError("LLMTestRunner tem referencias por atribuir no Inspector.");
            return false;
        }

        return true;
    }
}
