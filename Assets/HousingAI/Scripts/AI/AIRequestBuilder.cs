using UnityEngine;

public class AIRequestBuilder : MonoBehaviour
{
    [Header("References")]
    public FloorPlanLoader floorPlanLoader;
    public RequestTestBuilder requestTestBuilder;
    public RoomRulesLoader roomRulesLoader;
    public AreaValidationService areaValidationService;

    public AIRequestData BuildRequest()
    {
        BaseApartmentData apartment = floorPlanLoader.GetLoadedApartment();

        FloorPlanRequestData request =
            requestTestBuilder.CreateTestRequest();

        RoomRulesData rules =
            roomRulesLoader.GetRules();

        AreaValidationResult validation =
            areaValidationService.Validate(
                apartment,
                request,
                rules
            );

        AIRequestData aiRequest = new AIRequestData();

        aiRequest.baseApartment = apartment;
        aiRequest.userRequest = request;
        aiRequest.roomRules = rules;
        aiRequest.validationResult = validation;

        return aiRequest;
    }

    private void Start()
    {
        Invoke(nameof(PrintRequest), 0.2f);
    }

    private void PrintRequest()
    {
        AIRequestData request = BuildRequest();

        string json = JsonUtility.ToJson(
            request,
            true
        );

        Debug.Log("JSON completo para IA:");
        Debug.Log(json);
    }
}