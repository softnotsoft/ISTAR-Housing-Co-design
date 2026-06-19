using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayoutsVisualizationManager : MonoBehaviour
{
    [Header("Pipeline")]
    [SerializeField] private FloorPlanGenerationPipeline generationPipeline;
    [SerializeField] private RoomRulesLoader roomRulesLoader;
    [SerializeField] private AreaValidationService areaValidationService;

    [Header("Rendering")]
    [SerializeField] private FloorPlanRenderer floorPlanRenderer;
    [SerializeField] private FloorPlanCameraController cameraController;

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button generateButton;
    [SerializeField] private bool generateOnEnable = true;

    private bool isGenerating;
    private string lastGeneratedRequestKey;
    private GeneratedFloorPlanData lastGeneratedPlan;

    private void Awake()
    {
        if (generateButton != null)
        {
            generateButton.onClick.AddListener(GenerateLayout);
        }
    }

    private void OnDestroy()
    {
        if (generateButton != null)
        {
            generateButton.onClick.RemoveListener(GenerateLayout);
        }
    }

    private void OnEnable()
    {
        Debug.Log("Entrou na Page_LayoutsVisualization");

        if (generateOnEnable)
        {
            GenerateLayout();
        }
    }

    public async void GenerateLayout()
    {
        if (isGenerating)
        {
            return;
        }

        if (!HasRequiredReferences())
        {
            return;
        }

        BaseApartmentData apartment = UserSelectionData.SelectedApartment;
        FloorPlanRequestData request = UserSelectionData.UserRequest;

        if (!HasRequiredInput(apartment, request))
        {
            return;
        }

        string requestKey = BuildRequestKey(apartment, request);

        if (lastGeneratedPlan != null && lastGeneratedRequestKey == requestKey)
        {
            SetStatus("Layout ja gerado para este pedido.");
            return;
        }

        isGenerating = true;
        SetGenerateButtonEnabled(false);
        SetStatus("A preparar dados...");

        floorPlanRenderer.Render(apartment);
        FocusCamera();

        roomRulesLoader.LoadRules();
        RoomRulesData rules = roomRulesLoader.GetRules();

        if (rules == null)
        {
            FinishGeneration("Nao foi possivel carregar as regras das divisoes.");
            return;
        }

        AreaValidationResult areaValidation =
            areaValidationService.Validate(
                apartment,
                request,
                rules
            );

        if (!areaValidation.isValid)
        {
            FinishGeneration(areaValidation.message);
            return;
        }

        SetStatus("A gerar layout com IA...");

        GeneratedFloorPlanData generatedPlan =
            await generationPipeline.GenerateAndRenderAsync(
                apartment,
                request,
                rules
            );

        if (generatedPlan == null)
        {
            FinishGeneration("Nao foi possivel gerar uma planta valida.");
            return;
        }

        lastGeneratedPlan = generatedPlan;
        lastGeneratedRequestKey = requestKey;

        FocusCamera();
        FinishGeneration("Layout gerado e validado.");
    }

    private bool HasRequiredReferences()
    {
        if (
            generationPipeline == null ||
            roomRulesLoader == null ||
            areaValidationService == null ||
            floorPlanRenderer == null
        )
        {
            Debug.LogError("LayoutsVisualizationManager tem referencias por atribuir no Inspector.");
            SetStatus("Faltam referencias no Inspector.");
            return false;
        }

        return true;
    }

    private bool HasRequiredInput(
        BaseApartmentData apartment,
        FloorPlanRequestData request
    )
    {
        if (apartment == null)
        {
            Debug.LogError("SelectedApartment esta vazio.");
            SetStatus("Escolhe primeiro um apartamento.");
            return false;
        }

        if (request == null)
        {
            Debug.LogError("UserRequest esta vazio.");
            SetStatus("Escolhe primeiro os requisitos.");
            return false;
        }

        if (request.roomRequirements == null || request.roomRequirements.Length == 0)
        {
            Debug.LogError("UserRequest nao tem divisoes.");
            SetStatus("Escolhe pelo menos uma divisao.");
            return false;
        }

        Debug.Log("Apartamento recebido: " + apartment.name);
        Debug.Log("Request recebido:");
        Debug.Log(JsonUtility.ToJson(request, true));

        return true;
    }

    private string BuildRequestKey(
        BaseApartmentData apartment,
        FloorPlanRequestData request
    )
    {
        return apartment.apartmentId + "|" + JsonUtility.ToJson(request);
    }

    private void FocusCamera()
    {
        if (cameraController != null)
        {
            cameraController.FocusOnFloorPlan();
        }
    }

    private void FinishGeneration(string message)
    {
        isGenerating = false;
        SetStatus(message);
        SetGenerateButtonEnabled(true);
    }

    private void SetGenerateButtonEnabled(bool isEnabled)
    {
        if (generateButton != null)
        {
            generateButton.interactable = isEnabled;
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
    }
}
