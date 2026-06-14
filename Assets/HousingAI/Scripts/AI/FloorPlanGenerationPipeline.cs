using System.Threading.Tasks;
using UnityEngine;

public class FloorPlanGenerationPipeline : MonoBehaviour
{
    [Header("LLM")]
    [SerializeField] private LLMService llmService;
    [SerializeField] private int maxGenerationAttempts = 3;

    [Header("Validation")]
    [SerializeField] private GeneratedPlanValidationService generatedPlanValidationService;
    [SerializeField] private RoomRuleValidationService roomRuleValidationService;
    [SerializeField] private BoundaryValidationService boundaryValidationService;
    [SerializeField] private ValidationFeedbackBuilder validationFeedbackBuilder;

    [Header("Rendering")]
    [SerializeField] private FloorPlanRenderer floorPlanRenderer;

    public ValidationFeedbackData LastFeedback { get; private set; }

    public async Task<GeneratedFloorPlanData> GenerateAndRenderAsync(
        BaseApartmentData apartment,
        FloorPlanRequestData request,
        RoomRulesData rules
    )
    {
        if (!HasRequiredReferences())
        {
            return null;
        }

        if (apartment == null || request == null || rules == null)
        {
            Debug.LogError("FloorPlanGenerationPipeline recebeu dados de entrada invalidos.");
            return null;
        }

        string prompt = BuildInitialPrompt(apartment, request, rules);
        int attempts = Mathf.Max(1, maxGenerationAttempts);

        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            Debug.Log($"=== TENTATIVA LLM {attempt}/{attempts} ===");

            string extractedJson = await GenerateJsonAsync(prompt);

            if (string.IsNullOrWhiteSpace(extractedJson))
            {
                return null;
            }

            Debug.Log("Texto extraido do Gemini:");
            Debug.Log(extractedJson);

            GeneratedFloorPlanData generatedPlan =
                ParseGeneratedPlan(extractedJson);

            if (!IsGeneratedPlanUsable(generatedPlan))
            {
                return null;
            }

            LastFeedback = RunValidations(apartment, rules, generatedPlan);

            if (LastFeedback.isValid)
            {
                floorPlanRenderer.RenderGeneratedFloorPlan(generatedPlan);
                Debug.Log("Planta gerada pelo Gemini renderizada com sucesso.");
                return generatedPlan;
            }

            Debug.LogWarning("A planta gerada nao foi renderizada porque falhou as validacoes.");

            if (attempt == attempts)
            {
                Debug.LogError("Limite de tentativas atingido sem obter uma planta valida.");
                return null;
            }

            prompt = BuildCorrectionPrompt(
                apartment,
                request,
                rules,
                extractedJson,
                LastFeedback
            );
        }

