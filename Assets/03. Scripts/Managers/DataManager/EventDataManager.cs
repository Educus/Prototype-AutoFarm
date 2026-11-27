using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class EventData
{
    [JsonProperty("ID")]
    public int ID;

    [JsonProperty("Average")]
    public float Average;

    // 0 이면 단독 발생, 0 이 아니면 후속이벤트
    [JsonProperty("Linkage_Event")]
    public int Linkage;

    [JsonProperty("News_Title_KO")]
    public string NewsTitle;

    [JsonProperty("News_Contents_KO")]
    public string NewsContents;
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
}
