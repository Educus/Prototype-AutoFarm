using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum WarningBehavior
{
    Block,      // 안내만 보여주기
    Confirm     // 예, 아니오 확인창
}

public static class WarningID
{
    public const int None = 101;

    // UI
    public const int NotEnoughMoney = 201;
    public const int NotEnoughUpgradeParts = 202;
    public const int NoMatchingEquipment = 203;
    public const int NewGameConfirm = 211;
    public const int ReturnToTitleConfirm = 212;

    // Object
    public const int CannotDestroyBaseBuilding = 301;
    public const int RocketEmptyDeparture = 302;
    public const int RocketNoShopManager = 303;
    public const int RocketAlreadyDeparted = 304;
    public const int RocketTakeDeliveredFirst = 305;
    public const int RocketUnsellableItem = 306;
    public const int RocketNothingToTrade = 307;

    // Action
    public const int NotEnoughItem = 401;
}

[Serializable]
public class WarningMessageData
{
    public int ID;
    public string Type;
    public string Behavior;

    // Text_Kr 등 언어부분
    [JsonExtensionData]
    public Dictionary<string, object> Extra = new Dictionary<string, object>();
}

[DefaultExecutionOrder(-100)]
public class WarningMessageManager : MonoBehaviour
{
    public const string DefaultLanguage = "KR";
    private const string TextColumn = "Text_";

    public string Language { get; private set; } = DefaultLanguage;
    public event Action OnLanguageChanged;

    public Dictionary<int, WarningMessageData> warningMessageData { get; } = 
        new Dictionary<int, WarningMessageData>();

    private readonly Dictionary<int, Dictionary<string, string>> texts = new Dictionary<int, Dictionary<string, string>>();
    private readonly HashSet<int> warned = new HashSet<int>();

    private void Awake()
    {
        LoadDefaultData();
    }

    void LoadDefaultData()
    {
        TextAsset jsonFile =
             Resources.Load<TextAsset>("Json/WarningMessage");

        if (jsonFile == null)
        {
            Debug.LogError("WarningMessage 없음");
            return;
        }

        List<WarningMessageData> list =
            JsonConvert.DeserializeObject<List<WarningMessageData>>(jsonFile.text);

        warningMessageData.Clear();
        texts.Clear();

        foreach (WarningMessageData data in list)
        {
            if (warningMessageData.ContainsKey(data.ID))
                Debug.LogWarning($"[WarningMessageManager] ID 중복 : {data.ID}");

            warningMessageData[data.ID] = data;

            var byLanguage = new Dictionary<string, string>();

            if (data.Extra != null)
            {
                foreach (var pair in data.Extra)
                {
                    if (pair.Key.StartsWith(TextColumn, StringComparison.OrdinalIgnoreCase)
                        && pair.Value is string text)
                    {
                        byLanguage[pair.Key.Substring(TextColumn.Length).ToUpperInvariant()] = text;
                    }
                }
            }

            texts[data.ID] = byLanguage;
        }
    }

    public void SetLanguage(string code)
    {
        if (string.IsNullOrEmpty(code))
            return;

        code = code.ToUpperInvariant();

        if (code == Language)
            return;

        Language = code;
        OnLanguageChanged?.Invoke();
    }

    public bool TryGet(int id, out string message)
    {
        message = null;
        
        if (!texts.TryGetValue(id, out Dictionary<string, string> byLanguage))
            return false;

        return (byLanguage.TryGetValue(Language, out message) &&
            !string.IsNullOrEmpty(message))
            || (byLanguage.TryGetValue(DefaultLanguage, out message) &&
            !string.IsNullOrEmpty(message));
    }

    public string Get(int id)
    {
        if (TryGet(id, out string message))
            return message;

        if (warned.Add(id))
            Debug.LogWarning($"[WarningMessageManager] 해당 언어 없음 : ID = {id}_{Language}");

        return $"[W{id}]";
    }

    // 표의 {0}, {1} 자리에 값을 끼워 넣음
    public string Get(int id, params object[] args)
    {
        string message = Get(id);

        if (args == null || args.Length == 0)
            return message;

        try
        {
            return string.Format(message, args);
        }
        catch (FormatException)
        {
            Debug.LogWarning($"[WarningMessageManager] ID={id}의 {{0}}자리 표시가 값 갯수와 다릅니다.");
            return message;
        }
    }

    public WarningBehavior GetBehavior(int id)
    {
        if (warningMessageData.TryGetValue(id, out WarningMessageData data)
            && Enum.TryParse(data.Behavior, true, out WarningBehavior behavior))
            return behavior;

        return WarningBehavior.Block;
    }
}
