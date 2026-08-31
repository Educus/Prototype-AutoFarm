using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
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

    // NPC 정보 저장
    public void Register(NPC npc)
    {
        if (!npcs.ContainsKey(npc.id))
            npcs.Add(npc.id, npc);
    }

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
            if (npc.job.buildingIDs.Contains(buildingID))
                return true;
        }

        return false;
    }

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
