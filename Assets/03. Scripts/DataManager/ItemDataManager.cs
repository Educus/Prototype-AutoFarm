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
    Tutorial,
    Developer,
    ETC             // 기타 아이템
}
public class ItemData
{
    public int ID;
    public string name;
    public ItemType type;
    public int basicPrice;
    public int stack;           // 최대 스택 수
    public int storagePeriod;   // 보관 기간
    public int maxCapacity;     // 내구도
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
            itemData[item.ID] = item;
        }
    }

}
