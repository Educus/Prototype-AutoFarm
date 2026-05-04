using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShop : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private UIShopBuy uiShopBuy;

    [SerializeField] private GameObject shopIconPrefab;
    [SerializeField] private GameObject shopIconContent;

    private Dictionary<int, GameObject> shopItem = new Dictionary<int, GameObject>();
    void Start()
    {
        StartCoroutine(IEUIShopStart());
    }
    private IEnumerator IEUIShopStart()
    {
        // product의 아이템 수만큼 게임 오브젝트 생성
        // onDayEvent에 등록하여 매일 데이터 갱신
        yield return null;

        foreach (var items in dataManager.itemsData.Values)
        {
            if (items.itemType == ItemType.Product)
                continue;

            if (items.useToDemo == false)
                continue;

            GameObject shopIcon = Instantiate(shopIconPrefab, shopIconContent.transform);
            shopItem[items.itemID] = shopIcon;
            shopIcon.GetComponent<UIShopIcon>().GetInfo(dataManager, this, items.itemID);
        }

        ViewShopItem(0);
    }

    // 보여줄 아이템 목록
    public void ViewShopItem(int value)
    {
        List<ItemType> types = new List<ItemType>();

        switch (value)
        {
            case 0:
                types.Add(ItemType.Seed);
                types.Add(ItemType.Material);
                break;
            case 1:
                types.Add(ItemType.Object);
                break;
            case 2:
                types.Add(ItemType.UpgPerk);
                break;
            case 3:
                types.Add(ItemType.BuildingKit);
                break;

            default:
                types.Add(ItemType.ETC);
                break;
        }

        foreach (var item in shopItem)
        {
            if (types.Contains(dataManager.itemsData[item.Key].itemType))
            {
                item.Value.gameObject.SetActive(true);
            }
            else
            {
                item.Value.gameObject.SetActive(false);
            }
        }
    }

    public void OnClickShopButton(int itemID)
    {
        uiShopBuy.gameObject.SetActive(true);

        uiShopBuy.SetItem(dataManager.itemsData[itemID], dataManager.GetItemImage(itemID));
    }
}
