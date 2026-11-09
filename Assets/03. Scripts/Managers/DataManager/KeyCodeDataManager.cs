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
public static class KeyNames
{
    public static string Format(KeyCode key)
    {
        if (key == KeyCode.None) return "없음";

        string name = key.ToString();

        // 숫자 키: Alpha1 -> 1, Keypad1 -> 숫자패드 1
        if (name.StartsWith("Alpha")) return name.Substring(5);
        if (name.StartsWith("Keypad")) return "숫자패드 " + name.Substring(6);

        switch (key)
        {
            case KeyCode.Space: return "스페이스";
            case KeyCode.Return: return "엔터";
            case KeyCode.Backspace: return "백스페이스";
            case KeyCode.Tab: return "탭";
            case KeyCode.LeftShift: return "왼쪽 Shift";
            case KeyCode.RightShift: return "오른쪽 Shift";
            case KeyCode.LeftControl: return "왼쪽 Ctrl";
            case KeyCode.RightControl: return "오른쪽 Ctrl";
            case KeyCode.LeftAlt: return "왼쪽 Alt";
            case KeyCode.RightAlt: return "오른쪽 Alt";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.Mouse0: return "마우스 좌클릭";
            case KeyCode.Mouse1: return "마우스 우클릭";
            case KeyCode.Mouse2: return "마우스 휠클릭";
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

            if (Enum.TryParse(key.DefaultKeyCode, out KeyCode parsed)
                && Enum.IsDefined(typeof(KeyCode), parsed))
            {
                defaultKeys[key.Name] = parsed;
            }
            else
            {
                Debug.LogWarning($"Key Parse 실패 : {key.Name} (DefaultKeyCode='{key.DefaultKeyCode}')");
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
