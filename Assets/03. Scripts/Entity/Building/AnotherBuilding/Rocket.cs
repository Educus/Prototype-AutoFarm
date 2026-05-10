using System.Collections.Generic;
using UnityEngine;

public class Rocket : BuildingBase
{
    [SerializeField] UIStorageManagement uiStorageManagement;

    private Inventory inventory;
    public Dictionary<int, int> buyItems = new Dictionary<int, int>();

    private int hour;
    private int minute;

    private void Start()
    {
        uiStorageManagement = UIStorageManagement.Instance;

        SetInv();

        DataManager.Instance.InventoryManager.inventories.Add("Rocket", inventory);
    }
    private void Update()
    {
        hour = TimeManager.Instance.currentHour;
        minute = TimeManager.Instance.currentMinute;

        if (hour == 0 && minute == 30) BuyInv();
        if (hour == 23 && minute == 30) SellInv();
    }

    private void SetInv()
    {
        inventory.id = gameObject.name;
        inventory.type = InventoryType.Rocket;

        if (inventory.slots.Count == 0)
            inventory.Initialize(16);
    }

    private void SellInv()
    {
        // 판매
        Dictionary<int, ProductClosing> product = DataManager.Instance.productClosingData;
        int value = 0;

        foreach (var slot in inventory.slots)
        {
            // 최신?값
            value += product[slot.itemID].productsClosingPrice[0] * slot.count;
            slot.Clear();
        }

        DataManager.Instance.CurrencyManager.AddMoney(value);
    }
    private void BuyInv()
    {
        // 구매
        foreach (var item in buyItems)
        {
            inventory.AddItem(item.Key, item.Value);
        }
    }

    public override string GetJsonData()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadJsonData(string json)
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract()
    {
        uiStorageManagement.RocketInv();
        Debug.Log("dd");
    }
}
