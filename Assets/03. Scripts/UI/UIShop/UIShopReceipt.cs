using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIShopReceipt : MonoBehaviour
{
    // 영수증
    // 상점 구매 예정인 품목 나열 및 취소가능

    [SerializeField] private ShopManager shop;
    [SerializeField] private PlayerDataManager currency;
    [SerializeField] private DataManager dataManager;

    [Header("Lines")]
    [SerializeField] private Transform lineParent;
    [SerializeField] private UIShopReceiptLine linePrefab;
    [SerializeField] private GameObject emptyMessage;

    [Header("Summary")]
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private TMP_Text slotText;
    // [SerializeField] private TMP_Text moneyText;

    [SerializeField] private Color enoughColor = Color.white;
    [SerializeField] private Color shortColor = Color.red;

    private readonly List<UIShopReceiptLine> lines = new List<UIShopReceiptLine>();

    #region Unity
    private void OnEnable()
    {
        if (shop ==  null || currency == null || dataManager == null)
        {
            Debug.LogError("[UIShopReceipt] : Manager를 연결하세요");
            return;
        }

        shop.onCartChanged += Refresh;
        currency.OnMoneyChanged += OnMoneyChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (shop != null)
            shop.onCartChanged -= Refresh;

        if (currency != null)
            currency.OnMoneyChanged -= OnMoneyChanged;
    }
    #endregion

    private void OnMoneyChanged(int money)
    {
        UpdateSummary();
    }

    private void Refresh()
    {
        int index = 0;

        foreach (var pair in shop.Cart)
        {
            if (index >= lines.Count)
                lines.Add(Instantiate(linePrefab, lineParent));

            UIShopReceiptLine line = lines[index++];
            line.gameObject.SetActive(true);

            int itemID = pair.Key;
            int count = pair.Value;

            line.Setup(
                itemID,
                dataManager.itemsData[itemID].Name,
                dataManager.GetItemImage(itemID),
                count,
                shop.GetBuyPrice(itemID) * count,
                OnRemoveClicked
                );
        }

        for (; index < lines.Count; index++)
            lines[index].gameObject.SetActive(false);

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        int total = shop.GetCartTotal();
        int money = currency.Get();

        Debug.Log($"total:{total}, money:{money}");
        totalText.text = $"합계 : {total}<sprite=2>";
        totalText.color = total > money ? shortColor : enoughColor;

        slotText.text = $"{shop.GetCartSlotCount()} / {ShopManager.CartSlotLimit}";
        // moneyText.text = $"보유 {money:<sprite=2>}";

        if (emptyMessage != null)
            emptyMessage.SetActive(!shop.HasCartItems);
    }

    private void OnRemoveClicked(int itemID)
    {
        shop.RemoveCartItem(itemID);
    }
}
