using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// UI에 표시할 슬롯 정보
// 실제 슬롯과 어느 인벤토리에 속하는지 저장
public class DisplaySlotData
{
    public Inventory inventory;
    public InventorySlot slot;
}

public class UIBuildingStorage : MonoBehaviour
{
    // 슬롯 프리팹
    [SerializeField] private GameObject slotPrefab;

    // 스크롤 위치 초기화용
    [SerializeField] private ScrollRect scrollRect;

    // 슬롯이 생성될 부모 오브젝트
    [SerializeField] private Transform content;

    // 생성된 UI 슬롯 목록
    private readonly List<UIInvenSlot> slotList =
        new List<UIInvenSlot>();

    // 현재 구독중인 인벤토리 목록
    // UI 닫을 때 이벤트 해제용
    private readonly List<Inventory> subscribedInventories =
        new List<Inventory>();

    // 화면에 표시중인 슬롯 데이터
    // 단일창고 / 전체창고 모두 동일하게 처리
    private readonly List<DisplaySlotData> displaySlots =
        new List<DisplaySlotData>();

    // 로켓 창고 인벤토리
    private Inventory rocketInv;

    // 기본 생성 슬롯 수
    private const int DEFAULT_SLOT_COUNT = 30;

    // 로켓 창고 ID
    private const string rocketID =
        "Building_-101_0";

    private void Awake()
    {
        // 로켓 창고 찾기
        DataManager.Instance.InventoryManager
            .inventories
            .TryGetValue(rocketID, out rocketInv);

        // 최초 슬롯 30개 생성
        CreateDefaultSlots();
    }

    private void OnEnable()
    {
        // 스크롤 맨 위로 이동
        scrollRect.verticalNormalizedPosition = 1f;

        // 현재 창고 이벤트 등록
        SubscribeInventories();

        // UI 갱신
        RefreshUI();
    }

    private void OnDisable()
    {
        // 이벤트 해제
        UnsubscribeInventories();
    }

    #region Slot Create

    // 최초 슬롯 생성
    private void CreateDefaultSlots()
    {
        for (int i = 0; i < DEFAULT_SLOT_COUNT; i++)
        {
            CreateSlot();
        }
    }

