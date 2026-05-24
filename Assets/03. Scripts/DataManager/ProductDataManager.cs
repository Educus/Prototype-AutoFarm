using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class Product
{
    [JsonProperty("ItemID")]
    public int itemID;

    [JsonProperty("ItemName")]
    public string itemName;

    [JsonProperty("ItemType")]
    public ItemType itemType;

    [JsonProperty("PriceStdDev")]
    public float priceStdDev;

    [JsonProperty("BasicPrice")]
    public int basicCost;

    [JsonProperty("GrowthTime")]
    public int growthTime;

    [JsonProperty("MaxYield")]
    public int maximumYield;

    [JsonProperty("MinYield")]
    public int minimumYield;

    [JsonProperty("WaterConsumption")]
    public int waterConsumption;
}

public class ProductClosing
{
    // 종가 7일치 저장
    public List<int> productsClosingPrice = new List<int>();
}

public class ProductSubData
{
    [Tooltip("남은 성장 시간")]
    public int remainingGrowthTime = -1;    // 남은 성장 시간
    [Tooltip("남은 저장 기간")]
    public int remainingStoragePeriod = -1; // 남은 저장 기간
    public bool isBookMarked = false;       // 북마크 여부

    public bool GetBookMark()
    {
        return isBookMarked;
    }   
    public void OnOffBookMark()
    {
        isBookMarked = !isBookMarked;
    }
}

public class ProductDataManager : MonoBehaviour
{
    public Dictionary<int, Product> productData = new Dictionary<int, Product>();
    public Dictionary<int, ProductClosing> productClosingData = new Dictionary<int, ProductClosing>();
    public Dictionary<int, ProductSubData> productSubData = new Dictionary<int, ProductSubData>();

    private TextAsset jsonFile;

    private void Awake()
    {
        // 게임 실행 시 ProductDataTable 불러오기
        LoadProductDataTable();
    }

    private void Start()
    {
        // 테스트용: 모든 제품 데이터 출력
        PrintAll();
    }

    private void PrintAll()
    {
        foreach (var kvp in productClosingData)
        {
            int itemID = kvp.Key;
            ProductClosing productClosing = kvp.Value;
        }
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
        productClosingData.Clear();
        productSubData.Clear();

        foreach (var product in productList)
        {
            productData[product.itemID] = product;
            productClosingData[product.itemID] = new ProductClosing();
            productSubData[product.itemID] = new ProductSubData();
        }
    }
}
