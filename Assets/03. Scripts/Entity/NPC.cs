using System;
using UnityEngine;


public class UpgPerk
{
    // 메인 인벤토리 레벨
    int mainInvLv = 0;
    // 서브 인벤토리 레벨
    int subInvLv = 0;
    // 스피드 레벨
    int speedLv = 0;
    // ???
    int efficLv = 0;
}
public class NPC : StatusBase
{
    // NPC
    public string id;
    public int water;
    public int maxWater;

    public Inventory mainInventory;
    public Inventory subInventory;
    public Inventory upgradeInventory;

    private void Start()
    {
        mainInventory.Initialize(5);
        subInventory.Initialize(3);
        upgradeInventory.Initialize(4);

        DataManager.Instance.NPCManager.Register(this);

        InitializeInventories();
    }

    // 상호작용
    public override void OnInteract()
    {
        Debug.Log($"{entityName} 상호작용");
    }

    private void InitializeInventories()
    {
        // ID는 반드시 고유해야 함 (Save/Load 기준)
        mainInventory.id = $"{id}_main";
        subInventory.id = $"{id}_sub";
        upgradeInventory.id = $"{id}_upgrade";

        mainInventory.type = InventoryType.Main;
        subInventory.type = InventoryType.Sub;
        upgradeInventory.type = InventoryType.Upgrade;

        // 슬롯 초기화 (기본값)
        if (mainInventory.slots.Count == 0)
            mainInventory.Initialize(12);

        if (subInventory.slots.Count == 0)
            subInventory.Initialize(3);

        if (upgradeInventory.slots.Count == 0)
            upgradeInventory.Initialize(5);
    }

    #region 업그레이드 관리
    public bool HasUpgrade(int itemID)
    {
        return upgradeInventory.ContainsItem(itemID);
    }
    public bool AddUpgrade(int itemID)
    {
        return upgradeInventory.AddItem(itemID, 1) > 0;
    }
    public void RemoveUpgrade(int itemID)
    {
        upgradeInventory.TakeUpTo(itemID, 1);
    }
    #endregion

    #region 물 관리
    public void AddWater(int amount)
    {
        water = Mathf.Min(water + amount, maxWater);
    }

    public bool UseWater(int amount)
    {
        if (water < amount)
            return false;

        water -= amount;
        return true;
    }
    #endregion

    #region 인벤토리 접근 방식
    public int TakeItemFromInventory(int itemID, int amount)
    {
        // 메인 → 서브 순으로 가져감
        int taken = mainInventory.TakeUpTo(itemID, amount);

        if (taken < amount)
        {
            taken += subInventory.TakeUpTo(itemID, amount - taken);
        }

        return taken;
    }

    public int AddItemToInventory(int itemID, int amount)
    {
        int added = mainInventory.AddItem(itemID, amount);

        if (added < amount)
        {
            added += subInventory.AddItem(itemID, amount - added);
        }

        return added;

    }
    #endregion


    #region Save / Load
    public NPCSaveData GetSaveData()
    {
        return new NPCSaveData
        {
            id = id,
            entityName = entityName,
            position = transform.position,
            water = water,
            maxWater = maxWater,

            mainInventory = mainInventory.GetSaveData(),
            subInventory = subInventory.GetSaveData(),
            upgradeInventory = upgradeInventory.GetSaveData()
        };
    }
    public void Load(NPCSaveData data)
    {
        id = data.id;
        entityName = data.entityName;
        transform.position = data.position;

        water = data.water;
        maxWater = data.maxWater;

        InitializeInventories();

        mainInventory.Load(data.mainInventory);
        subInventory.Load(data.subInventory);
        upgradeInventory.Load(data.upgradeInventory);
    }
    #endregion
}
