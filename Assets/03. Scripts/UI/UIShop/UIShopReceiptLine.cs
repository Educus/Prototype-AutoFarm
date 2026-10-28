using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShopReceiptLine : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button removeButton;

    private int itemID;
    private Action<int> onRemove;

    private void Awake()
    {
        removeButton.onClick.AddListener(() => onRemove?.Invoke(itemID));
    }

    public void Setup(
        int itemID,
        string itemName,
        Sprite sprite,
        int count,
        int linePrice,
        Action<int> onRemove
        )
    {
        this.itemID = itemID;
        this.onRemove = onRemove;

        icon.sprite = sprite;
        icon.enabled = sprite != null;

        nameText.text = itemName;
        countText.text = $"x{count}";
        priceText.text = linePrice.ToString("N0");
    }
}
