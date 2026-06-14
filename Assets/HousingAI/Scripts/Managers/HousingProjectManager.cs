using UnityEngine;

public class HousingProjectManager : MonoBehaviour
{
    [Header("References")]
    public ValidationFeedbackBuilder validationFeedbackBuilder;
    public RoomRuleValidationService roomRuleValidationService;
    public FloorPlanLoader floorPlanLoader;
    public RoomRulesLoader roomRulesLoader;
    public RequestTestBuilder requestBuilder;
    public AreaValidationService validationService;
    public GeneratedFloorPlanLoader generatedFloorPlanLoader;
    public GeneratedPlanValidationService generatedPlanValidationService;
    public FloorPlanRenderer floorPlanRenderer;
    public BoundaryValidationService boundaryValidationService;

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

        GeneratedPlanValidationResult generatedValidation =
            generatedPlanValidationService.Validate(
                apartment,
                generatedPlan
            );

        RoomRuleValidationResult roomRuleValidation =
            roomRuleValidationService.Validate(
                generatedPlan,
                rules
            );

        Debug.Log($"Divisões geradas: {generatedPlan.rooms.Length}");
        Debug.Log("=== VALIDAÇÃO DA PLANTA GERADA ===");
        Debug.Log("=== VALIDAÇÃO DAS REGRAS ===");
        Debug.Log($"Válida: {roomRuleValidation.isValid}");

        foreach (string error in roomRuleValidation.errors)
        {
            Debug.LogWarning(error);
        }

        Debug.Log($"Área disponível: {generatedValidation.availableArea:F2} m²");
        Debug.Log($"Área utilizada: {generatedValidation.usedArea:F2} m²");
        Debug.Log($"Ocupação: {generatedValidation.usagePercentage:F2}%");
        Debug.Log($"Válida: {generatedValidation.isValid}");

        foreach (string error in generatedValidation.errors)
        {
            Debug.LogWarning(error);
        }

        BoundaryValidationResult boundaryValidation =
            boundaryValidationService.Validate(
                apartment,
                generatedPlan
            );

        ValidationFeedbackData validationFeedback =
            validationFeedbackBuilder.BuildFeedback(
                generatedValidation,
                roomRuleValidation,
                boundaryValidation
            );

        Debug.Log("=== VALIDAÇÃO DOS LIMITES ===");
        Debug.Log($"Válida: {validationFeedback.isValid}");

        foreach (string error in validationFeedback.errors)
        {
            Debug.LogWarning(error);
        }

        foreach (string error in validationFeedback.errors)
        {
            Debug.LogWarning(error);
        }

        Debug.Log("=== FEEDBACK FINAL PARA IA ===");
        Debug.Log($"Planta válida: {validationFeedback.isValid}");
        
        foreach (string error in validationFeedback.errors)
        {
            Debug.LogWarning(error);
        }

        floorPlanRenderer.RenderGeneratedFloorPlan(generatedPlan);

        Debug.Log("=== VALIDAÇÃO DO PEDIDO ===");
        Debug.Log($"Área disponível: {validation.availableArea} m²");
        Debug.Log($"Área necessária: {validation.requiredArea} m²");
        Debug.Log($"Válido: {validation.isValid}");
        Debug.Log(validation.message);
    }
}