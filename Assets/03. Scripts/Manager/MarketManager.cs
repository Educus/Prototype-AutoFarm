using UnityEngine;
using System;
using System.Collections.Generic;

#region MarketItemState
public enum MarketState
{
    Normal,
    Event,
    MeanReversion
}

[Serializable]
public class MarketEvent
{
    // EventData의 ID
    public int eventID;

    // 이벤트가 발생한 작물
    public int itemID;

    // 이벤트의 평균값 M
    public float mean;

    // 이벤트 시작 날짜
    public int startDay;

    // 이벤트 종료 날짜
    public int endDay;

    // 이벤트 시작 당시 가격
    public float startPrice;

    // 목표가격 계산에 사용한 평균 Z
    public float averageZ;

    // 이벤트 목표가격
    public float targetPrice;

    // 연계 이벤트인지
    public bool isFollowUp;
}

[Serializable]
public class MarketItemState
{
    public int itemID;

    public MarketState state = MarketState.Normal;

    // 현재 진행 중인 이벤트
    public MarketEvent currentEvent;
}

#endregion

public class MarketManager : MonoBehaviour
{
    // 가격변동을 담당
    [SerializeField] DataManager dataManager;
    [SerializeField] TimeManager timeManager;

    // 시세 저장 기간(최소 28일 한달)
    [SerializeField][Min(28)] private int closingPriceDays = 28;
    // 이벤트 진행 기간
    [SerializeField][Min(1)] private int eventDurationDays = 7;
    // 이벤트 목표가격 계산에 사용할 기간
    [SerializeField][Min(1)] private int eventCalculationDays = 7;
    // 일주일에 발생하는 이벤트 개수
    [SerializeField][Min(1)] private int weeklyEventCount = 2;

    // 이벤트 종료 후 새로운 이벤트를 발생시키는 기준
    [SerializeField] private float newEventDifferenceRate = 1.5f;
    // 평균회귀 강도
    [SerializeField] private float meanReversionStrength = 0.1f;

    // 하루에 시세가 변하는 횟수 (24시간 기준)
    private const int TicksPerDay = 24;
    // 보관할 최소 일 수
    private const int MinCandleDays = 28;

    private Dictionary<int, MarketItemState> marketStates = new Dictionary<int, MarketItemState>();

    #region Unity
    private void Start()
    {
        InitializeMarketStates();
        SeedInitialPrices();

        timeManager.onHourEvent += OnHourTick;
        timeManager.onDayEvent += StartNewDay;
        timeManager.onWeekEvent += ProcessWeeklyEvent;
    }

    private void OnDestroy()
    {
        if (timeManager == null) return;

        timeManager.onHourEvent -= OnHourTick;
        timeManager.onDayEvent -= StartNewDay;
        timeManager.onWeekEvent -= ProcessWeeklyEvent;
    }
    #endregion

    #region Initialize
    private void InitializeMarketStates()
    {
        marketStates.Clear();

        foreach (var pair in dataManager.productsData)
        {
            int itemID = pair.Key;

            marketStates[itemID] = new MarketItemState
            {
                itemID = itemID,
                state = MarketState.Normal
            };
        }
    }

    private void SeedInitialPrices()
    {
        foreach (var pair in dataManager.productClosingData)
        {
            if (!dataManager.itemsData.TryGetValue(pair.Key, out ItemData itemData))
            {
                Debug.LogError($"SeedInitialPrices 실패: itemsData에 itemID={pair.Key}가 없습니다.");

                continue;
            }

            ProductClosing closing = pair.Value;

            if (closing.HasData)
                continue;

            closing.candles.Add(new PriceCandle(itemData.basicPrice));
            closing.exactPrice = itemData.basicPrice;
        }
    }
    #endregion

    #region OnHourEvent / OnDayEvent
    // 1시간마다 : 오늘의 종가/고가/저가 갱신
    private void OnHourTick(int hour)
    {
        foreach (var pair in dataManager.productClosingData)
        {
            int itemID = pair.Key;
            ProductClosing closing = pair.Value;

            if (!closing.HasData)
                continue;

            if (!dataManager.productsData.TryGetValue(itemID, out Product product))
                continue;

            if (!marketStates.TryGetValue(itemID, out MarketItemState state))
                continue;

            // M을 하루에 한번 변하는 만큼
            float mean = GetCurrentMean(state) / TicksPerDay;
            float stdDev = product.priceStdDev / Mathf.Sqrt(TicksPerDay);

            float newPrice = GetNextPrice(closing.exactPrice, mean, stdDev);

            Debug.Log($"[Market] {itemID} 이전={closing.exactPrice:F2} " 
                + $"mean={mean:F4} stdDev={stdDev:F4} " 
                + $"→ 새가격={newPrice:F2} (candles={closing.candles.Count})");

            closing.ApplyPrice(newPrice);

            // 평균회귀 상태 확인
            CheckMeanReversion(state, closing.CurrentPrice);
        }
    }

