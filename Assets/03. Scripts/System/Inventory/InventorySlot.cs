using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    // 인벤토리 슬롯
    // 아이템 아이디와 개수를 저장
    // 아이템의 남은 보관기간 저장

    public int itemId;
    public int count;
    public int remainingStoragePeriodl = -1;

    public InventorySlot(int itemId, int count, int remainingStoragePeriodl)
    {
        this.itemId = itemId;
        this.count = count;
        this.remainingStoragePeriodl = remainingStoragePeriodl;
    }

    public bool IsEmpty()
    {
        return itemId <= 0;
    }

    public void Clear()
    {
        itemId = 0;
        count = 0;
    }

    
}
