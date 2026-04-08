using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoUI : MonoBehaviour
{
    // 건설할 아이템의 정보를 표시하는 UI
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemInfo;

    [SerializeField] private DataManager dataManager;

    public void Show(int itemID, BuildingData buildingData, Sprite icon)
    {
        var item = dataManager.itemsData[itemID];

        if (item == null)
        {
            Debug.LogError($"{itemID}의 데이터 없음");
            return;
        }

        itemIcon.sprite = icon;
        itemName.text = item.itemName;
        itemInfo.text = $"{buildingData.width}X{buildingData.height} / -1";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
