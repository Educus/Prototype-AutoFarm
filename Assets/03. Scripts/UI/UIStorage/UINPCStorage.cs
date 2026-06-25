using NUnit.Framework.Internal.Execution;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    [Header("WorkMenu")]
    [SerializeField] private Transform workMenuParent;

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
    private List<UIInvenSlot> workMenuSlots = new();

    private List<UIInvenSlot> upgInvSlots = new();
    private List<UIInvenSlot> mainInvSlots = new();
    private List<UIInvenSlot> subInvSlots = new();

    private bool isRenaming;

    private List<InventorySlot> farmItems = new List<InventorySlot>();
    private List<InventorySlot> ranchItems = new List<InventorySlot>();

    #region Unity
    private void Awake()
    {
        workSlots = GetSlotList(workSlotParent);
        workMenuSlots = GetSlotList(workMenuParent);

        upgInvSlots = GetSlotList(upgInvParent);
        mainInvSlots = GetSlotList(mainInvParent);
        subInvSlots = GetSlotList(subInvParent);

        InitializeWorkSlots();

        BindButtons();

        nameInputField.onEndEdit.AddListener(OnNameEditEnd);
        nameInputField.gameObject.SetActive(false);
        workMenuParent.gameObject.SetActive(false);
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

        workMenuParent.gameObject.SetActive(false);

        Refresh();
        SlotItemsUpdate();
    }

    private void OnDisable()
    {
        if (target == null)
            return;

        target.mainInventory.OnInventoryChanged -= RefreshInventoriesMain;
        target.subInventory.OnInventoryChanged -= RefreshInventoriesSub;
        target.upgradeInventory.OnInventoryChanged -= RefreshInventoriesUpg;
    }

    private void SlotItemsUpdate()
    {
        farmItems.Clear();
        ranchItems.Clear();

        foreach (var farmItem in DataManager.Instance.itemsData.Values)
        {
            if (!farmItem.useToDemo)
                continue;

            if (farmItem.itemType == ItemType.Seed)
            {
                farmItems.Add(new InventorySlot
                {
                    itemID = farmItem.itemID + 1,
                    count = 1,
                    remainingStoragePeriod = -1
                });
            }
        }

        foreach (var ranchItem in DataManager.Instance.itemsData.Values)
        {
            if (!ranchItem.useToDemo)
                continue;

            if (ranchItem.itemName == "Milk")
            {
                ranchItems.Add(new InventorySlot
                {
                    itemID = ranchItem.itemID,
                    count = 1,
                    remainingStoragePeriod = -1
                });
            }
        }
    }
    #endregion

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

        // Work
        for (int i = 0; i < workSlots.Count; i++)
        {
            int index = i;

            Button button =
                workSlots[i].GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(
                    () => OnClickWorkSlot(index));
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

            Debug.Log(target.job.jobType);
            Debug.Log(sprite.name);
            icon.sprite = sprite;
        }
    }

    private void RefreshWorkItem()
    {
        Sprite sprite =
            DataManager.Instance.GetItemImage(
                target.job.productItemID + 1);

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
                Debug.LogWarning($"Unknown JobType: {target.job.jobType}");
                return noneSprite;
        }
    }
    #endregion

    #region WorkMenu
    private void RefreshWorkMenu()
    {
        // NPC의 작업장, 인벤토리에 따라 작업 메뉴 갱신

        // 작업 메뉴 활성화 상태에서만 갱신
        if (!workMenuParent.gameObject.activeSelf)
            return;

        Debug.Log("NPC 작업 : " + target.job.jobType);

        switch (target.job.jobType)
        {
            case JobType.None:
                // 작업 메뉴 비활성화
                ClearWorkMenu();
                break;
            case JobType.Farm:
                // 농장 작업 메뉴 갱신
                RefreshFarmWorkMenu();
                break;
            case JobType.Ranch:
                // 목장 작업 메뉴 갱신
                RefreshRanchWorkMenu();
                break;
            default:
                Debug.LogWarning($"Unknown JobType: {target.job.jobType}");
                break;
        }
    }

    private void ClearWorkMenu()
    {
        foreach (var slot in workMenuSlots)
        {
            slot.ClearSlot();
            slot.SetColor(Color.white);
            slot.GetComponent<Button>().interactable = false;
            slot.GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }
    private void RefreshFarmWorkMenu()
    {
        // 창고와 현 로봇의 Sub인벤토리를 검사하여 농장 작업 메뉴에 필요한 씨앗 아이템 ID 목록 생성
        List<int> seedIDs = new();

        // Seed 아이템은 완제품 ID가 +1 규칙을 사용
        // UI에는 씨앗이 아닌 실제 생산품을 표시

        // 창고 검사
        foreach (var building in DataManager.Instance.BuildingManager.GetAll())
        {
            if (building.type != BuildingType.Storage)
                continue;

            if (building.inventory == null)
                continue;

            foreach (var slot in building.inventory.slots)
            {
                if (slot.itemID <= 0)
                    continue;

                ItemData item =
                    DataManager.Instance.itemsData[slot.itemID];

                if (item == null)
                    continue;

                if (item.itemType != ItemType.Seed)
                    continue;

                if (!seedIDs.Contains(slot.itemID))
                {
                    seedIDs.Add(slot.itemID);
                }
            }
        }

        // 선택된 NPC의 Sub인벤토리 검사
        if (target != null && target.subInventory != null)
        {
            foreach (var slot in target.subInventory.slots)
            {
                if (slot.itemID <= 0)
                    continue;

                ItemData item =
                    DataManager.Instance.itemsData[slot.itemID];

                if (item == null)
                    continue;

                if (item.itemType != ItemType.Seed)
                    continue;

                if (!seedIDs.Contains(slot.itemID))
                {
                    seedIDs.Add(slot.itemID);
                }
            }
        }

        Debug.Log("작업에 필요한 씨앗 아이템 ID 목록");
        foreach (var seedID in seedIDs)
        {
            Debug.Log($"씨앗 아이템 ID: {seedID}");
        }

        ClearWorkMenu();

        for (int i = 0; i < workMenuSlots.Count; i++)
        {
            if (farmItems.Count > i)
            {
                workMenuSlots[i].SetSlot(farmItems[i]);
                workMenuSlots[i].SetColor(Color.gray);

                foreach (var seedID in seedIDs)
                {
                    if (farmItems[i].itemID == seedID + 1)
                    {
                        workMenuSlots[i].SetColor(Color.white);
                        workMenuSlots[i].GetComponent<Button>().interactable = true;
                        workMenuSlots[i].GetComponent<Button>().onClick.AddListener(() => OnClickWorkMenuSlot(seedID));
                        
                        Debug.Log($"버튼 활성화 및 부여{seedID}");
                        break;
                    }
                }
            }
        }
    }
    private void RefreshRanchWorkMenu()
    {
        List<int> productIDs = new();

        foreach (string buildingID in target.job.buildingIDs)
        {
            RanchBuilding ranch =
                DataManager.Instance
                .BuildingManager
                .Get<RanchBuilding>(buildingID);

            if (ranch == null)
                continue;

            if (ranch.animals.Count == 0)
                continue;

            int productID =
                ranch.animals[0].productItemID;

            if (!productIDs.Contains(productID))
            {
                productIDs.Add(productID);
            }
        }

        ClearWorkMenu();

        for (int i = 0; i < workMenuSlots.Count; i++)
        {
            if (ranchItems.Count > i)
            {
                workMenuSlots[i].SetSlot(ranchItems[i]);
                workMenuSlots[i].SetColor(Color.gray);

                foreach (var productID in productIDs)
                {
                    if (ranchItems[i].itemID == productID)
                    {
                        workMenuSlots[i].SetColor(Color.white);
                        workMenuSlots[i].GetComponent<Button>().interactable = true;
                        workMenuSlots[i].GetComponent<Button>().onClick.AddListener(() => OnClickWorkMenuSlot(productID));
                        
                        Debug.Log($"버튼 활성화 및 부여{productID}");
                        break;
                    }
                }
            }
        }
    }
    private void SetWorkMenuItems(List<int> itemIDs)
    {
        for (int i = 0; i < workMenuSlots.Count; i++)
        {
            if (i >= itemIDs.Count)
            {
                workMenuSlots[i].ClearSlot();
                continue;
            }

            InventorySlot tempSlot = new InventorySlot
            {
                itemID = itemIDs[i],
                count = 1,
                remainingStoragePeriod = -1
            };

            workMenuSlots[i].SetSlot(tempSlot);
        }
    }

    public void OpenWorkMenu()
    {
        bool workMenuActive = workMenuParent.gameObject.activeSelf;
        workMenuParent.gameObject.SetActive(!workMenuActive);

        if (!workMenuActive)
        {
            RefreshWorkMenu();
            Debug.Log("Work Menu Opened");
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
        if (target != null)
        {
            string newName = nameInputField.text.Trim();

            if (!string.IsNullOrEmpty(newName))
            {
                target.SetName(newName);
            }
        }

        isRenaming = false;

        nameInputField.gameObject.SetActive(false);

        npcName.gameObject.SetActive(true);

        RefreshName();
    }

    // 이름 변경 완료 이벤트
    public void OnNameEditEnd(string value)
    {
        EndRename();
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

    // 작업 슬롯 버튼 연결
    public void OnClickWorkSlot(int index)
    {
        if (target == null)
            return;

        GameManager.Instance.selectedNPC = target;

        gameObject.SetActive(false);

        GameManager.Instance.EnterWorkMode();
    }

    // 작업 메뉴 슬롯 버튼 연결
    public void OnClickWorkMenuSlot(int index)
    {
        if (target == null)
            return;

        target.job.productItemID = index;
        RefreshWorkItem();
        Debug.Log($"Work Menu Slot Click : {index}");
    }
    #endregion
}