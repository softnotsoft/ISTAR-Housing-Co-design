using UnityEngine;

public class HousingProjectManager : MonoBehaviour
{
    [Header("References")]
    public FloorPlanLoader floorPlanLoader;
    public RoomRulesLoader roomRulesLoader;
    public RequestTestBuilder requestBuilder;
    public AreaValidationService validationService;
    public GeneratedFloorPlanLoader generatedFloorPlanLoader;

    private void Start()
    {
        RunPipeline();
    }

    private void RunPipeline()
    {
        Debug.Log("=== INÍCIO DO PIPELINE ===");

        floorPlanLoader.LoadFloorPlan();
        roomRulesLoader.LoadRules();
        generatedFloorPlanLoader.LoadGeneratedFloorPlan();

        BaseApartmentData apartment =
            floorPlanLoader.GetLoadedApartment();

        RoomRulesData rules =
            roomRulesLoader.GetRules();

        FloorPlanRequestData request =
            requestBuilder.CreateTestRequest();

        AreaValidationResult validation =
            validationService.Validate(
                apartment,
                request,
                rules
            );
        
        GeneratedFloorPlanData generatedPlan =
            generatedFloorPlanLoader.GetLoadedFloorPlan();

        Debug.Log($"Divisões geradas: {generatedPlan.rooms.Length}");
        Debug.Log("Resultado da validação pelo HousingProjectManager:");
        Debug.Log($"Área disponível: {validation.availableArea} m²");
        Debug.Log($"Área necessária: {validation.requiredArea} m²");
        Debug.Log($"Válido: {validation.isValid}");
        Debug.Log(validation.message);
    }
}