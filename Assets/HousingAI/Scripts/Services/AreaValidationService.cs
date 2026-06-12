using UnityEngine;

public class AreaValidationService : MonoBehaviour
{
    public AreaValidationResult Validate(
        BaseApartmentData apartment,
        FloorPlanRequestData request,
        RoomRulesData rulesData
    )
    {
        float availableArea = CalculatePolygonArea(apartment.boundary);
        float requiredArea = CalculateRequiredArea(request, rulesData);

        AreaValidationResult result = new AreaValidationResult();

        result.availableArea = availableArea;
        result.requiredArea = requiredArea;
        result.isValid = requiredArea <= availableArea;

        result.message = result.isValid
            ? "O pedido é compatível com a área disponível."
            : "O pedido excede a área disponível.";

        return result;
    }

    private float CalculateRequiredArea(
        FloorPlanRequestData request,
        RoomRulesData rulesData
    )
    {
        float total = 0f;

        foreach (RoomRequirementData requirement in request.roomRequirements)
        {
            RoomRuleData rule = FindRule(
                requirement.type,
                requirement.people,
                rulesData
            );

            if (rule != null)
            {
                total += rule.minArea;
            }
            else
            {
                Debug.LogWarning(
                    $"Não foi encontrada regra para {requirement.type} com {requirement.people} pessoa(s)."
                );
            }
        }

        return total;
    }

    private RoomRuleData FindRule(
        string roomType,
        int people,
        RoomRulesData rulesData
    )
    {
        foreach (RoomRuleData rule in rulesData.rules)
        {
            if (rule.roomType == roomType && rule.people == people)
            {
                return rule;
            }
        }

        return null;
    }

    private float CalculatePolygonArea(PointData[] points)
    {
        float area = 0f;

        for (int i = 0; i < points.Length; i++)
        {
            PointData current = points[i];
            PointData next = points[(i + 1) % points.Length];

            area += current.x * next.y - next.x * current.y;
        }

        return Mathf.Abs(area) / 2f;
    }
}