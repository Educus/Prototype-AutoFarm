using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    // 인벤토리 슬롯
    // 아이템 아이디와 개수를 저장

    public int itemId;
    public int count;

    public InventorySlot(int itemId, int count)
    {
        this.itemId = itemId;
        this.count = count;
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
