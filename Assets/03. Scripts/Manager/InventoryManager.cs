using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Dictionary<string, Inventory> inventories =
        new Dictionary<string, Inventory>();

    // Inspector 확인용
    [System.Serializable]
    public class InventoryDebugData
    {
        public string id;
        public Inventory inventory;
    }

    [SerializeField]
    private List<InventoryDebugData> debugInventories =
       new List<InventoryDebugData>();

    public void Register(Inventory inv)
    {
        if (!inventories.ContainsKey(inv.id))
            inventories.Add(inv.id, inv);
    }

    public Inventory Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Inventory ID is null or empty.");
            return null;
        }

        if (inventories.TryGetValue(id, out Inventory inventory))
        {
            return inventory;
        }

        Debug.LogWarning($"Inventory not found: {id}");

        return null;
    }

    public Dictionary<string, Inventory> GetInvType(InventoryType type)
    {
        Dictionary<string, Inventory> result = new Dictionary<string, Inventory>();

        return inventories.Where(pair => pair.Value.type == type)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public int GetTotalItemCount(int itemID)
    {
        int total = 0;

        foreach (var inv in inventories.Values)
        {
            foreach (var slot in inv.slots)
            {
                if (slot.itemID == itemID)
                    total += slot.count;
            }
        }

        return total;
    }

    // Dictionary -> List 변환
    private void RefreshDebugList()
    {
        debugInventories.Clear();

        foreach (var pair in inventories)
        {
            debugInventories.Add(new InventoryDebugData
            {
                id = pair.Key,
                inventory = pair.Value
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
