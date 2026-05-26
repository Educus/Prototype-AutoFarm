using System;
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

    public event Action OnInventoryChanged;

    private void OnEnable()
    {
        TimeManager.Instance.onDayEvent += OnDayPassed;
    }
    private void OnDisable()
    {
        TimeManager.Instance.onDayEvent -= OnDayPassed;
    }

    public void InvokeChange()
    {
        OnInventoryChanged?.Invoke();
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
    public int AddNewItem(int itemID)
    {
        return 0;
    }

    public int AddItem(int itemID, int amount, int storagePeriod)
    {
        var data = DataManager.Instance.itemsData[itemID];
        if (data == null) return 0; // if (data != null) return 0; <= ?
        if (!CanAddItem(itemID)) return 0;

        storagePeriod = (storagePeriod == -1) ? data.storagePeriod : storagePeriod;

        if (type == InventoryType.Upgrade)
        {
            foreach (var slot in slots)
            {
                if (slot.IsEmpty())
                {
                    slot.itemID = itemID;
                    slot.count = 1;
                    slot.remainingStoragePeriod = -1;
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
            slot.remainingStoragePeriod == storagePeriod)
            {
                int canAdd = data.stack - slot.count;
                int add = Mathf.Min(canAdd, remaining);

                slot.count += add;
                remaining -= add;

                if (remaining <= 0)
                {
                    InvokeChange();
                    return amount;
                }
            }
        }

        // 빈 슬롯 사용
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty()) continue;

            int add = Mathf.Min(data.stack, remaining);

            slot.itemID = itemID;
            slot.count = add;
            slot.remainingStoragePeriod = storagePeriod;
            remaining -= add;

            if (remaining <= 0)
            {
                InvokeChange();
                return amount;
            }
        }

        // 못 넣은 양 반환
        int added = amount - remaining;

        if (added > 0)
            InvokeChange();

        return added;
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

        int removed = amount - remaining;

        if (removed > 0)
            InvokeChange();

        return removed;
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

        if (taken > 0)
            InvokeChange();

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

            case InventoryType.Rocket:
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
        bool changed = false;

        if (type != InventoryType.Unified &&
            type != InventoryType.Main)
            return;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            var slot = slots[i];

            if (slot.remainingStoragePeriod < 0)
                continue;

            var data = DataManager.Instance.itemsData[slot.itemID];

            if (data.itemType != ItemType.Product)
                continue;

            slot.remainingStoragePeriod--;
            changed = true;

            if (slot.remainingStoragePeriod <= 0)
            {
                slot.Clear();
                changed = true;
            }

            if (changed)
                InvokeChange();
        }
    }
    #endregion

    #region Sort
    // 기본 정렬
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

        InvokeChange();
    }

    // 역순
    public void SortExpiry(bool descending)
    {
        var validSlots = slots
            .Where(s => !s.IsEmpty() &&
                   s.remainingStoragePeriod >= 0)
            .ToList();

        var invalidSlots = slots
            .Where(s => s.IsEmpty() ||
                   s.remainingStoragePeriod < 0)
            .ToList();

        if (descending)
        {
            validSlots = validSlots
                .OrderByDescending(s => s.remainingStoragePeriod)
                .ToList();
        }
        else
        {
            validSlots = validSlots
                .OrderBy(s => s.remainingStoragePeriod)
                .ToList();
        }

        slots = validSlots
            .Concat(invalidSlots)
            .ToList();

        InvokeChange();
    }

    private int GetExpiryPriority(InventorySlot slot)
    {
        // 빈 슬롯은 항상 맨 아래
        if (slot.IsEmpty())
            return int.MaxValue;

        var data =
            DataManager.Instance.itemsData[slot.itemID];

        // Product가 아닌 경우 맨 아래
        if (data.itemType != ItemType.Product)
            return int.MaxValue - 1;

        // 유통기한 없는 경우 맨 아래
        if (slot.remainingStoragePeriod < 0)
            return int.MaxValue - 2;

        return slot.remainingStoragePeriod;
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
                remainingDays = slot.remainingStoragePeriod
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
                remainingStoragePeriod = s.remainingDays
            });
        }

        InvokeChange();
    }
    #endregion
}