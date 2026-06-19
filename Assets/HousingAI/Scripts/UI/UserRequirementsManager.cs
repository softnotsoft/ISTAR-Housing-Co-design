using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserRequirementsManager : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject pageUserRequirements;
    [SerializeField] private GameObject pageLayoutsVisualization;

    [Header("Panels")]
    [SerializeField] private GameObject householdPeoplePanel;
    [SerializeField] private GameObject peopleQuestionPanel;
    [SerializeField] private GameObject emptyState;

    [Header("Room Cards")]
    [SerializeField] private Transform roomCardsContainer;
    [SerializeField] private RoomRequirementCardUI roomCardPrefab;

    [Header("Household People Buttons")]
    [SerializeField] private Button householdOption1;
    [SerializeField] private Button householdOption2;
    [SerializeField] private Button householdOption3;
    [SerializeField] private Button householdOption4;

    [Header("Room People Buttons")]
    [SerializeField] private Button roomPeopleOption1;
    [SerializeField] private Button roomPeopleOption2;
    [SerializeField] private Button roomPeopleOption3;
    [SerializeField] private Button roomPeopleOption4;

    private int totalHouseholdPeople;
    private readonly List<RoomRequirementCardUI> roomCards = new();
    private RoomRequirementCardUI selectedRoomCard;

    private void Start()
    {
        nextButton.interactable = false;

        householdPeoplePanel.SetActive(true);
        peopleQuestionPanel.SetActive(false);
        emptyState.SetActive(false);
        roomCardsContainer.gameObject.SetActive(false);

        householdOption1.onClick.AddListener(() => SelectHouseholdPeople(1));
        householdOption2.onClick.AddListener(() => SelectHouseholdPeople(2));
        householdOption3.onClick.AddListener(() => SelectHouseholdPeople(3));
        householdOption4.onClick.AddListener(() => SelectHouseholdPeople(4));

        roomPeopleOption1.onClick.AddListener(() => SelectRoomPeople(1));
        roomPeopleOption2.onClick.AddListener(() => SelectRoomPeople(2));
        roomPeopleOption3.onClick.AddListener(() => SelectRoomPeople(3));
        roomPeopleOption4.onClick.AddListener(() => SelectRoomPeople(4));
        // Estes botões vão ser usados para escolher pessoas da divisão selecionada.

        nextButton.interactable = false;
        nextButton.onClick.AddListener(GoToLayoutsVisualization);
    }

    private void SelectHouseholdPeople(int peopleCount)
    {
        totalHouseholdPeople = peopleCount;

        householdPeoplePanel.SetActive(false);
        peopleQuestionPanel.SetActive(false);
        emptyState.SetActive(true);
        roomCardsContainer.gameObject.SetActive(true);

        CreateInitialRoomCards();
    }

    private void CreateInitialRoomCards()
    {
        ClearExistingCards();
        
        CreateRoomCard("Sala", totalHouseholdPeople, true, true);
        CreateRoomCard("Cozinha", totalHouseholdPeople, true, true);
        CreateRoomCard("Quarto");
        CreateRoomCard("Casa de banho");
        CreateRoomCard("Suíte");
        CreateRoomCard("Divisão Personalizada");

        ValidateRequirements();
    }
    
    private RoomRequirementCardUI CreateRoomCard(string roomName, int peopleCount = 0, bool startChosen = false, bool locked = false)
    {
        RoomRequirementCardUI card = Instantiate(roomCardPrefab, roomCardsContainer);
        card.gameObject.SetActive(true);

        card.Setup(roomName);
        card.SetLocked(locked);
        card.OnCardClicked += HandleRoomCardClicked;

        if (startChosen)
        {
            card.SetChosen(peopleCount);
        }

        roomCards.Add(card);
        return card;
    }

    private void ClearExistingCards()
    {
        foreach (RoomRequirementCardUI card in roomCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }

        roomCards.Clear();
    }

    private void HandleRoomCardClicked(RoomRequirementCardUI card)
    {
        if (card.IsLocked)
        {
            return;
        }

        if (selectedRoomCard != null && selectedRoomCard.CurrentState == RoomRequirementCardState.Selected)
        {
            selectedRoomCard.SetUnselected();
        }

        selectedRoomCard = card;
        selectedRoomCard.SetSelected();

        emptyState.SetActive(false);
        peopleQuestionPanel.SetActive(true);
    }

    private void SelectRoomPeople(int peopleCount)
    {
        if (selectedRoomCard == null)
        {
            return;
        }

        peopleCount = Mathf.Min(peopleCount, totalHouseholdPeople);
        selectedRoomCard.SetChosen(peopleCount);
        selectedRoomCard = null;

        peopleQuestionPanel.SetActive(false);
        emptyState.SetActive(true);
        ValidateRequirements();
    }
    
    private void ValidateRequirements()
    {
        bool hasSala = false;
        bool hasCozinha = false;
        bool hasQuarto = false;
        bool hasCasaBanho = false;
        bool hasSuite = false;

        foreach (RoomRequirementCardUI card in roomCards)
        {
            if (card.CurrentState != RoomRequirementCardState.Chosen)
                continue;

            if (card.RoomName == "Sala")
                hasSala = true;

            if (card.RoomName == "Cozinha")
                hasCozinha = true;

            if (card.RoomName == "Quarto")
                hasQuarto = true;

            if (card.RoomName == "Casa de banho")
                hasCasaBanho = true;

            if (card.RoomName == "Suíte")
                hasSuite = true;
        }

        nextButton.interactable = hasSala && hasCozinha && hasQuarto && hasCasaBanho && hasSuite;
    }

    private void GoToLayoutsVisualization()
    {
        FloorPlanRequestData request = BuildUserRequest();
        
        if (request == null)
            return;

        UserSelectionData.SelectedApartment = ApartmentSelectionManager.SelectedApartment;
        UserSelectionData.UserRequest = request;
        UserSelectionData.UserRequirementsJson = JsonUtility.ToJson(request, true);

        pageUserRequirements.SetActive(false);
        pageLayoutsVisualization.SetActive(true);
    }

    private FloorPlanRequestData BuildUserRequest()
    {
        BaseApartmentData selectedApartment = ApartmentSelectionManager.SelectedApartment;
        
        if (selectedApartment == null)
        {
            Debug.LogError("Nenhum apartamento selecionado encontrado.");
            return null;
        }

        List<RoomRequirementData> requirements = new List<RoomRequirementData>();

        foreach (RoomRequirementCardUI card in roomCards)
        {
            if (card.CurrentState != RoomRequirementCardState.Chosen)
                continue;

            requirements.Add(new RoomRequirementData
            {
                type = ConvertRoomNameToType(card.RoomName),
                people = card.PeopleCount
            });
        }

        FloorPlanRequestData request = new FloorPlanRequestData
        {
            apartmentId = selectedApartment.apartmentId,
            totalResidents = totalHouseholdPeople,
            roomRequirements = requirements.ToArray()
        };
        
        Debug.Log("User requirements JSON:");
        Debug.Log(JsonUtility.ToJson(request, true));

        return request;
    }

    private string ConvertRoomNameToType(string roomName)
    {
        switch (roomName)
        {
            case "Sala":
                return "living_room";
            
            case "Cozinha":
                return "kitchen";

            case "Quarto":
                return "bedroom";

            case "Casa de banho":
                return "bathroom";

            case "Suíte":
                return "suite";

            default:
                return "custom_room";
        }
    }

}