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

    // 시간당 평균 값
    // 연장 시 부호반전 확률 있음
    public float mean;

    // 처음 발생 시 평균 값
    public float baseMean;

    // 이벤트 시작 날짜
    public int startDay;

    // 이벤트 처음 시작 날짜(쿨타임)
    public int firstDay;

    // 이벤트 시작 당시 가격
    public float startPrice;

    // 이벤트 목표가격
    public float targetPrice;

    // 연계 이벤트인지
    public bool isFollowUp;

    // 긴급 이벤트의 총 기간(일). 일일 이벤트는 0
    public int durationDays;

    // 이벤트가 진행된 연속 일 수(시작날 = 1)
    public int consecutiveDays;
}

// 가상의 지지 / 저항 선
[Serializable]
public class MarketLine
{
    public float price;

    // 선 강도
    public float strength;

    // 가상 선
    public bool isVirtual;

    // 가격을 막은 횟수
    public int blockCount;

    // 마지막으로 돌파를 판정한 날 / 방향
    public int judgedDay;
    public int judgedDir;

    // 가상 선이 막힌 경우 : 이 날(다음 장날)이 지나면 삭제 (0 = 없음)
    public int expireDay;
}

[Serializable]
public class MarketItemState
{
    public int itemID;

    public MarketState state = MarketState.Normal;

    // 현재 진행 중인 이벤트
    public MarketEvent currentEvent;

    // 가격 변동 소문 (표준편차만 변동)
    public bool isRumor;
    public int rumorEventID;

    public float reversionMean;


    // 긴급 이벤트
    // 마지막 긴급 이벤트
    public int lastUrgentID;

    // 마지막 긴급 이벤트 시작날
    public int lastUrgentStartDay;

    // 연계 이벤트
    public List<int> usedLinkedIDs = new List<int>();

    // 지지 / 저항 선
    public List<MarketLine> lines = new List<MarketLine>();

    public List<float> urgentCloses = new List<float>();
    public int virtualClearDay;

    // 긴급 종료 후 유예 기간
    public bool graceActive;
    public int graceEndDay;
}

public struct MarketNews
{
    public int itemID;
    public int eventID;
}
#endregion

[DefaultExecutionOrder(-50)]
public class MarketManager : MonoBehaviour
{
    // 가격변동을 담당
    private TimeManager timeManager;
    private static OriginDataManager Origin => OriginDataManager.Instance;

    // 시세 저장 기간 : ConfigDataTable.DaysPerYear
    private static int closingPriceDays => OriginDataManager.Instance.Config.DaysPerYear;

    #region Daily Event
    [Header("Daily Event")]
    // 매일 9시 갱신
    // 이벤트 발생 확률
    [SerializeField][Range(0f, 1f)] private float dailyEventChance = 0.25f;
    // 이벤트 연장 확률(0.7^연속일수)
    [SerializeField][Range(0f, 1f)] private float extendBase = 0.7f;
    // 목표 미달 시 반전 확률
    [SerializeField] private float reverseRate = 0.3f;
    // 목표 초과 시 배율
    [SerializeField] private float overshootMultiplier = 1.3f;
    #endregion

    #region Urgent Event
    [Header("Urgent Event")]
    // 긴급 이벤트 발생 확률
    [SerializeField][Range(0f, 1f)] private float urgentEventChance = 0.1f;
    [SerializeField] private int urgentMinDays = 7;
    [SerializeField] private int urgentMaxDays = 14;
    // 긴급 이벤트 쿨타임 (시작일 기준)
    [SerializeField] private int urgentCooldownDays = 28;
    // 목표 가격 = 9시 가격 x e^(이 값 x 평균)
    [SerializeField] private float urgentTargetHours = 140f;
    // 일수 / 목표 도달시 조기 종료
    [SerializeField] private int urgentEarlyEndDay = 7;
    // 종료 후 평균 회귀 유예
    [SerializeField] private int graceDaysReached = 10;
    [SerializeField] private int graceDaysExpired = 7;
    #endregion

    #region Support / Resistance
    [Header("Support / Resistance")]
    // 정산 주기(일) : 14일차부터 2주마다
    [SerializeField] private int lineSettleDays = 14;
    // 표본 보관 일수
    [SerializeField] private int lineSampleDays = 28;
    // 가격대(구간) 폭 : 원가 x 이 값 x σ
    [SerializeField] private float lineBinWidth = 0.5f;
    // 선 강도 = 이 값 x (가격대 표본 수 / 전체 표본 수)
    [SerializeField] private float lineStrengthScale = 10f;
    // 선이 가격을 막을 때마다 강도 배율
    [SerializeField] private float lineBlockDecay = 0.7f;
    // 이벤트가 없거나 이벤트와 반대 방향일 때 (평균 회귀, 소문 포함) 돌파 힘
    [SerializeField] private float defaultBreakForce = 0.3f;
    // 가상 선 강도 배율 (실제 선 강도 공식 x 0.3)
    [SerializeField] private float virtualStrengthRate = 0.3f;
    // 평균 회귀 유예가 끝난 뒤 가상 선이 삭제 기간
    [SerializeField] private int virtualClearAfterGraceDays = 7;
    #endregion

