using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIChartView : MonoBehaviour, IScrollHandler
{
    [SerializeField] private DataManager dataManager;
    [SerializeField] private RectTransform chartArea;

    [Header("Candle")]
    [SerializeField] private UICandle candlePrefab;

    [FormerlySerializedAs("maxCandles")]
    [SerializeField] private int visibleCandles = 28;   // 화면에 보이는 캔들 수

    [SerializeField][Range(0.2f, 1f)] private float bodyWidthRatio = 0.6f;
    [SerializeField] private float wickWidth = 2f;

    [Header("Y축 범위")]
    [Tooltip("기본 가격 BasicPrice 기준. 0.1 = 10%")]
    [SerializeField][Range(0f, 1f)] private float minRangeRatio = 0.1f;
    [Tooltip("실제 가격 폭이 최소 폭보다 클 때 위아래로 더하는 여백 비율")]
    [SerializeField][Range(0f, 0.5f)] private float dataPaddingRatio = 0.0f;

    [Header("스크롤")]
    [SerializeField] private int scrollStep = 2;            // 휠 한 칸당 이동하는 캔들 수
    [SerializeField] private bool invertScroll = false;

    [Header("Colors")]
    [SerializeField] private Color upColor = Color.red;     // 상승
    [SerializeField] private Color downColor = Color.blue;  // 하락
    [SerializeField] private Color flatColor = Color.gray;  // 변화X

    [Header("Item Info")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemDescription;

    [Header("가격 표시")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text maxPriceText;
    [SerializeField] private TMP_Text midPriceText;
    [SerializeField] private TMP_Text minPriceText;

    private readonly List<UICandle> candleViews = new List<UICandle>();

    private int currentItemID = -1;
    private int scrollOffset;
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
        if (subscribed)
            return;

        MarketManager.onPricesUpdated += OnPricesUpdated;

        subscribed = true;
    }

    private void UnSubscribe()
    {
        if (!subscribed) 
            return;

        MarketManager.onPricesUpdated -= OnPricesUpdated;

        subscribed = false;
    }

    private void OnPricesUpdated()
    {
        if (currentItemID >= 0)
            DrawChart(currentItemID);
    }
    #endregion

    #region Scroll
    public void OnScroll(PointerEventData eventData)
    {
        if (currentItemID < 0)
            return;

        float delta = invertScroll ? -eventData.scrollDelta.y : eventData.scrollDelta.y;

        if (Mathf.Approximately(delta, 0f))
            return;

        // 휠 위로 = 과거로, 아래로 = 최신으로
        scrollOffset += (delta > 0f ? 1 : -1) * Mathf.Max(1, scrollStep);

        DrawChart(currentItemID);
    }
    #endregion


    public void DrawChart(int itemID)
    {
        // 다른 아이템이면 맨 최근부터 보여줌
        if (itemID != currentItemID)
            scrollOffset = 0;

        currentItemID = itemID;
        Subscribe();

        if (!dataManager.productClosingData.TryGetValue(itemID, out ProductClosing closing)
            || !closing.HasData)
        {
            Debug.LogWarning($"No data available for itemID : {itemID}");
            HideAllCandles();
            return;
        }

        // 보이는 구간 [start, end]
        // 최근 maxCandles일치만 그림 (마지막이 진행 중인 오늘 캔들)
        int total = closing.candles.Count;
        int maxOffset = Mathf.Max(0, total - visibleCandles);
        scrollOffset = Mathf.Clamp(scrollOffset, 0, maxOffset);

        int end = total - scrollOffset;
        int start = Mathf.Max(0, end - visibleCandles);
        int count = end - start;

        // 보이는 구간의 최저/최고
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

        CalcRange(itemID, lowest, highest, out float minY, out float maxY);

        EnsureCandleViews();

        float widthArea = chartArea.rect.width;
        float heightArea = chartArea.rect.height;

        float cellWidth = widthArea / visibleCandles;
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
            int slot = visibleCandles - count + i;
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

        SetPriceLabels(minY, maxY);
        SetItemInfo(itemID, closing);
    }

    // Y축 범위 계산: 중심 = 보이는 구간의 중간값, 폭 = max(기본가 x 비율, 실제 폭 + 여백)
    private void CalcRange(int itemID, int lowest, int highest, out float minY, out float maxY)
    {
        float center = (lowest + highest) * 0.5f;

        float basicPrice = 0f;
        if (dataManager.itemsData.TryGetValue(itemID, out ItemData itemData))
            basicPrice = itemData.basicPrice;

        // 기본 가격을 모르면 현재 중심값을 기준으로 함
        float reference = basicPrice > 0f ? basicPrice : center;

        float minSpan = Mathf.Max(reference * minRangeRatio, 1f);
        float dataSpan = (highest - lowest) * (1f + dataPaddingRatio * 2f);

        float span = Mathf.Max(minSpan, dataSpan);

        minY = center - span * 0.5f;
        maxY = center + span * 0.5f;

        // 가격은 0 아래로 내려가지 않음
        if (minY < 0f)
        {
            maxY -= minY;
            minY = 0f;
        }
    }

    private void SetPriceLabels(float minY, float maxY)
    {
        if (maxPriceText != null) maxPriceText.text = Mathf.RoundToInt(maxY).ToString();
        if (midPriceText != null) midPriceText.text = Mathf.RoundToInt((minY + maxY) * 0.5f).ToString();
        if (minPriceText != null) minPriceText.text = Mathf.RoundToInt(minY).ToString();
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
        while (candleViews.Count < visibleCandles)
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

        SetPriceLabels(0, 0);
    }

    private void SetItemInfo(int itemID, ProductClosing closing)
    {
        // 정보 텍스트는 항상 "오늘" 기준 (스크롤해도 현재 시세)
        PriceCandle today = closing.Today;

        // 전일 종가 대비 등락
        int nowPrice = today.close;
        int diff = nowPrice - closing.PreviousClose;
        float percent = closing.PreviousClose > 0 ? diff * 100f / closing.PreviousClose : 0f;

        Color priceColor = diff > 0 ? upColor : diff < 0 ? downColor : flatColor;
        string colorHex = ColorUtility.ToHtmlStringRGB(priceColor);

        string arrow = diff > 0 ? "▲" : diff < 0 ? "▼" : "■";

        string itemName = dataManager.itemsData.TryGetValue(itemID, out ItemData itemData)
            ? itemData.name : itemID.ToString();

        // 아이템 이미지
        itemImage.sprite = dataManager.GetItemImage(itemID);

        string upHex = ColorUtility.ToHtmlStringRGB(upColor);
        string downHex = ColorUtility.ToHtmlStringRGB(downColor);

        // 아이템 텍스트
        itemNameText.text = $"{itemData.name}";
        itemDescription.text =
            $"<color=#{colorHex}>{nowPrice}{arrow}{Mathf.Abs(diff)} ({percent:+0.0;-0.0;0.0}%)</color>\n" +
            $"<color=#{upHex}>최고 {today.high}</color>\n" +
            $"<color=#{downHex}> 최저 {today.low}</color>";

        if (scrollOffset > 0)
            itemDescription.text += $"\n<size=70%>과거 보는 중 ({scrollOffset}일 전)</size>";
    }
}
