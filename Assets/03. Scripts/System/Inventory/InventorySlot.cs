using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    // 인벤토리 슬롯

    public int itemID;
    public int count;
    public int remainingStoragePeriodl = -1;

    public bool IsEmpty()
    {
        return itemID == 0 || count <= 0;
    }

    public void Clear()
    {
        itemID = 0;
        count = 0;
        remainingStoragePeriodl = -1;
    }
}
