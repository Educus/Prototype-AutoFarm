using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public enum BuildingType
{
    None,
    Farm,
    Ranch,
    Rocket,
    Storage
}

[System.Serializable]
public class DefaultBuildingData
{
    public int itemID;
    public Vector2Int gridPosition;
}

public class BuildingManager : MonoBehaviour
{
    [Header("Building")]
    [SerializeField] private GameObject[] buildingPrefabs;

    [Header("Manager")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ChunkManager chunkManager;

    [Header("Preview")]
    [SerializeField] private float alpha = 0.7f;

    [Header("Default Buildings")]
    [SerializeField]
    private List<DefaultBuildingData> defaultBuildings;

    private GameObject currentPrefab;
    private GameObject previewObj;

    private BuildingData currentData;

    private bool isPlacing = false;

    private int buildingIndex = 0;

    public int CurrentItemID { get; private set; } = -1;

    public Dictionary<string, BuildingBase> buildings =
        new Dictionary<string, BuildingBase>();

    #region Unity

    private void Start()
    {
        CreateDefaultBuildings();
    }

    private void Update()
    {
        if (!GameManager.Instance.isBuildMode)
            return;

        if (!isPlacing)
            return;

        Vector2 mousePos =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition);

        Vector2Int gridPos =
            gridManager.WorldToGrid(mousePos);

        gridPos -= new Vector2Int(
            currentData.width / 2,
            currentData.height / 2
        );

        UpdatePreview(gridPos);

        if (Input.GetMouseButtonDown(0) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            TryPlace(gridPos);
        }
    }

    #endregion

    #region Register

    public void Register(BuildingBase building)
    {
        if (!buildings.ContainsKey(building.id))
        {
            buildings.Add(building.id, building);
        }
    }

    #endregion

    #region Default Building

    private void CreateDefaultBuildings()
    {
        foreach (var data in defaultBuildings)
        {
            CreateBuilding(
                data.itemID,
                data.gridPosition);
        }
    }

    #endregion

    #region Placement

    public void StartPlacement(int itemID)
    {
        CancelPlacement();

        currentPrefab = Array.Find(
            buildingPrefabs,
            p => p.name == itemID.ToString());

        if (currentPrefab == null)
        {
            Debug.LogError($"Prefab Missing : {itemID}");
            return;
        }

        previewObj = Instantiate(currentPrefab);

        SetPreviewAlpha(previewObj, alpha);

        currentData =
            previewObj
            .GetComponent<BuildingBase>()
            .data;

        CurrentItemID = itemID;

        isPlacing = true;
    }

    private void UpdatePreview(Vector2Int pos)
    {
        Vector3 world =
            gridManager.GridToWorld(pos.x, pos.y);

        Vector3 center = world + new Vector3(
            currentData.width * 0.5f,
            currentData.height * 0.5f,
            0
        );

        previewObj.transform.position = center;

        bool canPlace = CanPlace(pos);

        Color color =
            canPlace ? Color.green : Color.red;

        color.a = 0.5f;

        previewObj
            .GetComponent<SpriteRenderer>()
            .color = color;
    }

    private void SetPreviewAlpha(
        GameObject obj,
        float alpha)
    {
        SpriteRenderer sr =
            obj.GetComponent<SpriteRenderer>();

        if (sr == null)
            return;

        Color c = sr.color;
        c.a = alpha;

        sr.color = c;
    }

    private bool CanPlace(Vector2Int pos)
    {
        for (int x = 0; x < currentData.width; x++)
        {
            for (int y = 0; y < currentData.height; y++)
            {
                int index =
                    y * currentData.width + x;

                if (index >= currentData.patternFlat.Length)
                {
                    Debug.LogError(
                        $"PatternFlat Out Of Range : {index}");

                    return false;
                }

                if (currentData.patternFlat[index])
                    continue;

                Vector2Int checkPos =
                    pos + new Vector2Int(x, y);

                Node node =
                    gridManager.GetNode(
                        checkPos.x,
                        checkPos.y);

                if (node == null)
                    return false;

                if (!node.isWalkable)
                    return false;

                if (!chunkManager.IsUnlocked(checkPos))
                    return false;

                if (node.tileType == TileType.Water)
                    return false;
            }
        }

        return true;
    }

