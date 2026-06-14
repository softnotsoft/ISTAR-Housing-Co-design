using System.Collections.Generic;
using UnityEngine;

public class ValidationFeedbackBuilder : MonoBehaviour
{
    public ValidationFeedbackData BuildFeedback(
        GeneratedPlanValidationResult generatedPlanValidation,
        RoomRuleValidationResult roomRuleValidation,
        BoundaryValidationResult boundaryValidation
    )
    {
        List<string> errors = new List<string>();

        errors.AddRange(generatedPlanValidation.errors);
        errors.AddRange(roomRuleValidation.errors);
        errors.AddRange(boundaryValidation.errors);

        ValidationFeedbackData feedback = new ValidationFeedbackData();

        feedback.errors = errors.ToArray();
        feedback.isValid = errors.Count == 0;

        return feedback;
    }
}