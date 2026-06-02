using System;
using System.Collections.Generic;

public enum JobType
{
    None,
    Farm,
    Ranch
}

public enum JobStep
{
    Idle,
    MoveToStorage,
    TakeResource,
    MoveToBuilding,
    Work,
    Waiting
}

[Serializable]
public class NPCJobConfig
{
    public JobType jobType = JobType.None;

    // 할당된 건물
    public List<string> buildingIDs =
        new List<string>();

    // 생산 품목
    public int productItemID = 0;

    public JobStep step = JobStep.Idle;

    // 최대 작업 슬롯
    public int maxWorkSlots = 4;

    public bool IsValid()
    {
        return jobType != JobType.None &&
               buildingIDs.Count > 0 &&
               productItemID != 0;
    }

    // 현재 사용 슬롯 수
    public int GetUsedSlots()
    {
        int used = 0;

        foreach (string buildingID in buildingIDs)
        {
            BuildingBase building =
                DataManager.Instance
                .BuildingManager
                .Get(buildingID);

            if (building == null)
                continue;

            used += building.data.workSlotCost;
        }

        return used;
    }

    public int GetRemainSlots()
    {
        return maxWorkSlots - GetUsedSlots();
    }
}
