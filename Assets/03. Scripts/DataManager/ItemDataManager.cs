using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public enum ItemType
{
    Water,
    Seed,
    Product,
    Material,
    UpgPerk,
    BuildingKit,
    Battery,
    Object,
    ETC             // 기타 아이템
}
public class ItemData
{
    public int itemID;
    public string itemName;
    public ItemType itemType;
    public int basicPrice;
    public int stack;
    public int storagePeriod;
    public bool useToDemo;
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

    // 기본 베이스 아이템 데이터 테이블 불러오기
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

}
