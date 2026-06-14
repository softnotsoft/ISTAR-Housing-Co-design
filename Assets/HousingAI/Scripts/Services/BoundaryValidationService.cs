using System.Collections.Generic;
using UnityEngine;

public class BoundaryValidationService : MonoBehaviour
{
    public BoundaryValidationResult Validate(
        BaseApartmentData apartment,
        GeneratedFloorPlanData generatedPlan
    )
    {
        BoundaryValidationResult result =
            new BoundaryValidationResult();

        List<string> errors =
            new List<string>();

        foreach (RoomData room in generatedPlan.rooms)
        {
            ValidateRoom(
                room,
                apartment.boundary,
                errors
            );
        }

        result.errors = errors.ToArray();
        result.isValid = errors.Count == 0;

        return result;
    }

    private void ValidateRoom(
        RoomData room,
        PointData[] boundary,
        List<string> errors
    )
    {
        foreach (PointData point in room.points)
        {
            bool inside =
    IsPointInsidePolygon(
        point,
        boundary
    );

            bool onBoundary =
                IsPointOnPolygonBoundary(
                    point,
                    boundary
                );

            if (!inside && !onBoundary)
            {
                errors.Add(
                    $"{room.name} possui um ponto fora do apartamento: ({point.x}, {point.y})"
                );
            }
        }
    }

    private bool IsPointInsidePolygon(
    PointData point,
    PointData[] polygon
    )
    {
        bool inside = false;

        int j = polygon.Length - 1;

        for (int i = 0; i < polygon.Length; i++)
        {
            bool intersect =
                (
                    (polygon[i].y > point.y)
                    !=
                    (polygon[j].y > point.y)
                )
                &&
                (
                    point.x <
                    (
                        polygon[j].x - polygon[i].x
                    )
                    *
                    (
                        point.y - polygon[i].y
                    )
                    /
                    (
                        polygon[j].y - polygon[i].y
                    )
                    +
                    polygon[i].x
                );

            if (intersect)
            {
                inside = !inside;
            }

            j = i;
        }

        return inside;
    }

    private bool IsPointOnPolygonBoundary(
    PointData point,
    PointData[] polygon
)
    {
        for (int i = 0; i < polygon.Length; i++)
        {
            PointData start = polygon[i];
            PointData end = polygon[(i + 1) % polygon.Length];

            if (IsPointOnLineSegment(point, start, end))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointOnLineSegment(
        PointData point,
        PointData start,
        PointData end
    )
    {
        float tolerance = 0.001f;

        float crossProduct =
            (point.y - start.y) * (end.x - start.x)
            -
            (point.x - start.x) * (end.y - start.y);

        if (Mathf.Abs(crossProduct) > tolerance)
        {
            return false;
        }

        float dotProduct =
            (point.x - start.x) * (end.x - start.x)
            +
            (point.y - start.y) * (end.y - start.y);

        if (dotProduct < -tolerance)
        {
            return false;
        }

        float squaredLength =
            (end.x - start.x) * (end.x - start.x)
            +
            (end.y - start.y) * (end.y - start.y);

        if (dotProduct - squaredLength > tolerance)
        {
            return false;
        }

        return true;
    }
}