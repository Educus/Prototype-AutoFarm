using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-150)]
public class OriginDataManager : MonoBehaviour
{
    public static OriginDataManager Instance { get; private set; }

    [RequiredRef, SerializeField] private ItemDataManager itemsDataManager;
    [RequiredRef, SerializeField] private ProductDataManager productDataManager;
    [RequiredRef, SerializeField] private ObjectDataManager objectDataManager;
    [RequiredRef, SerializeField] private EventDataManager eventDataManager;
    [RequiredRef, SerializeField] private KeyCodeDataManager keyCodeDataManager;
     
    [RequiredRef, SerializeField] private Sprite[] itemImage;
    [RequiredRef, SerializeField] private Sprite[] cropsImage;
    [RequiredRef, SerializeField] private GameObject[] objPrefabs;

    // 읽기 전용 데이터
    public Dictionary<int, ItemData> itemsData => itemsDataManager.itemData;
    public Dictionary<int, Product> productsData => productDataManager.productData;
    public Dictionary<int, EventData> eventsData => eventDataManager.eventData;
    public Dictionary<int, ObjectData> objectsData => objectDataManager.objectData;
    public KeyCodeDataManager KeyCodes => keyCodeDataManager;

    // 이미지, 프리팹 캐시
    private Dictionary<string, Sprite> itemImages;
    private Dictionary<string, Sprite> cropImages;
    private Dictionary<string, GameObject> prefabs;
    private Dictionary<string, int> itemIdByName;   // 소문자 이름 -> ID

    private void Awake()
    {
        if (Instance == null && Instance == this)
        {
            Destroy(gameObject);
            return;
        }
            
        Instance = this;

        itemImages = BuildMap(itemImage);
        cropImages = BuildMap(cropsImage);
        prefabs = BuildMap(objPrefabs);
    }

    private static Dictionary<string, T> BuildMap<T>(T[] array) where T : Object
    {
        var map = new Dictionary<string, T>();

        if (array == null)
        {
            return map;
        }

        foreach (var obj in array)
        {
            if (obj == null)
                continue;

            map[obj.name] = obj;
        }
        return map;
    }

    #region TryGet
    public bool TryGetItem(int id, out ItemData data) => itemsData.TryGetValue(id, out data);
    public bool TryGetProduct(int id, out Product data) => productsData.TryGetValue(id, out data);
    public bool TryGetObject(int id, out ObjectData data) => objectsData.TryGetValue(id, out data);
    public bool TryGetEvent(int id, out EventData data) => eventsData.TryGetValue(id, out data);
    #endregion

    #region 이미지 / 프리팹
    public Sprite GetItemImage(int itemId)
    {
        itemImages.TryGetValue(itemId.ToString(), out Sprite sprite);

        return sprite;
    }

    public Sprite GetCropImage(int itemId, float growth)
    {
        if (itemId == 0)
            return null;

        if (!TryGetProduct(itemId, out Product product))
            return null;

        int value = Mathf.RoundToInt(growth * product.growthTime);

        cropImages.TryGetValue(itemId + "_" + value, out Sprite sprite);

        return sprite;
    }

    public GameObject GetObjectPrefabs(int itemId)
    {
        prefabs.TryGetValue(itemId.ToString(), out GameObject prefab);

        return prefab;
    }
    #endregion

    //데이터 테이블에 비교하는 것 이기 때문에 정확히 일치하도록 비교
    public int GetItemIDNumber(string itemName)
    {
        if (itemIdByName == null || itemIdByName.Count == 0)
        {
            itemIdByName = new Dictionary<string, int>();

            foreach (var item in itemsData)
                itemIdByName[item.Value.Name.ToLower()] = item.Key;
        }

        if (itemIdByName.TryGetValue(itemName.ToLower(), out int id))
            return id;

        Debug.Log("해당 아이템은 테이블에 존재하지 않습니다. : " + itemName);
        return -1;
    }
}