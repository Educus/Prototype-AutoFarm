using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
public class ItemData
{
    [SerializeField] public int itemID;
    [SerializeField] public string itemName;
    [SerializeField] public ItemType itemType;
    [SerializeField] public int basicPrice;
    [SerializeField] public int stack;
    [SerializeField] public int storagePeriod;
}
public class ItemDataManager : MonoBehaviour
{
    public Dictionary<int, ItemData> itemData = new Dictionary<int, ItemData>();

    private TextAsset jsonFile;

    private void Awake()
    {
        // 게임 실행 시 ProductDataTable 불러오기
        LoadItemDataTable();

        // PrintAll();
    }

    private void LoadItemDataTable()
    {
        jsonFile = Resources.Load<TextAsset>("Json/ItemDataTable");

        if (jsonFile == null)
        {
            Debug.Log("파일 없음");
            return;
        }

        List<ItemData> itemList = JsonConvert.DeserializeObject<List<ItemData>>(jsonFile.text);

        itemData.Clear();

        foreach (var item in itemList)
        {
            itemData[item.itemID] = item;
        }
    }

    // 테스트 출력
    public void PrintAll()
    {
        foreach (var pair in itemData)
        {
            Debug.Log($"Key:{pair.Key} ID:{pair.Value.itemID} Name:{pair.Value.itemName} Price:{pair.Value.itemType}");
        }
    }
}
