using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class EventData
{
    [SerializeField] public int ID;
    [SerializeField] public string Type;
    [SerializeField] public float Average;
    [SerializeField] public int Linkage;
    [SerializeField] public string Text_Kr;
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
            eventData[events.ID] = events;
        }
    }

    // 테스트 출력
    public void PrintAll()
    {
        foreach (var pair in eventData)
        {
            Debug.Log($"Key:{pair.Key} ID:{pair.Value.ID} Name:{pair.Value.Type} Text:{pair.Value.Text_Kr}");
        }
    }
}