    private void TryPlace(Vector2Int pos)
    {
        if (!CanPlace(pos))
        {
            Debug.Log("설치 불가");
            return;
        }

        bool hasMoney = true;   // 테스트용 가능
        // bool haveItem = DataManager.Instance.InventoryManager.GetTotalItemCount(1000) > 0;

        if (!hasMoney)
        {
            Debug.Log("비용 부족");
            return;
        }

        CreateBuilding(CurrentItemID, pos);
    }

    public BuildingBase CreateBuilding(
        int itemID,
        Vector2Int pos)
    {
        GameObject prefab = Array.Find(
            buildingPrefabs,
            p => p.name == itemID.ToString());

        if (prefab == null)
        {
            Debug.LogError($"Prefab Missing : {itemID}");
            return null;
        }

        BuildingBase prefabBuilding =
            prefab.GetComponent<BuildingBase>();

        BuildingData data =
            prefabBuilding.data;

        GameObject obj = Instantiate(prefab);

        obj.name =
            $"Building_{itemID}_{buildingIndex++}";

        Vector3 world =
            gridManager.GridToWorld(pos.x, pos.y);

        Vector3 center = world + new Vector3(
            data.width * 0.5f,
            data.height * 0.5f,
            0
        );

        obj.transform.position = center;

        BuildingBase building =
            obj.GetComponent<BuildingBase>();

        building.Initialize();

        ApplyToGrid(pos, data);

        return building;
    }

    private void ApplyToGrid(
        Vector2Int pos,
        BuildingData data)
    {
        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                int index =
                    y * data.width + x;

                Vector2Int p =
                    pos + new Vector2Int(x, y);

                if (data.patternFlat[index])
                {
                    gridManager.SetBlocked(
                        p.x,
                        p.y,
                        false);
                }
                else
                {
                    gridManager.SetBlocked(
                        p.x,
                        p.y,
                        true);
                }
            }
        }
    }

    public void CancelPlacement()
    {
        if (previewObj != null)
        {
            Destroy(previewObj);
        }

        isPlacing = false;

        CurrentItemID = -1;
    }

    #endregion

    #region Find

    public BuildingBase GetBuilding(int itemID)
    {
        GameObject prefab = Array.Find(
            buildingPrefabs,
            p => p.name == itemID.ToString());

        if (prefab == null)
            return null;

        return prefab.GetComponent<BuildingBase>();
    }

    public T Get<T>(string id)
        where T : BuildingBase
    {
        if (buildings.TryGetValue(id, out BuildingBase building))
        {
            return building as T;
        }

        return null;
    }

    public IEnumerable<BuildingBase> GetAll()
    {
        return buildings.Values;
    }

    // NPC가 사용할 창고 위치
    public Vector2Int GetStoragePosition()
    {
        foreach (var building in buildings.Values)
        {
            if (building.type == BuildingType.Storage)
            {
                return GridManager.Instance.WorldToGrid(
                    building.transform.position);
            }
        }

        return Vector2Int.zero;
    }

    #endregion

    #region Save / Load

    public List<BuildingSaveData> GetSaveData()
    {
        List<BuildingSaveData> list =
            new List<BuildingSaveData>();

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
            GameObject prefab = Array.Find(
                buildingPrefabs,
                p => p.name == data.itemId.ToString());

            if (prefab == null)
            {
                Debug.LogError(
                    $"Load Prefab Missing : {data.itemId}");

                continue;
            }

            GameObject obj = Instantiate(prefab);

            obj.transform.position = data.position;

            obj.name = data.id;

            BuildingBase building =
                obj.GetComponent<BuildingBase>();

            building.Initialize();

            building.LoadJsonData(data.jsonData);

            BuildingData dataInfo = building.data;

            Vector2Int pos =
                gridManager.WorldToGrid(data.position);

            ApplyToGrid(pos, dataInfo);
        }

        UpdateBuildingIndex();
    }

    private void UpdateBuildingIndex()
    {
        int maxIndex = -1;

        foreach (var building in buildings.Values)
        {
            string[] split =
                building.id.Split('_');

            if (split.Length < 3)
                continue;

            if (int.TryParse(split[2], out int index))
            {
                if (index > maxIndex)
                {
                    maxIndex = index;
                }
            }
        }

        buildingIndex = maxIndex + 1;
    }

    #endregion
}