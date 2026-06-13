using System;

[Serializable]
public class AIRequestData
{
    public BaseApartmentData baseApartment;
    public FloorPlanRequestData userRequest;
    public RoomRulesData roomRules;
    public AreaValidationResult validationResult;
}