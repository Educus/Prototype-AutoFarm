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

        if (isEmpty)
        {
            ClearSlot();
            return;
        }

        itemImage.gameObject.SetActive(true);

        itemImage.sprite =
            DataManager.Instance.GetItemImage(slot.itemID);

        countText.text = slot.count.ToString();

        RefreshTimer(slot);
    }

    public void SetColor(Color color)
    {
        itemImage.color = color;
    }

    public void ClearSlot()
    {
        itemImage.sprite = null;
        itemImage.gameObject.SetActive(false);

        countText.text = "";

        timerImage.fillAmount = 0f;
        timerImage.color = Color.white;
        timerImage.gameObject.SetActive(false);
    }

    private void RefreshTimer(InventorySlot slot)
    {
        bool hasTimer = slot.remainingStoragePeriod != -1;

        timerImage.gameObject.SetActive(hasTimer);

        if (!hasTimer)
        {
            timerImage.fillAmount = 0f;
            return;
        }

        float current = slot.remainingStoragePeriod;

        float max =
            DataManager.Instance.itemsData[slot.itemID].storagePeriod;

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