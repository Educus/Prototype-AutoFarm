using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Tooltip("현실 시간 기준 하루 길이(초)")]
    private float realSecondsPerDay = 1800f; // 30분

    [Tooltip("표시 시간 단위")]
    public int miniteStep = 10; // 10분 단위로 표시

    private float timer = 0f;

    // 현재 날짜
    public int currentDay {get; private set;} = 1;
    public int currentHour {get; private set;} = 0;
    public int currentMinute { get; private set; }  = 0;

    // UI용 분 단위 표시
    private int displayMinute = 0;

    // 이벤트
    public event Action onTimeSetpEvent;        // 시간 단위 이벤트
    public event Action onDayEvent;             // 매일
    public event Action onWeekEvent;            // 매주
    public event Action<int> onSpecificDay;     // 특정 날짜

    void Update()
    {
        // 게임매니저에서 게임이 진행 중인지 확인
        if (!gameManager.isPlay) return;

        timer += Time.deltaTime;

        float timerGameMinute = realSecondsPerDay / (24f * 60f); // 게임 내 시간으로 변환

        while (timer >= timerGameMinute)
        {
            timer -= timerGameMinute;
            AddMinute(1);
        }
    }

    void AddMinute(int minutes)
    {
        currentMinute += minutes;

        if (currentMinute >= 60)
        {
            currentMinute = 0;
            currentHour++;
        }

        if (currentHour >= 24)
        {
            currentHour = 0;
            currentDay++;

            CheckDayEvent();        // 하루가 지날 때마다 이벤트 발생
            CheckWeekEvent();       // 일주가 지날 때마다 이벤트 발생
            CheckSpecificDayEvent();// 특정 날짜 이벤트 발생
        }

        OnTimeSetpEvent();
    }

    // 이벤트 추가 방법
    // TimeManager.onTimeSetEvent += 함수(); 이하동문
    void OnTimeSetpEvent()
    {
        int newDisplayMinute = (currentMinute / miniteStep) * miniteStep; // miniteStep분 단위로 계산

        if (newDisplayMinute != displayMinute)
        {
            displayMinute = newDisplayMinute;
            onTimeSetpEvent?.Invoke();
        }
    }
    void CheckDayEvent()
    {
        onDayEvent?.Invoke();
    }
    void CheckWeekEvent()
    {
        if (currentDay % 7 == 0)
        {
            onWeekEvent?.Invoke();
        }
    }
    void CheckSpecificDayEvent()
    {
        onSpecificDay?.Invoke(currentDay);
    }

    // UI용 시간 반환
    public int[] GetTimeString()
    {
        return new int[] { currentDay, currentHour, displayMinute };
    }
}
