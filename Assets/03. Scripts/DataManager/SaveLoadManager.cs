using System.Collections.Generic;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.save += Save;
        GameManager.Instance.load += Load;
    }
    public void Save()
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

        // Building
        data.buildings = DataManager.Instance.BuildingManager.GetSaveData();

        // Animal


        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(GetPath(), json);
    }

    // public void Load(GameSaveData data)
    public void Load()
    {
        string json = System.IO.File.ReadAllText(GetPath());
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);


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

        // Building
        DataManager.Instance.BuildingManager.Load(data.buildings);

        // Animal

    }

    private string GetPath()
    {
        return Application.persistentDataPath + "/save.json";
    }
}