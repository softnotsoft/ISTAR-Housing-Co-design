using System.Collections.Generic;
using UnityEngine;

public class RoomRuleValidationService : MonoBehaviour
{
    public RoomRuleValidationResult Validate(
        GeneratedFloorPlanData generatedPlan,
        RoomRulesData rules
    )
    {
        RoomRuleValidationResult result =
            new RoomRuleValidationResult();

        List<string> errors =
            new List<string>();

        foreach (RoomData room in generatedPlan.rooms)
        {
            ValidateRoom(room, rules, errors);
        }

        result.errors = errors.ToArray();
        result.isValid = errors.Count == 0;

        return result;
    }

    private void ValidateRoom(
        RoomData room,
        RoomRulesData rules,
        List<string> errors
    )
    {
        RoomRuleData matchingRule = null;

        foreach (RoomRuleData rule in rules.rules)
        {
            if (
                rule.roomType == room.type &&
                rule.people == room.people
            )
            {
                matchingRule = rule;
                break;
            }
        }

        if (matchingRule == null)
        {
            errors.Add(
                $"Não existe regra para {room.type} ({room.people} pessoas)."
            );

            return;
        }

        if (room.area < matchingRule.minArea)
        {
            errors.Add(
                $"{room.name} tem {room.area:F2} m² mas necessita de {matchingRule.minArea:F2} m²."
            );
        }
    }
}