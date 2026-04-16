using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField] DataManager dataManager;
    [SerializeField] TimeManager timeManager;

    private Dictionary<int, EventData> eventsData = new Dictionary<int, EventData>();

    void Start()
    {
        eventsData.Clear();
        eventsData = dataManager.eventsData;

        timeManager.onEvent += TriggerEvent;
    }

    private void TriggerEvent()
    {
        // 이벤트 발생 로직(임시 고정)
        int eventID = timeManager.currentDay % 2 == 0 ? 101 : 102;

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
