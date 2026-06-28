using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class Rocket : BuildingBase
{
    [Header("UI")]
    [SerializeField] private UIStorageManagement uiStorageManagement;

    [Header("Rocket")]
    [SerializeField] private int defaultSlotCount = 16;

    // 판매 금액
    private int money = 0;

    // 구매 아이템 배송 대기 여부
    private bool hasPendingBuyDelivery = false;

    // 날짜 확인용
    private int lastCalculateDay = -1;
    private int lastSellDay = -1;

    // 23:30 출발 시 확정된 구매 목록
    private Dictionary<int, int> reservedBuyItems = new();

    private Collider2D collider;

    #region Unity

    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Rocket;
        collider = GetComponent<Collider2D>();

        uiStorageManagement = UIStorageManagement.Instance;
    }

    private void Update()
    {
        int hour = TimeManager.Instance.currentHour;
        int minute = TimeManager.Instance.currentMinute;
        int day = TimeManager.Instance.currentDay;

        // 00:30 판매 정산 및 아이템 적재
        if (hour == 0 && minute == 30)
        {
            if (lastCalculateDay != day)
            {
                lastCalculateDay = day;

                Calculate();
            }
        }

        // 23:30 판매
        if (hour == 23 && minute == 30)
        {
            if (lastSellDay != day)
            {
                lastSellDay = day;

                LaunchRocket();
            }
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= CheckRocketAvailable;
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
        inventory.OnInventoryChanged -= CheckRocketAvailable;
        inventory.OnInventoryChanged += CheckRocketAvailable;

        // 초기 상태 체크
        CheckRocketAvailable();
    }

    #endregion

    #region Inventory State

    // 구매 배송 아이템을 모두 꺼냈는지 검사
    private void CheckRocketAvailable()
    {
        // 이미 비어있는 상태면 검사 안 함
        if (!hasPendingBuyDelivery)
            return;

        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty())
            {
                return;
            }
        }

        // 전부 비었음
        hasPendingBuyDelivery = false;

        Debug.Log("구매 배송 아이템 회수 완료");
    }

    private bool HasSellItems()
    {
        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty())
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Launch
    private void LaunchRocket()
    {
        // 이전 구매 아이템 남아있음
        if (hasPendingBuyDelivery)
        {
            Debug.Log("구매 아이템이 남아있어 로켓 출발 불가");

            return;
        }

        bool hasSellItems = HasSellItems();

        bool hasBuyItems = uiStorageManagement.buyItems.Count > 0;
        
        // 아무것도 없음
        if (!hasSellItems && !hasBuyItems)
        {
            return;
        }

        Debug.Log("로켓 출발");

        // 판매 처리
        SellItems();

        // 구매 목록 확정
        reservedBuyItems = new Dictionary<int, int>(uiStorageManagement.buyItems);

        // UI 구매 목록 초기화
        uiStorageManagement.buyItems.Clear();

        // TODO:
        // 로켓 비활성화 / 이동 연출
        collider.enabled = false;
    }

    private void SellItems()
    {
        Dictionary<int, ProductClosing> productData =
            DataManager.Instance.productClosingData;

        foreach (var slot in inventory.slots)
        {
            if (slot.IsEmpty())
                continue;

            if (!productData.ContainsKey(slot.itemID))
            {
                Debug.LogWarning($"판매 데이터 없음 : {slot.itemID}");

                continue;
            }

            var prices = productData[slot.itemID].productsClosingPrice;

            if (prices == null || prices.Count == 0)
            {
                Debug.LogWarning($"판매 가격 데이터 없음 : {slot.itemID}");

                continue;
            }

            money += prices[0] * slot.count;

            slot.Clear();
        }

        inventory.InvokeChange();
    }

    #endregion

    #region Calculate
    // 00:30 도착 처리
    private void Calculate()
    {
        Debug.Log("로켓 도착");

        // 돈 지급
        if (money > 0)
        {
            DataManager.Instance.CurrencyManager.AddMoney(money);

            money = 0;
        }

        // 구매 목록 없음
        if (reservedBuyItems.Count <= 0)
        {
            return;
        }

        // 모든 창고
        Dictionary<string, Inventory> storages =
            DataManager.Instance.InventoryManager
            .GetInvType(InventoryType.Unified);

        Dictionary<int, int> remainItems = new();

        foreach (var buyItem in reservedBuyItems)
        {
            int itemID = buyItem.Key;
            int remaining = buyItem.Value;

            ItemData itemData =
              DataManager.Instance.itemsData[itemID];

            // 창고에 먼저 저장
            foreach (var storage in storages.Values)
            {
                if (remaining <= 0)
                    break;

                int added =
                    storage.AddItem(itemID, remaining, itemData.storagePeriod);

                remaining -= added;
            }

            // 남은 아이템 기록
            if (remaining > 0)
            {
                remainItems[itemID] = remaining;
            }
        }

        // 남은 아이템 로켓에 적재
        foreach (var remainItem in remainItems)
        {
            int added =
                inventory.AddItem(
                    remainItem.Key,
                    remainItem.Value,
                    DataManager.Instance
                    .itemsData[remainItem.Key]
                    .storagePeriod);

            // 적재 실패
            if (added < remainItem.Value)
            {
                Debug.LogWarning(
                    $"로켓 적재 실패 : " +
                    $"ItemID={remainItem.Key}, " +
                    $"요청={remainItem.Value}, " +
                    $"적재={added}");
            }
        }

        inventory.InvokeChange();

        // 남은 구매 아이템 존재
        hasPendingBuyDelivery =
            remainItems.Count > 0;

        // 구매 목록 초기화
        reservedBuyItems.Clear();

        // TODO:
        // 로켓 활성화 / 도착 연출
        collider.enabled = true;
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

    public override void OnInteract(int itemId)
    {
        uiStorageManagement.TargetBuilding(id);
        uiStorageManagement.RocketInv();
    }

    #endregion
}