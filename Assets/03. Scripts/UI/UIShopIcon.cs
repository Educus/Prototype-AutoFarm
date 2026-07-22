using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopIcon : MonoBehaviour
{
    // 상점 UI 각 슬롯에 할당되는 스크립트
    private DataManager dataManager;
    private UIShop uiShop;

    private int itemID;

    // 구매 버튼
    private Button button;

    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemPrice;
    [SerializeField] private TMP_Text itemAmount;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        SetData();
    }

    public void GetInfo(DataManager data, UIShop ui, int id)
    {
        dataManager = data;
        uiShop = ui;
        itemID = id;

        SetInfo();
        SetData();
    }
    // 최초 정보 설정
    private void SetInfo()
    {
        button.onClick.AddListener(() => uiShop.OnClickShopButton(itemID));
        itemImage.sprite = dataManager.GetItemImage(itemID);
        itemName.text = dataManager.itemsData[itemID].itemName;

        int price = dataManager.itemsData[itemID].basicPrice;
        itemPrice.text = price.ToString();
    }

    // 데이터 갱신
    public void SetData()
    {
        if (itemID == 0)
            return;

        itemAmount.text = "보유량:" + DataManager.Instance.InventoryManager.HaveTotalItem(itemID).ToString();
    }
}
