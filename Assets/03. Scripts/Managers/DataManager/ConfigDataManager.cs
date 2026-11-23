using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ConfigData
{
    // 오류 대비용 기본값

    // 시간 / 달력
    public float RealTimePerDay = 600f;      // 현실 초 / 게임 하루
    public int DaysPerYear = 336;
    public int DaysPerMonth = 28;
    public int UITimeStepMinutes = 10;       // 시계 / onMinuteEvent 단위(분)
    public float TimeScaleStep = 0.5f;       // 배속 간격
    public float TimeScaleMax = 2f;          // 최대 배속

    // 로켓
    public int RocketSlotAmount = 16;
    public int RocketDeliveryTime = 7200;    // 게임 시간(초). 편도 이동 시간
    public int RocketLoadingTime = 7200;     // 게임 시간(초). 적재 1회(판매 / 구매)당 시간

    // 일 / 상점
    public int UsedBatteryPerWork = 1;
    public int MarketOpenHour = 9;
    public int MarketCloseHour = 23;

    // 시작 초기 보유 자원
    public int StartingPotatoSeed = 9;
    public int StartingPotato = 10;
    public int StartingNPC = 1;
    public int StartingMoney = 10000;

    // 엔딩 / 세금 / 유예기간
    public int BadEndGraceDays = 7;
    public float GraceInterest = 0.1f;
    public int TaxPeriodDays = 28;
    public long EndingGoalMoney = 100000000;
    public int ExpireWarningDays = 1;

    // 시세 이벤트 / 소문
    public float EventProb = 0.25f;
    public float RumorProb = 0.25f;
    public float RumorStdMul = 1.3f;
    public float FollowUpDecay = 0.7f;
    public float FollowUpOverMeanMul = 1.3f;

    // 긴급 이벤트
    public float EmgProb = 0.1f;
    public int EmgMinDays = 7;
    public int EmgMaxDays = 14;
    public int EmgCoolDownDays = 28;
    public int EmgAfterLockReachDays = 10;
    public int EmgAfterLockExpireDays = 7;
    public float EmgLockDecay = 0.7f;
    public float EmgVirtualLineMul = 0.3f;
    public int EmgVirtualLineKeepDays = 7;

    // 평균회귀 / 기준선
    public float RegressionRangeMin = 0.5f;
    public float RegressionRangeMax = 2f;
    public int LineSampleDays = 28;
    public int LineSettleDays = 14;
    public float LineBinSigma = 0.5f;
    public int LineMinSamples = 2;
    public float LineBlockDecay = 0.7f;
    public float LineQuietPower = 0.3f;

    // 밭 / 원자로
    public int FieldMaxExpand = 2;
    public int ReactorMinLevel = 1;
    public int ReactorMaxLevel = 3;
    public int ReactorUpgPartPerStep = 5;
    public int ReactorExpandTiles = 3;
    public float ReactorBaseChargeRate = 1f;
    public float ReactorChargeRateStep = 2f;

    // 계산 편의성
    // 로켓 이동 시간(분) = (이동 + 적재 × 횟수) / 60
    // loads : 판매or구매만 = 1, 둘 다 = 2
    public int GetRocketTripMinutes(int loads)
    {
        return (RocketDeliveryTime + RocketLoadingTime * Mathf.Max(1, loads)) / 60;
    }
}

[DefaultExecutionOrder(-110)]
public class ConfigDataManager : MonoBehaviour
{
    private const string ResourcePath = "Json/ConfigDataTable";

    public ConfigData Data { get; private set; } = new ConfigData();

    private void Awake()
    {
        Load();
    }

    private void Load()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(ResourcePath);

        if (jsonFile == null)
        {
            Debug.LogError($"[ConfigDataManager] {ResourcePath} 파일 없음");
            return;
        }

        try
        {
            List<ConfigData> rows =
                JsonConvert.DeserializeObject<List<ConfigData>>(jsonFile.text);

            if (rows == null || rows.Count == 0)
            {
                Debug.LogError("[ConfigDataManager] 파일 내용 비어있음");
                return;
            }

            if (rows.Count > 1)
            {
                Debug.LogWarning("[ConfigDataMAnager] 중복 행이 있음");
            }

            Data = rows[0];
        }
        catch (Exception e)
        {
            Debug.LogError($"[ConfigDataManager] 읽기 실패 {e.Message}");
        }
    }
}