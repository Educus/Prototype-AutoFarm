using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Dictionary<string, Inventory> inventories =
        new Dictionary<string, Inventory>();

    public void Register(Inventory inv)
    {
        if (!inventories.ContainsKey(inv.id))
            inventories.Add(inv.id, inv);
    }

    public Inventory Get(string id)
    {
        return inventories[id];
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
}
