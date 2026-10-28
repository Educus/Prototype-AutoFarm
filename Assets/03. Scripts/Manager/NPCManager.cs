using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    // NPC 구매 아이템 ID
    public const int NpcItemID = 6021;

    public Dictionary<string, NPC> npcs = new Dictionary<string, NPC>();

    // Inspector 확인용
    [System.Serializable]
    public class NPCDebugData
    {
        public string id;
        public NPC npc;
    }

    [SerializeField]
    private List<NPCDebugData> debugNPCs =
        new List<NPCDebugData>();

    #region 등록 / 해제

    // NPC 정보 저장
    public void Register(NPC npc)
    {
        if (npcs.ContainsKey(npc.id))
        {
            Debug.LogWarning($"[NPCManager] NPC id 중복: {npc.id}");
            return;
        }

        npcs.Add(npc.id, npc);
    }

    public void Unregister(NPC npc)
    {
        if (npc == null)
            return;

        npcs.Remove(npc.id);
    }
    #endregion

    #region 찾기

    // NPC 정보 찾기
    public NPC Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("NPC ID is null or empty.");
            return null;
        }

        if (npcs.TryGetValue(id, out NPC npc))
        {
            return npc;
        }

        Debug.LogWarning($"NPC not found: {id}");

        return null;
    }

    public bool IsBuildingAssigned(string buildingID)
    {
        foreach (var npc in npcs.Values)
        {
            if (npc == null || npc.job == null || npc.job.buildingIDs == null)
                continue;

            if (npc.job.buildingIDs.Contains(buildingID))
                return true;
        }

        return false;
    }

    // 건물이 파괴될 때 그 건물에 배정된 NPC의 배정을 해제
    // (BuildingBase.OnWorkInteract의 해제 처리와 같은 규칙: 마지막 건물이면 직업 초기화)
    public void ReleaseBuildingAssignment(string buildingID)
    {
        foreach (var npc in npcs.Values)
        {
            if (npc == null || npc.job == null || npc.job.buildingIDs == null)
                continue;

            if (!npc.job.buildingIDs.Remove(buildingID))
                continue;

            // 마지막 건물 제거 시 직업 초기화
            if (npc.job.buildingIDs.Count == 0)
                npc.job.jobType = JobType.None;

            // NPCJobController가 작업 상태를 초기화함
            npc.JobChanged();
        }
    }

    #endregion

    #region 생성 / 정리

    public NPC Spawn(string npcID, Vector3 position)
    {
        GameObject prefab = DataManager.Instance.GetObjectPrefabs(NpcItemID);

        if (prefab == null)
        {
            Debug.LogWarning("NPC Prefab is null.");
            return null;
        }

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);

        NPC npc = obj.GetComponent<NPC>();
        npc.Initialize(npcID);

        return npc;
    }

    // 불러오기 전에 기존 NPC를 모두 정리
    public void Clear()
    {
        GameManager gm = GameManager.Instance;

        foreach (var npc in npcs.Values)
        {
            if (npc == null)
                continue;

            npc.StopMove();                 // 진행 중인 이동 코루틴 정리
            npc.UnregisterInventories();    // InventoryManager 등록 해제

            // 파괴될 NPC를 가리키지 않도록 선택 상태 해제 (targetLock 기본값은 null)
            if (gm.selectedNPC == npc)
                gm.selectedNPC = null;

            if (gm.targetLock == npc.gameObject)
                gm.targetLock = null;

            // Destroy는 프레임 끝에 실행되지만 딕셔너리는 바로 비우므로
            // 같은 프레임에 같은 id로 새 NPC를 등록해도 충돌하지 않음
            Destroy(npc.gameObject);
        }

        npcs.Clear();
    }

    #endregion

    // Dictionary -> List 변환
    private void RefreshDebugList()
    {
        debugNPCs.Clear();

        foreach (var pair in npcs)
        {
            debugNPCs.Add(new NPCDebugData
            {
                id = pair.Key,
                npc = pair.Value
            });
        }
    }

#if UNITY_EDITOR
    // Inspector 실시간 갱신
    private void Update()
    {
        RefreshDebugList();
    }
#endif
}
