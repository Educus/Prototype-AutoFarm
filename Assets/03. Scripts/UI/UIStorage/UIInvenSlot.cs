using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInvenSlot : MonoBehaviour
{
    [SerializeField] private Sprite[] timerSprite;
    [SerializeField] private Image timerImage;
    [SerializeField] private Image plusImage;
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

        SetPlus(false);

        itemImage.gameObject.SetActive(true);

        itemImage.sprite =
            OriginDataManager.Instance.GetItemImage(slot.itemID);

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

        HideTimer();

        SetPlus(true);
    }

    private void SetPlus(bool active)
    {
        if (plusImage != null)
            plusImage.gameObject.SetActive(active);
    }

    private void RefreshTimer(InventorySlot slot)
    {
        // 유통기한 없는 아이템
        if (slot.remainingStoragePeriod == -1)
        {
            HideTimer();
            return;
        }

        if (timerSprite == null || timerSprite.Length == 0)
        {
            HideTimer();
            return;
        }

        float max = OriginDataManager.Instance.itemsData[slot.itemID].StoragePeriod;

        float value = max > 0f ? Mathf.Clamp01(slot.remainingStoragePeriod / max) : 1f;

        timerImage.sprite = timerSprite[GetStage(value)];
        timerImage.gameObject.SetActive(true);
    }

    private int GetStage(float value)
    {
        int count = timerSprite.Length;

        int stage = Mathf.FloorToInt((1f - value) * count);

        return Mathf.Clamp(stage, 0, count - 1);
    }

    private void HideTimer()
    {
        timerImage.gameObject.SetActive(false);
    }
}