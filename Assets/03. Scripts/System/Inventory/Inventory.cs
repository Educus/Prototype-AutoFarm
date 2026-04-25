using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InventoryType
{
    Unified,    // 건물
    Rocket,     // 로켓
    Main,       // 메인 인벤
    Sub,        // 서브 인벤
    Upgrade     // 업그레이드 인벤
}

public enum SortType
{
    ItemID,     // 아이템 ID 순
    ExpiryDate  // 유통기한 순
}

public class Inventory : MonoBehaviour
{
    // 인벤토리
    public string id;
    public InventoryType type;
    public List<InventorySlot> slots = new List<InventorySlot>();

    private void OnEnable()
    {
        TimeManager.Instance.onDayEvent += OnDayPassed;
    }
    private void OnDisable()
    {
        TimeManager.Instance.onDayEvent -= OnDayPassed;
    }

    public void Initialize(int slotCount)
    {
        slots = new List<InventorySlot>();

        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    #region Add / Remove
    public int AddItem(int itemID, int amount)
    {
        var data = DataManager.Instance.itemsData[itemID];
        if (data != null) return 0;
        if (!CanAddItem(itemID)) return 0;

        if (type == InventoryType.Upgrade)
        {
            foreach (var slot in slots)
            {
                if (slot.IsEmpty())
                {
                    slot.itemID = itemID;
                    slot.count = 1;
                    slot.remainingStoragePeriodl = -1;
                    return 1;
                }
            }

            return 0; // 빈 슬롯 없음
        }

        int remaining = amount;

        // 같은 유통기한 슬릇 찾기
        foreach (var slot in slots)
        {
            if (slot.IsEmpty()) continue;

            if (slot.itemID == itemID &&
            slot.remainingStoragePeriodl == data.storagePeriod)
            {
                int canAdd = data.stack - slot.count;
                int add = Mathf.Min(canAdd, remaining);

                slot.count += add;
                remaining -= add;

                if (remaining <= 0)
                    return amount;
            }
        }

        // 빈 슬롯 사용
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty()) continue;

            int add = Mathf.Min(data.stack, remaining);

            slot.itemID = itemID;
            slot.count = add;
            slot.remainingStoragePeriodl = data.storagePeriod;
            remaining -= add;

            if (remaining <= 0)
                return amount;
        }

        // 못 넣은 양 반환
        return amount - remaining;
    }

    public int RemoveItem(int itemID, int amount)
    {
        int remaining = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];

            if (slot.itemID != itemID)
                continue;

            int remove = Mathf.Min(slot.count, remaining);
            slot.count -= remove;
            remaining -= remove;

            if (slot.count <= 0)
                slot.Clear();

            if (remaining <= 0)
                break;
        }

        return amount - remaining;
    }

    public int TakeUpTo(int itemID, int amount)
    {
        int remaining = amount;
        int taken = 0;

        var ordered = slots
            .Where(s => s.itemID == itemID)
            .OrderBy(s => GetExpiryPriority(s))
            .ToList();

        foreach (var slot in ordered)
        {
            int take = Mathf.Min(slot.count, remaining);

            slot.count -= take;
            remaining -= take;
            taken += take;

            if (slot.count <= 0)
                slot.Clear();

            if (remaining <= 0)
                break;
        }

        return taken;
    }
    #endregion

    #region Filter
    public bool CanAddItem(int itemID)
    {
        var data = DataManager.Instance.itemsData[itemID];

        switch (type)
        {
            case InventoryType.Unified:
                return true;

            case InventoryType.Main:
                return data.itemType != ItemType.Seed && 
                       data.itemType != ItemType.UpgPerk;

            case InventoryType.Sub:
                return data.itemType == ItemType.Seed;

            case InventoryType.Upgrade:
                return data.itemType == ItemType.UpgPerk && !ContainsItem(itemID);;
        }

        return false;
    }

    public bool ContainsItem(int itemID)
    {
        foreach (var slot in slots)
        {
            if (slot.itemID == itemID)
                return true;
        }
        return false;
    }
    #endregion

    #region Expiry
    void OnDayPassed()
    {
        if (type != InventoryType.Unified &&
            type != InventoryType.Main)
            return;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];
            var data = DataManager.Instance.itemsData[slot.itemID];

            if (data.itemType != ItemType.Product)
                continue;

            if (slot.remainingStoragePeriodl < 0)
                continue;

            slot.remainingStoragePeriodl--;

            if (slot.remainingStoragePeriodl <= 0)
                slots.RemoveAt(i);
        }
    }
    #endregion

    #region Sort
    public void Sort(SortType sortType)
    {
        switch (sortType)
        {
            case SortType.ItemID:
                slots = slots.OrderBy(s => s.itemID).ToList();
                break;

            case SortType.ExpiryDate:
                slots = slots
                    .OrderBy(s => GetExpiryPriority(s))
                    .ThenBy(s => s.itemID)
                    .ToList();
                break;
        }
    }

    private int GetExpiryPriority(InventorySlot slot)
    {
        var data = DataManager.Instance.itemsData[slot.itemID];

        if (data.itemType != ItemType.Product)
            return int.MaxValue;

        if (slot.remainingStoragePeriodl < 0)
            return int.MaxValue - 1;

        return slot.remainingStoragePeriodl;
    }
    #endregion

    #region Save / Load
    public InventorySaveData GetSaveData()
    {
        var data = new InventorySaveData();
        data.id = id;
        data.type = type;

        data.slots = new List<InventorySlotSaveData>();

        foreach (var slot in slots)
        {
            data.slots.Add(new InventorySlotSaveData
            {
                itemID = slot.itemID,
                count = slot.count,
                remainingDays = slot.remainingStoragePeriodl
            });
        }

        return data;
    }
    public void Load(InventorySaveData data)
    {
        id = data.id;
        type = data.type;

        slots = new List<InventorySlot>();

        foreach (var s in data.slots)
        {
            slots.Add(new InventorySlot
            {
                itemID = s.itemID,
                count = s.count,
                remainingStoragePeriodl = s.remainingDays
            });
        }
    }
    #endregion
}