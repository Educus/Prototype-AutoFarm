using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySlotData
{
    public Inventory inventory;
    public InventorySlot slot;
}

public class UIBuildingStorage : MonoBehaviour
{
    // 로켓 판매 부분
    // 모든 창고를 한번에 보여주고 상호작용
    [SerializeField] GameObject slotPrefab;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Transform content;

    private List<UIInvenSlot> slotList = new List<UIInvenSlot>();
    private Inventory rocketInv;

    // 지금 보여주는 창고 인벤토리
    private List<Inventory> subscribedInventories = new List<Inventory>();
    private List<DisplaySlotData> displaySlots = new List<DisplaySlotData>();

    private string rocketID = "Building_-101_0";

    private void Awake()
    {
        if (!DataManager.Instance.InventoryManager.inventories
            .TryGetValue(rocketID, out rocketInv))
        {
            Debug.LogError("Rocket inventory not found.");
        }
    }

    private void OnEnable()
    {
        scrollRect.verticalNormalizedPosition = 1f;

        SubscribeInventories();

        if (subscribedInventories.Count > 0)
        {
            RefreshUI();
        }
    }

    private void OnDisable()
    {
        UnsubscribeInventories();
    }

    #region Subscribe
    private void SubscribeInventories()
    {
        UnsubscribeInventories();

        string target = UIStorageManagement.Instance.targetBuilding;

        Debug.Log($"target : {target}");
        Debug.Log($"IsTarget: {target == rocketID}");

        // null 또는 빈 문자열 방어
        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("Target building is null or empty.");
            return;
        }

