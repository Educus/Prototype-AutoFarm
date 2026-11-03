using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


[Serializable]
public class KeyCodeData
{
    public int ID;
    public string Name;
    public string Type;
    public string DefaultKeyCode;
    public string Modifier1;
    public string Modifier2;
    public int IsRebindable;
    public string Description_KR;
}

[DefaultExecutionOrder(-100)]
public class KeyCodeDataManager : MonoBehaviour
{
    private readonly Dictionary<string, KeyCodeData> table = new Dictionary<string, KeyCodeData>();
    private readonly Dictionary<string, KeyCode> defaultKeys= new Dictionary<string, KeyCode>();

    public IReadOnlyDictionary<string, KeyCodeData> Table => table;
    public IReadOnlyDictionary<string, KeyCode> DefaultKeys => defaultKeys;

    private void Awake()
    {
        LoadDefaultData();
    }

    void LoadDefaultData()
    {
        TextAsset jsonFile =
             Resources.Load<TextAsset>("Json/KeycodeDataTable");

        if (jsonFile == null)
        {
            Debug.LogError("KeycodeDataTable 없음");
            return;
        }

        List<KeyCodeData> keyList =
            JsonConvert.DeserializeObject<List<KeyCodeData>>(jsonFile.text);

        table.Clear();
        defaultKeys.Clear();

        foreach (var key in keyList)
        {
            table[key.Name] = key;

            if (Enum.TryParse(key.DefaultKeyCode, out KeyCode parsed))
            {
                defaultKeys[key.Name] = parsed;
            }
            else
            {
                Debug.LogWarning($"Key Parse 실패 : {key.Name}");
            }
        }
    }

    public bool TryGetDefaultKey(string actionName, out KeyCode keyCode)
    {
        return defaultKeys.TryGetValue(actionName, out keyCode);
    }

    public bool IsRebindable(string actionName)
    {
        return table.TryGetValue(actionName, out KeyCodeData data) && data.IsRebindable == 1;
    }
}
