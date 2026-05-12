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
    [SerializeField] private List<UIInvenSlot> itemSlots;

    private Inventory rocketInv;

    private void Start()
    {
        rocketInv = DataManager.Instance.InventoryManager.inventories["Rocket"];

        for(int i = 0; i < itemSlots.Count; i++)
        {
            int index = i;

            itemSlots[index].GetComponent<Button>().onClick.AddListener(() => MoveItemButton(i));
        }
    }

    private void Update()
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
            if (slot.itemID == 0 || slot.count <= 0) return;

            count ++;
            gold += DataManager.Instance.productClosingData[slot.itemID].productsClosingPrice[0] * slot.count;
        }
        slotCount.text = $"{count}/16";
        totalGold.text = gold.ToString();
    }
    // 로켓에 적재된 아이템 표시
    private void ViewItems()
    {
        for (int i = 0; i < rocketInv.slots.Count; i++)
        {
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

        ViewItems();
    }

    // 준비 완료 버튼
    public void Ready()
    {
        // 용도??
        // 이후 수정
    }
}
