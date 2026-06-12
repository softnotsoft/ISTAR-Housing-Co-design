using UnityEngine;

public class RoomRulesLoader : MonoBehaviour
{
    [Header("JSON File")]
    public TextAsset roomRulesJson;

    private RoomRulesData roomRules;

    private void Start()
    {
        LoadRules();
    }

    public void LoadRules()
    {
        if (roomRulesJson == null)
        {
            Debug.LogError("Nenhum ficheiro de regras foi atribuído.");
            return;
        }

        roomRules = JsonUtility.FromJson<RoomRulesData>(roomRulesJson.text);

        if (roomRules == null || roomRules.rules == null)
        {
            Debug.LogError("Erro ao carregar as regras.");
            return;
        }

        Debug.Log($"Foram carregadas {roomRules.rules.Length} regras.");

        foreach (RoomRuleData rule in roomRules.rules)
        {
            Debug.Log(
                $"Tipo: {rule.roomType} | " +
                $"Pessoas: {rule.people} | " +
                $"Área mínima: {rule.minArea} m² | " +
                $"Largura mínima: {rule.minWidth} m"
            );
        }
    }

    public RoomRulesData GetRules()
    {
        return roomRules;
    }
}