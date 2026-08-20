using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private DataManager dataManager;
    private TimeManager timeManager;

    private Dictionary<int, EventData> eventsData = new Dictionary<int, EventData>();

    private void Awake()
    {
        dataManager = DataManager.Instance;
        timeManager = TimeManager.Instance;
    }
    void Start()
    {
        eventsData.Clear();
        eventsData = dataManager.eventsData;

        // 농작물 관련 주간 이벤트 발생
        timeManager.onWeekEvent += TriggerEvent;
    }

    private void TriggerEvent(int week)
    {
        // 이벤트 발생 로직(임시 고정)
        int eventID = timeManager.day % 2 == 0 ? 101 : 102;

        if (eventsData.TryGetValue(eventID, out EventData eventData))
        {
            Debug.Log($"이벤트 발생! ID: {eventData.eventID}");
        }
         else
        {
            Debug.Log("이벤트 데이터 없음");
        }
    }
}
