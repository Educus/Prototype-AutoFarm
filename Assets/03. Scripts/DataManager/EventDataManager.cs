using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

enum EventType
{

}
public class EventData
{
    [SerializeField] public int eventID;
    [SerializeField] public string eventType;
    [SerializeField] public float average;
    [SerializeField] public int linkage;
    [SerializeField] public string text;
}

[DefaultExecutionOrder(-100)]
public class EventDataManager : MonoBehaviour
{
    public Dictionary<int, EventData> eventData = new Dictionary<int, EventData>();

    private TextAsset jsonFile;

    private void Awake()
    {
        // 게임 실행 시 ProductDataTable 불러오기
        LoadItemDataTable();

        // PrintAll();
    }

    private void LoadItemDataTable()
    {
        jsonFile = Resources.Load<TextAsset>("Json/EventDataTable");

        if (jsonFile == null)
        {
            Debug.Log("파일 없음");
            return;
        }

        List<EventData> eventDataList = JsonConvert.DeserializeObject<List<EventData>>(jsonFile.text);

        eventData.Clear();

        foreach (var events in eventDataList)
        {
            eventData[events.eventID] = events;
        }
    }

    // 테스트 출력
    public void PrintAll()
    {
        foreach (var pair in eventData)
        {
            Debug.Log($"Key:{pair.Key} ID:{pair.Value.eventID} Name:{pair.Value.eventType} Text:{pair.Value.text}");
        }
    }
}
