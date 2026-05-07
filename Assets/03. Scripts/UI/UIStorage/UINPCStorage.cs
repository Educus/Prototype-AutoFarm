using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINPCStorage : MonoBehaviour
{
    [SerializeField] private TMP_Text npcName;
    [SerializeField] private Image[] workSlots;
    [SerializeField] private Image workItems;
    [SerializeField] private GameObject[] UpgInv;
    [SerializeField] private GameObject[] MainInv;
    [SerializeField] private GameObject[] SubInv;

    private NPC target;

    private void Update()
    {
        NPC npc = GameManager.Instance.selectedNPC;

        if (target == npc) return;

        target = npc;

        if (target == null) return;

        ViewNPCInv();
    }

    private void ViewNPCInv()
    {
        Debug.Log("1");
        //// npc name
        npcName.text = target.GetName();

        //// workSlot & workitem
        foreach (var item in workSlots)
        {

        }

        workItems.sprite = DataManager.Instance.GetItemImage(target.job.productItemID);
        workItems.sprite = null;

        //// Inv
        int i = 0;

        // Main
        foreach (var item in MainInv)
        {
            InventorySlot slots = target.mainInventory.slots[i];
            Image itemImage = item.transform.GetChild(0).GetComponent<Image>();

            if (slots.itemID <= 0)
            {
                itemImage.sprite = null;
            }
            else
            {
                itemImage.sprite = DataManager.Instance.GetItemImage(slots.itemID);
            }

            i++;
        }
        i = 0;
        // Sub
        foreach (var item in SubInv)
        {
            InventorySlot slots = target.subInventory.slots[i];
            Image itemImage = item.transform.GetChild(0).GetComponent<Image>();

            if (slots.itemID <= 0)
            {
                itemImage.sprite = null;
            }
            else
            {
                itemImage.sprite = DataManager.Instance.GetItemImage(slots.itemID);
            }

            i++;
        }
        i = 0;
        foreach (var item in UpgInv)
        {
            InventorySlot slots = target.upgradeInventory.slots[i];
            Image itemImage = item.transform.GetChild(0).GetComponent<Image>();

            if (slots.itemID <= 0)
            {
                itemImage.sprite = null;
            }
            else
            {
                itemImage.sprite = DataManager.Instance.GetItemImage(slots.itemID);
            }

            i++;
        }
    }
}