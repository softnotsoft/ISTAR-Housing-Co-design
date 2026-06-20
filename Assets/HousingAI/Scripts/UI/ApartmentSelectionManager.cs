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
        // O botao Seguinte so fica disponivel depois de existir uma selecao.
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

        // Clicar novamente no mesmo cartao funciona como desselecao.
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

        // Apenas um apartamento pode estar selecionado de cada vez.
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        // A propriedade estatica permite que a pagina seguinte aceda a selecao.
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
        // A navegacao entre paginas e feita ativando e desativando GameObjects.
        if (selectedApartment == null)
            return;

        pageApartmentSelection.SetActive(false);
        pageUserRequirements.SetActive(true);
    }
}
