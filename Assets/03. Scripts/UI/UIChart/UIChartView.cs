using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIChartView : MonoBehaviour
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private RectTransform chartArea;

    [Header("Candle")]
    [SerializeField] private UICandle candlePrefab;
    [SerializeField] private int maxCandles = 28;
    [SerializeField][Range(0.2f, 1f)] private float bodyWidthRatio = 0.6f;
    [SerializeField] private float wickWidth = 2f;

    [Header("Colors")]
    [SerializeField] private Color upColor = Color.red;     // 상승
    [SerializeField] private Color downColor = Color.blue;  // 하락
    [SerializeField] private Color flatColor = Color.gray;  // 변화X

    [Header("Item Info")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemDescription;

    private readonly List<UICandle> candleViews = new List<UICandle>();

    private int currentItemID = -1;
    private bool subscribed;

    #region Unity
    private void OnEnable()
    {
        Subscribe();

        if (currentItemID >= 0)
            DrawChart(currentItemID);
    }

    private void OnDisable()
    {
        UnSubscribe();
    }

    private void Subscribe()
    {
        if (subscribed || TimeManager.Instance == null)
            return;

        TimeManager.Instance.onHourEvent += OnHourChanged;
        subscribed = true;
    }

    private void UnSubscribe()
    {
        if (!subscribed) 
            return;

        if (TimeManager.Instance != null)
            TimeManager.Instance.onHourEvent -= OnHourChanged;

        subscribed = false;
    }

    private void OnHourChanged(int hour)
    {
        if (currentItemID >= 0)
            DrawChart(currentItemID);
    }
    #endregion

    public void DrawChart(int itemID)
    {
        currentItemID = itemID;
        Subscribe();

        if (!dataManager.productClosingData.TryGetValue(itemID, out ProductClosing closing)
            || !closing.HasData)
        {
            Debug.LogWarning($"No data available for itemID : {itemID}");
            HideAllCandles();
            return;
        }

        // 최근 maxCandles일치만 그림 (마지막이 진행 중인 오늘 캔들)
        int total = closing.candles.Count;
        int count = Mathf.Min(total, maxCandles);
        int start = total - count;

        // Y 범위 : 보이는 차트의 최저가 ~ 최고가 (+여백)
        int lowest = int.MaxValue;
        int highest = int.MinValue;

        for (int i = 0; i < count; i++)
        {
            PriceCandle candle = closing.candles[start + i];

            if (candle.low < lowest)
                lowest = candle.low;
            if (candle.high > highest)
                highest = candle.high;
        }

        float range = Mathf.Max(highest - lowest, 1f);
        float padding = range * 0.1f;

        float minY = lowest - padding;
        float maxY = highest + padding;

        EnsureCandleViews();

        float widthArea = chartArea.rect.width;
        float heightArea = chartArea.rect.height;

        float cellWidth = widthArea / maxCandles;
        float bodyWidth = cellWidth * bodyWidthRatio;

        for (int i = 0; i < candleViews.Count; i++)
        {
            if (i >= count)
            {
                candleViews[i].gameObject.SetActive(false);
                continue;
            }

            PriceCandle candle = closing.candles[start + i];

            // 우측정렬
            int slot = maxCandles - count + i;
            float xPos = (slot + 0.5f) * cellWidth - widthArea * 0.5f;

            candleViews[i].gameObject.SetActive(true);
            candleViews[i].Set(
                xPos,
                bodyWidth, wickWidth,
                ToY(candle.open, minY, maxY, heightArea),
                ToY(candle.close, minY, maxY, heightArea),
                ToY(candle.high, minY, maxY, heightArea),
                ToY(candle.low, minY, maxY, heightArea),
                GetColor(candle)
                );
        }

        SetItemInfo(itemID, closing);
    }

    // 가격 -> 차트 가운데 기준 세로 위치
    private static float ToY(float price, float minY, float maxY, float height)
    {
        float normalized = Mathf.InverseLerp(minY, maxY, price);
        return (normalized - 0.5f) * height;
    }

    private Color GetColor(PriceCandle candle)
    {
        if (candle.close > candle.open) return upColor;
        if (candle.close < candle.open) return downColor;

        return flatColor;
    }

    // 차트 오브젝트를 maxCandles개까지 만들어 두고 재사용
    private void EnsureCandleViews()
    {
        while (candleViews.Count < maxCandles)
        {
            UICandle view = Instantiate(candlePrefab, chartArea);
            view.gameObject.SetActive(false);
            candleViews.Add(view);
        }
    }

    private void HideAllCandles()
    {
        foreach (var view in candleViews)
            view.gameObject.SetActive(false);
    }

    private void SetItemInfo(int itemID, ProductClosing closing)
    {
        var itemData = dataManager.itemsData[itemID];

        PriceCandle today = closing.Today;

        // 전일 종가 대비 등락
        int nowPrice = today.close;
        int diff = nowPrice - closing.PreviousClose;
        float percent = closing.PreviousClose > 0 ? diff * 100f / closing.PreviousClose : 0f;

        Color priceColor = diff > 0 ? upColor : diff < 0 ? downColor : flatColor;
        string colorHex = ColorUtility.ToHtmlStringRGB(priceColor);

        string arrow = diff > 0 ? "▲" : diff < 0 ? "▼" : "■";

        // 아이템 이미지
        itemImage.sprite = dataManager.GetItemImage(itemID);

        // 아이템 텍스트
        itemDescription.text =
            $"{itemData.name}\n" +
            $"<color=#{colorHex}>{nowPrice}{arrow}{Mathf.Abs(diff)} ({percent:+0.0;-0.0;0.0}%)</color>\n" +
            $"시 {today.open}  고 {today.high}  저 {today.low}";
    }
}
