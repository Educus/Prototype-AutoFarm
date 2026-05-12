using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingStorage : MonoBehaviour
{
    // 로켓 판매 부분
    // 모든 창고를 한번에 보여주고 상호작용
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform content;

    private List<UIInvenSlot> slotList = new List<UIInvenSlot>();

    private void OnEnable()
    {
        CreateSlot();
    }

    // 생성 혹은 액티브True로 변경
    private void CreateSlot()
    {
        int soltCount = content.childCount;

        if (UIStorageManagement.Instance.targetBuilding == "Rocket")
        {
            // 모든 창고
            Dictionary<string, Inventory> totalStorage = DataManager.Instance.InventoryManager.GetInvType(InventoryType.Unified);

            int totalStorageCount = totalStorage.Count * 30;

            if (totalStorageCount > soltCount)
            {
                int createCount = totalStorageCount - soltCount;
                CreateSlot(createCount);
            }
            else
            {
                ActiveSlot(totalStorageCount);
            }


            // 
            ViewItemInfo(totalStorage);
        }
        else
        {
            // 특정 창고
            ActiveSlot(30); // 기본 창고 30칸

            ViewItemInfo(DataManager.Instance.InventoryManager.Get(UIStorageManagement.Instance.targetBuilding));
        }
    }

    #region 창고 슬롯 생성 및 활성화
    private void CreateSlot(int value)
    {
        for (int i = 0; i < value; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, content);

            UIInvenSlot slot = slotObj.GetComponent<UIInvenSlot>();

            slotList.Add(slot);
        }
    }

    private void ActiveSlot(int value)
    {
        // 창고에 맞는 슬롯 활성화
        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < value)
            {
                slotList[i].gameObject.SetActive(true);
            }
            else
            {
                slotList[i].gameObject.SetActive(false);
            }
        }
    }
    #endregion

    private void ViewItemInfo(Dictionary<string, Inventory> totalStorage)
    {
        // 모든 창고 보여주기
        int slotIndex = 0;

        foreach (var storage in totalStorage.Values)
        {
            for (int i = 0; i < storage.slots.Count; i++)
            {
                slotList[slotIndex].SetSlot(storage.slots[i]);

                slotIndex++;
            }
        }
    }

    private void ViewItemInfo(Inventory storage)
    {
        // 특정 창고 보여주기
        for (int i = 0; i < storage.slots.Count; i++)
        {
            slotList[i].SetSlot(storage.slots[i]);
        }
    }
}