    // 매일 00시 어제 종가를 시가로 주식 그래프 시작
    public void StartNewDay(int day)
    {
        int maxDays = Mathf.Max(MinCandleDays, closingPriceDays);

        foreach (var closing in dataManager.productClosingData.Values)
        {
            closing.StartNewDay(maxDays);
        }
    }

    private float GetCurrentMean(MarketItemState state)
    {
        switch (state.state)
        {
            case MarketState.Normal:

                return 0f;


            case MarketState.Event:

                if (state.currentEvent == null)
                    return 0f;

                return state.currentEvent.mean;


            case MarketState.MeanReversion:

                return GetMeanReversionMean(
                    state
                );


            default:

                return 0f;
        }
    }

    // 가격 변동 공식
    float GetNextPrice(float currentPrice, float mean, float stdDev)
    {
        float rand = GetStandardNormal();

        float exponent = mean + stdDev * rand;
        float multiplier = Mathf.Exp(exponent);

        // 변동 제한 (최대 20% 상승 또는 하락) // 필요 없으면 삭제
        // multiplier = Mathf.Clamp(multiplier, 0.8f, 1.2f);

        float newPrice = currentPrice * multiplier;

        // 최소 가격 제한 (1 이상)
        return Mathf.Max(1f, newPrice);
    }

    // 정규 분포 난수 생성 (Box-Muller 변환)
    float GetStandardNormal()
    {
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;

        return Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
               Mathf.Sin(2.0f * Mathf.PI * u2);
    }
    #endregion

    #region 평균회귀 CheckMeanReversion
    private void CheckMeanReversion(MarketItemState state, float currentPrice)
    {
        // 이벤트 중에는 평균회귀 진입X
        if (state.state == MarketState.Event)
            return;

        ItemData itemData = dataManager.itemsData[state.itemID];
        Product product = dataManager.productsData[state.itemID];

        float basicPrice = itemData.basicPrice;

        float stdDev = product.priceStdDev;


        // 평균회귀 진입
        if (state.state == MarketState.Normal)
        {
            float upper = basicPrice * (1f + stdDev * 2f);

            float lower = basicPrice * (1f - stdDev * 2f);

            if (currentPrice >= upper || currentPrice <= lower)
            {
                state.state = MarketState.MeanReversion;
            }
        }

        // 평균회귀 종료
        else if (state.state == MarketState.MeanReversion)
        {
            float upper = basicPrice * (1f + stdDev);

            float lower = basicPrice * (1f - stdDev);

            if (currentPrice <= upper && currentPrice >= lower)
            {
                state.state = MarketState.Normal;
            }
        }
    }

    // 하루 기준 평균회귀 값 (OnHourTick에서 24로 나눠서 사용)
    private float GetMeanReversionMean(MarketItemState state)
    {
        ItemData itemData = dataManager.itemsData[state.itemID];

        float basicPrice = itemData.basicPrice;

        float currentPrice = GetCurrentPrice(state.itemID);

        if (basicPrice <= 0f)
            return 0f;

        float difference = (basicPrice - currentPrice) / basicPrice;

        return difference * meanReversionStrength;
    }

    private float GetCurrentPrice(int itemID)
    {
        ProductClosing closing = dataManager.productClosingData[itemID];

        if (!closing.HasData)
        {
            return dataManager.itemsData[itemID].basicPrice;
        }

        return closing.CurrentPrice;
    }
    #endregion

