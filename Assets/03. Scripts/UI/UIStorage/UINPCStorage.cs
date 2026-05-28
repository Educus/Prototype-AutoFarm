using NUnit.Framework.Internal.Execution;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINPCStorage : MonoBehaviour
{
    // NPC 클릭 상호작용시 표시되는 UI

    [Header("NPC Info")]
    [SerializeField] private TMP_Text npcName;
    [SerializeField] private TMP_InputField nameInputField;

    [Header("Work")]
    [SerializeField] private Transform workSlotParent;
    [SerializeField] private Image workItem;

    [Header("Inventory Parents")]
    [SerializeField] private Transform upgInvParent;
    [SerializeField] private Transform mainInvParent;
    [SerializeField] private Transform subInvParent;

    [Header("Work Sprites")]
    [SerializeField] private Sprite noneSprite;
    [SerializeField] private Sprite farmSprite;
    [SerializeField] private Sprite ranchSprite;

    private NPC target;

    private List<UIInvenSlot> workSlots = new();

    private List<UIInvenSlot> upgInvSlots = new();
    private List<UIInvenSlot> mainInvSlots = new();
    private List<UIInvenSlot> subInvSlots = new();

    private bool isRenaming;

    private void Awake()
    {
        workSlots = GetSlotList(workSlotParent);

        upgInvSlots = GetSlotList(upgInvParent);
        mainInvSlots = GetSlotList(mainInvParent);
        subInvSlots = GetSlotList(subInvParent);

        InitializeWorkSlots();

        BindButtons();

        nameInputField.gameObject.SetActive(false);
    }

    private void Update()
    {
        //HandleRenameInput();
    }

    private void OnEnable()
    {
        target = GameManager.Instance.selectedNPC;

        if (target == null)
            return;

        target.mainInventory.OnInventoryChanged += RefreshInventoriesMain;
        target.subInventory.OnInventoryChanged += RefreshInventoriesSub;
        target.upgradeInventory.OnInventoryChanged += RefreshInventoriesUpg;

        Refresh();
    }

    private void OnDisable()
    {
        if (target == null)
            return;

        target.mainInventory.OnInventoryChanged -= RefreshInventoriesMain;
        target.subInventory.OnInventoryChanged -= RefreshInventoriesSub;
        target.upgradeInventory.OnInventoryChanged -= RefreshInventoriesUpg;
    }

    private List<UIInvenSlot> GetSlotList(Transform parent)
    {
        List<UIInvenSlot> list = new();

        foreach (Transform child in parent)
        {
            UIInvenSlot slot =
                child.GetComponent<UIInvenSlot>();

            if (slot != null)
            {
                list.Add(slot);
            }
        }

        return list;
    }

    // Work 슬롯 초기화
    // timerImage 비활성화
    private void InitializeWorkSlots()
    {
        foreach (var slot in workSlots)
        {
            slot.ClearSlot();
        }
    }

    private void BindButtons()
    {
        // Main
        for (int i = 0; i < mainInvSlots.Count; i++)
        {
            int index = i;

            Button button =
                mainInvSlots[i].GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(
                    () => OnClickMainSlot(index));
            }
        }

        // Sub
        for (int i = 0; i < subInvSlots.Count; i++)
        {
            int index = i;

            Button button =
                subInvSlots[i].GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(
                    () => OnClickSubSlot(index));
            }
        }

        // Upgrade
        for (int i = 0; i < upgInvSlots.Count; i++)
        {
            int index = i;

            Button button =
                upgInvSlots[i].GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(
                    () => OnClickUpgradeSlot(index));
            }
        }
    }

    private void Refresh()
    {
        RefreshName();
        RefreshWorkSlots();
        RefreshWorkItem();
        RefreshInventoriesMain();
        RefreshInventoriesSub();
        RefreshInventoriesUpg();
    }

    #region Name
    private void RefreshName()
    {
        //// npc name
        npcName.text = target.GetName();
    }

    private void HandleRenameInput()
    {
        if (!nameInputField.gameObject.activeSelf)
            return;

        // Enter
        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            string newName =
                nameInputField.text.Trim();

            if (!string.IsNullOrEmpty(newName))
            {
                target.SetName(newName);
            }

            RefreshName();

            EndRename();
        }

        // 좌클릭 탈출
        if (Input.GetMouseButtonDown(0))
        {
            EndRename();
        }
    }
    #endregion

    #region Work
    private void RefreshWorkSlots()
    {
        Sprite sprite = GetWorkSprite();

        int count =
            target.job.buildingIDs.Count;

        for (int i = 0; i < workSlots.Count; i++)
        {
            bool active = i < count;

            if (!active)
            {
                workSlots[i].ClearSlot();
                continue;
            }

            // Work 슬롯은 아이템처럼 사용
            InventorySlot tempSlot =
                new InventorySlot
                {
                    itemID = -1,
                    count = 1,
                    remainingStoragePeriod = -1
                };

            workSlots[i].SetSlot(tempSlot);

            Image icon =
                workSlots[i]
                .transform.GetChild(1)
                .GetComponent<Image>();

            icon.sprite = sprite;
        }
    }

    private void RefreshWorkItem()
    {
        Sprite sprite =
            DataManager.Instance.GetItemImage(
                target.job.productItemID);

        bool hasItem = sprite != null;

        workItem.gameObject.SetActive(hasItem);

        if (hasItem)
        {
            workItem.sprite = sprite;
        }
    }

    private Sprite GetWorkSprite()
    {
        switch (target.job.jobType)
        {
            case JobType.Farm:
                return farmSprite;

            case JobType.Ranch:
                return ranchSprite;

            default:
                return noneSprite;
        }
    }
    #endregion

    #region Inventory
    private void RefreshInventoriesMain()
    {
        RefreshInventory(
            mainInvSlots,
            target.mainInventory.slots);
    }
    private void RefreshInventoriesSub()
    {
        RefreshInventory(
            subInvSlots,
            target.subInventory.slots);
    }

    private void RefreshInventoriesUpg()
    {
        RefreshInventory(
            upgInvSlots,
            target.upgradeInventory.slots);
    }


    private void RefreshInventory(List<UIInvenSlot> uiSlots, List<InventorySlot> dataSlots)
    {
        int count =
            Mathf.Min(
                uiSlots.Count,
                dataSlots.Count);

        // 슬롯 갱신
        for (int i = 0; i < count; i++)
        {
            uiSlots[i].SetSlot(dataSlots[i]);
        }

        // 남는 슬롯 초기화
        for (int i = count; i < uiSlots.Count; i++)
        {
            uiSlots[i].ClearSlot();
        }
    }

    #endregion

    #region Buttons

    // 이름 변경
    public void StartRename()
    {
        if (target == null) return;

        isRenaming = true;

        npcName.gameObject.SetActive(false);

        nameInputField.gameObject.SetActive(true);

        nameInputField.SetTextWithoutNotify("");

        nameInputField.Select();
        nameInputField.ActivateInputField();
    }
    private void EndRename()
    {
        isRenaming = false;

        nameInputField.gameObject.SetActive(false);

        npcName.gameObject.SetActive(true);

        RefreshName();
    }

    // 메인 인벤토리 슬롯 버튼 연결
    public void OnClickMainSlot(int index)
    {
        if (target == null) return;
        if (index < 0 || index >= target.mainInventory.slots.Count) return;

        InventorySlot slot =
            target.mainInventory.slots[index];

        Debug.Log($"Main Slot Click : {index}");
    }

    // 서브 인벤토리 슬롯 버튼 연결
    public void OnClickSubSlot(int index)
    {
        if (target == null) return;
        if (index < 0 || index >= target.subInventory.slots.Count) return;

        InventorySlot slot =
            target.subInventory.slots[index];

        Debug.Log($"Sub Slot Click : {index}");
    }

    // 업그레이드 인벤토리 슬롯 버튼 연결
    public void OnClickUpgradeSlot(int index)
    {
        if (target == null) return;
        if (index < 0 || index >= target.upgradeInventory.slots.Count) return;

        InventorySlot slot =
            target.upgradeInventory.slots[index];

        Debug.Log($"Upgrade Slot Click : {index}");
    }

    #endregion
}