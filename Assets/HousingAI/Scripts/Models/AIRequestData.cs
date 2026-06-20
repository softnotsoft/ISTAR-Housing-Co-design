using System;

[Serializable]
/// <summary>
/// Agrega toda a informacao necessaria para descrever um pedido de geracao.
/// Este modelo pertence ao fluxo de teste/diagnostico; o pipeline atual recebe
/// apartamento, pedido e regras como parametros separados.
/// </summary>
public class AIRequestData
{
    public BaseApartmentData baseApartment;
    public FloorPlanRequestData userRequest;
    public RoomRulesData roomRules;
    public AreaValidationResult validationResult;
}
