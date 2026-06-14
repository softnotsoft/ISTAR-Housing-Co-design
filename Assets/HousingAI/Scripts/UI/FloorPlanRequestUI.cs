using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorPlanRequestUI : MonoBehaviour
{
    [System.Serializable]
    public class ApartmentOption
    {
        public string label;
        public TextAsset floorPlanJson;
    }

    [Header("Pipeline")]
    [SerializeField] private FloorPlanGenerationPipeline generationPipeline;

    [Header("Loaders")]
    [SerializeField] private FloorPlanLoader floorPlanLoader;
    [SerializeField] private RoomRulesLoader roomRulesLoader;

    [Header("Wizard Pages")]
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Apartment Options")]
    [SerializeField] private ApartmentOption[] apartments;

    private readonly List<RoomRequirementData> selectedRooms =
        new List<RoomRequirementData>();

    private int currentPageIndex;
    private int selectedApartmentIndex = -1;
    private int selectedTotalResidents = 1;
    private string pendingRoomType;
    private bool isGenerating;

    private void Awake()
    {
        ShowPage(0);

        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextPage);
        }
    }

    private void OnDestroy()
    {
        if (previousButton != null)
        {
            previousButton.onClick.RemoveListener(PreviousPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(NextPage);
        }
    }

    public void SelectApartment(int apartmentIndex)
    {
        if (apartments == null || apartments.Length == 0)
        {
            SetStatus("Nao existem apartamentos configurados.");
            return;
        }

        selectedApartmentIndex =
            Mathf.Clamp(
                apartmentIndex,
                0,
                apartments.Length - 1
            );

        string label = apartments[selectedApartmentIndex].label;

        if (string.IsNullOrWhiteSpace(label))
        {
            label = $"Apartamento {selectedApartmentIndex + 1}";
        }

        SetStatus($"Apartamento selecionado: {label}");
        NextPage();
    }

    public void SelectTotalResidents(int totalResidents)
    {
        selectedTotalResidents = Mathf.Max(1, totalResidents);

        SetStatus($"Total de pessoas: {selectedTotalResidents}");
        NextPage();
    }

    public void SelectRoomType(string roomType)
    {
        if (string.IsNullOrWhiteSpace(roomType))
        {
            SetStatus("Tipo de divisao invalido.");
            return;
        }

        pendingRoomType = roomType;
        SetStatus($"Divisao selecionada: {roomType}. Escolhe para quantas pessoas.");
    }

    public void AddSelectedRoomForPeople(int people)
    {
        if (string.IsNullOrWhiteSpace(pendingRoomType))
        {
            SetStatus("Escolhe primeiro uma divisao.");
            return;
        }

        RoomRequirementData requirement = new RoomRequirementData();
        requirement.type = pendingRoomType;
        requirement.people = Mathf.Max(1, people);

        selectedRooms.Add(requirement);

        SetStatus(
            $"Adicionado: {requirement.type} para {requirement.people} pessoa(s)."
        );

        pendingRoomType = null;
    }

    public void ClearSelectedRooms()
    {
        selectedRooms.Clear();
        pendingRoomType = null;
        SetStatus("Divisoes selecionadas removidas.");
    }

    public async void Generate()
    {
        if (isGenerating)
        {
            return;
        }

        if (!HasRequiredReferences())
        {
            return;
        }

        if (!ApplySelectedApartment())
        {
            return;
        }

        if (selectedRooms.Count == 0)
        {
            SetStatus("Seleciona pelo menos uma divisao.");
            return;
        }

        isGenerating = true;
        SetNavigationEnabled(false);
        SetStatus("A gerar planta com IA...");

        floorPlanLoader.LoadFloorPlan();
        roomRulesLoader.LoadRules();

        BaseApartmentData apartment = floorPlanLoader.GetLoadedApartment();
        RoomRulesData rules = roomRulesLoader.GetRules();
        FloorPlanRequestData request = BuildRequest(apartment);

        if (apartment == null || rules == null || request == null)
        {
            FinishGeneration("Nao foi possivel preparar os dados.");
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
            FinishGeneration("Nao foi possivel gerar uma planta valida.");
            return;
        }

        FinishGeneration("Planta valida gerada e renderizada.");
    }

    public void ShowPage(int pageIndex)
    {
        if (pages == null || pages.Length == 0)
        {
            currentPageIndex = 0;
            return;
        }

        currentPageIndex =
            Mathf.Clamp(
                pageIndex,
                0,
                pages.Length - 1
            );

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        UpdateNavigationButtons();
    }

    public void NextPage()
    {
        ShowPage(currentPageIndex + 1);
    }

    public void PreviousPage()
    {
        ShowPage(currentPageIndex - 1);
    }

    private bool HasRequiredReferences()
    {
        if (
            generationPipeline == null ||
            floorPlanLoader == null ||
            roomRulesLoader == null
        )
        {
            Debug.LogError("FloorPlanRequestUI tem referencias por atribuir no Inspector.");
            SetStatus("Faltam referencias no Inspector.");
            return false;
        }

        return true;
    }

    private bool ApplySelectedApartment()
    {
        if (apartments == null || apartments.Length == 0)
        {
            SetStatus("Nao existem apartamentos configurados.");
            return false;
        }

        if (selectedApartmentIndex < 0)
        {
            SetStatus("Escolhe primeiro um apartamento.");
            return false;
        }

        TextAsset selectedFloorPlan =
            apartments[selectedApartmentIndex].floorPlanJson;

        if (selectedFloorPlan == null)
        {
            SetStatus("O apartamento selecionado nao tem JSON associado.");
            return false;
        }

        floorPlanLoader.floorPlanJson = selectedFloorPlan;
        return true;
    }

    private FloorPlanRequestData BuildRequest(BaseApartmentData apartment)
    {
        if (apartment == null)
        {
            return null;
        }

        FloorPlanRequestData request = new FloorPlanRequestData();
        request.apartmentId = apartment.apartmentId;
        request.totalResidents = selectedTotalResidents;
        request.roomRequirements = selectedRooms.ToArray();

        Debug.Log("Pedido do utilizador para IA:");
        Debug.Log(JsonUtility.ToJson(request, true));

        return request;
    }

    private void UpdateNavigationButtons()
    {
        if (previousButton != null)
        {
            previousButton.interactable = currentPageIndex > 0;
        }

        if (nextButton != null && pages != null && pages.Length > 0)
        {
            nextButton.interactable = currentPageIndex < pages.Length - 1;
        }
    }

    private void SetNavigationEnabled(bool isEnabled)
    {
        if (previousButton != null)
        {
            previousButton.interactable = isEnabled && currentPageIndex > 0;
        }

        if (nextButton != null && pages != null && pages.Length > 0)
        {
            nextButton.interactable =
                isEnabled && currentPageIndex < pages.Length - 1;
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

    private void FinishGeneration(string message)
    {
        isGenerating = false;
        SetStatus(message);
        SetNavigationEnabled(true);
    }
}
