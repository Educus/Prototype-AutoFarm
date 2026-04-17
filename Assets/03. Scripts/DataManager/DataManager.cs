using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    [SerializeField] private ItemDataManager itemsDataManager;
    public Dictionary<int, ItemData> itemsData { get; private set; }

    [SerializeField] private ProductDataManager productDataManager;
    public Dictionary<int, Product> productsData { get; private set; }
    public Dictionary<int, ProductClosing> productClosingData { get; private set; }

    [SerializeField] private EventDataManager eventDataManager;
    public Dictionary<int, EventData> eventsData { get; private set; }

    [SerializeField] private SaveLoadManager saveLoadManager;
    public SaveLoadManager SaveLoadManager { get; private set; }

    public int nowEventID = -1;

    [SerializeField] private Sprite[] itemImage;
    public Sprite[] ItemImage { get { return itemImage; }  }

    private void Start()
    {
        LoadData();
    }

    private void LoadData()
    {
        // 원본 데이터 로드
        itemsData = itemsDataManager.itemData;
        productsData = productDataManager.productData;
        productClosingData = productDataManager.productClosingData;
        eventsData = eventDataManager.eventData;

        // 세이브 데이터 로드

    }

    public void ContinueGame()
    {
        saveLoadManager.ContinueGame();
    }

    public Sprite GetItemImage(int itemId)
    {
        foreach (var sprite in itemImage)
        {
            if (sprite.name == itemId.ToString())
            {
                return sprite;
            }
        }

        return null; // 아이템 ID에 해당하는 이미지가 없는 경우
    }
}