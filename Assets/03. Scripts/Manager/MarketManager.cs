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

    // 종가 저장 기간(최소 7일)
    [SerializeField][Min(7)] private int closingPriceDays = 7;
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


    private Dictionary<int, MarketItemState> marketStates = new Dictionary<int, MarketItemState>();

    #region Unity
    private void Start()
    {
        InitializeMarketStates();

        timeManager.onDayEvent += UpdatePrice;
        timeManager.onWeekEvent += ProcessWeeklyEvent;

        UpdatePrice(0);
    }

    private void OnDestroy()
    {
        if (timeManager == null) return;

        timeManager.onDayEvent -= UpdatePrice;
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
    #endregion

    #region OnDayEvent
    public void UpdatePrice(int day)
    {
        foreach (var pair in dataManager.productClosingData)
        {
            int itemID = pair.Key;

            ProductClosing closing = pair.Value;

            Product product =
                dataManager.productsData[itemID];


            // 최초 가격
            if (closing.productsClosingPrice.Count == 0)
            {
                closing.productsClosingPrice.Add(
                    product.basicCost
                );

                continue;
            }


            // 전일 종가
            float beforePrice =
                closing.productsClosingPrice[
                    closing.productsClosingPrice.Count - 1
                ];

            // 현재 시장 상태
            MarketItemState state =
                marketStates[itemID];

            // 현재 M
            float mean = GetCurrentMean(state);

            // 표준편차 S
            float stdDev = product.priceStdDev;

            // 새로운 가격
            int newPrice =
                Mathf.RoundToInt(
                    GetNextPrice(
                        beforePrice,
                        mean,
                        stdDev
                    )
                );

            // 종가 저장
            closing.productsClosingPrice.Add(newPrice);

            // 설정된 기간만큼만 저장(최소 7일)
            TrimClosingPrices(closing);

            // 평균회귀 상태 확인
            CheckMeanReversion(state, newPrice);
        }

        /*
        foreach (var pair in dataManager.productClosingData)
        {
            // 데이터가 없을 경우
            if (pair.Value.productsClosingPrice.Count == 0)
            {
                // 초기 가격 설정 (기본 가격)
                pair.Value.productsClosingPrice.Add(dataManager.productsData[pair.Key].basicCost);
            }
            else
            {
                // 가격 변동
                // 1.가격 변동 후 = 변동 전 * (1 + 평균값 + 표준편차 * 난수)
                // 2.가격 변동 후 = 변동 전 * e^(평균값 + 표준편차 * 난수)

                // 변동 전 가격
                float beforePrice = pair.Value.productsClosingPrice[pair.Value.productsClosingPrice.Count - 1];
                // 평균값
                float mean = 0f;
                // float mean = dataManager.eventsData[-1].average;
                // 표준편차
                float stdDev = dataManager.productsData[pair.Key].priceStdDev;

                // 변동 후 가격
                int newPrice = (int)GetNextPrice(beforePrice, mean, stdDev);

                // 종가 업데이트
                pair.Value.productsClosingPrice.Add(newPrice);

                // 7일치 가격만 저장
                if (pair.Value.productsClosingPrice.Count > 7)
                {
                    pair.Value.productsClosingPrice.RemoveAt(0);
                }
            }
        }
        */
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

    #region Save Closing Prices
    private void TrimClosingPrices(ProductClosing closing)
    {
        // 최소 7일 보장
        int maxDays = Mathf.Max(7, closingPriceDays);

        while (closing.productsClosingPrice.Count > maxDays)
        {
            closing.productsClosingPrice.RemoveAt(0);
        }
    }
    #endregion

    #region CheckMeanReversion
    private void CheckMeanReversion(MarketItemState state, float currentPrice)
    {
        // 이벤트 중에는 평균회귀 진입하지 않음
        if (state.state == MarketState.Event)
            return;

        Product product = dataManager.productsData[state.itemID];

        float basicPrice = product.basicCost;

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

    private float GetMeanReversionMean(MarketItemState state)
    {
        Product product = dataManager.productsData[state.itemID];

        float basicPrice = product.basicCost;

        float currentPrice = GetCurrentPrice(state.itemID);

        if (basicPrice <= 0f)
            return 0f;

        float difference = (basicPrice - currentPrice) / basicPrice;

        return difference * meanReversionStrength;
    }

    private float GetCurrentPrice(int itemID)
    {
        ProductClosing closing = dataManager.productClosingData[itemID];

        if (closing.productsClosingPrice.Count == 0)
        {
            return dataManager.productsData[itemID].basicCost;
        }

        return closing.productsClosingPrice[closing.productsClosingPrice.Count - 1];
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
    private void EvaluateEventResult(
    MarketItemState state,
    MarketEvent marketEvent,
    float differenceRate,
    int currentDay)
    {
        float S =
            dataManager
                .productsData[state.itemID]
                .priceStdDev
            * 100f;


        // =====================================================
        // 목표보다 크게 벗어난 경우
        // =====================================================

        if (differenceRate < -S)
        {
            EndEvent(state);
            return;
        }


        // =====================================================
        // 목표가격 범위에 들어온 경우
        // =====================================================

        if (differenceRate < S)
        {
            EndEvent(state);
            return;
        }


        // =====================================================
        // 목표 미달
        // → 이벤트 연장
        // =====================================================

        ExtendEvent(
            state,
            marketEvent,
            currentDay
        );
    }

    // 이벤트 연장
    private void ExtendEvent(
    MarketItemState state,
    MarketEvent oldEvent,
    int currentDay)
    {
        float currentPrice =
            GetCurrentPrice(
                state.itemID
            );


        float stdDev =
            dataManager
                .productsData[state.itemID]
                .priceStdDev;


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
            currentDay +
            eventDurationDays;


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
    private void EndEvent(
    MarketItemState state)
    {
        state.currentEvent = null;

        state.state =
            MarketState.Normal;
    }
    #endregion

    #region OnWeekEvent
    // =========================================================
    // ⑥ 주간 이벤트
    // =========================================================

    private void ProcessWeeklyEvent(
        int currentDay)
    {
        // 먼저 종료된 이벤트 처리
        ProcessFinishedEvents(
            currentDay
        );


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
                UnityEngine.Random.Range(
                    0,
                    candidates.Count
                );


            int itemID =
                candidates[randomIndex];


            // 중복 방지
            candidates.RemoveAt(
                randomIndex
            );


            // 랜덤 이벤트
            int eventID =
                GetRandomEventID();


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


        foreach (
            var pair
            in dataManager.productsData
        )
        {
            int itemID =
                pair.Key;


            MarketItemState state =
                marketStates[itemID];


            // 이미 이벤트 중이면 제외
            if (
                state.state ==
                MarketState.Event
            )
            {
                continue;
            }


            candidates.Add(
                itemID
            );
        }


        return candidates;
    }

    // 랜덤 이벤트 선택
    private int GetRandomEventID()
    {
        if (
            dataManager.eventsData.Count == 0
        )
        {
            return -1;
        }


        List<int> eventIDs =
            new List<int>(
                dataManager.eventsData.Keys
            );


        int randomIndex =
            UnityEngine.Random.Range(
                0,
                eventIDs.Count
            );


        return eventIDs[randomIndex];
    }
    #endregion
}