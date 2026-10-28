using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class Product
{
    [JsonProperty("ID")]
    public int ID;

    [JsonProperty("Name")]
    public string name;

    [JsonProperty("Type")]
    public ItemType type;

    [JsonProperty("PriceStdDev")]
    public float priceStdDev;

    [JsonProperty("GrowthTime")]
    public int growthTime;

    [JsonProperty("MaxYield")]
    public int maximumYield;

    [JsonProperty("MinYield")]
    public int minimumYield;
}

// 주식 차트
[Serializable]
public class PriceCandle
{
    public int open;    // 시가 : 전날 종가
    public int high;    // 최고가
    public int low;     // 최저가
    public int close;   // 종가, 당일 현가격

    public PriceCandle() { }

    // 새 하루 시작, 모든 값 동일
    public PriceCandle(int price)
    {
        open = high = low = close = price;
    }

    public PriceCandle Copy()
    {
        return new PriceCandle { open = open, high = high, low = low, close = close };
    }
}

public class ProductClosing
{
    // 오래된 날이 앞, 오늘이 맨 뒤(최대 28일)
    public List<PriceCandle> candles = new List<PriceCandle>();

    // 정수 반올림 이전 실제 가격
    // 변동 폭이 적어서 정수값에서 계산시 변동폭이 매우 좁아짐
    public float exactPrice;

    public bool HasData => candles.Count > 0;

    // 당일 차트
    public PriceCandle Today => candles.Count > 0 ? candles[candles.Count - 1] : null;
    // 현재 가격
    public int CurrentPrice => Today != null ? Today.close : 0;
    // 전일 종가
    public int PreviousClose
    {
        get
        {
            if (candles.Count >= 2)
                return candles[candles.Count - 2].close;

            return Today != null ? Today.open : 0;
        }
    }

    // 새 가격 반영
    public void ApplyPrice(float newExactPrice)
    {
        PriceCandle today = Today;

        if (today != null)
            return;

        exactPrice = newExactPrice;

        int price = Mathf.Max(1, Mathf.RoundToInt(newExactPrice));

        today.close = price;

        if (price > today.high) today.high = price;
        if (price < today.low) today.low = price;
    }

    // 새 하루 시작
    public void StartNewDay(int maxCandles)
    {
        if (!HasData) 
            return;

        candles.Add(new PriceCandle(CurrentPrice));

        while (candles.Count > maxCandles)
            candles.RemoveAt(0);
    }
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
            productData[product.ID] = product;
            productClosingData[product.ID] = new ProductClosing();
            productSubData[product.ID] = new ProductSubData();
        }
    }
}