    #region Event
    private void CreateMarketEvent(int itemID, int eventID, int currentDay, bool isFollowUp = false)
    {
        // 이벤트 데이터 확인
        if (!dataManager.eventsData.TryGetValue(eventID, out EventData eventData))
        {
            Debug.LogWarning($"존재하지 않는 EventID : {eventID}");
            return;
        }

        // 상품 상태
        MarketItemState state = marketStates[itemID];

        // 현재 가격
        float currentPrice = GetCurrentPrice(itemID);

        // 상품의 표준편차
        float stdDev = dataManager.productsData[itemID].priceStdDev;

        // 이벤트 계산용 평균 Z
        float averageZ = GetEventAverageZ();

        // 목표가격 계산
        float targetPrice = CalculateTargetPrice(currentPrice, eventData.average, stdDev, averageZ);

        // 실제 이벤트 생성
        MarketEvent marketEvent = new MarketEvent();

        marketEvent.eventID = eventID;

        marketEvent.itemID = itemID;

        // EventData.average = M
        marketEvent.mean = eventData.average;

        marketEvent.startDay = currentDay;

        marketEvent.endDay = currentDay + eventDurationDays;

        marketEvent.startPrice = currentPrice;

        marketEvent.averageZ = averageZ;

        marketEvent.targetPrice = targetPrice;

        marketEvent.isFollowUp = isFollowUp;

        // 상태 저장
        state.currentEvent = marketEvent;

        state.state = MarketState.Event;

        Debug.Log(
            $"시장 이벤트 발생 : " +
            $"ItemID={itemID}, " +
            $"EventID={eventID}, " +
            $"Target={targetPrice}"
        );
    }

    // 이벤트 목표가격 계산
    private float CalculateTargetPrice(float previousPrice, float mean, float stdDev, float averageZ)
    {
        int duration = Mathf.Max(1, eventDurationDays);

        return previousPrice * Mathf.Exp((mean + stdDev * averageZ) * duration);
    }

    // 이벤트 목표가격 계산에 사용할 평균 Z 계산
    private float GetEventAverageZ()
    {
        int count = Mathf.Max(1, eventCalculationDays);

        float total = 0f;

        for (int i = 0; i < count; i++)
        {
            total += GetStandardNormal();
        }

        return total / count;
    }

    // 이벤트 목표가격과 현재 가격의 차이율 계산
    private float CalculateDifferenceRate(float targetPrice, float currentPrice)
    {
        if (targetPrice <= 0f)
            return 0f;

        return ((targetPrice - currentPrice) / targetPrice) * 100f;
    }

    // 이벤트 종료 처리
    private void ProcessFinishedEvents(int currentDay)
    {
        foreach (var pair in marketStates)
        {
            MarketItemState state = pair.Value;

            // 이벤트가 아니면 무시
            if (state.currentEvent == null)
            {
                continue;
            }

            MarketEvent marketEvent = state.currentEvent;

            // 아직 이벤트 기간이 끝나지 않음
            if (currentDay < marketEvent.endDay)
            {
                continue;
            }

            ProcessEventResult(state, marketEvent, currentDay);
        }
    }

    // 이벤트 결과 처리
    private void ProcessEventResult(MarketItemState state, MarketEvent marketEvent, int currentDay)
    {
        // 이벤트 데이터
        if (!dataManager.eventsData.TryGetValue(marketEvent.eventID, out EventData eventData))
        {
            EndEvent(state);
            return;
        }


        // 1. 연계 이벤트가 있는 경우
        if (eventData.linkage != 0)
        {
            CreateMarketEvent(state.itemID, eventData.linkage, currentDay, true);

            return;
        }

        // 2. 일반 이벤트 결과 판정
        float currentPrice = GetCurrentPrice(state.itemID);

        float differenceRate = CalculateDifferenceRate(marketEvent.targetPrice, currentPrice);

        EvaluateEventResult(state, marketEvent, differenceRate, currentDay);
    }

    // 여기서부터
    // 차액비율 판정
    private void EvaluateEventResult(MarketItemState state, MarketEvent marketEvent, 
                                        float differenceRate, int currentDay)
    {
        float S = dataManager.productsData[state.itemID].priceStdDev * 100f;


        // 목표보다 크게 벗어난 경우
        if (differenceRate < -S)
        {
            EndEvent(state);
            return;
        }


        // 목표가격 범위에 들어온 경우
        if (differenceRate < S)
        {
            EndEvent(state);
            return;
        }


        // 목표 미달 → 이벤트 연장
        ExtendEvent(
            state,
            marketEvent,
            currentDay
        );
    }