        // 로켓이면 전체 창고 구독
        if (target == rocketID)
        {
            Dictionary<string, Inventory> storages =
                DataManager.Instance.InventoryManager.GetInvType(InventoryType.Unified);

            Debug.Log($"Storages count: {storages.Count}");

            foreach (var inventory in storages.Values)
            {
                if (inventory == null)
                    continue;

                inventory.OnInventoryChanged += RefreshUI;
                subscribedInventories.Add(inventory);
            }
        }
        // 특정 창고 구독
        else
        {
            Inventory inventory =
                DataManager.Instance.InventoryManager.Get(target);

            Debug.Log($"Storages count: {inventory.slots.Count}");

            // inventory 못 찾았을 때 방어
            if (inventory == null)
            {
                Debug.LogWarning($"Inventory not found: {target}");
                return;
            }

            inventory.OnInventoryChanged += RefreshUI;
            subscribedInventories.Add(inventory);
        }
    }

    private void UnsubscribeInventories()
    {
        foreach (var inven in subscribedInventories)
        {
            if (inven != null)
                inven.OnInventoryChanged -= RefreshUI;
        }

        subscribedInventories.Clear();
    }
    #endregion

    #region Refresh UI

    private void RefreshUI()
    {
        BuildDisplaySlots();

        int requiredSlotCount = displaySlots.Count;

        EnsureSlotCount(requiredSlotCount);

        foreach (var slot in slotList)
        {
            slot.ClearSlot();
        }

        ActiveSlot(requiredSlotCount);

        ViewItems();
    }

    // 현재 필요한 슬롯 개수 계산
    private int GetRequiredSlotCount()
    {
        // 모든 창고 표시
        if (UIStorageManagement.Instance.targetBuilding == rocketID)
        {
            Dictionary<string, Inventory> storages =
                DataManager.Instance.InventoryManager.GetInvType(InventoryType.Unified);

            int total = 0;

            foreach (var inventory in storages.Values)
            {
                total += inventory.slots.Count;
            }

            Debug.Log($"total : {total}");
            return total;
        }

        // 특정 창고
        Inventory storage =
            DataManager.Instance.InventoryManager.Get(
                UIStorageManagement.Instance.targetBuilding);

        Debug.Log($"storage slots : {storage.slots.Count}");
        return storage.slots.Count;
    }

    // 슬롯 부족하면 생성
    private void EnsureSlotCount(int requiredCount)
    {
        int currentCount = slotList.Count;

        if (currentCount >= requiredCount)
            return;

        int createCount = requiredCount - currentCount;

        for (int i = 0; i < createCount; i++)
        {
            GameObject slotObj =
                Instantiate(slotPrefab, content);

            UIInvenSlot slot =
                slotObj.GetComponent<UIInvenSlot>();

            slotList.Add(slot);

            int index = slotList.Count - 1;

            slotObj.GetComponent<Button>()
                .onClick
                .AddListener(() => MoveItemToRocket(index));
        }
    }

    // 필요한 슬롯만 활성화
    private void ActiveSlot(int activeCount)
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            bool active = i < activeCount;

            slotList[i].gameObject.SetActive(active);

            // 사용하지 않는 슬롯 데이터 초기화
            if (!active)
            {
                slotList[i].ClearSlot();
            }
        }
    }

    #endregion

    #region View Items

    private void ViewItems()
    {
        for (int i = 0; i < displaySlots.Count; i++)
        {
            slotList[i].SetSlot(displaySlots[i].slot);
        }

        return;
        // 모든 창고 표시
        if (UIStorageManagement.Instance.targetBuilding == rocketID)
        {
            ViewAllStorages();
        }
        // 특정 창고 표시
        else
        {
            Inventory storage =
                DataManager.Instance.InventoryManager.Get(
                    UIStorageManagement.Instance.targetBuilding);

            ViewSingleStorage(storage);
        }
    }

    // 모든 창고 표시
    private void ViewAllStorages()
    {
        Dictionary<string, Inventory> storages =
            DataManager.Instance.InventoryManager.GetInvType(InventoryType.Unified);

        int slotIndex = 0;

        foreach (var storage in storages.Values)
        {
            for (int i = 0; i < storage.slots.Count; i++)
            {
                slotList[slotIndex].SetSlot(storage.slots[i]);

                slotIndex++;
            }
        }
    }

    // 특정 창고 표시
    private void ViewSingleStorage(Inventory storage)
    {
        for (int i = 0; i < storage.slots.Count; i++)
        {
            slotList[i].SetSlot(storage.slots[i]);
        }
    }
    #endregion

    // 로켓으로 아이템 이동
    private void MoveItemToRocket(int slotIndex)
    {
        if (!UIStorageManagement.Instance
       .RocketStorage
       .activeInHierarchy)
        {
            return;
        }

        if (slotIndex < 0 ||
            slotIndex >= displaySlots.Count)
        {
            return;
        }

        Inventory sourceInventory =
            displaySlots[slotIndex].inventory;

        InventorySlot sourceSlot =
            displaySlots[slotIndex].slot;

        if (sourceSlot == null || sourceSlot.IsEmpty())
            return;

        if (DataManager.Instance.itemsData[sourceSlot.itemID]
            .itemType != ItemType.Product)
        {
            Debug.Log("Product 타입만 이동 가능");
            return;
        }

        int added =
            rocketInv.AddItem(
                sourceSlot.itemID,
                sourceSlot.count,
                sourceSlot.remainingStoragePeriod);

        if (added > 0)
        {
            sourceSlot.count -= added;

            if (sourceSlot.count <= 0)
            {
                sourceSlot.Clear();
            }

            sourceInventory.InvokeChange();
            rocketInv.InvokeChange();
        }
    }

    // 전체 창고를 가져왔을 때
    // 어느 창고의 몇번째 슬롯인지 확인
    private bool TryGetStorageSlot(int globalIndex, out Inventory inventory, out InventorySlot slot)
    {
        inventory = null;
        slot = null;

        Dictionary<string, Inventory> storages =
            DataManager.Instance.InventoryManager
            .GetInvType(InventoryType.Unified);

        int current = 0;

        foreach (var storage in storages.Values)
        {
            if (globalIndex < current + storage.slots.Count)
            {
                int localIndex = globalIndex - current;

                inventory = storage;
                slot = storage.slots[localIndex];

                return true;
            }

            current += storage.slots.Count;
        }

        return false;
    }

    private void BuildDisplaySlots()
    {
        displaySlots.Clear();

        // 전체 창고
        if (UIStorageManagement.Instance.targetBuilding
            == rocketID)
        {
            Dictionary<string, Inventory> storages =
                DataManager.Instance.InventoryManager
                .GetInvType(InventoryType.Unified);

            foreach (var storage in storages.Values)
            {
                foreach (var slot in storage.slots)
                {
                    displaySlots.Add(new DisplaySlotData
                    {
                        inventory = storage,
                        slot = slot
                    });
                }
            }
        }
        // 특정 창고
        else
        {
            Inventory storage =
                DataManager.Instance.InventoryManager.Get(
                    UIStorageManagement.Instance.targetBuilding);

            foreach (var slot in storage.slots)
            {
                displaySlots.Add(new DisplaySlotData
                {
                    inventory = storage,
                    slot = slot
                });
            }
        }
    }

    public void SortExpiryButton(bool descending)
    {
        var validSlots = displaySlots
            .Where(s =>
                !s.slot.IsEmpty() &&
                s.slot.remainingStoragePeriod >= 0)
            .ToList();

        var invalidSlots = displaySlots
            .Where(s =>
                s.slot.IsEmpty() ||
                s.slot.remainingStoragePeriod < 0)
            .ToList();

        if (descending)
        {
            validSlots = validSlots
                .OrderByDescending(
                    s => s.slot.remainingStoragePeriod)
                .ToList();
        }
        else
        {
            validSlots = validSlots
                .OrderBy(
                    s => s.slot.remainingStoragePeriod)
                .ToList();
        }

        displaySlots = validSlots
            .Concat(invalidSlots)
            .ToList();

        ViewItems();
    }
}

    // 생성 혹은 액티브True로 변경
    #region 창고 슬롯 생성 및 활성화(이전)
    /*
    private void RefreshUI()
    {
        CreateSlot();
    }
    private void CreateSlot()
    {
        int soltCount = slotList.Count;

        if (UIStorageManagement.Instance.targetBuilding == "Rocket")
        {
            // 모든 창고
            Dictionary<string, Inventory> totalStorage = DataManager.Instance.InventoryManager.GetInvType(InventoryType.Unified);

            int totalStorageCount = totalStorage.Count * 30;

            if (totalStorageCount > soltCount)
            {
                int createCount = totalStorageCount - soltCount;
                CreateSlot(createCount);
            }
            else
            {
                ActiveSlot(totalStorageCount);
            }


            // 
            ViewItemInfo(totalStorage);
        }
        else
        {
            // 특정 창고
            ActiveSlot(30); // 기본 창고 30칸

            ViewItemInfo(DataManager.Instance.InventoryManager.Get(UIStorageManagement.Instance.targetBuilding));
        }
    }
    private void CreateSlot(int value)
    {
        for (int i = 0; i < value; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, content);

            UIInvenSlot slot = slotObj.GetComponent<UIInvenSlot>();

            slotList.Add(slot);
        }
    }

    private void ActiveSlot(int value)
    {
        // 창고에 맞는 슬롯 활성화
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < value)
            {
                slotList[i].gameObject.SetActive(true);
            }
            else
            {
                slotList[i].gameObject.SetActive(false);
            }
        }
    }

    private void ViewItemInfo(Dictionary<string, Inventory> totalStorage)
    {
        // 모든 창고 보여주기
        int slotIndex = 0;

        foreach (var storage in totalStorage.Values)
        {
            for (int i = 0; i < storage.slots.Count; i++)
            {
                slotList[slotIndex].SetSlot(storage.slots[i]);

                slotIndex++;
            }
        }
    }

    private void ViewItemInfo(Inventory storage)
    {
        // 특정 창고 보여주기
        for (int i = 0; i < storage.slots.Count; i++)
        {
            slotList[i].SetSlot(storage.slots[i]);
        }
    }
    */
    #endregion
