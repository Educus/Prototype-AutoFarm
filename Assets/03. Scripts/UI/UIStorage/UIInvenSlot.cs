using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInvenSlot : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text countText;

    public void SetSlot(InventorySlot slot)
    {
        bool isEmpty = slot.itemID == 0 || slot.count <= 0;

        itemImage.gameObject.SetActive(!isEmpty);
        countText.text = isEmpty ? "" : slot.count.ToString();

        if (isEmpty)
        {
            timerImage.gameObject.SetActive(false);
            return;
        }

        itemImage.sprite =
            DataManager.Instance.GetItemImage(slot.itemID);

        RefreshTimer(slot);
    }

    private void RefreshTimer(InventorySlot slot)
    {
        bool hasTimer = slot.remainingStoragePeriod != -1;

        timerImage.gameObject.SetActive(hasTimer);

        if (!hasTimer)
            return;

        float current = slot.remainingStoragePeriod;

        float max =
            DataManager.Instance.productsData[slot.itemID].storagePeriod;

        float value = current / max;

        timerImage.fillAmount = value;

        if (value >= 0.6f)
        {
            timerImage.color = Color.green;
        }
        else if (value >= 0.3f)
        {
            timerImage.color = new Color(1f, 0.75f, 0f);
        }
        else
        {
            timerImage.color = Color.red;
        }
    }
}