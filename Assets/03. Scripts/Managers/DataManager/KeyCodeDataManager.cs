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
    public string DefaultKeyCode1;
    public string DefaultKeyCode2;
    public int IsRebindable;
    public string Description_KR;
}
public static class KeyNames
{
    public static string Format(KeyCode key)
    {
        if (key == KeyCode.None) return "None";

        string name = key.ToString();

        // 숫자 키: Alpha1 -> 1, Keypad1 -> Num 1
        if (name.StartsWith("Alpha")) return name.Substring(5);
        if (name.StartsWith("Keypad")) return "Num " + name.Substring(6);

        switch (key)
        {
            case KeyCode.Return: return "Enter";
            case KeyCode.LeftShift: return "Left Shift";
            case KeyCode.RightShift: return "Right Shift";
            case KeyCode.LeftControl: return "Left Ctrl";
            case KeyCode.RightControl: return "Right Ctrl";
            case KeyCode.LeftAlt: return "Left Alt";
            case KeyCode.RightAlt: return "Right Alt";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.Mouse0: return "Left Click";
            case KeyCode.Mouse1: return "Right Click";
            case KeyCode.Mouse2: return "Middle Click";
        }

        return name;
    }
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

            if (Enum.TryParse(key.DefaultKeyCode1, out KeyCode parsed)
                && Enum.IsDefined(typeof(KeyCode), parsed))
            {
                defaultKeys[key.Name] = parsed;
            }
            else
            {
                Debug.LogWarning($"Key Parse 실패 : {key.Name} (DefaultKeyCode='{key.DefaultKeyCode1}')");
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
