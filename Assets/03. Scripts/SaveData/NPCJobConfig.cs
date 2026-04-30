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
    public List<String> buildingIDs = new List<string>();

    // 할당된 생산아이템
    public int productItemID;

    public JobStep step = JobStep.Idle;

    public bool IsValid()
    {
        return  jobType != JobType.None &&
                buildingIDs.Count > 0 &&
                productItemID != 0;
    }
}
