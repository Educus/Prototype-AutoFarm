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
}
