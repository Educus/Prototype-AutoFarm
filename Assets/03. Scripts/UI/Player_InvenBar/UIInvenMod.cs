using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvenMod : MonoBehaviour
{
    [SerializeField] private Transform subInvenParent;
    [SerializeField] private Transform mainInvenParent;

    [SerializeField] private RectTransform selectImage;

    private Inventory subInven;
    private Inventory mainInven;

    private List<UIInvenSlot> subItemSlots = new();
    private List<UIInvenSlot> mainItemSlots = new();

    public int selectedSubSlotIndex { get; private set; } = -1;

    private void Awake()
    {
        for (int i = 0; i < subInvenParent.childCount; i++)
        {
            UIInvenSlot slot = 
                subInvenParent.GetChild(i).GetComponent<UIInvenSlot>();

            if (slot == null)
                continue;

            subItemSlots.Add(slot);

            Button button = slot.GetComponent<Button>();

            if (button != null)
            {
                int index = i;

                button.onClick.AddListener(() =>
                {
                    OnSubSlotClicked(index);
                });
            }
        }

        for (int i = 0; i < mainInvenParent.childCount; i++)
        {
            UIInvenSlot slot =
                mainInvenParent.GetChild(i).GetComponent<UIInvenSlot>();

            if (slot == null)
                continue;

            mainItemSlots.Add(slot);

            Button button = slot.GetComponent<Button>();

            if (button != null)
            {
                int index = i;

                button.onClick.AddListener(() =>
                {
                    OnMainSlotClicked(index);
                });
            }
        }
    }

    private void Start()
    {
        StartCoroutine(IEStart());
    }

    private IEnumerator IEStart()
    {
        while (GameManager.Instance == null || GameManager.Instance.player == null)
        {
            yield return null;
        }

        mainInven = GameManager.Instance.player.mainInventory;
        subInven = GameManager.Instance.player.subInventory;

        mainInven.OnInventoryChanged += RefreshMainUI;
        subInven.OnInventoryChanged += RefreshSubUI;

        RefreshMainUI();
        RefreshSubUI();

        SelectSubSlot(0);
    }

    private void OnDestroy()
    {
        if (mainInven != null)
            mainInven.OnInventoryChanged -= RefreshMainUI;

        if (subInven != null)
            subInven.OnInventoryChanged -= RefreshSubUI;
    }

    private void RefreshMainUI()
    {
        RefreshInventory(mainInven, mainItemSlots);
    }
    
    private void RefreshSubUI()
    {
        RefreshInventory(subInven, subItemSlots);
    }

    private void RefreshInventory(Inventory inven, List<UIInvenSlot> uiSlots)
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (i < inven.slots.Count)
                uiSlots[i].SetSlot(inven.slots[i]);
            else
                uiSlots[i].ClearSlot();
        }
    }

    public void SelectSubSlot(int index)
    {
        if (index < 0 || index >= subItemSlots.Count)
            return;

        selectedSubSlotIndex = index;

        MoveSelectImage();
    }

    private void MoveSelectImage()
    {
        RectTransform slotRect =
            subItemSlots[selectedSubSlotIndex].GetComponent<RectTransform>();

        selectImage.position = slotRect.position;
    }

    private void OnMainSlotClicked(int slotIndex)
    {
        if (GameManager.Instance.CurrentMode != GameMode.Popup)
            return;

        string target = UIStorageManagement.Instance.targetBuilding;

        if (string.IsNullOrEmpty(target))
            return;

        Inventory targetInv =
            DataManager.Instance.InventoryManager.Get(target);

        if (targetInv == null)
            return;

        if (targetInv.type != InventoryType.Unified)
            return;

        // 해당 슬롯
        InventorySlot mainSlot = mainInven.slots[slotIndex];

        int itemID = mainSlot.itemID;
        int remaining = mainSlot.count;
        int storagePeriod = mainSlot.remainingStoragePeriod;

        if (itemID == 0 || remaining <= 0)
            return;

        // 창고에 넣기
        int added = targetInv.AddItem(
            itemID,
            remaining,
            storagePeriod);

        remaining -= added;

        mainSlot.count = remaining;

        if (mainSlot.count <= 0)
        {
            mainSlot.Clear();
        }

        mainInven.InvokeChange();
    }

    private void OnSubSlotClicked(int slotIndex)
    {
        if (GameManager.Instance.CurrentMode != GameMode.Popup)
            return;

        string target = UIStorageManagement.Instance.targetBuilding;

        if (string.IsNullOrEmpty(target))
            return;

        Inventory targetInv =
            DataManager.Instance.InventoryManager.Get(target);

        if (targetInv == null)
            return;

        if (targetInv.type != InventoryType.Unified)
            return;

        // 해당 슬롯
        InventorySlot subSlot = subInven.slots[slotIndex];

        int itemID = subSlot.itemID;
        int remaining = subSlot.count;
        int storagePeriod = subSlot.remainingStoragePeriod;
        if (itemID == 0 || remaining <= 0)
            return;

        // 창고에 넣기
        int added = targetInv.AddItem(
            itemID,
            remaining,
            storagePeriod);

        remaining -= added;

        subSlot.count = remaining;

        if (subSlot.count <= 0)
        {
            subSlot.Clear();
        }

        subInven.InvokeChange();
    }
}
