using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField] DataManager dataManager;

    private Dictionary<int, EventData> eventsData = new Dictionary<int, EventData>();

    void Start()
    {
        eventsData.Clear();
        eventsData = dataManager.eventsData;
    }

    void Update()
    {
        
    }
}
