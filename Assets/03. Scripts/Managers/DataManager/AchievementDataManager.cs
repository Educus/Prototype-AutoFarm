using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class AchievementData
{
    [JsonProperty("ID")]
    public int ID;

    [JsonProperty("Name")]
    public string Name;

    [JsonProperty("Type")]
    public string Type;

    [JsonProperty("Target_Count")]
    public int TargetCount;

    [JsonProperty("Compensation_Type")]
    public string CompensationType;

    [JsonProperty("Compensation_Amount")]
    public int CompensationAmount;
}

[DefaultExecutionOrder(-100)]
public class AchievementDataManager : MonoBehaviour
{
    private const string ResourcePath = "Json/AchievementDataTable";

    public Dictionary<int, AchievementData> achievementData { get; } =
        new Dictionary<int, AchievementData>();

    private void Awake()
    {
        Load();
    }

    private void Load()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(ResourcePath);

        if (jsonFile == null)
        {
            Debug.LogError($"[AchievementDataManager] {ResourcePath} 파일 없음");
            return;
        }

        List<AchievementData> list;

        try
        {
            list = JsonConvert.DeserializeObject<List<AchievementData>>(jsonFile.text);
        }
        catch (Exception e)
        {
            Debug.LogError($"[AchievementDataManager] 읽기 실패 : {e.Message}");
            return;
        }

        achievementData.Clear();

        if (list == null)
            return;

        foreach (AchievementData data in list)
        {
            if (achievementData.ContainsKey(data.ID))
                Debug.LogWarning($"[AchievementDataManager] ID중복 : {data.ID}");

            achievementData[data.ID] = data;
        }
    }

    public bool TryGet(int id, out AchievementData data) => achievementData.TryGetValue(id, out data);
}