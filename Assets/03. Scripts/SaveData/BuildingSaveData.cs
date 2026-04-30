using UnityEngine;

[System.Serializable]
public class BuildingSaveData
{
    public int itemId;  // 아이템 아이디
    public string id;   // 건물 이름(저장 키)
    public Vector2 position;

    public BuildingType type;   // 건물 타입
    public string jsonData;     // 건물 상세 데이터
}
