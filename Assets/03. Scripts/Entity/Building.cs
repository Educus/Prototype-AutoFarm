using UnityEngine;

public enum BuildingType
{

}

public class Building : MonoBehaviour, ISaveable
{
    // 프리팹에 저장된 데이터
    public int itemID;
    public BuildingData data;

    public Sprite icon;

    public float upgradeProgress;   // 건물 업그레이드 진행 상황
    public float growthNum;         // 재배중인 작물 번호
    public float growthProgress;    // 작물 성장 진행 상황

    public string Save()
    {
        BuildingExtraData extra = new BuildingExtraData
        {
            upgradeProgress = this.upgradeProgress,
            growthNum = this.growthNum,
            growthProgress = this.growthProgress
        };

        return JsonUtility.ToJson(extra);
    }

    public void Load(string json)
    {
        BuildingExtraData extra = JsonUtility.FromJson<BuildingExtraData>(json);

        this.upgradeProgress = extra.upgradeProgress;
        this.growthNum = extra.growthNum;
        this.growthProgress = extra.growthProgress;
    }

}
