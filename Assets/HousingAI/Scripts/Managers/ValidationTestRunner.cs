using UnityEngine;

public class ValidationTestRunner : MonoBehaviour
{
    [Header("References")]
    public FloorPlanLoader floorPlanLoader;
    public RequestTestBuilder requestTestBuilder;
    public RoomRulesLoader roomRulesLoader;
    public AreaValidationService areaValidationService;

    private void Start()
    {
        Invoke(nameof(RunValidationTest), 0.1f);
    }

    private void RunValidationTest()
    {
        BaseApartmentData apartment = floorPlanLoader.GetLoadedApartment();
        FloorPlanRequestData request = requestTestBuilder.CreateTestRequest();
        RoomRulesData rules = roomRulesLoader.GetRules();

        AreaValidationResult result = areaValidationService.Validate(
            apartment,
            request,
            rules
        );

        Debug.Log("Resultado da validação:");
        Debug.Log($"Área disponível: {result.availableArea} m²");
        Debug.Log($"Área necessária: {result.requiredArea} m²");
        Debug.Log($"Válido: {result.isValid}");
        Debug.Log(result.message);
    }
}