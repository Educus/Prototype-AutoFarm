using System.Collections.Generic;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public GameSaveData CreateSaveData()
    {
        GameSaveData data = new GameSaveData();

        // Inventory
        data.inventories = new List<InventorySaveData>();
        foreach (var inv in DataManager.Instance.InventoryManager.inventories.Values)
        {
            data.inventories.Add(inv.GetSaveData());
        }

        // NPC
        data.npcs = new List<NPCSaveData>();
        foreach (var npc in DataManager.Instance.NPCManager.npcs.Values)
        {
            data.npcs.Add(npc.GetSaveData());
        }

        // Currency
        data.currency = DataManager.Instance.CurrencyManager.GetSaveData();

        return data;
    }

    public void Load(GameSaveData data)
    {
        // Inventory
        foreach (var invData in data.inventories)
        {
            var inv = DataManager.Instance.InventoryManager.Get(invData.id);
            inv.Load(invData);
        }

        // NPC
        foreach (var npcData in data.npcs)
        {
            var npc = DataManager.Instance.NPCManager.Get(npcData.id);
            npc.Load(npcData);
        }

        // Currency
        DataManager.Instance.CurrencyManager.Load(data.currency);
    }
}