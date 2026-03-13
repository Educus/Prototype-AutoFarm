using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class Product
{
    [SerializeField] public int itemID;
    [SerializeField] public string itemName;
    [SerializeField] public ItemType itemType;
    [SerializeField] public float priceStdDev;
    [SerializeField] public int basicCost;
    [SerializeField] public int growthTime;
    [SerializeField] public int maximumYield;
    [SerializeField] public int waterConsumption;
    [SerializeField] public int storagePeriod;
}
public class ProductDataManager : MonoBehaviour
{
    public Dictionary<int, Product> productData = new Dictionary<int, Product>();

    private TextAsset jsonFile;

    private void Awake()
    {
        // 게임 실행 시 ProductDataTable 불러오기
        LoadProductDataTable();

        // PrintAll();
    }

    private void LoadProductDataTable()
    {
        jsonFile = Resources.Load<TextAsset>("Json/ProductDataTable");

        if (jsonFile == null)
        {
            Debug.Log("파일 없음");
            return;
        }

        List<Product> productList = JsonConvert.DeserializeObject<List<Product>>(jsonFile.text);

        productData.Clear();

        foreach (var product in productList)
        {
            productData[product.itemID] = product;
        }
    }

    // 테스트 출력
    public void PrintAll()
    {
        foreach (var pair in productData)
        {
            Debug.Log($"Key:{pair.Key} ID:{pair.Value.itemID} Name:{pair.Value.itemName} Price:{pair.Value.itemType}");
        }
    }
}
