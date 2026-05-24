using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UINPCStorage : MonoBehaviour
{
    // NPC 클릭 상호작용시 표시되는 UI

    [SerializeField] private TMP_Text npcName;
    [SerializeField] private Image[] workSlots;
    [SerializeField] private Image workItems;
    [SerializeField] private GameObject[] upgInv;
    [SerializeField] private GameObject[] mainInv;
    [SerializeField] private GameObject[] subInv;

    [SerializeField] private Sprite noneSprite;
    [SerializeField] private Sprite farmSprite;
    [SerializeField] private Sprite ranchSprite;

    private NPC target;

    private void Update()
    {
        target = GameManager.Instance.selectedNPC;

        if (target == null) return;

        Refresh();
    }

    private void Refresh()
    {
        RefresName();
        RefresWorkSlot();
        RefresWork();
        RefresInv();
    }
    private void RefresName()
    {
        //// npc name
        npcName.text = target.GetName();
    }
    private void RefresWorkSlot()
    {
        //// workSlot & workitem
        Sprite sprite = GetWorkSprite();

        int count = target.job.buildingIDs.Count;

        for (int i = 0; i < workSlots.Length; i++)
        {
            bool active = i < count;

            if (active)
            {
                workSlots[i].sprite = sprite;
            }
        }
    }
    private void RefresWork()
    {
        Sprite sprite = DataManager.Instance.GetItemImage(target.job.productItemID);

        bool hasItem = sprite != null;

        workItems.gameObject.SetActive(hasItem);

        if (hasItem) workItems.sprite = sprite;
    }
    private Sprite GetWorkSprite()
    {
        switch (target.job.jobType)
        {
            case JobType.Farm:
                return farmSprite;

            case JobType.Ranch:
                return ranchSprite;

            default:
                return noneSprite;
        }
    }
    private void RefresInv()
    {
        RefreshInventory(mainInv, target.mainInventory.slots);
        RefreshInventory(subInv, target.subInventory.slots);
        RefreshInventory(upgInv, target.upgradeInventory.slots);
    }

    private void RefreshInventory(GameObject[] uiSlots, List<InventorySlot> dataSlots)
    {
        int count = Mathf.Min(uiSlots.Length, dataSlots.Count);

        for (int i = 0; i < count; i++)
        {
            RefreshSlot(uiSlots[i], dataSlots[i]);
        }

        for (int i = count; i < uiSlots.Length; i++)
        {
            ClearSlot(uiSlots[i]);
        }
    }

    private void RefreshSlot(GameObject slotObj, InventorySlot slot)
    {
        Image timer = GetTimer(slotObj);
        Image icon = GetIcon(slotObj);
        TMP_Text count = GetCount(slotObj);

        bool hasItem = slot.itemID > 0;

        icon.gameObject.SetActive(hasItem);

        if (!hasItem)
        {
            ClearSlot(slotObj);
            return;
        }

        // 아이템 이미지
        icon.sprite = DataManager.Instance.GetItemImage(slot.itemID);

        // 아이템 수량
        count.text = slot.count.ToString();

        // 타이머
        RefreshTimer(timer, slot);
    }

    private void ClearSlot(GameObject slotObj)
    {
        GetIcon(slotObj).gameObject.SetActive(false);
        GetIcon(slotObj).sprite = null;

        GetCount(slotObj).text = "";

        GetTimer(slotObj).gameObject.SetActive(false);
    }

    private void RefreshTimer(Image timer, InventorySlot slot)
    {
        bool hasTimer = slot.remainingStoragePeriod != -1;

        timer.gameObject.SetActive(hasTimer);

        if (!hasTimer) return;

        float current = slot.remainingStoragePeriod;

        float max = DataManager.Instance.itemsData[slot.itemID].storagePeriod;

        float value = current / max;

        timer.fillAmount = value;

        if (value >= 0.6f)
            timer.color = Color.green;
        else if (value >= 0.3f)
            timer.color = new Color(1f, 0.75f, 0f);
        else
            timer.color = Color.red;
    }

    private Image GetTimer(GameObject slotObj)
    {
        return slotObj.transform.GetChild(0).GetComponent<Image>();
    }

    private Image GetIcon(GameObject slotObj)
    {
        return slotObj.transform.GetChild(1).GetComponent<Image>();
    }

    private TMP_Text GetCount(GameObject slotObj)
    {
        return slotObj.transform.GetChild(2).GetComponent<TMP_Text>();
    }

    /*
    private void RefresInv()
    {
        //// Inv
        int i = 0;

        // Main
        foreach (var item in MainInv)
        {
            InventorySlot slots = target.mainInventory.slots[i];

            SetSlots(item, slots);
                        
            i++;
        }
        i = 0;
        // Sub
        foreach (var item in SubInv)
        {
            InventorySlot slots = target.subInventory.slots[i];
            
            SetSlots(item, slots);

            i++;
        }
        i = 0;
        foreach (var item in UpgInv)
        {
            InventorySlot slots = target.upgradeInventory.slots[i];
            
            SetSlots(item, slots);

            i++;
        }
    }

    private void SetSlots(GameObject item, InventorySlot slot)
    {
        InventorySlot slots = slot;
        Image itemTimer = item.transform.GetChild(0).GetComponent<Image>();
        Image itemImage = item.transform.GetChild(1).GetComponent<Image>();
        TMP_Text itemCount = item.transform.GetChild(2).GetComponent<TMP_Text>();

        // 아이템 목록 & 숫자
        if (slots.itemID <= 0)
        {
            itemImage.gameObject.SetActive(false);
            itemImage.sprite = null;

            itemCount.text = "";
        }
        else
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = DataManager.Instance.GetItemImage(slots.itemID);

            itemCount.text = slots.count.ToString();
        }

        // 아이템 남은 시간
        if (slots.itemID <= 0 || slots.remainingStoragePeriodl == -1)
        {
            itemTimer.gameObject.SetActive(false);
        }
        else
        {
            itemTimer.gameObject.SetActive(true);
            float timePercent = slots.remainingStoragePeriodl;
            float maxTimePercent = DataManager.Instance.productsData[slots.itemID].storagePeriod;

            ItemTimer(itemTimer, timePercent, maxTimePercent);
        }
    }

    private void ItemTimer(Image image, float time, float maxTime)
    {
        float value = time / maxTime;
        
        Color color;
        if (value >= 0.6f)
            color = Color.green;
        else if (value >= 0.3f)
            color = Color.yellow;
        else
            color = Color.red;

        image.color = color;
        image.fillAmount = value;
    }
    */
}