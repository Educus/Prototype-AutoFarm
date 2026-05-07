using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    [SerializeField] private ItemDataManager itemsDataManager;
    public Dictionary<int, ItemData> itemsData { get; private set; }

    [SerializeField] private ProductDataManager productDataManager;
    public Dictionary<int, Product> productsData { get; private set; }
    public Dictionary<int, ProductClosing> productClosingData { get; private set; }
    public Dictionary<int, ProductSubData> productSubData { get; private set; }

    [SerializeField] private EventDataManager eventDataManager;
    public Dictionary<int, EventData> eventsData { get; private set; }
    [SerializeField] private ObjectDataManager objectDataManager;
    public Dictionary<int, ObjectData> objectsData { get; private set; }

    [SerializeField] private SaveLoadManager saveLoadManager;
    public SaveLoadManager SaveLoadManager { get; private set; }

    [SerializeField] private CurrencyManager currencyManager;
    public CurrencyManager CurrencyManager { get; private set; }

    [SerializeField] private NPCManager npcManager;
    public NPCManager NPCManager { get; private set; }
    
    [SerializeField] AnimalManager animalManager;
    public AnimalManager AnimalManager { get; private set; }

    [SerializeField] private InventoryManager inventoryManager;
    public InventoryManager InventoryManager { get; private set; }

    [SerializeField] private BuildingManager buildingManager;
    public BuildingManager BuildingManager { get; private set; }


    public int nowEventID = -1;

    [SerializeField] private Sprite[] itemImage;
    [SerializeField] private Sprite[] cropsImage;
    [SerializeField] private GameObject[] objPrefabs;

    public static DataManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        productSubData = productDataManager.productSubData;
        eventsData = eventDataManager.eventData;
        objectsData = objectDataManager.objectData;
        SaveLoadManager = saveLoadManager;
        CurrencyManager = currencyManager;
        NPCManager = npcManager;
        AnimalManager = animalManager;
        InventoryManager = inventoryManager;
        BuildingManager = buildingManager;
    }

    public void ContinueGame()
    {
        saveLoadManager.Load();
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

    public Sprite GetCropImage(int itemId, float growth)
    {
        if (itemId == 0) return null;

        int value = Mathf.RoundToInt(growth * productDataManager.productData[itemId].growthTime);
        string name = itemId.ToString() + "_" + value;

        foreach (var sprite in cropsImage)
        {
            if (sprite.name == name)
            {
                return sprite;
            }
        }

        return null;
    }

    public GameObject GetObjectPrefabs(int itemId)
    {
        foreach (var obj in objPrefabs)
        {
            if (obj.name == itemId.ToString())
            {
                return obj;
            }
        }

        return null; // 아이템 ID에 해당하는 프리팹이 없는 경우
    }
}