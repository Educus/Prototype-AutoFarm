using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-150)]
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("런타임 매니저 (이 오브젝트의 자식)")]
    [RequiredRef, SerializeField] private PlayerDataManager playerDataManager;
    [RequiredRef, SerializeField] private NPCManager npcManager;
    [RequiredRef, SerializeField] private AnimalManager animalManager;
    [RequiredRef, SerializeField] private InventoryManager inventoryManager;
    [RequiredRef, SerializeField] private BuildingManager buildingManager;
    [RequiredRef, SerializeField] private ShopManager shopManager;
    [RequiredRef, SerializeField] private MarketManager marketManager;

    public PlayerDataManager PlayerDataManager => playerDataManager;
    public NPCManager NPCManager => npcManager;
    public AnimalManager AnimalManager => animalManager;
    public InventoryManager InventoryManager => inventoryManager;
    public BuildingManager BuildingManager => buildingManager;
    public ShopManager ShopManager => shopManager;
    public MarketManager MarketManager => marketManager;

    // 저장/불러오기는 DataTotalManager에서 관리
    public SaveLoadManager SaveLoadManager =>
        DataTotalManager.Instance != null ? DataTotalManager.Instance.SaveLoad : null;

    // 시세 MarketManager에서 관리
    public Dictionary<int, ProductClosing> productClosingData => marketManager.productClosingData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region [임시] 기존 코드가 컴파일되도록 남겨 둔 전달 프로퍼티 - 모두 OriginDataManager로 교체 예정
    [Obsolete("OriginDataManager.Instance.itemsData 를 사용하세요")]
    public Dictionary<int, ItemData> itemsData => OriginDataManager.Instance.itemsData;

    [Obsolete("OriginDataManager.Instance.productsData 를 사용하세요")]
    public Dictionary<int, Product> productsData => OriginDataManager.Instance.productsData;

    [Obsolete("OriginDataManager.Instance.eventsData 를 사용하세요")]
    public Dictionary<int, EventData> eventsData => OriginDataManager.Instance.eventsData;

    [Obsolete("OriginDataManager.Instance.objectsData 를 사용하세요")]
    public Dictionary<int, ObjectData> objectsData => OriginDataManager.Instance.objectsData;

    [Obsolete("OriginDataManager.Instance.GetItemImage 를 사용하세요")]
    public Sprite GetItemImage(int itemId) => OriginDataManager.Instance.GetItemImage(itemId);

    [Obsolete("OriginDataManager.Instance.GetCropImage 를 사용하세요")]
    public Sprite GetCropImage(int itemId, float growth) => OriginDataManager.Instance.GetCropImage(itemId, growth);

    [Obsolete("OriginDataManager.Instance.GetObjectPrefabs 를 사용하세요")]
    public GameObject GetObjectPrefabs(int itemId) => OriginDataManager.Instance.GetObjectPrefabs(itemId);

    [Obsolete("OriginDataManager.Instance.GetItemIDNumber 를 사용하세요")]
    public int GetItemIDNumber(string itemName) => OriginDataManager.Instance.GetItemIDNumber(itemName);

    [Obsolete("DataTotalManager.Instance.SaveLoad.ContinueGame() 을 사용하세요")]
    public void ContinueGame() => SaveLoadManager.ContinueGame();
    #endregion
}
