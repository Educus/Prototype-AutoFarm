using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

[Serializable]
public class InventorySaveData
{
    public int itemId;
    public int count;
}

public abstract class InventoryBase : MonoBehaviour
{
    // 인벤토리 베이스
    // 하위에서 플레이어와 npc 인벤토리를 구현
    [SerializeField]
    protected List<InventorySlot> slots = new List<InventorySlot>();

    protected abstract string saveFileName { get; }

    protected string savePath =>
        Path.Combine(Application.dataPath, $"Resources/Json/{saveFileName}.json");

    // 초기화
    protected void Initialized(int size)
    {
        slots.Clear();

        for (int i = 0; i < size; i++)
        {
            slots.Add(new InventorySlot(0, 0));
        }
    }

    // 아이템 추가
    public virtual bool AddItem(int itemId, int count)
    {
        // 같은 아이템이 있으면 개수 증가
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.itemId == itemId)
            {
                slot.count += count;
                return true;
            }
        }

        // 빈 슬롯이 있으면 아이템 추가
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
            {
                slot.itemId = itemId;
                slot.count = count;
                return true;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다.");
        return false;
    }

    // 아이템 제거
    public virtual bool RemoveItem(int itemId, int count)
    {
        foreach (var slot in slots)
        {
            if (slot.itemId == itemId)
            {
                if (slot.count >= count)
                {
                    slot.count -= count;

                    if (slot.count <= 0)
                    {
                        slot.Clear();
                    }

                    return true;
                }
                else
                {
                    Debug.Log("아이템 개수가 부족합니다.");
                    return false;
                }
            }
        }
        Debug.Log("아이템을 보유하고 있지 않습니다.");
        return false;
    }

    // 아이템 사용 시 보유여부
    // 단일 아이템(설계도 사용 시)
    public bool HasItem(int itemId, int count = 1)
    {
        foreach (var slot in slots)
        {
            if (slot.itemId == itemId && slot.count >= count)
            {
                return true;
            }
        }
        return false;
    }

    // 아이템 보유 갯수
    // 아이템 제작 시 보유 갯수 표시
    public int GetItemCount(int itemId)
    {
        int total = 0;

        foreach (var slot in slots)
        {
            if (slot.itemId == itemId)
            {
                total += slot.count;
            }
        }
        return total;
    }

    // 인벤토리 저장
    public void SaveInventory()
    {
        try
        {
            List<InventorySaveData> saveData = new List<InventorySaveData>();
            
            foreach (var slot in slots)
            {
                saveData.Add(new InventorySaveData { itemId = slot.itemId, count = slot.count });
            }

            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            
            File.WriteAllText(savePath, json);
            
            Debug.Log("인벤토리 저장 성공: " + savePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{gameObject.name} 인벤토리 저장 실패: {ex.Message}");
        }
    }

    // 인벤토리 로드
    public void LoadInventory()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning($"{gameObject.name} : 저장된 인벤토리가 없습니다");
            }

            string json = File.ReadAllText(savePath);
            
            List<InventorySaveData> saveData = JsonConvert.DeserializeObject<List<InventorySaveData>>(json);
            
            for (int i = 0; i < slots.Count && i < saveData.Count; i++)
            {
                slots[i].itemId = saveData[i].itemId;
                slots[i].count = saveData[i].count;
            }
 
            Debug.Log($"{gameObject.name} 인벤토리 로드 성공");
        }
        catch (Exception ex)
        {
            Debug.LogError($"{gameObject.name} 인벤토리 로드 실패: {ex.Message}");
        }
    }


    // 디버그 출력
    public void PrintInventory()
    {
        Debug.Log($"--- {this.GetType().Name} ---");

        for (int i = 0; i < slots.Count; i++)
        {
            Debug.Log($"Slot {i}: ID={slots[i].itemId}, Count={slots[i].count}");
        }
    }
}