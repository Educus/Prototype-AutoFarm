using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public Dictionary<string, NPC> npcs = new Dictionary<string, NPC>();

    public void Register(NPC npc)
    {
        if (!npcs.ContainsKey(npc.id))
            npcs.Add(npc.id, npc);
    }

    public NPC Get(string id)
    {
        return npcs[id];
    }
}
