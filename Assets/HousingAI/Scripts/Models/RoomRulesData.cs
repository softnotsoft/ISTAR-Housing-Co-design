using System;

[Serializable]
/// <summary>
/// Conjunto de regras carregado de room_rules.json.
/// </summary>
public class RoomRulesData
{
    public string unit;
    public RoomRuleData[] rules;
}