    // 슬롯 하나 생성
    private void CreateSlot()
    {
        GameObject obj =
            Instantiate(slotPrefab, content);

        UIInvenSlot slot =
            obj.GetComponent<UIInvenSlot>();

        slotList.Add(slot);

        // 슬롯 인덱스 저장
        int index = slotList.Count - 1;

        Button button =
            obj.GetComponent<Button>();

        // 버튼 클릭 등록
        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                OnSlotClicked(index);
            });
        }
    }

    // 필요한 슬롯 수 만큼 생성
    // 전체 창고 표시 시 사용
    private void EnsureSlotCount(int requiredCount)
    {
        while (slotList.Count < requiredCount)
        {
            CreateSlot();
        }
    }

    #endregion

    #region Subscribe

    // 현재 열려있는 창고 이벤트 등록
    private void SubscribeInventories()
    {
        UnsubscribeInventories();

        string target =
            UIStorageManagement.Instance.targetBuilding;

        if (string.IsNullOrEmpty(target))
            return;

        // 로켓 창고
        // 전체 창고 표시
        if (target == rocketID)
        {
            Dictionary<string, Inventory> storages =
                DataManager.Instance.InventoryManager
                .GetInvType(InventoryType.Unified);

            foreach (Inventory inventory in storages.Values)
            {
                inventory.OnInventoryChanged += RefreshUI;

                subscribedInventories.Add(inventory);
            }
        }
        // 일반 창고
        else
        {
            Inventory inventory =
                DataManager.Instance.InventoryManager
                .Get(target);

            if (inventory == null)
                return;

            inventory.OnInventoryChanged += RefreshUI;

            subscribedInventories.Add(inventory);
        }
    }

    // 이벤트 해제
    private void UnsubscribeInventories()
    {
        foreach (Inventory inventory in subscribedInventories)
        {
            if (inventory != null)
            {
                inventory.OnInventoryChanged -= RefreshUI;
            }
        }

        subscribedInventories.Clear();
    }

    #endregion

    #region Refresh UI

    // UI 전체 갱신
    private void RefreshUI()
    {
        // 표시할 슬롯 목록 생성
        BuildDisplaySlots();

        // 슬롯이 부족하면 생성
        EnsureSlotCount(displaySlots.Count);

        // 기존 슬롯 초기화
        ClearAllSlots();

        // 필요한 슬롯만 활성화
        ActiveSlots(displaySlots.Count);

        // 슬롯 정보 표시
        for (int i = 0; i < displaySlots.Count; i++)
        {
            slotList[i].SetSlot(displaySlots[i].slot);
        }
    }

    // 모든 슬롯 비우기
    private void ClearAllSlots()
    {
        foreach (UIInvenSlot slot in slotList)
        {
            slot.ClearSlot();
        }
    }

    // 필요한 슬롯만 활성화
    private void ActiveSlots(int activeCount)
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            bool active = i < activeCount;

            slotList[i].gameObject.SetActive(active);

            if (!active)
            {
                slotList[i].ClearSlot();
            }
        }
    }

    // 현재 화면에 표시할 슬롯 구성
    private void BuildDisplaySlots()
    {
        displaySlots.Clear();

        string target =
            UIStorageManagement.Instance.targetBuilding;

        // 전체 창고 표시
        if (target == rocketID)
        {
            Dictionary<string, Inventory> storages =
                DataManager.Instance.InventoryManager
                .GetInvType(InventoryType.Unified);

            foreach (Inventory storage in storages.Values)
            {
                foreach (InventorySlot slot in storage.slots)
                {
                    displaySlots.Add(
                        new DisplaySlotData
                        {
                            inventory = storage,
                            slot = slot
                        });
                }
            }
        }
        // 단일 창고 표시
        else
        {
            Inventory storage =
                DataManager.Instance.InventoryManager
                .Get(target);

            if (storage == null)
                return;

            foreach (InventorySlot slot in storage.slots)
            {
                displaySlots.Add(
                    new DisplaySlotData
                    {
                        inventory = storage,
                        slot = slot
                    });
            }
        }
    }

    #endregion

    #region Slot Click

    // 슬롯 클릭
    private void OnSlotClicked(int slotIndex)
    {
        if (slotIndex >= displaySlots.Count)
            return;

        // 로켓 창고 UI
        // 클릭 시 로켓으로 이동
        if (UIStorageManagement.Instance.targetBuilding
            == rocketID)
        {
            MoveItemToRocket(slotIndex);
        }
        // 일반 창고 UI
        // 클릭 시 플레이어로 이동
        else
        {
            MoveItemToPlayer(slotIndex);
        }
    }

    #endregion

    #region Move Item

    // 창고 → 플레이어 인벤토리
    private void MoveItemToPlayer(int slotIndex)
    {
        DisplaySlotData data =
            displaySlots[slotIndex];

        Inventory sourceInventory =
            data.inventory;

        InventorySlot sourceSlot =
            data.slot;

        if (sourceSlot == null ||
            sourceSlot.IsEmpty())
        {
            return;
        }

        Player player =
            GameManager.Instance.player;

        if (player == null)
            return;

        ItemData itemData =
            DataManager.Instance.itemsData[sourceSlot.itemID];

        Inventory targetInventory;

        // 씨앗은 서브 인벤
        if (itemData.itemType == ItemType.Seed)
        {
            targetInventory =
                player.subInventory;
        }
        else
        {
            targetInventory =
                player.mainInventory;
        }

        int added =
            targetInventory.AddItem(
                sourceSlot.itemID,
                sourceSlot.count,
                sourceSlot.remainingStoragePeriod);

        if (added <= 0)
            return;

        sourceSlot.count -= added;

        if (sourceSlot.count <= 0)
        {
            sourceSlot.Clear();
        }

        sourceInventory.InvokeChange();
        targetInventory.InvokeChange();
    }

    // 창고 → 로켓 창고
    private void MoveItemToRocket(int slotIndex)
    {
        DisplaySlotData data =
            displaySlots[slotIndex];

        Inventory sourceInventory =
            data.inventory;

        InventorySlot sourceSlot =
            data.slot;

        if (sourceSlot == null ||
            sourceSlot.IsEmpty())
        {
            return;
        }

        // Product 타입만 이동 가능
        if (DataManager.Instance.itemsData[sourceSlot.itemID]
            .itemType != ItemType.Product)
        {
            return;
        }

        int added =
            rocketInv.AddItem(
                sourceSlot.itemID,
                sourceSlot.count,
                sourceSlot.remainingStoragePeriod);

        if (added <= 0)
            return;

        sourceSlot.count -= added;

        if (sourceSlot.count <= 0)
        {
            sourceSlot.Clear();
        }

        sourceInventory.InvokeChange();
        rocketInv.InvokeChange();
    }

    #endregion
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
