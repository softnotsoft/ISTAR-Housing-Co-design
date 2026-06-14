using UnityEngine;

public class LLMTestRunner : MonoBehaviour
{
    [Header("LLM")]
    [SerializeField] private LLMService llmService;

    [Header("Data")]
    [SerializeField] private FloorPlanLoader floorPlanLoader;
    [SerializeField] private RoomRulesLoader roomRulesLoader;

    [Header("Validation")]
    [SerializeField] private GeneratedPlanValidationService generatedPlanValidationService;
    [SerializeField] private RoomRuleValidationService roomRuleValidationService;
    [SerializeField] private BoundaryValidationService boundaryValidationService;
    [SerializeField] private ValidationFeedbackBuilder validationFeedbackBuilder;

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

        if (apartment == null || rules == null)
        {
            Debug.LogError("Nao foi possivel carregar a planta base ou as regras.");
            return;
        }

        string prompt = BuildPrompt(apartment, rules);
        string rawResponse = await llmService.GenerateAsync(prompt);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            Debug.LogError("A resposta bruta do LLM esta vazia.");
            return;
        }

        string extractedText = LLMResponseParser.ExtractText(rawResponse);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            Debug.LogError("Nao foi possivel extrair JSON da resposta do Gemini.");
            return;
        }

        Debug.Log("Texto extraido do Gemini:");
        Debug.Log(extractedText);

        GeneratedFloorPlanData generatedPlan = ParseGeneratedPlan(extractedText);

        if (!IsGeneratedPlanUsable(generatedPlan))
        {
            return;
        }

        RunValidations(apartment, rules, generatedPlan);
    }

    private bool HasRequiredReferences()
    {
        if (
            llmService == null ||
            floorPlanLoader == null ||
            roomRulesLoader == null ||
            generatedPlanValidationService == null ||
            roomRuleValidationService == null ||
            boundaryValidationService == null ||
            validationFeedbackBuilder == null
        )
        {
            Debug.LogError("LLMTestRunner tem referencias por atribuir no Inspector.");
            return false;
        }

        return true;
    }

    private string BuildPrompt(
        BaseApartmentData apartment,
        RoomRulesData rules
    )
    {
        string apartmentJson = JsonUtility.ToJson(apartment, true);
        string rulesJson = JsonUtility.ToJson(rules, true);

        return @"
Return ONLY valid JSON.
Do not use markdown.
Do not explain.

Generate a simple apartment floor plan compatible with this Unity C# model:

{
  ""generatedPlanId"": ""generated_apt_01_llm_test"",
  ""sourceBaseApartmentId"": ""apt_01"",
  ""unit"": ""meters"",
  ""rooms"": [
    {
      ""id"": ""living_room_01"",
      ""name"": ""Sala"",
      ""type"": ""living_room"",
      ""people"": 3,
      ""color"": ""#C8A2C8"",
      ""area"": 12.0,
      ""points"": [
        { ""x"": 0.0, ""y"": 0.0 },
        { ""x"": 3.0, ""y"": 0.0 },
        { ""x"": 3.0, ""y"": 4.0 },
        { ""x"": 0.0, ""y"": 4.0 }
      ],
      ""doors"": []
    }
  ]
}

Use this base apartment as the outer boundary. All room points must stay inside it:
" + apartmentJson + @"

Use these room rules:
" + rulesJson + @"

Generate exactly these rooms:
- living_room for 3 people
- kitchen for 3 people
- bedroom for 2 people
- bedroom for 1 person
- bathroom for 3 people
- corridor for 3 people

Use simple polygons.
Fill as much of the apartment as possible.
Every room must include id, name, type, people, color, area, points and doors.
The area value must match the polygon area approximately.
Return JSON only.";
    }

    private GeneratedFloorPlanData ParseGeneratedPlan(string json)
    {
        try
        {
            GeneratedFloorPlanData generatedPlan =
                JsonUtility.FromJson<GeneratedFloorPlanData>(json);

            return generatedPlan;
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Erro ao converter JSON para GeneratedFloorPlanData.");
            Debug.LogError(exception.Message);
            return null;
        }
    }

    private bool IsGeneratedPlanUsable(GeneratedFloorPlanData generatedPlan)
    {
        if (generatedPlan == null)
        {
            Debug.LogError("GeneratedFloorPlanData ficou null apos o parse.");
            return false;
        }

        if (generatedPlan.rooms == null || generatedPlan.rooms.Length == 0)
        {
            Debug.LogError("GeneratedFloorPlanData nao contem rooms.");
            return false;
        }

        foreach (RoomData room in generatedPlan.rooms)
        {
            if (room.points == null || room.points.Length < 3)
            {
                Debug.LogError($"A divisao {room.id} nao tem pontos suficientes.");
                return false;
            }
        }

        Debug.Log($"GeneratedFloorPlanData criado com {generatedPlan.rooms.Length} divisoes.");
        return true;
    }

    private void RunValidations(
        BaseApartmentData apartment,
        RoomRulesData rules,
        GeneratedFloorPlanData generatedPlan
    )
    {
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

        BoundaryValidationResult boundaryValidation =
            boundaryValidationService.Validate(
                apartment,
                generatedPlan
            );

        ValidationFeedbackData feedback =
            validationFeedbackBuilder.BuildFeedback(
                generatedValidation,
                roomRuleValidation,
                boundaryValidation
            );

        Debug.Log("=== RESULTADO DO TESTE LLM -> GeneratedFloorPlanData -> Validacoes ===");
        Debug.Log($"Area disponivel: {generatedValidation.availableArea:F2}");
        Debug.Log($"Area utilizada: {generatedValidation.usedArea:F2}");
        Debug.Log($"Ocupacao: {generatedValidation.usagePercentage:F2}%");
        Debug.Log($"Planta valida: {feedback.isValid}");

        foreach (string error in feedback.errors)
        {
            Debug.LogWarning(error);
        }
    }
}
