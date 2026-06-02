using UnityEngine;

[CreateAssetMenu(menuName = "Building/Data")]
public class BuildingData : ScriptableObject
{
    [Header("Building")]
    public int itemID;
    public string buildingName;

    public int width;
    public int height;

    public int cost;

    [Header("Work")]
    public JobType jobType = JobType.None;

    // NPC 작업 슬롯 사용량
    public int workSlotCost = 1;

    // true = 통과 가능
    // false = 막힘
    public bool[] patternFlat;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (patternFlat == null ||
            patternFlat.Length != width * height)
        {
            patternFlat = new bool[width * height];
        }

        workSlotCost = Mathf.Max(1, workSlotCost);
    }
#endif
}