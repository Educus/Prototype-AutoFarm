using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LightTransport;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

public enum BuildingType
{
    None,
    Farm,
    Ranch,
    Rocket,
    Storage
}

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private GameObject[] buildingPrefabs;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ChunkManager chunkManager;

    private GameObject currentPrefab;
    private GameObject previewObj;
    private BuildingData currentData;

    [SerializeField] private float alpha = 0.7f;

    public int CurrentItemID { get; private set; } = -1;

    public Dictionary<string, BuildingBase> buildings = new Dictionary<string, BuildingBase>();

    private bool isPlacing = false;

    void Update()
    {
        if (!GameManager.Instance.isBuildMode) return;
        if (!isPlacing) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = gridManager.WorldToGrid(mousePos);

        gridPos -= new Vector2Int(
            currentData.width / 2,
            currentData.height / 2
        );

        UpdatePreview(gridPos);

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            TryPlace(gridPos);
        }
    }

    public void Register(BuildingBase building)
    {
        if (!buildings.ContainsKey(building.id))
            buildings.Add(building.id, building);
    }

    public void StartPlacement(int itemID)
    {
        CancelPlacement();

        currentPrefab = System.Array.Find(buildingPrefabs, p => p.name == itemID.ToString());

        if (currentPrefab == null)
        {
            Debug.LogError("프리팹 없음");
            return;
        }

        previewObj = Instantiate(currentPrefab);

        SetPreviewAlpha(previewObj, alpha);

        currentData = previewObj.GetComponent<BuildingBase>().data;

        CurrentItemID = itemID;
        isPlacing = true;
    }

    void UpdatePreview(Vector2Int pos)
    {
        Vector3 world = gridManager.GridToWorld(pos.x, pos.y);

        Vector3 center = world + new Vector3(
            currentData.width * 0.5f,
            currentData.height * 0.5f,
            0
        );

        previewObj.transform.position = center;

        bool canPlace = CanPlace(pos);

        Color color = canPlace ? Color.green : Color.red;
        color.a = 0.5f;

        previewObj.GetComponent<SpriteRenderer>().color = color;
    }

    void SetPreviewAlpha(GameObject obj, float alpha)
    {
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    bool CanPlace(Vector2Int pos)
    {
        for (int x = 0; x < currentData.width; x++)
        {
            for (int y = 0; y < currentData.height; y++)
            {
                int index = y * currentData.width + x;

                if (index >= currentData.patternFlat.Length)
                {
                    Debug.LogError($"patternFlat 크기 부족 index:{index}");
                    return false;
                }

                if (currentData.patternFlat[index]) continue;

                Vector2Int checkPos = pos + new Vector2Int(x, y);

                Node node = gridManager.GetNode(checkPos.x, checkPos.y);

                if (node == null) return false;

                if (!node.isWalkable) return false;
                if (!chunkManager.IsUnlocked(checkPos)) return false;
                if (node.tileType == TileType.Water) return false;
            }
        }

        return true;
    }

    // 설치
    void TryPlace(Vector2Int pos)
    {
        if (!CanPlace(pos))
        {
            Debug.Log("설치 불가");
            return;
        }

        bool hasMoney = true; // 테스트용 가능
        // bool haveItem = DataManager.Instance.InventoryManager.GetTotalItemCount(1000) > 0;

        if (!hasMoney)
        {
            Debug.Log("비용부족");
            return;
        }

        GameObject prefab = System.Array.Find(buildingPrefabs, p => p.name == CurrentItemID.ToString());
        GameObject obj = Instantiate(prefab);
        obj.name = "Building_" + CurrentItemID.ToString() + $"_{buildings.Count}";
        obj.GetComponent<BuildingBase>().name = "Building_" + CurrentItemID.ToString() + $"_{buildings.Count}";
        obj.GetComponent<BuildingBase>().isClone = true;

        Vector3 world = gridManager.GridToWorld(pos.x, pos.y);

        Vector3 center = world + new Vector3(
            currentData.width * 0.5f,
            currentData.height * 0.5f,
            0
        );

        obj.transform.position = center;

        ApplyToGrid(pos);
    }

    void ApplyToGrid(Vector2Int pos)
    {
        for (int x = 0; x < currentData.width; x++)
        {
            for (int y = 0; y < currentData.height; y++)
            {
                int index = y * currentData.width + x;
                Vector2Int p = pos + new Vector2Int(x, y);

                if (currentData.patternFlat[index])
                    gridManager.SetBlocked(p.x, p.y, false);
                else
                    gridManager.SetBlocked(p.x, p.y, true);
            }
        }
    }

    public BuildingBase GetBuilding(int itemID)
    {
        GameObject prefab = System.Array.Find(buildingPrefabs, p => p.name == itemID.ToString());

        if (prefab == null)
        {
            return null;
        }

        return prefab.GetComponent<BuildingBase>();
    }

    public void CancelPlacement()
    {
        if (previewObj != null)
            Destroy(previewObj);

        isPlacing = false;
        CurrentItemID = -1;
    }

    public T Get<T>(string id) where T : BuildingBase
    {
        return buildings[id] as T;
    }

    #region Save / Load
    public List<BuildingSaveData> GetSaveData()
    {
        var list = new List<BuildingSaveData>();

        foreach (var b in buildings.Values)
        {
            list.Add(new BuildingSaveData
            {
                id = b.id,
                itemId = b.itemID,

                position = b.transform.position,

                jsonData = b.GetJsonData()
            });
        }

        return list;
    }
    public void Load(List<BuildingSaveData> dataList)
    {
        foreach (var data in dataList)
        {
            // 1 프리팹 로드
            GameObject prefab = System.Array.Find(buildingPrefabs, p => p.name == data.itemId.ToString());

            // 2 생성
            GameObject obj = Instantiate(prefab);
            obj.transform.position = data.position;
            obj.name = data.id;
            // grid 위치 저장(길찾기)
            Vector2Int pos = new Vector2Int((int)data.position.x, (int)data.position.y);
            ApplyToGrid(pos);

            // 3 컴포넌트
            var building = obj.GetComponent<BuildingBase>();

            // 4 데이터 로드
            building.LoadJsonData(data.jsonData);
        }
    }
    #endregion
}