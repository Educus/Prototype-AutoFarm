using System;
using System.Collections;
using UnityEngine;

public class UIShop : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private UIShopBuy uiShopBuy;

    [SerializeField] private GameObject shopIconPrefab;
    [SerializeField] private GameObject shopIconContent;

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
            if (items.itemType == ItemType.Seed)
                continue;

            if (items.useToDemo == false)
                continue;

            GameObject shopIcon = Instantiate(shopIconPrefab, shopIconContent.transform);
            shopIcon.GetComponent<UIShopIcon>().GetInfo(dataManager, this, items.itemID);
        }
    }

    public void OnClickShopButton(int itemID)
    {
        uiShopBuy.gameObject.SetActive(true);

        uiShopBuy.SetItem(dataManager.itemsData[itemID], dataManager.GetItemImage(itemID));
    }
}
