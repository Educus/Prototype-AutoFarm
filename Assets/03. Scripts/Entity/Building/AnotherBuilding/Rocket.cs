using System.Collections.Generic;
using UnityEngine;

public class Rocket : BuildingBase
{
    [SerializeField] UIStorageManagement uiStorageManagement;

    private Inventory inventory;
    public Dictionary<int, int> buyItems = new Dictionary<int, int>();

    private int hour;
    private int minute;
    private int money = 0;

    private bool isInvenClear;

    private void Start()
    {
        uiStorageManagement = UIStorageManagement.Instance;

        inventory = GetComponent<Inventory>();

        SetInv();

        DataManager.Instance.InventoryManager.inventories.Add("Rocket", inventory);
    }
    private void Update()
    {
        hour = TimeManager.Instance.currentHour;
        minute = TimeManager.Instance.currentMinute;

        // 판매, 구매 시간 체크
        if (hour == 0 && minute == 30) Calculate();
        if (hour == 23 && minute == 30) SellInv();

        // isInvenClear 체크
        CheckInvenClear();
    }
    private void CheckInvenClear()
    {
        if (isInvenClear) return;

        foreach (var slot in inventory.slots)
        {
            if (slot.itemID != 0)
            {
                return;
            }
        }

        isInvenClear = true;
    }

    // 시작 인벤토리 초기화
    private void SetInv()
    {
        inventory.id = gameObject.name;
        inventory.type = InventoryType.Rocket;
        isInvenClear = true;

        if (inventory.slots.Count == 0)
            inventory.Initialize(16);
    }

    // 판매
    private void SellInv()
    {
        if (!isInvenClear) return;

        // 판매
        Dictionary<int, ProductClosing> product = DataManager.Instance.productClosingData;

        foreach (var slot in inventory.slots)
        {
            // 최신?값
            money += product[slot.itemID].productsClosingPrice[0] * slot.count;
            slot.Clear();
        }
    }

    // 구매(상점에서 호출)
    public void BuyInv(int itemID, int amount)
    {
        // 구매
        if (buyItems.ContainsKey(itemID))
            buyItems[itemID] += amount;
        else
            buyItems[itemID] = amount;
    }

    // 정산
    private void Calculate()
    {
        // 골드 정산
        DataManager.Instance.CurrencyManager.AddMoney(money);
        money = 0;

        // 아이템 정산
        // 구매한 아이템 창고로 이동
        Dictionary<string, Inventory> buildingStorage = DataManager.Instance.InventoryManager.GetInvType(InventoryType.Unified);

        List<int> removeKeys = new List<int>();

        foreach (var buyItem in buyItems)
        {
            int itemID = buyItem.Key;
            int remaining = buyItem.Value;

            foreach (var inven in buildingStorage)
            {
                if (remaining <= 0)
                    break;

                int added = inven.Value.AddItem(itemID, remaining, -1);

                remaining -= added;
            }

            // 모두 이동됨
            if (remaining <= 0)
            {
                removeKeys.Add(itemID);
            }
            else
            {
                // 남은 수량만 유지
                buyItems[itemID] = remaining;
            }
        }

        // Dictionary foreach 중 수정 방지
        foreach (var key in removeKeys)
        {
            buyItems.Remove(key);
        }

        // 남은 아이템 로켓 인벤토리로 이동
        isInvenClear = false;

        foreach (var buyItem in buyItems)
        {
            int itemID = buyItem.Key;
            int amount = buyItem.Value;
            inventory.AddItem(itemID, amount, -1);
        }
    }

    // 저장기능
    public override string GetJsonData()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadJsonData(string json)
    {
        throw new System.NotImplementedException();
    }

    // 상호작용
    public override void OnInteract()
    {
        uiStorageManagement.RocketInv();
        Debug.Log("dd");
    }
}