    #region Rumor
    [Header("Rumor")]
    // 이벤트 없는 상품 한정
    // 소문이 적용 확률
    [SerializeField][Range(0f, 1f)] private float rumorChance = 0.25f;
    // 표준편차 배율
    [SerializeField] private float rumorStdDevMultiplier = 1.3f;
    #endregion

    // 하루에 시세가 변하는 횟수 (24시간 기준)
    private const int TicksPerDay = 24;

    // 9시 ~ 23시 변동
    private const int EventStartHour = 9;
    private const int EventEndHour = 23;
    private const int EventHours = EventEndHour - EventStartHour;

    // EventDataTable ID
    private const int UrgentEventMinID = 20000;

    private Dictionary<int, MarketItemState> marketStates = new Dictionary<int, MarketItemState>();
    public Dictionary<int, ProductClosing> productClosingData { get; } = new Dictionary<int, ProductClosing>();

    // 오늘의 뉴스, UIChart에서 구현
    private readonly List<MarketNews> todayNews = new List<MarketNews>();
    public IReadOnlyList<MarketNews> TodayNews => todayNews;

    public static event Action onNewsChanged;


    #region Unity
    private void Awake()
    {
        BuildClosingData();
    }
    private void Start()
    {
        timeManager = TimeManager.Instance;

        if (timeManager == null)
        {
            Debug.LogError("[MarketManager] TimeManager 가 없습니다.");
            enabled = false;
            return;
        }

        if (productClosingData.Count == 0)
            BuildClosingData();

        InitializeMarketStates();
        SeedInitialPrices();

        timeManager.onHourEvent += OnHourTick;
        timeManager.onDayEvent += StartNewDay;
    }

    private void OnDestroy()
    {
        if (timeManager == null) return;

        timeManager.onHourEvent -= OnHourTick;
        timeManager.onDayEvent -= StartNewDay;
    }
    #endregion

    #region Initialize
    private void BuildClosingData()
    {
        productClosingData.Clear();

        if (Origin == null)
        {
            Debug.LogError("[MarketManager] OriginDataManager.Instance 가 null 입니다.");
            return;
        }

        foreach (int itemID in Origin.productsData.Keys)
            productClosingData[itemID] = new ProductClosing();
    }

    private void InitializeMarketStates()
    {
        marketStates.Clear();

        foreach (var pair in Origin.productsData)
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
        foreach (var pair in productClosingData)
        {
            if (!Origin.TryGetItem(pair.Key, out ItemData itemData))
            {
                Debug.LogError($"SeedInitialPrices 실패: itemsData에 itemID={pair.Key}가 없습니다.");

                continue;
            }

            ProductClosing closing = pair.Value;

            if (closing.HasData)
                continue;

            closing.candles.Add(new PriceCandle(itemData.BasicPrice));
            closing.exactPrice = itemData.BasicPrice;
        }
    }
    #endregion

    #region OnHourEvent / OnDayEvent
    // 1시간마다 : 오늘의 종가/고가/저가 갱신
    public static event System.Action onPricesUpdated;

    private void OnHourTick(int hour)
    {
        if (IsMarketClosed(hour))
            return;

        bool isEventHour = IsEventHour(hour);

        foreach (var pair in productClosingData)
        {
            int itemID = pair.Key;
            ProductClosing closing = pair.Value;

            if (!closing.HasData)
                continue;

            if (!Origin.TryGetProduct(itemID, out Product product))
                continue;

            if (!marketStates.TryGetValue(itemID, out MarketItemState state))
                continue;

            // 시간당 평균 / 시간당 표준편차
            float mean = GetHourlyMean(state, isEventHour);
            float stdDev = product.priceStdDev / Mathf.Sqrt(TicksPerDay);

            // 소문이 적용된 상품의 표준편차
            if (state.isRumor && isEventHour)
                stdDev *= rumorStdDevMultiplier;

            float oldPrice = closing.exactPrice;
            float newPrice = GetNextPrice(oldPrice, mean, stdDev);

            newPrice = ApplyLines(state, closing, oldPrice, newPrice, isEventHour, product.priceStdDev / Mathf.Sqrt(TicksPerDay));

            closing.ApplyPrice(newPrice);
        }

        // 23시 목표 여부 판단
        if (hour == EventEndHour)
        {
            EvaluateDailyEvents();

            int marketDay = GetMarketDay(timeManager.day);

            if (marketDay >= lineSettleDays && marketDay % lineSettleDays == 0)
                SettleLines(timeManager.day);
        }

        // 9시 새 이벤트, 소문 생성
        if (hour == EventStartHour)
        {
            StartDailyEvents();
            UpdateMeanReversion();
        }

        onPricesUpdated?.Invoke();
    }

