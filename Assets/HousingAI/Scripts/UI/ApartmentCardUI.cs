using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApartmentCardUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private TextAsset apartmentJson;
    [SerializeField] private Sprite thumbnailSprite;

    [Header("UI")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image floorPlanSlot;
    [SerializeField] private TMP_Text apartmentTitle;
    [SerializeField] private TMP_Text apartmentArea;
    
    [Header("Selection")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.85f, 0.93f, 1f);

    private BaseApartmentData apartmentData;

    private void Start()
    {
        Setup();
        SetSelected(false);
    }

    private void Setup()
    {
        if (apartmentJson == null)
        {
            Debug.LogError("Apartment JSON não atribuído.", this);
            return;
        }

        apartmentData = JsonUtility.FromJson<BaseApartmentData>(apartmentJson.text);

        if (apartmentData == null)
        {
            Debug.LogError("Erro ao converter JSON do apartamento.", this);
            return;
        }

        if (floorPlanSlot != null)
            floorPlanSlot.sprite = thumbnailSprite;

        if (apartmentTitle != null)
            apartmentTitle.text = apartmentData.name;

        if (apartmentArea != null)
            apartmentArea.text = $"{CalculateArea(apartmentData):F1} m²";
    }

    private float CalculateArea(BaseApartmentData apartment)
    {
        if (apartment.boundary == null || apartment.boundary.Length < 3)
            return 0f;

        float area = 0f;

        for (int i = 0; i < apartment.boundary.Length; i++)
        {
            PointData current = apartment.boundary[i];
            PointData next = apartment.boundary[(i + 1) % apartment.boundary.Length];

            area += current.x * next.y - next.x * current.y;
        }

        return Mathf.Abs(area) / 2f;
    }

    public BaseApartmentData GetApartmentData()
    {
        return apartmentData;
    }

    public void SetSelected(bool selected)
    {
        if (cardBackground != null)
            cardBackground.color = selected ? selectedColor : normalColor;
    }
}