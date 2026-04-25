using System;
using System.Collections;
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