        return null;
    }

    private bool HasRequiredReferences()
    {
        if (
            llmService == null ||
            generatedPlanValidationService == null ||
            roomRuleValidationService == null ||
            boundaryValidationService == null ||
            validationFeedbackBuilder == null ||
            floorPlanRenderer == null
        )
        {
            Debug.LogError("FloorPlanGenerationPipeline tem referencias por atribuir no Inspector.");
            return false;
        }

        return true;
    }

    private async Task<string> GenerateJsonAsync(string prompt)
    {
        string rawResponse = await llmService.GenerateAsync(prompt);

        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            Debug.LogError("A resposta bruta do LLM esta vazia.");
            return null;
        }

        string extractedText = LLMResponseParser.ExtractText(rawResponse);

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            Debug.LogError("Nao foi possivel extrair JSON da resposta do Gemini.");
            return null;
        }

        return extractedText;
    }

    private string BuildInitialPrompt(
        BaseApartmentData apartment,
        FloorPlanRequestData request,
        RoomRulesData rules
    )
    {
        string apartmentJson = JsonUtility.ToJson(apartment, true);
        string requestJson = JsonUtility.ToJson(request, true);
        string rulesJson = JsonUtility.ToJson(rules, true);

        return @"
Return ONLY valid JSON.
Do not use markdown.
Do not explain.

Generate a simple apartment floor plan compatible with this Unity C# model:

{
  ""generatedPlanId"": ""generated_apt_01_llm"",
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

Base apartment:
" + apartmentJson + @"

User request:
" + requestJson + @"

Room rules:
" + rulesJson + @"

Generate all rooms requested by the user.
You may add one corridor if it is needed to use the apartment area and connect rooms.
Use European Portuguese room names only.
The corridor must work as a distribution area.
Bedrooms and bathroom must have direct access from the corridor, not only through the living room, kitchen, bathroom or another bedroom.
Living room and kitchen may connect to the corridor or entrance area.
Every room must include at least one door in the doors array.
Each door must be placed on a wall shared with the corridor, entrance area or an adjacent room that makes functional sense.
Do not invent exterior windows. Use the existing base apartment windows as fixed constraints.
Prefer placing bedrooms and living room next to exterior walls with windows when possible.
All room points must stay inside the base apartment boundary.
Use simple polygons.
Fill as much of the apartment as possible.
Every room must include id, name, type, people, color, area, points and doors.
The area value must match the polygon area approximately.
Return JSON only.";
    }

    private string BuildCorrectionPrompt(
        BaseApartmentData apartment,
        FloorPlanRequestData request,
        RoomRulesData rules,
        string previousJson,
        ValidationFeedbackData feedback
    )
    {
        string apartmentJson = JsonUtility.ToJson(apartment, true);
        string requestJson = JsonUtility.ToJson(request, true);
        string rulesJson = JsonUtility.ToJson(rules, true);
        string feedbackJson = JsonUtility.ToJson(feedback, true);

        return @"
Return ONLY valid JSON.
Do not use markdown.
Do not explain.

The previous apartment floor plan was invalid.
Correct it using the validation feedback below.

Base apartment:
" + apartmentJson + @"

User request:
" + requestJson + @"

Room rules:
" + rulesJson + @"

Previous invalid generated plan:
" + previousJson + @"

Validation feedback:
" + feedbackJson + @"

Return a corrected GeneratedFloorPlanData JSON with this structure:
{
  ""generatedPlanId"": ""generated_apt_01_llm_corrected"",
  ""sourceBaseApartmentId"": ""apt_01"",
  ""unit"": ""meters"",
  ""rooms"": []
}

Keep all rooms requested by the user.
You may add one corridor if it is needed to use the apartment area and connect rooms.
Use European Portuguese room names only.
The corridor must work as a distribution area.
Bedrooms and bathroom must have direct access from the corridor, not only through the living room, kitchen, bathroom or another bedroom.
Living room and kitchen may connect to the corridor or entrance area.
Every room must include at least one door in the doors array.
Each door must be placed on a wall shared with the corridor, entrance area or an adjacent room that makes functional sense.
Do not invent exterior windows. Use the existing base apartment windows as fixed constraints.
Prefer placing bedrooms and living room next to exterior walls with windows when possible.
Every room must include id, name, type, people, color, area, points and doors.
All room points must stay inside the base apartment boundary.
The full generated plan should use at least 98% of the available apartment area.
Return JSON only.";
    }

    private GeneratedFloorPlanData ParseGeneratedPlan(string json)
    {
        try
        {
            return JsonUtility.FromJson<GeneratedFloorPlanData>(json);
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

    private ValidationFeedbackData RunValidations(
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

        Debug.Log("=== RESULTADO DO PIPELINE LLM -> GeneratedFloorPlanData -> Validacoes ===");
        Debug.Log($"Area disponivel: {generatedValidation.availableArea:F2}");
        Debug.Log($"Area utilizada: {generatedValidation.usedArea:F2}");
        Debug.Log($"Ocupacao: {generatedValidation.usagePercentage:F2}%");
        Debug.Log($"Planta valida: {feedback.isValid}");

        foreach (string error in feedback.errors)
        {
            Debug.LogWarning(error);
        }

        return feedback;
    }
}
