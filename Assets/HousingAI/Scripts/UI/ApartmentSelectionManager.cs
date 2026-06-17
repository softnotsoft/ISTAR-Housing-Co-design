using UnityEngine;
using UnityEngine.UI;

public class ApartmentSelectionManager : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private ApartmentCardUI[] apartmentCards;

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject pageApartmentSelection;
    [SerializeField] private GameObject pageUserRequirements;

    private ApartmentCardUI selectedCard;
    private BaseApartmentData selectedApartment;
    public static BaseApartmentData SelectedApartment { get; private set; }

    private void Start()
    {
        nextButton.interactable = false;
        nextButton.onClick.AddListener(GoToUserRequirements);

        if (nextButton != null)
            nextButton.interactable = false;

        foreach (ApartmentCardUI card in apartmentCards)
        {
            if (card == null)
                continue;

            Button button = card.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(() => SelectApartment(card));
            }

            card.SetSelected(false);
        }
    }

    private void SelectApartment(ApartmentCardUI card)
    {
        Debug.Log("CLIQUE DETETADO");

        // Se clicar novamente no mesmo card, desseleciona
        if (selectedCard == card)
        {
            selectedCard.SetSelected(false);
            selectedCard = null;
            selectedApartment = null;
            SelectedApartment = null;

            if (nextButton != null)
                nextButton.interactable = false;

            Debug.Log("Apartamento desselecionado.");
            return;
        }

        // Se havia outro card selecionado, limpa-o
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        // Seleciona o novo card
        selectedCard = card;
        selectedApartment = card.GetApartmentData();
        SelectedApartment = selectedApartment;
        
        selectedCard.SetSelected(true);

        if (nextButton != null)
            nextButton.interactable = true;

        Debug.Log($"Apartamento selecionado: {selectedApartment.name} ({selectedApartment.apartmentId})");
    }

    public BaseApartmentData GetSelectedApartment()
    {
        return selectedApartment;
    }

    private void GoToUserRequirements()
    {
        if (selectedApartment == null)
            return;

        pageApartmentSelection.SetActive(false);
        pageUserRequirements.SetActive(true);
    }
}