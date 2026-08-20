using System;
using System.Collections.Generic;
using UnityEngine;


public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Tooltip("시간 배율")]
    private float timeScale = 0f;

    [Tooltip("현실 시간 기준 하루 길이(초)")]
    private float realSecondsPerDay = 300f; // 테스트 5분

    [Tooltip("표시 시간 단위")]
    public int miniteStep = 10; // 10분 단위로 표시

    private float timer;

    // 현재 날짜
    public int minute { get; private set; }  = 0;
    public int hour {get; private set;} = 0;
    public int day {get; private set;} = 1;

    // 몇주차인지, 요일은 몇요일인지
    public int week => ((day - 1) / 7) + 1;
    public int weekDay => (day - 1) % 7; //(0 = 월요일 ~ 6 = 일요일)

    // UI용 분 단위 표시
    private int displayMinute = 0;

    // 기본 시간 이벤트
    public event Action<int> onMinuteEvent;          // 10분 단위 이벤트
    public event Action<int> onHourEvent;            // 1시간 단위 이벤트
    public event Action<int> onDayEvent;             // 매일
    public event Action<int> onWeekEvent;            // 매주

    #region Schedule
    public enum ScheduleType
    {
        Once,       // 일회성
        Daily,      // 매일
        Weekly,     // 매주
        Monthly,    // 매월
        Interval    // 일정 주기
    }
    public class ScheduledEvent
    {
        public int ID;

        public ScheduleType tpye;
    
        public int day;
        public int weekDay;
    
        public int hour;
        public int minute;
    
        // Interval 타입일 경우, 몇일마다 발생하는지
        public int intervalDays;
    
        // 발생시킬 이벤트
        public Action callback;

        // 현재 시간과 비교하여 이벤트 발생 여부를 판단
        public bool IsMatch(TimeManager time)
        {
            if (hour != time.hour || minute != time.minute)
                return false;

            return tpye switch
            {
                ScheduleType.Once => day == time.day,
                ScheduleType.Daily => true,
                ScheduleType.Weekly => weekDay == time.weekDay,
                _ => false
            };
        }
    }

    private List<ScheduledEvent> scheduledEvents = new();
    private int nextEventID = 0;
    #endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 시간 배율이 0 이하이면 시간 진행하지 않음
        if (timeScale <= 0f) return;

        timer += Time.deltaTime * timeScale;

        float timerGameMinute = realSecondsPerDay / (24f * 60f); // 게임 내 시간으로 변환

        while (timer >= timerGameMinute)
        {
            timer -= timerGameMinute;
            AddMinute();
        }
    }

    #region Time Control
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0f, scale);
    }

    public void Pause()
    {
        timeScale = 0f;
    }

    public void Resume()
    {
        timeScale = 1f;
    }

    public void SetTime(int day, int hour, int minute)
    {
        this.day = Mathf.Max(1, day);
        this.hour = Mathf.Clamp(hour, 0, 23);
        this.minute = Mathf.Clamp(minute, 0, 59);
    }
    #endregion

    #region Time Flow
    public const int minuteStep = 10;
    void AddMinute(int amount = 1)
    {
        while (amount-- > 0)
        {
            minute++;

            if (minute >= 60)
            {
                minute = 0;
                AddHour();
            }

            if (minute % miniteStep == 0)
            {
                onMinuteEvent?.Invoke(minute);
                CheckSchedules();
            }
        }
    }

    public void AddHour()
    {
        hour++;
        
        onHourEvent?.Invoke(hour);

        if (hour >= 24)
        {
            hour = 0;
            AddDay();
        }
    }

    public void AddDay()
    {
        day++;
        
        onDayEvent?.Invoke(day);

        if (weekDay == 0)
        {
            onWeekEvent?.Invoke(week);
        }
    }
    #endregion

    #region Skip Time(수면 시간 스킵용, 개발자용)
    public void SkipMinute(int amount)
    {
        AddMinute(amount);
    }
    
    public void SkipHour(int amount)
    {
       SkipMinute(amount * 60);
    }

    public void SkipDay(int amount)
    {
        SkipMinute(amount * 60 * 24);
    }
    #endregion

    #region Utility
    public bool IsTime(int hour, int minute)
    {
        return this.hour == hour && this.minute == minute;
    }

    public bool IsDay(int day)
    {
        return this.day == day;
    }
    #endregion

    #region Event
    // 이벤트 등록
    public int RegisterDaily(int hour, int minute, Action callback)
    {
        MinuteDebug(minute);

        ScheduledEvent newEvent = new ScheduledEvent()
        {
            ID = nextEventID++,
            tpye = ScheduleType.Daily,
            hour = hour,
            minute = minute,
            callback = callback
        };

        scheduledEvents.Add(newEvent);

        return newEvent.ID;
    }

    public int RegisterWeekly(int weekDay, int hour, int minute, Action callback)
    {
        MinuteDebug(minute);

        ScheduledEvent newEvent = new ScheduledEvent()
        {
            ID = nextEventID++,
            tpye = ScheduleType.Weekly,
            weekDay = weekDay,
            hour = hour,
            minute = minute,
            callback = callback
        };

        scheduledEvents.Add(newEvent);

        return newEvent.ID;
    }

    // 일회성 이벤트 등록
    public int RegisterOnce(int day, int hour, int minute, Action callback)
    {
        MinuteDebug(minute);

        ScheduledEvent newEvent = new ScheduledEvent()
        {
            ID = nextEventID++,
            tpye = ScheduleType.Once,
            day = day,
            hour = hour,
            minute = minute,
            callback = callback
        };

        scheduledEvents.Add(newEvent);

        return newEvent.ID;
    }

    public void MinuteDebug(int minutes)
    {
        if (minutes % minuteStep != 0)
        {
            Debug.LogError("분은 10분 단위만 사용할 수 있습니다.");
        }
    }

    // 이벤트 제거
    public bool RemoveSchedule(int id)
    {
        int index = scheduledEvents.FindIndex(e => e.ID == id);

        if (index < 0) return false;

        scheduledEvents.RemoveAt(index);

        return true;
    }

    // 이벤트 실행
    private void CheckSchedules()
    {
        foreach (var schedule in scheduledEvents)
        {
            if (schedule.IsMatch(this))
            {
                schedule.callback?.Invoke();
            }
        }
    }
    #endregion
}
