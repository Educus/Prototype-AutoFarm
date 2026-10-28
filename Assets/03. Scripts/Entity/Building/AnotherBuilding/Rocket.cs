using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class Rocket : BuildingBase
{
    [Header("UI")]
    [SerializeField] private UIStorageManagement uiStorageManagement;

    [Header("Rocket")]
    [SerializeField] private int defaultSlotCount = 16;

    [Header("Trip")]
    [SerializeField] private int oneWayTripHours = 4;   // 판매 or 구매만
    [SerializeField] private int bothTripHours = 6;     // 판매 구매 둘 다

    private ShopManager shop => DataManager.Instance.ShopManager;

    // 판매 금액 (출발 시 계산, 도착 시 지급)
    private int money = 0;

    // 구매 아이템 배송 대기 여부 (로켓 내부에 꺼내야 할 배송품 여부)
    private bool hasPendingBuyDelivery = false;

    // 복귀 여부, 복귀까지 남은 시간
    private bool isAway;
    private int remainingMinutes;

    // 출발 시 확정된 구매 목록
    private Dictionary<int, int> reservedBuyItems = new();

    private Collider2D collider;
    private SpriteRenderer spriteRenderer;

    public bool IsAway => isAway;
    public int RemainingMinutes => remainingMinutes;

    public override string GetDestroyBlockReason()
    {
        return "로켓은 파괴할 수 없습니다.";
    }

    #region Unity

    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Rocket;
        collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        uiStorageManagement = UIStorageManagement.Instance;
    }

    private void Start()
    {
        TimeManager.Instance.onMinuteEvent += OnMinute;
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= CheckRocketAvailable;
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.onMinuteEvent -= OnMinute;
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

    // 판매 가격 데이터가 없는 물품 검사
    private bool HasUnsellableItems()
    {
        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty() && !shop.CanSell(slot.itemID))
                return true;
        }

        return false;
    }

    #endregion

    #region Launch

    // 출발 버튼에서 호출. 실패 이유는 UI에 표시
    // 로켓 찾기: DataManager.Instance.BuildingManager.Get<Rocket>(id)
    public bool TryLaunch(out string reason)
    {
        reason = null;

        if (shop == null)
        {
            reason = "상점 매니저를 찾을 수 없습니다.";
            return false;
        }

        if (isAway)
        {
            reason = "로켓이 이미 출발했습니다.";
            return false;
        }

        // 이전 구매 배송 아이템이 남아 있음
        if (hasPendingBuyDelivery)
        {
            reason = "배송된 아이템을 먼저 꺼내야 합니다.";
            return false;
        }

        // 판매할 수 없는 물품이 하나라도 있으면 출발 불가
        if (HasUnsellableItems())
        {
            reason = "판매할 수 없는 물품이 들어 있습니다.";
            return false;
        }

        bool hasSellItems = HasSellItems();
        bool hasBuyItems = shop.HasCartItems;

        if (!hasSellItems && !hasBuyItems)
        {
            reason = "판매하거나 구매할 물품이 없습니다.";
            return false;
        }

        // 구매 대금은 출발 버튼을 누를 때 차감 (돈이 부족하면 출발하지 않음)
        // 상태를 바꾸기 전에 실패 조건을 모두 확인: 판매 물품을 비우기 전에 먼저 검사
        CurrencyManager currency = DataManager.Instance.CurrencyManager;
        int cost = shop.GetCartTotal();

        if (cost > 0 && currency.Get() < cost)
        {
            reason = "돈이 부족합니다.";
            return false;
        }

        Debug.Log("로켓 출발");

        // 판매 처리 (정산은 돌아왔을 때)
        SellItems();

        // 구매 대금 차감 (위에서 확인했으므로 성공함. 판매 대금은 로켓의 money에 쌓이므로 영향 없음)
        if (cost > 0)
            currency.Spend(cost);

        // 구매 목록 확정: 장바구니를 복사가 아니라 이동 (이미 결제된 주문이라 저장 대상)
        reservedBuyItems = shop.TakeCart();

        // 둘 다 있으면 6시간, 하나만 있으면 4시간
        int hours = (hasSellItems && hasBuyItems) ? bothTripHours : oneWayTripHours;
        remainingMinutes = hours * 60;
        isAway = true;

        // TODO: 이동 연출
        SetVisible(false);

        return true;
    }

    private void SellItems()
    {
        foreach (var slot in inventory.slots)
        {
            if (slot.IsEmpty())
                continue;

            // TryLaunch에서 판매 불가 물품을 이미 걸러냈음
            money += shop.GetSellPrice(slot.itemID) * slot.count;

            slot.Clear();
        }

        inventory.InvokeChange();
    }

    private void SetVisible(bool visible)
    {
        spriteRenderer.enabled = visible;
        collider.enabled = visible;
    }

    #endregion

    #region Arrive

    // 10분 단위 게임 시간 이벤트 (minute = 한 번에 흐른 게임 분)
    private void OnMinute(int minute)
    {
        if (!isAway)
            return;

        remainingMinutes -= minute;

        if (remainingMinutes <= 0)
            Arrive();
    }

    // 도착 처리 (정산)
    private void Arrive()
    {
        Debug.Log("로켓 도착");

        isAway = false;
        remainingMinutes = 0;

        // 돈 지급
        if (money > 0)
        {
            DataManager.Instance.CurrencyManager.AddMoney(money);

            money = 0;
        }

        // 구매 목록 없음
        if (reservedBuyItems.Count <= 0)
        {
            SetVisible(true);
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

            if (itemData.type == ItemType.Object)
            {
                // NPC 구매
                if (itemID == NPCManager.NpcItemID)
                {
                    for (int i = 0; i < remaining; i++)
                    {
                        SpawnNPC(i);
                    }

                    // NPC는 모두 지급되었으므로 다음 아이템 처리
                    continue;
                }
                // 동물 구매
                else
                {
                    for (int i = 0; i < remaining; i++)
                    {
                        SpawnAnimal(itemID);
                    }

                    continue;
                }
            }

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

        // TODO: 도착 연출
        SetVisible(true);
    }

    private void SpawnNPC(int x)
    {
        NPCManager npcManager = DataManager.Instance.NPCManager;

        string npcID = $"{NPCManager.NpcItemID}_{npcManager.npcs.Count}";

        npcManager.Spawn(npcID, transform.position + new Vector3(x, -3f, 0));
    }

    private void SpawnAnimal(int animalItemID)
    {
        AnimalManager animalManager = DataManager.Instance.AnimalManager;

        string animalID = $"{animalItemID}_{animalManager.animals.Count}";

        animalManager.Spawn(animalItemID, animalID, transform.position + new Vector3(0, -4f, 0));
    }

    #endregion

    #region Save / Load

    // 인벤토리 내용은 전체 인벤토리 저장에 들어 있으므로 여기서는 이동 상태만 저장
    public override string GetJsonData()
    {
        var data = new RocketSaveData
        {
            isAway = isAway,
            remainingMinutes = remainingMinutes,
            money = money,
            hasPendingBuyDelivery = hasPendingBuyDelivery,
            reservedBuyItems = ToEntries(reservedBuyItems)
        };

        return JsonUtility.ToJson(data);
    }

    public override void LoadJsonData(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        RocketSaveData data = JsonUtility.FromJson<RocketSaveData>(json);

        isAway = data.isAway;
        remainingMinutes = data.remainingMinutes;
        money = data.money;
        hasPendingBuyDelivery = data.hasPendingBuyDelivery;

        reservedBuyItems = FromEntries(data.reservedBuyItems);

        // 출발 중이었다면 숨긴 채로 이어서 날아감 (타이머는 OnMinute가 이어받음)
        SetVisible(!isAway);
    }

    private static List<ItemCountEntry> ToEntries(Dictionary<int, int> dict)
    {
        var list = new List<ItemCountEntry>();

        foreach (var pair in dict)
            list.Add(new ItemCountEntry { itemID = pair.Key, count = pair.Value });

        return list;
    }

    private static Dictionary<int, int> FromEntries(List<ItemCountEntry> list)
    {
        var dict = new Dictionary<int, int>();

        if (list == null)
            return dict;

        foreach (var e in list)
            dict[e.itemID] = e.count;

        return dict;
    }

    #endregion

    #region Interaction

    public override void OnNormalInteract()
    {
        Debug.Log("Rocket OnNormalInteract");

        uiStorageManagement.TargetBuilding(id);
        uiStorageManagement.RocketInv();
    }
    public override void OnRInteract()
    {
        Debug.Log("Rocket OnRInteract");

        uiStorageManagement.TargetBuilding(id);
        uiStorageManagement.RocketInv();
    }

    #endregion
}