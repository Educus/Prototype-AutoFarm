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
    public string Name;
    public ItemType Type;
    public int BasicPrice;
    public int Stack;           // 최대 스택 수
    public int StoragePeriod;   // 보관 기간
    public bool UseToDemo;
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

        // 오류 시 json 파일 내용 확인(디버그용)
        // string json = jsonFile.text;
        // 
        // // 실제 파일 확인
        // Debug.Log("파일 이름: " + jsonFile.name);
        // Debug.Log("JSON 첫 부분:\n" + json.Substring(0, Mathf.Min(100, json.Length)));

        List<ItemData> itemList = JsonConvert.DeserializeObject<List<ItemData>>(jsonFile.text);

        itemData.Clear();

        foreach (var item in itemList)
        {
            itemData[item.ID] = item;
        }

        Debug.Log("아이템 데이터 로드 완료: " + itemData.Count);
    }
}