    // 휴장 요일
    // 0 = 월 ~ 6 = 일
    private bool IsMarketClosed(int hour)
    {
        int weekDay = timeManager.weekDay;
        return weekDay == 6 || (weekDay == 5 && hour == 24);
    }

    // 개장 시간(9 ~ 23시)
    private static bool IsEventHour(int hour)
    {
        return hour > EventStartHour && hour <= EventEndHour;
    }

    // 매일 00시 어제 종가를 시가로 주식 그래프 시작
    public void StartNewDay(int day)
    {
        int maxDays = closingPriceDays;

        foreach (var closing in productClosingData.Values)
        {
            closing.StartNewDay(maxDays);
        }

        onPricesUpdated?.Invoke();
    }

    // 시간당 평균
    private float GetHourlyMean(MarketItemState state, bool isEventHour)
    {
        switch (state.state)
        {
            case MarketState.Event:
                if (state.currentEvent == null || !isEventHour)
                    return 0f;

                return state.currentEvent.mean;

            case MarketState.MeanReversion:
                return isEventHour ? state.reversionMean : 0f;

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
    private const float ReversionDivisor = 28f;
    private void UpdateMeanReversion()
    {
        foreach (var state in marketStates.Values)
        {
            if (state.state == MarketState.Event)
            {
                state.reversionMean = 0f;
                continue;
            }

            if (IsInGrace(state, timeManager.day))
            {
                state.reversionMean = 0f;
                continue;
            }

            if (!Origin.TryGetProduct(state.itemID, out Product product))
                continue;

            float price = GetExactPrice(state.itemID);
            float target = GetReversionTarget(state, price);

            if (target <= 0f || price <= 0f)
                continue;

            // 표쥰편차
            float S = product.priceStdDev;

            if (state.state == MarketState.Normal)
            {
                // 발동 조건 : 목표 x (1 ± 2σ) 범위를 벗어남
                if (price >= target * (1f + 2f * S) || price <= target * (1f - 2f * S))
                    state.state = MarketState.MeanReversion;
            }
            else if (state.state == MarketState.MeanReversion)
            {
                // 종료 : 목표 x (1 ± σ) 범위 안에 들어옴
                if (price <= target * (1f + S) && price >= target * (1f - S))
                    state.state = MarketState.Normal;
            }

            if (state.state != MarketState.MeanReversion)
            {
                state.reversionMean = 0f;
                continue;
            }

            // M = |정규본포난수(0, 회귀표준편차)| , 종가 기준 가격이 목표보다 높으면 음수 / 낮으면 양수
            float reversionStdDev = Mathf.Abs(Mathf.Log(target / price)) / ReversionDivisor;
            float m = Mathf.Abs(GetStandardNormal() * reversionStdDev);

            state.reversionMean = price > target ? -m : m;
        }
    }

    // 회귀 목표 금액
    private float GetReversionTarget(MarketItemState state, float price)
    {
        if (!Origin.TryGetItem(state.itemID, out ItemData itemData))
            return 0f;

        float basePrice = itemData.BasicPrice;

        if (price <= basePrice * 0.5f || price >= basePrice * 2f)
            return basePrice;

        float target = basePrice;

        if (price > basePrice)
        {
            float best = 0f;

            foreach (var line in state.lines)
            {
                if (line.isVirtual || line.price >= price || line.price <= basePrice)
                    continue;

                if (line.price > best)
                    best = line.price;
            }

            if (best > 0f)
                target = best;
        }
        else if (price < basePrice)
        {
            float best = float.MaxValue;

            foreach (var line in state.lines)
            {
                if (line.isVirtual || line.price <= price || line.price >= basePrice)
                    continue;

                if (line.price < best)
                    best = line.price;
            }

            if (best < float.MaxValue)
                target = best;
        }

        return target;
    }

    // 정수 반올림 이전의 실제 가격 (이벤트 목표 계산 / 판정에 사용)
    private float GetExactPrice(int itemID)
    {
        if (productClosingData.TryGetValue(itemID, out ProductClosing closing) && closing.HasData)
            return closing.exactPrice;

        return Origin.TryGetItem(itemID, out ItemData itemData) ? itemData.BasicPrice : 0f;
    }
    #endregion

    #region Daily Event
    private void StartDailyEvents()
    {
        int day = timeManager.day;

        // 가상 선 삭제
        foreach (var state in marketStates.Values)
            state.lines.RemoveAll(l => l.isVirtual && l.expireDay > 0 && day > l.expireDay);

        // 소문 해제
        foreach (var state in marketStates.Values)
        {
            state.isRumor = false;
            state.rumorEventID = 0;
        }

        // 연장된 이벤트 목표 다시 계산
        foreach (var state in marketStates.Values)
        {
            MarketEvent e = state.currentEvent;

            if (e != null && e.eventID < UrgentEventMinID && e.startDay != day)
                RestartEvent(e, day);
        }

        List<int> pool = GetDailyEventPool();

        if (pool.Count > 0)
        {
            // 이벤트가 진행 중이 아닌 상품 이벤트 생성
            foreach (var state in marketStates.Values)
            {
                if (state.state == MarketState.Event)
                    continue;

                if (CanStartUrgent(state, day) && UnityEngine.Random.value < urgentEventChance)
                {
                    int urgentID = PickUrgentEventID(state);

                    if (urgentID != 0)
                    {
                        CreateUrgentEvent(state, urgentID, day);
                        continue;
                    }
                }

                if (UnityEngine.Random.value >= dailyEventChance)
                    continue;

                CreateDailyEvent(state.itemID, pool[UnityEngine.Random.Range(0, pool.Count)], day);
            }

            // 이벤트가 없는 상품 소문 생성
            foreach (var state in marketStates.Values)
            {
                if (state.state == MarketState.Event)
                    continue;

                if (UnityEngine.Random.value >= rumorChance)
                    continue;

                state.isRumor = true;
                state.rumorEventID = pool[UnityEngine.Random.Range(0, pool.Count)];
            }
        }

        RebuildNews();
    }

    // 일일 이벤트
    private List<int> GetDailyEventPool()
    {
        List<int> pool = new List<int>();

        foreach (var pair in Origin.eventsData)
        {
            if (pair.Key >= UrgentEventMinID)
                continue;

            if (pair.Value.Linkage != 0)
                continue;

            pool.Add(pair.Key);
        }

        return pool;
    }

    private void CreateDailyEvent(int itemID, int eventID, int day)
    {
        if (!Origin.TryGetEvent(eventID, out EventData eventData))
        {
            Debug.LogWarning($"존재하지 않는 EventID : {eventID}");
            return;
        }

        if (!marketStates.TryGetValue(itemID, out MarketItemState state))
            return;

        if (!Origin.TryGetProduct(itemID, out Product product))
            return;

        // 실제 평균 값 = 이벤트의 평균 값 * 상품의 시간당 표준편차
        float hourlyStdDev = product.priceStdDev / Mathf.Sqrt(TicksPerDay);
        float mean = eventData.Average * hourlyStdDev;

        MarketEvent marketEvent = new MarketEvent
        {
            eventID = eventID,
            itemID = itemID,
            mean = mean,
            baseMean = mean,
            consecutiveDays = 1,
            firstDay = day,
            isFollowUp = false
        };

        RestartEvent(marketEvent, day);

        state.currentEvent = marketEvent;
        state.state = MarketState.Event;

        Debug.Log($"일일 이벤트 발생 : ItemID:{itemID}, EventID:{eventID}");
    }

    // 목표 가격 = 종가 * e^(14 * 평균)
    private void RestartEvent(MarketEvent marketEvent, int day)
    {
        marketEvent.startDay = day;
        marketEvent.startPrice = GetExactPrice(marketEvent.itemID);
        marketEvent.targetPrice = marketEvent.startPrice * Mathf.Exp(EventHours * marketEvent.mean);
    }

    // 달성 여부
    private void EvaluateDailyEvents()
    {
        foreach (var state in marketStates.Values)
        {
            MarketEvent marketEvent = state.currentEvent;

            if (marketEvent == null)
                continue;

            if (marketEvent.eventID >= UrgentEventMinID)
                EvaluateUrgentEvent(state, marketEvent);
            else
                EvaluateDailyEvent(state, marketEvent);
        }
    }
    private void EvaluateDailyEvent(MarketItemState state, MarketEvent marketEvent)
    {
        if (!Origin.TryGetProduct(state.itemID, out Product product) || marketEvent.targetPrice <= 0f)
        {
            EndEvent(state);
            return;
        }

        float close = GetExactPrice(state.itemID);

        // 차액 비율 n (%) : 방향 * ((종가 - 목표금액) / 목표금액) * 100
        float n = Mathf.Sign(marketEvent.mean) * (close - marketEvent.targetPrice) / marketEvent.targetPrice * 100f;

        // 달성 범위 D = S x sqrt(14 / 24) x 100  (S = 하루 표준편차)
        float D = product.priceStdDev * Mathf.Sqrt((float)EventHours / TicksPerDay) * 100f;

        // 목표 달성 : -D <= n < D (후속 이벤트 없이 종료)
        if (n >= -D && n < D)
        {
            EndEvent(state);
            return;
        }

        marketEvent.consecutiveDays = GetElapsedDays(marketEvent);

        // 목표 미달 / 초과 : 0.7^(연속 일수) 확률로 연장
        float extendChance = Mathf.Pow(extendBase, marketEvent.consecutiveDays);

        if (UnityEngine.Random.value >= extendChance)
        {
            EndEvent(state);
            return;
        }

        if (n < -D)
        {
            // 목표 미달 : (|n| / D - 1) x 30% 확률로 평균 부호 반전, 아니면 같은 평균 값
            float reverseChance = Mathf.Clamp01((Mathf.Abs(n) / D - 1f) * reverseRate);

            if (UnityEngine.Random.value < reverseChance)
                marketEvent.mean = -marketEvent.mean;
        }
        else
        {
            // 목표 초과 : 원래 평균 값의 1.3배
            marketEvent.mean = Mathf.Sign(marketEvent.mean) * Mathf.Abs(marketEvent.baseMean) * overshootMultiplier;
        }

        marketEvent.isFollowUp = true;

        // 시작 가격 / 목표 가격은 다음 날 9시에 다시 계산됨 (RestartEvent)
    }

    // 이벤트 종료
    private void EndEvent(MarketItemState state)
    {
        state.currentEvent = null;

        state.state = MarketState.Normal;
    }
    #endregion

    #region Support / Resistance
    private float ApplyLines(MarketItemState state, ProductClosing closing, float oldPrice, float newPrice, bool isEventHour, float hourlyStdDev)
    {
        if (state.lines.Count == 0 || Mathf.Approximately(oldPrice, newPrice))
            return newPrice;

        int dir = newPrice > oldPrice ? 1 : -1;
        int day = timeManager.day;

        // 지지선
        List<MarketLine> crossed = state.lines.FindAll(l =>
           dir > 0 ? (l.price > oldPrice && l.price <= newPrice)
                   : (l.price < oldPrice && l.price >= newPrice));

        if (crossed.Count == 0)
            return newPrice;

        crossed.Sort((a, b) => dir > 0 ? a.price.CompareTo(b.price) : b.price.CompareTo(a.price));

        foreach (MarketLine line in crossed)
        {
            bool broken;

            // 이미 막힌 선은 같은 날 다시 막힘
            if (line.judgedDay == day && line.judgedDir == dir)
            {
                broken = false;
            }
            else
            {
                float force = GetBreakForce(state, line, dir, isEventHour, hourlyStdDev);
                float chance = force + line.strength > 0f ? force / (force + line.strength) : 1f;

                broken = UnityEngine.Random.value < chance;

                line.judgedDay = day;
                line.judgedDir = dir;

                if (!broken)
                {
                    line.blockCount++;

                    if (line.isVirtual)
                    {
                        // 막혀도 다음 장날까지만 유지
                        line.expireDay = NextMarketDay(day);
                    }
                    else
                    {
                        line.strength *= lineBlockDecay;
                    }
                }
            }

            if (broken)
            {
                state.lines.Remove(line);
                continue;
            }

            // 실패 시 선 안쪽에서 마감
            PriceCandle today = closing.Today;

            if (today != null)
            {
                int linePrice = Mathf.RoundToInt(line.price);

                if (dir > 0 && linePrice > today.high) today.high = linePrice;
                if (dir < 0 && linePrice < today.low) today.low = linePrice;
            }

            return line.price * (dir > 0 ? 0.9999f : 1.0001f);
        }

        return newPrice;
    }

    // 돌파력
    private float GetBreakForce(MarketItemState state, MarketLine line, int dir, bool isEventHour, float hourlyStdDev)
    {
        MarketEvent e = state.currentEvent;

        if (state.state != MarketState.Event || e == null || !isEventHour || Mathf.Sign(e.mean) != dir || hourlyStdDev <= 0f)
            return defaultBreakForce;

        float force = dir * (e.targetPrice - line.price) / (line.price * hourlyStdDev);

        return Mathf.Max(0f, force);
    }

    // 다음 장날 (일요일 휴장)
    private int NextMarketDay(int day)
    {
        int next = day + 1;

        if (next % 7 == 0)
            next++;

        return next;
    }

    // 가격이 속한 구간 번호 (원가 기준, 폭 = 원가 x 0.5 x σ)
    private int GetBin(float price, float basePrice, float width)
    {
        return Mathf.FloorToInt((price - basePrice) / width);
    }

    // 정산 : 최근 28장날 종가 표본 선
    private void SettleLines(int day)
    {
        int marketDay = GetMarketDay(day);

        foreach (var state in marketStates.Values)
        {
            // 평균 회귀 유예가 끝난 뒤 7장날이 지났으면 가상 선 전체 삭제
            if (state.virtualClearDay > 0 && marketDay >= state.virtualClearDay)
            {
                state.lines.RemoveAll(l => l.isVirtual);
                state.virtualClearDay = 0;
            }

            if (!productClosingData.TryGetValue(state.itemID, out ProductClosing closing) || !closing.HasData)
                continue;

            if (!Origin.TryGetItem(state.itemID, out ItemData itemData) || !Origin.TryGetProduct(state.itemID, out Product product))
                continue;

            float basePrice = itemData.BasicPrice;
            float width = basePrice * lineBinWidth * product.priceStdDev;

            // 표본 : 최근 28장날의 종가 (휴장일 제외)
            List<float> samples = new List<float>();
            int count = closing.candles.Count;

            for (int i = count - 1; i >= 0 && samples.Count < lineSampleDays; i--)
            {
                int candleDay = day - (count - 1 - i);

                if (candleDay % 7 == 0)
                    continue;

                samples.Add(closing.candles[i].close);
            }

            // 직전 실제 선
            List<MarketLine> oldLines = state.lines.FindAll(l => !l.isVirtual);
            state.lines.RemoveAll(l => !l.isVirtual);

            if (samples.Count == 0 || width <= 0f)
                continue;

            Dictionary<int, List<float>> bins = new Dictionary<int, List<float>>();

            foreach (float price in samples)
            {
                int bin = GetBin(price, basePrice, width);

                if (!bins.TryGetValue(bin, out List<float> list))
                    bins[bin] = list = new List<float>();

                list.Add(price);
            }

            foreach (var pair in bins)
            {
                int bin = pair.Key;
                int cnt = pair.Value.Count;

                int left = bins.TryGetValue(bin - 1, out List<float> l1) ? l1.Count : 0;
                int right = bins.TryGetValue(bin + 1, out List<float> r1) ? r1.Count : 0;

                // 양옆 구간보다 표본이 많은 구간 (표본 수가 같은 연속 구간은 오른쪽 끝 하나만)
                if (!(cnt >= left && cnt > right))
                    continue;

                // 양옆 구간과 합쳐 표본 2개 이상 (직전 선과 한 구간 이내면 1개 이상)
                bool nearOld = oldLines.Exists(o => Mathf.Abs(GetBin(o.price, basePrice, width) - bin) <= 1);

                if (left + cnt + right < (nearOld ? 1 : 2))
                    continue;

                // 선 가격 = 그 구간과 양옆 구간 표본의 평균
                float sum = 0f;
                int n = 0;

                for (int b = bin - 1; b <= bin + 1; b++)
                {
                    if (!bins.TryGetValue(b, out List<float> list))
                        continue;

                    foreach (float v in list) { sum += v; n++; }
                }

                MarketLine line = new MarketLine
                {
                    price = sum / n,
                    strength = lineStrengthScale * cnt / samples.Count
                };

                // 같은 가격대의 이전 선이 약화 시 그 비율을 적용
                MarketLine same = oldLines.Find(o => GetBin(o.price, basePrice, width) == bin);

                if (same != null)
                {
                    line.blockCount = same.blockCount;
                    line.strength *= Mathf.Pow(lineBlockDecay, same.blockCount);
                }

                state.lines.Add(line);
            }
        }
    }

    // 긴급 이벤트 종료 (이벤트 기간 가상 선 생성)
    private void CreateVirtualLines(MarketItemState state)
    {
        float strength = lineStrengthScale * (1f / lineSampleDays) * virtualStrengthRate;

        foreach (float close in state.urgentCloses)
        {
            state.lines.Add(new MarketLine
            {
                price = close,
                strength = strength,
                isVirtual = true
            });
        }

        state.urgentCloses.Clear();
    }
    #endregion

    #region Urgent Event
    // 긴급 이벤트 발생
    private bool CanStartUrgent(MarketItemState state, int day)
    {
        return state.lastUrgentStartDay == 0
            || GetMarketDay(day) - GetMarketDay(state.lastUrgentStartDay) >= urgentCooldownDays;
    }

    // 긴급 이벤트 목록 (긴급 ID 범위 + Linkage 0)
    private List<int> GetUrgentRootPool()
    {
        List<int> pool = new List<int>();

        foreach (var pair in Origin.eventsData)
        {
            if (pair.Key >= UrgentEventMinID && pair.Value.Linkage == 0)
                pool.Add(pair.Key);
        }

        return pool;
    }

    // parentID를 부모로 하는 연계 이벤트들
    private List<int> GetLinkedEvents(int parentID)
    {
        List<int> list = new List<int>();

        foreach (var pair in Origin.eventsData)
        {
            if (pair.Value.Linkage == parentID)
                list.Add(pair.Key);
        }

        return list;
    }

    // 이번에 발생할 긴급 이벤트 선정
    // 연계 이벤트가 없으면 목록 중 무작위
    // 직전 긴급 이벤트에 연계 이벤트가 있으면 연계로 확정
    // (여러 개면 무작위, 한 번 쓴 것은 제외, 남은 게 없으면 초기화)
    private int PickUrgentEventID(MarketItemState state)
    {
        if (state.lastUrgentID != 0)
        {
            List<int> linked = GetLinkedEvents(state.lastUrgentID);

            if (linked.Count > 0)
            {
                List<int> available = linked.FindAll(id => !state.usedLinkedIDs.Contains(id));

                if (available.Count > 0)
                {
                    int picked = available[UnityEngine.Random.Range(0, available.Count)];
                    state.usedLinkedIDs.Add(picked);

                    return picked;
                }
            }
        }

        // 연계 이벤트를 모두 썼으면 연계되지 않은 이벤트가 발생

        List<int> roots = GetUrgentRootPool();

        if (roots.Count > 0)
            return roots[UnityEngine.Random.Range(0, roots.Count)];

        // 발생 가능한 긴급 이벤트가 없으면 초기화 후 다시 선정
        state.usedLinkedIDs.Clear();

        List<int> all = new List<int>(GetLinkedEvents(state.lastUrgentID));

        return all.Count == 0 ? 0 : all[UnityEngine.Random.Range(0, all.Count)];
    }

    private void CreateUrgentEvent(MarketItemState state, int eventID, int day)
    {
        if (!Origin.TryGetEvent(eventID, out EventData eventData)
            || !Origin.TryGetProduct(state.itemID, out Product product))
            return;

        float hourlyStdDev = product.priceStdDev / Mathf.Sqrt(TicksPerDay);
        float mean = eventData.Average * hourlyStdDev;

        MarketEvent marketEvent = new MarketEvent
        {
            eventID = eventID,
            itemID = state.itemID,
            mean = mean,
            baseMean = mean,
            consecutiveDays = 1,
            firstDay = day,
            durationDays = UnityEngine.Random.Range(urgentMinDays, urgentMaxDays + 1),
            isFollowUp = false,
            startDay = day,
            startPrice = GetExactPrice(state.itemID)
        };

        // 긴급 이벤트 목표 가격 = 9시 가격 x e^(140 x 평균)
        marketEvent.targetPrice = marketEvent.startPrice * Mathf.Exp(urgentTargetHours * mean);

        state.currentEvent = marketEvent;
        state.state = MarketState.Event;
        state.reversionMean = 0f;
        state.graceActive = false;
        state.urgentCloses.Clear();
        state.virtualClearDay = 0;
        state.lastUrgentID = eventID;
        state.lastUrgentStartDay = day;

        Debug.Log($"긴급 이벤트 발생 : ItemID:{state.itemID}, EventID:{eventID}");
    }

    // 모든 기간은 장날 기준(휴장일 제외)
    private static int GetMarketDay(int day)
    {
        return day - day / 7;
    }

    // 이벤트가 시작한 날부터 오늘까지의 장날 수 (시작한 날 = 1)
    private int GetElapsedDays(MarketEvent marketEvent)
    {
        int first = marketEvent.firstDay > 0 ? marketEvent.firstDay : marketEvent.startDay;

        return Mathf.Max(1, GetMarketDay(timeManager.day) - GetMarketDay(first) + 1);
    }

    // 기간 만료 / 목표 도달 조기 종료 판정
    private void EvaluateUrgentEvent(MarketItemState state, MarketEvent marketEvent)
    {
        float close = GetExactPrice(state.itemID);

        // 이벤트 방향으로 목표 가격에 도달 (상승 이벤트 : 목표 이상 / 하락 이벤트 : 목표 이하)
        bool reached = Mathf.Sign(marketEvent.mean) * (close - marketEvent.targetPrice) >= 0f;

        state.urgentCloses.Add(close);

        marketEvent.consecutiveDays = GetElapsedDays(marketEvent);

        bool expired = marketEvent.consecutiveDays >= marketEvent.durationDays;
        bool earlyEnd = marketEvent.consecutiveDays >= urgentEarlyEndDay && reached;

        if (!expired && !earlyEnd)
            return;

        // 종료 후 평균 회귀 유예 : 목표 도달 10일 / 기간 만료 7일
        state.graceActive = true;
        state.graceEndDay = GetMarketDay(timeManager.day) + (reached ? graceDaysReached : graceDaysExpired);

        // 이벤트 기간의 종가로 가상 지지 / 저항 선 생성
        CreateVirtualLines(state);
        state.virtualClearDay = 0;

        EndEvent(state);
    }

    // 유예 중이면 true (평균 회귀 x)
    // 유예 기간이 끝난 뒤에는 매일 0.7^(경과 일수) 확률로 유예 유지
    private bool IsInGrace(MarketItemState state, int calendarDay)
    {
        if (!state.graceActive)
            return false;

        int day = GetMarketDay(calendarDay);

        if (day <= state.graceEndDay)
            return true;

        int elapsed = day - state.graceEndDay;

        if (UnityEngine.Random.value < Mathf.Pow(extendBase, elapsed))
            return true;

        // 유예 종료 : 7일이 지난 뒤 처음 진행되는 정산에서 가상 선 삭제
        state.graceActive = false;
        state.virtualClearDay = day + virtualClearAfterGraceDays;
        return false;
    }
    #endregion

    #region News (UI용)
    private void RebuildNews()
    {
        todayNews.Clear();

        foreach (var state in marketStates.Values)
        {
            if (!Origin.TryGetItem(state.itemID, out ItemData newsItem) || !newsItem.UseToDemo)
                continue;

            if (state.currentEvent != null)
            {
                todayNews.Add(new MarketNews { itemID = state.itemID, eventID = state.currentEvent.eventID });
            }
            else if (state.isRumor)
            {
                todayNews.Add(new MarketNews { itemID = state.itemID, eventID = state.rumorEventID });
            }
        }

        // 무작위 순서 (Fisher-Yates)
        for (int i = todayNews.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);

            (todayNews[i], todayNews[j]) = (todayNews[j], todayNews[i]);
        }

        onNewsChanged?.Invoke();
    }

    // 뉴스 제목 / 내용 ({ItemName}은 상품 이름으로 바뀜)
    public bool TryGetNewsText(MarketNews news, out string title, out string contents)
    {
        title = string.Empty;
        contents = string.Empty;

        if (!Origin.TryGetEvent(news.eventID, out EventData eventData))
            return false;

        string itemName = Origin.TryGetItem(news.itemID, out ItemData itemData) ? itemData.Name : string.Empty;

        title = (eventData.NewsTitle ?? string.Empty).Replace("{ItemName}", itemName);
        contents = (eventData.NewsContents ?? string.Empty).Replace("{ItemName}", itemName);

        return true;
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
                currentEvent = state.currentEvent ?? new MarketEvent(),

                isRumor = state.isRumor,
                rumorEventID = state.rumorEventID,
                reversionMean = state.reversionMean,

                lastUrgentID = state.lastUrgentID,
                lastUrgentStartDay = state.lastUrgentStartDay,
                usedLinkedIDs = new List<int>(state.usedLinkedIDs),
                graceActive = state.graceActive,
                graceEndDay = state.graceEndDay,

                lines = new List<MarketLine>(state.lines),
                urgentCloses = new List<float>(state.urgentCloses),
                virtualClearDay = state.virtualClearDay
            });
        }

        foreach (var pair in productClosingData)
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

            state.isRumor = s.isRumor;
            state.rumorEventID = s.rumorEventID;
            state.reversionMean = s.reversionMean;

            state.lastUrgentID = s.lastUrgentID;
            state.lastUrgentStartDay = s.lastUrgentStartDay;
            state.usedLinkedIDs = s.usedLinkedIDs != null ? new List<int>(s.usedLinkedIDs) : new List<int>();
            state.graceActive = s.graceActive;
            state.graceEndDay = s.graceEndDay;

            state.lines = s.lines != null ? new List<MarketLine>(s.lines) : new List<MarketLine>();
            state.urgentCloses = s.urgentCloses != null ? new List<float>(s.urgentCloses) : new List<float>();
            state.virtualClearDay = s.virtualClearDay;
        }

        foreach (var c in data.closingPrices)
        {
            if (!productClosingData.TryGetValue(c.itemID, out ProductClosing closing))
                continue;

            closing.candles.Clear();

            foreach (var candle in c.candles)
                closing.candles.Add(candle.Copy());

            closing.exactPrice = c.exactPrice;

            // 저장된 값이 없을 경우 기본 가격
            if (!closing.HasData && Origin.TryGetItem(c.itemID, out ItemData itemData))
            {
                closing.candles.Add(new PriceCandle(itemData.BasicPrice));
                closing.exactPrice = itemData.BasicPrice;
            }
        }

        RebuildNews();
    }
    #endregion
}