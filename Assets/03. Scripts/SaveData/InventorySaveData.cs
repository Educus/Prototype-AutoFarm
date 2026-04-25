using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public string id;
    public InventoryType type;

    public List<InventorySlotSaveData> slots;
}

[Serializable]
public class InventorySlotSaveData
{
    public int itemID;
    public int count;
    public int remainingDays;
}