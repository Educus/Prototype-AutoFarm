using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


[Serializable]
public class KeyCodeData
{
    public int KeyID;
    public string ActionName;
    public string ActionType;
    public string DefaultKeyCode;
    public string Modifier1;
    public string Modifier2;
    public int IsRebindable;
    public string Description_KR;
}

public class KeyCodeDataManager : MonoBehaviour
{
    public static KeyCodeDataManager Instance;

    // 실제 사용 키
    private Dictionary<string, KeyCode> keyBindings =
        new Dictionary<string, KeyCode>();

    private const string SAVE_KEY = "KEY_BINDINGS";

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

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

        keyBindings.Clear();

        foreach (var key in keyList)
        {
            try
            {
                KeyCode parsedKey =
                    (KeyCode)Enum.Parse(
                        typeof(KeyCode),
                        key.DefaultKeyCode
                    );

                keyBindings[key.ActionName] = parsedKey;
            }
            catch
            {
                Debug.LogWarning($"Key Parse 실패 : {key.ActionName}");
            }
        }
    }

    public List<KeyCodeSaveData> GetSaveData()
    {
        List<KeyCodeSaveData> list =
            new List<KeyCodeSaveData>();

        foreach (var pair in keyBindings)
        {
            list.Add(new KeyCodeSaveData
            {
                actionName = pair.Key,
                keyCode = pair.Value.ToString()
            });
        }

        return list;
    }

    public void Load(List<KeyCodeSaveData> data)
    {
        if (data == null || data.Count == 0)
            return;

        keyBindings.Clear();

        foreach (var item in data)
        {
            try
            {
                KeyCode key =
                    (KeyCode)Enum.Parse(
                        typeof(KeyCode),
                        item.keyCode
                    );

                keyBindings[item.actionName] = key;
            }
            catch
            {
                Debug.LogWarning($"로드 실패 : {item.actionName}");
            }
        }
    }

    public bool GetKeyDown(string actionName)
    {
        if (!keyBindings.ContainsKey(actionName))
            return false;

        return Input.GetKeyDown(keyBindings[actionName]);
    }

    public KeyCode GetKey(string actionName)
    {
        if (!keyBindings.ContainsKey(actionName))
            return KeyCode.None;

        return keyBindings[actionName];
    }

    public void RebindKey(string actionName, KeyCode newKey)
    {
        keyBindings[actionName] = newKey;
    }

    public void ResetToDefault()
    {
        LoadDefaultData();
    }
}
