using System.Collections.Generic;
using UnityEngine;

public class RoomRuleValidationService : MonoBehaviour
{
    /// <summary>
    /// Valida a area declarada de cada divisao contra a regra correspondente ao
    /// seu tipo e numero de pessoas.
    /// </summary>
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

        // As regras sao identificadas pela mesma chave composta usada na
        // validacao preliminar: tipo da divisao e numero de pessoas.
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
