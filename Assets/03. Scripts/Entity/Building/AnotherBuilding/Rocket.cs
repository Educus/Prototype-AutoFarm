using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class Rocket : BuildingBase
{
    [Header("UI")]
    [SerializeField] private UIStorageManagement uiStorageManagement;

    [Header("Rocket")]
    [SerializeField] private int defaultSlotCount = 16;

    // 구매 예약 아이템
    public Dictionary<int, int> buyItems = new();

    private int money = 0;

    // true = 로켓 비어있음
    // false = 구매 아이템 남아있음
    private bool isInvenClear = true;

    #region Unity

    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Rocket;
    }

    private void Start()
    {
        uiStorageManagement = UIStorageManagement.Instance;
    }

    private void Update()
    {
        int hour = TimeManager.Instance.currentHour;
        int minute = TimeManager.Instance.currentMinute;

        // 00:30 정산
        if (hour == 0 && minute == 30)
        {
            Calculate();
        }

        // 23:30 판매
        if (hour == 23 && minute == 30)
        {
            SellInv();
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= CheckInvenClear;
        }
    }

    #endregion

    #region Initialize

    public override void Initialize()
    {
        // 건물 등록
        base.Initialize();

        if (inventory == null)
        {
            Debug.LogError($"Rocket Inventory Missing : {gameObject.name}");
            return;
        }

        // 인벤토리 설정
        inventory.id = id;
        inventory.type = InventoryType.Rocket;

        // 처음 생성 시만 슬롯 생성
        if (inventory.slots == null ||
            inventory.slots.Count == 0)
        {
            inventory.Initialize(defaultSlotCount);
        }

        // 인벤토리 등록
        DataManager.Instance.InventoryManager.Register(inventory);

        // 변경 이벤트 연결
        inventory.OnInventoryChanged += CheckInvenClear;

        // 초기 상태 체크
        CheckInvenClear();
    }

    #endregion

    #region Inventory State

    private void CheckInvenClear()
    {
        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty())
            {
                isInvenClear = false;
                return;
            }
        }

        isInvenClear = true;
    }

    #endregion

    #region Sell

    private void SellInv()
    {
        // 구매 아이템 남아있으면 판매 불가
        if (!isInvenClear)
            return;

        Dictionary<int, ProductClosing> productData =
            DataManager.Instance.productClosingData;

        foreach (var slot in inventory.slots)
        {
            if (slot.IsEmpty())
                continue;

            money += productData[slot.itemID]
                .productsClosingPrice[0] * slot.count;

            slot.Clear();
        }

        inventory.InvokeChange();
    }

    #endregion

    #region Buy

    // 상점 구매 예약
    public void BuyInv(int itemID, int amount)
    {
        if (buyItems.ContainsKey(itemID))
        {
            buyItems[itemID] += amount;
        }
        else
        {
            buyItems[itemID] = amount;
        }
    }

    #endregion

    #region Calculate

    private void Calculate()
    {
        // 돈 지급
        DataManager.Instance.CurrencyManager.AddMoney(money);

        money = 0;

        // 모든 창고
        Dictionary<string, Inventory> storages =
            DataManager.Instance.InventoryManager
            .GetInvType(InventoryType.Unified);

        List<int> removeKeys = new();

        foreach (var buyItem in buyItems)
        {
            int itemID = buyItem.Key;
            int remaining = buyItem.Value;

            // 창고에 먼저 저장
            foreach (var storage in storages.Values)
            {
                if (remaining <= 0)
                    break;

                int added =
                    storage.AddItem(itemID, remaining, DataManager.Instance.itemsData[itemID].storagePeriod);

                if (added > 0)
                {
                    storage.InvokeChange();
                }

                remaining -= added;
            }

            // 전부 저장 성공
            if (remaining <= 0)
            {
                removeKeys.Add(itemID);
            }
            else
            {
                // 남은 수량 유지
                buyItems[itemID] = remaining;
            }
        }

        // 제거
        foreach (var key in removeKeys)
        {
            buyItems.Remove(key);
        }

        // 남은 아이템 로켓 보관
        if (buyItems.Count > 0)
        {
            isInvenClear = false;

            foreach (var buyItem in buyItems)
            {
                inventory.AddItem(
                    buyItem.Key,
                    buyItem.Value,
                    DataManager.Instance.itemsData[buyItem.Key].storagePeriod);
            }

            inventory.InvokeChange();
        }
    }

    #endregion

    #region Save / Load

    public override string GetJsonData()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadJsonData(string json)
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Interaction

    public override void OnInteract()
    {
        uiStorageManagement.TargetBuilding(id);
        uiStorageManagement.RocketInv();
    }

    #endregion
}