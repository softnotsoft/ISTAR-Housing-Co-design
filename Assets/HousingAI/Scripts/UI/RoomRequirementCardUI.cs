using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum RoomRequirementCardState
{
    Unselected,
    Selected,
    Chosen
}

public class RoomRequirementCardUI : MonoBehaviour
{
    [Header("States")]
    [SerializeField] private GameObject unselectedState;
    [SerializeField] private GameObject selectedState;
    [SerializeField] private GameObject chosenState;

    [Header("Unselected UI")]
    [SerializeField] private TMP_Text unselectedRoomNameText;

    [Header("Selected UI")]
    [SerializeField] private TMP_Text selectedRoomNameText;

    [Header("Chosen UI")]
    [SerializeField] private TMP_Text chosenRoomNameText;
    [SerializeField] private TMP_Text peopleText;
    [SerializeField] private Button cardButton;

    public event Action<RoomRequirementCardUI> OnCardClicked;
    public bool IsLocked { get; private set; }

    private string roomName;
    private int peopleCount;
    private RoomRequirementCardState currentState;

    public string RoomName => roomName;
    public int PeopleCount => peopleCount;
    public RoomRequirementCardState CurrentState => currentState;

    /// <summary>Inicializa textos, estado visual e evento de clique do cartao.</summary>
    public void Setup(string newRoomName)
    {
        roomName = newRoomName;

        unselectedRoomNameText.text = roomName;
        selectedRoomNameText.text = roomName;
        chosenRoomNameText.text = roomName;
        
        SetUnselected();

        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(() => OnCardClicked?.Invoke(this));
    }

    /// <summary>Mostra o cartao ainda nao escolhido.</summary>
    public void SetUnselected()
    {
        currentState = RoomRequirementCardState.Unselected;

        unselectedState.SetActive(true);
        selectedState.SetActive(false);
        chosenState.SetActive(false);
    }

    /// <summary>Mostra o cartao que aguarda a escolha do numero de pessoas.</summary>
    public void SetSelected()
    {
        currentState = RoomRequirementCardState.Selected;

        unselectedState.SetActive(false);
        selectedState.SetActive(true);
        chosenState.SetActive(false);
    }

    /// <summary>Confirma a divisao e apresenta a ocupacao selecionada.</summary>
    public void SetChosen(int selectedPeopleCount)
    {
        peopleCount = selectedPeopleCount;
        currentState = RoomRequirementCardState.Chosen;
        chosenRoomNameText.text = roomName;
        
        peopleText.text = peopleCount == 1
            ? "1 pessoa"
            : peopleCount + " pessoas";

        unselectedState.SetActive(false);
        selectedState.SetActive(false);
        chosenState.SetActive(true);
    }
    /// <summary>Impede que uma divisao obrigatoria seja editada pela UI.</summary>
    public void SetLocked(bool locked)
    {
        IsLocked = locked;
    }
}
