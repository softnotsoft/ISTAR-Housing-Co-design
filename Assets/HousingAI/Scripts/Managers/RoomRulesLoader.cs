using UnityEngine;

public class RoomRulesLoader : MonoBehaviour
{
    [Header("JSON File")]
    public TextAsset roomRulesJson;

    [Header("Debug")]
    [SerializeField] private bool logRules;

    private RoomRulesData roomRules;

    //private void Start()
    //{
    //    LoadRules();
    //}

    /// <summary>
    /// Carrega room_rules.json para memoria. As regras ficam disponiveis para a
    /// validacao preliminar, para o prompt e para a validacao da resposta.
    /// </summary>
    public void LoadRules()
    {
        if (roomRulesJson == null)
        {
            Debug.LogError("Nenhum ficheiro de regras foi atribuído.");
            return;
        }

        RoomRulesData loadedRules = JsonUtility.FromJson<RoomRulesData>(roomRulesJson.text);

        if (loadedRules == null || loadedRules.rules == null || loadedRules.rules.Length == 0)
        {
            Debug.LogError("Erro ao carregar as regras.");
            return;
        }

        roomRules = loadedRules;

        Debug.Log($"Foram carregadas {roomRules.rules.Length} regras.");

        if (!logRules)
        {
            return;
        }

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

    /// <summary>Devolve o conjunto de regras carregado.</summary>
    public RoomRulesData GetRules()
    {
        return roomRules;
    }
}
