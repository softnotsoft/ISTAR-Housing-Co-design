using System.Collections.Generic;
using UnityEngine;

public class GeneratedPlanValidationService : MonoBehaviour
{
    /// <summary>
    /// Compara a soma das areas geometricas das divisoes com a area total do
    /// apartamento. O criterio atual exige pelo menos 98% de ocupacao.
    /// </summary>
    public GeneratedPlanValidationResult Validate(
        BaseApartmentData apartment,
        GeneratedFloorPlanData generatedPlan
    )
    {
        GeneratedPlanValidationResult result =
            new GeneratedPlanValidationResult();

        List<string> errors = new List<string>();

        float availableArea =
            CalculatePolygonArea(apartment.boundary);

        float usedArea = 0f;

        // A area e recalculada a partir dos pontos; nao se confia no campo
        // room.area devolvido pelo modelo para esta metrica global.
        foreach (RoomData room in generatedPlan.rooms)
        {
            float roomArea =
                CalculatePolygonArea(room.points);

            usedArea += roomArea;
        }

        float usagePercentage =
            (usedArea / availableArea) * 100f;

        if (usagePercentage < 98f)
        {
            errors.Add(
                $"A planta utiliza apenas {usagePercentage:F2}% da área disponível."
            );
        }

        result.availableArea = availableArea;
        result.usedArea = usedArea;
        result.usagePercentage = usagePercentage;

        result.errors = errors.ToArray();
        result.isValid = errors.Count == 0;

        return result;
    }

    private float CalculatePolygonArea(PointData[] points)
    {
        float area = 0f;

        int j = points.Length - 1;

        for (int i = 0; i < points.Length; i++)
        {
            area +=
                (points[j].x + points[i].x) *
                (points[j].y - points[i].y);

            j = i;
        }

        return Mathf.Abs(area / 2f);
    }
}
