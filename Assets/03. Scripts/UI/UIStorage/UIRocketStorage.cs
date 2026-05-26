using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRocketStorage : MonoBehaviour
{
    // 로켓 UI 표시용
    // 판매할 물품 적재 or 구매한 물품 적재
    [SerializeField] private TMP_Text slotCount;
    [SerializeField] private TMP_Text totalGold;
    [SerializeField] private Transform slotParent;

    private List<UIInvenSlot> itemSlots = new();
    private Inventory rocketInv;
    private int sort = 0;

    private void Awake()
    {
        itemSlots = new List<UIInvenSlot>(
            slotParent.GetComponentsInChildren<UIInvenSlot>(true));
    }

    private void Start()
    {
        if (!DataManager.Instance.InventoryManager.inventories.TryGetValue("Building_-101_0", out rocketInv))
        {
            Debug.LogError("Rocket inventory not found.");
            return;
        }

        if (rocketInv == null)
        {
            Debug.LogError("Rocket inventory is null.");
            return;
        }

        rocketInv.OnInventoryChanged += RefreshUI;

        RefreshUI();

        for (int i = 0; i < itemSlots.Count; i++)
        {
            int index = i;

            if (itemSlots[index] == null)
            {
                Debug.LogError($"Item Slot {index} is null.");
                continue;
            }

            itemSlots[index]
                .GetComponent<Button>()
                .onClick
                .AddListener(() => MoveItemButton(index));
        }
    }

    private void OnEnable()
    {
        if (rocketInv != null)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        // 로켓이 열렸을 때 UI 표시
        ViewItems();
        ViewText();
    }

    private void ViewText()
    {
        int count = 0;
        int gold = 0;

        foreach (var slot in rocketInv.slots)
        {
            if (slot.itemID == 0 || slot.count <= 0)
                continue;

            count++;

            var itemData =
                DataManager.Instance.itemsData[slot.itemID];

            // Product만 판매 가격 계산
            if (itemData.itemType == ItemType.Product)
            {
                if (DataManager.Instance.productClosingData
                    .TryGetValue(slot.itemID, out var closingData))
                {
                    if (closingData.productsClosingPrice.Count > 0)
                    {
                        gold +=
                            closingData.productsClosingPrice[0]
                            * slot.count;
                    }
                }
            }
        }

        slotCount.text = $"{count}/16";
        totalGold.text = gold.ToString();
    }
    // 로켓에 적재된 아이템 표시
    private void ViewItems()
    {
        if (rocketInv == null)
            return;

        int count = Mathf.Min(itemSlots.Count, rocketInv.slots.Count);

        for (int i = 0; i < count; i++)
        {
            if (itemSlots[i] == null)
            {
                Debug.LogWarning($"itemSlots[{i}] is null");
                continue;
            }

            itemSlots[i].SetSlot(rocketInv.slots[i]);
        }
    }

    // 로켓 내부 아이템을 창고로 이동시키는 버튼
    public void MoveItemButton(int value)
    {
        Dictionary<string, Inventory> storageInvs = DataManager.Instance.InventoryManager.GetInvType(InventoryType.Unified);

        // 해당 칸의 아이템
        InventorySlot rocketSlot = rocketInv.slots[value];

        int itemID = rocketSlot.itemID;
        int remaining = rocketSlot.count;
        int storagePeriod = rocketSlot.remainingStoragePeriod;

        // 아이템이 없거나 갯수가 0 이하인 경우
        if (itemID == 0 || remaining <= 0)
            return;

        // 창고에 순서대로 넣기
        foreach (var inven in storageInvs)
        {
            if (remaining <= 0)
                break;

            int added = inven.Value.AddItem(itemID, remaining, storagePeriod);

            remaining -= added;
        }

        // 실제 이동된 양
        int moved = rocketSlot.count - remaining;

        rocketSlot.count = remaining;

        if (rocketSlot.count <= 0)
        {
            rocketSlot.Clear();
        }

        RefreshUI();
    }

    // 준비 완료 버튼
    public void Ready()
    {
        // 용도??
        // 이후 수정
    }

    // 정렬
    // 유통기한 순, 갯수 순
    public void SortingButton(int value)
    {
        if (sort == value) value += 1;
        sort = value;

        switch (value)
        {
            case 0:
            case 1:
                SortPeriodInv(false);
                break;
            case 2:
                SortPeriodInv(true);
                break;
            case 3:
                SortNumInv(false);
                break;
            case 4:
                SortNumInv(true);
                break;
            default:
                SortPeriodInv(false);
                break;
        }
    }

    // 유통기한 순
    private void SortPeriodInv(bool descending)
    {
        rocketInv.slots.Sort((a, b) =>
        {
            // 빈 슬롯은 아래로
            bool aEmpty = a.IsEmpty();
            bool bEmpty = b.IsEmpty();

            if (aEmpty && bEmpty) return 0;
            if (aEmpty) return 1;
            if (bEmpty) return -1;

            // 유통기한 비교
            int compare = a.remainingStoragePeriod.CompareTo(b.remainingStoragePeriod);

            return descending ? -compare : compare;
        });

        ViewItems();
    }

    // 갯수 순
    private void SortNumInv(bool descending)
    {
        // itemID별 총 수량 계산
        Dictionary<int, int> totalCounts = new Dictionary<int, int>();

        foreach (var slot in rocketInv.slots)
        {
            if (slot.IsEmpty())
                continue;

            if (!totalCounts.ContainsKey(slot.itemID))
                totalCounts[slot.itemID] = 0;

            totalCounts[slot.itemID] += slot.count;
        }

        rocketInv.slots.Sort((a, b) =>
        {
            bool aEmpty = a.IsEmpty();
            bool bEmpty = b.IsEmpty();

            if (aEmpty && bEmpty) return 0;
            if (aEmpty) return 1;
            if (bEmpty) return -1;

            int aTotal = totalCounts[a.itemID];
            int bTotal = totalCounts[b.itemID];

            int compare = aTotal.CompareTo(bTotal);

            return descending ? -compare : compare;
        });

        ViewItems();
    }
}
