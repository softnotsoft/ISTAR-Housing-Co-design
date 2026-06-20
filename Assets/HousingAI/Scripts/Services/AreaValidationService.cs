using UnityEngine;

public class AreaValidationService : MonoBehaviour
{
    /// <summary>
    /// Verifica, antes de chamar o Gemini, se a soma das areas minimas das
    /// divisoes pedidas cabe na area do poligono do apartamento.
    /// </summary>
    public AreaValidationResult Validate(
        BaseApartmentData apartment,
        FloorPlanRequestData request,
        RoomRulesData rulesData
    )
    {
        AreaValidationResult result = new AreaValidationResult();

        if (
            apartment == null ||
            apartment.boundary == null ||
            request == null ||
            request.roomRequirements == null ||
            rulesData == null ||
            rulesData.rules == null
        )
        {
            result.isValid = false;
            result.message = "Dados insuficientes para validar a area.";
            return result;
        }

        // A formula de Shoelace calcula a area de qualquer poligono simples.
        float availableArea = CalculatePolygonArea(apartment.boundary);
        float requiredArea = CalculateRequiredArea(request, rulesData);

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

        // Cada requisito e associado pela combinacao tipo + numero de pessoas.
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
