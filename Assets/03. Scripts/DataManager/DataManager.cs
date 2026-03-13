using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    [SerializeField] ItemDataManager itemsDataManager;
    public Dictionary<int, ItemData> itemsData { get; private set; }
    [SerializeField] ProductDataManager productDataManager;
    public Dictionary<int, Product> productsData { get; private set; }
    [SerializeField] EventDataManager eventDataManager;
    public Dictionary<int, EventData> eventsData { get; private set; }



    private void Start()
    {
        LoadData();
    }

    private void LoadData()
    {
        itemsData = itemsDataManager.itemData;
        productsData = productDataManager.productData;
        eventsData = eventDataManager.eventData;
    }
}