    // 이벤트 연장
    private void ExtendEvent(MarketItemState state, MarketEvent oldEvent, int currentDay)
    {
        float currentPrice =
            GetCurrentPrice(state.itemID);

        float stdDev =
            dataManager.productsData[state.itemID].priceStdDev;

        float averageZ =
            GetEventAverageZ();

        MarketEvent newEvent =
            new MarketEvent();

        newEvent.eventID =
            oldEvent.eventID;

        newEvent.itemID =
            oldEvent.itemID;

        newEvent.mean =
            oldEvent.mean;

        newEvent.startDay =
            currentDay;

        newEvent.endDay =
            currentDay + eventDurationDays;

        newEvent.startPrice =
            currentPrice;

        newEvent.averageZ =
            averageZ;

        newEvent.targetPrice =
            CalculateTargetPrice(
                currentPrice,
                oldEvent.mean,
                stdDev,
                averageZ
            );

        newEvent.isFollowUp =
            true;

        state.currentEvent =
            newEvent;

        state.state =
            MarketState.Event;
    }

    // 이벤트 종료
    private void EndEvent(MarketItemState state)
    {
        state.currentEvent = null;

        state.state = MarketState.Normal;
    }
    #endregion

    #region OnWeekEvent
    // 주간 이벤트
    private void ProcessWeeklyEvent(int week)
    {
        int currentDay = timeManager.day;

        // 먼저 종료된 이벤트 처리
        ProcessFinishedEvents(currentDay);

        // 이벤트 후보 가져오기
        List<int> candidates =
            GetEventCandidates();

        // 후보가 부족하면 종료
        if (candidates.Count == 0)
            return;

        // 실제 발생 개수
        int eventCount =
            Mathf.Min(
                weeklyEventCount,
                candidates.Count
            );

        for (int i = 0; i < eventCount; i++)
        {
            // 랜덤 후보
            int randomIndex =
                UnityEngine.Random.Range(0, candidates.Count);

            int itemID =
                candidates[randomIndex];

            // 중복 방지
            candidates.RemoveAt(randomIndex);

            // 랜덤 이벤트
            int eventID = GetRandomEventID();

            CreateMarketEvent(
                itemID,
                eventID,
                currentDay
            );
        }
    }

    // 이벤트 후보 가져오기
    private List<int> GetEventCandidates()
    {
        List<int> candidates =
            new List<int>();

        foreach (var pair in dataManager.productsData)
        {
            int itemID = pair.Key;

            MarketItemState state =
                marketStates[itemID];

            // 이미 이벤트 중이면 제외
            if (state.state == MarketState.Event)
                continue;

            candidates.Add(itemID);
        }

        return candidates;
    }

    // 랜덤 이벤트 선택
    private int GetRandomEventID()
    {
        if (dataManager.eventsData.Count == 0)
        {
            return -1;
        }

        List<int> eventIDs =
            new List<int>(dataManager.eventsData.Keys);

        int randomIndex =
            UnityEngine.Random.Range(0, eventIDs.Count);

        return eventIDs[randomIndex];
    }
    #endregion

    #region Save / Load
    // 저장 : 상품별 시장 상태 + 상품별 주식 차트 + exactPrice
    public MarketSaveData GetSaveData()
    {
        var data = new MarketSaveData();

        foreach (var state in marketStates.Values)
        {
            data.states.Add(new MarketItemSaveData
            {
                itemID = state.itemID,
                state = state.state,

                hasEvent = state.currentEvent != null,
                currentEvent = state.currentEvent ?? new MarketEvent()
            });
        }

        foreach (var pair in dataManager.productClosingData)
        {
            var entry = new ClosingPriceSaveData
            {
                itemID = pair.Key,
                exactPrice = pair.Value.exactPrice,
                candles = new List<PriceCandle>()
            };

            foreach (var candle in pair.Value.candles)
                entry.candles.Add(candle.Copy());

            data.closingPrices.Add(entry);
        }

        return data;
    }

    // Start 이후에 호출
    public void Load(MarketSaveData data)
    {
        if (data == null)
            return;

        foreach (var s in data.states)
        {
            if (!marketStates.TryGetValue(s.itemID, out MarketItemState state))
                continue;

            state.state = s.state;
            state.currentEvent = s.hasEvent ? s.currentEvent : null;
        }

        foreach (var c in data.closingPrices)
        {
            if (!dataManager.productClosingData.TryGetValue(c.itemID, out ProductClosing closing))
                continue;

            closing.candles.Clear();

            foreach (var candle in c.candles)
                closing.candles.Add(candle.Copy());

            closing.exactPrice = c.exactPrice;

            // 저장된 값이 없을 경우 기본 가격
            if (!closing.HasData && dataManager.itemsData.TryGetValue(c.itemID, out ItemData itemData))
            {
                closing.candles.Add(new PriceCandle(itemData.basicPrice));
                closing.exactPrice = itemData.basicPrice;
            }
        }
    }
    #endregion
}