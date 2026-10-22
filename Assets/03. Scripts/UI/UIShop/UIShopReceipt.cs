using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIShopReceipt : MonoBehaviour
{
    // 영수증
    // 상점 구매 예정인 품목 나열 및 취소가능

    [SerializeField] private ShopManager shopManager;
    [SerializeField] private CurrencyManager currencyManager;
    [SerializeField] private DataManager dataManager;

    [Header("Lines")]
    [SerializeField] private Transform lineParent;
    [SerializeField] private UIShopReceiptLine linePrefab;
    [SerializeField] private GameObject emptyMessage;

    [Header("Summary")]
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private TMP_Text slotText;
    [SerializeField] private TMP_Text moneyText;

    [SerializeField] private Color enoughColor = Color.white;
    [SerializeField] private Color shortColor = Color.red;

    private readonly List<UIShopReceiptLine> lines = new List<UIShopReceiptLine>();

    #region Unity
    private void OnEnable()
    {
        if (shopManager ==  null || currencyManager == null || dataManager == null)
        {
            Debug.LogError("[UIShopReceipt] Manager를 연결하세요");
            return;
        }

        shopManager.onCartChanged += Refresh;
        currencyManager.OnMoneyChanged += OnMoneyChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (shopManager != null)
            shopManager.onCartChanged -= Refresh;

        if (currencyManager != null)
            currencyManager.OnMoneyChanged -= OnMoneyChanged;
    }
    #endregion

    private void OnMoneyChanged(int money)
    {
        UpdateSummary();
    }

    private void Refresh()
    {
        int index = 0;

        foreach (var pair in shopManager.Cart)
        {
            if (index >= lines.Count)
                lines.Add(Instantiate(linePrefab, lineParent));

            UIShopReceiptLine line = lines[index++];
            line.gameObject.SetActive(true);

            int itemID = pair.Key;
            int count = pair.Value;

            line.Setup(
                itemID,
                dataManager.itemsData[itemID].name,
                dataManager.GetItemImage(itemID),
                count,
                shopManager.GetBuyPrice(itemID) * count,
                OnRemoveClicked
                );
        }

        for (; index < lines.Count; index++)
            lines[index].gameObject.SetActive(false);

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        int total = shopManager.GetCartTotal();
        int money = currencyManager.Get();

        totalText.text = $"합계 {total:NO}";
        totalText.color = total > money ? shortColor : enoughColor;

        slotText.text = $"{shopManager.GetCartSlotCount()} / {ShopManager.CartSlotLimit}";
        moneyText.text = $"보유 {money:NO}";

        if (emptyMessage != null)
            emptyMessage.SetActive(!shopManager.HasCartItems);
    }

    private void OnRemoveClicked(int itemID)
    {
        shopManager.RemoveCartItem(itemID);
    }
}
