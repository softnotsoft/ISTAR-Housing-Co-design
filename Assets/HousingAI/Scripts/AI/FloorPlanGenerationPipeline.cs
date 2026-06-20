using System.Threading.Tasks;
using System.Collections.Generic;
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

    /// <summary>
    /// Coordena a geracao completa: constroi o prompt, chama o Gemini, converte
    /// a resposta, valida a geometria e renderiza a primeira planta valida.
    /// </summary>
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

        // Cada iteracao representa uma chamada ao modelo. Quando ha erros, o
        // prompt seguinte inclui a planta anterior e o feedback das validacoes.
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

            // Primeiro extrai-se o JSON do envelope Gemini e depois converte-se
            // esse JSON para o modelo usado pelos validadores e pelo renderer.
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
        // Enviar apenas regras relevantes reduz o tamanho do prompt e impede que
        // regras de divisoes nao pedidas confundam o modelo.
        string apartmentJson = JsonUtility.ToJson(apartment, true);
        string requestJson = JsonUtility.ToJson(request, true);
        string rulesJson = JsonUtility.ToJson(
            BuildRelevantRules(request, rules),
            true
        );

        return @"
Return only compact valid JSON.
Do not use markdown.
Do not explain.

Task:
Generate a first-pass 2D apartment layout for Unity.
Focus only on room geometry. Doors may be empty arrays.

Output schema:
{
  ""generatedPlanId"": ""generated_layout"",
  ""sourceBaseApartmentId"": """ + apartment.apartmentId + @""",
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

Base apartment JSON:
" + apartmentJson + @"

User request JSON:
" + requestJson + @"

Minimum room rules JSON:
" + rulesJson + @"

Generation rules:
- Generate every requested room exactly once unless the request contains repeated room types.
- You may add one corridor with type ""corridor"" only if useful.
- Use European Portuguese room names.
- Use simple rectangles whenever possible. Use L-shapes only if needed.
- Use 4 to 6 points per room. Do not create detailed or curved polygons.
- All room points must be inside or on the base apartment boundary.
- Rooms must not overlap.
- The area field must approximately match the polygon area.
- Use at least the minimum area for each requested room.
- Try to use most of the available apartment area, but valid simple geometry is more important.
- Keep doors as empty arrays: ""doors"": [].
- Do not add explanations, comments, markdown, or trailing text.
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
        // O prompt de correcao conserva o contexto original e acrescenta tanto
        // a tentativa invalida como os erros concretos a corrigir.
        string apartmentJson = JsonUtility.ToJson(apartment, true);
        string requestJson = JsonUtility.ToJson(request, true);
        string rulesJson = JsonUtility.ToJson(
            BuildRelevantRules(request, rules),
            true
        );
        string feedbackJson = JsonUtility.ToJson(feedback, true);

        return @"
Return only compact valid JSON.
Do not use markdown.
Do not explain.

The previous layout was invalid. Correct only the room geometry using the validation feedback.
Doors may remain empty arrays.

Base apartment JSON:
" + apartmentJson + @"

User request JSON:
" + requestJson + @"

Minimum room rules JSON:
" + rulesJson + @"

Previous invalid layout JSON:
" + previousJson + @"

Validation feedback JSON:
" + feedbackJson + @"

Return a corrected GeneratedFloorPlanData JSON:
{
  ""generatedPlanId"": ""generated_layout_corrected"",
  ""sourceBaseApartmentId"": """ + apartment.apartmentId + @""",
  ""unit"": ""meters"",
  ""rooms"": []
}

Correction rules:
- Keep every requested room.
- You may add one corridor with type ""corridor"" only if useful.
- Use European Portuguese room names.
- Use simple rectangles whenever possible. Use L-shapes only if needed.
- Use 4 to 6 points per room.
- All room points must stay inside or on the base apartment boundary.
- Rooms must not overlap.
- The area field must approximately match the polygon area.
- Use at least the minimum area for each requested room.
- Keep doors as empty arrays: ""doors"": [].
- Do not add explanations, comments, markdown, or trailing text.
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

    private RoomRulesData BuildRelevantRules(
        FloorPlanRequestData request,
        RoomRulesData rules
    )
    {
        if (
            request == null ||
            request.roomRequirements == null ||
            rules == null ||
            rules.rules == null
        )
        {
            return rules;
        }

        List<RoomRuleData> relevantRules = new List<RoomRuleData>();

        // As regras de corredor sao sempre enviadas porque o modelo pode criar
        // um corredor mesmo que nao tenha sido pedido explicitamente.
        foreach (RoomRuleData rule in rules.rules)
        {
            if (rule == null)
            {
                continue;
            }

            if (rule.roomType == "corridor")
            {
                relevantRules.Add(rule);
                continue;
            }

            foreach (RoomRequirementData requirement in request.roomRequirements)
            {
                if (
                    requirement != null &&
                    rule.roomType == requirement.type &&
                    rule.people == requirement.people
                )
                {
                    relevantRules.Add(rule);
                    break;
                }
            }
        }

        RoomRulesData relevantData = new RoomRulesData();
        relevantData.unit = rules.unit;
        relevantData.rules = relevantRules.ToArray();

        return relevantData;
    }

    private bool IsGeneratedPlanUsable(GeneratedFloorPlanData generatedPlan)
    {
        // Esta e uma verificacao estrutural minima antes dos validadores de
        // negocio e geometria. Uma divisao precisa de pelo menos tres vertices.
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
        // As tres validacoes sao independentes e o feedback final agrega todos
        // os erros para que o Gemini os possa corrigir numa unica tentativa.
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
