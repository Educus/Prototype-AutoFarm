using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject[] buildingPrefabs;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ChunkManager chunkManager;

    private GameObject currentPrefab;
    private GameObject previewObj;
    private BuildingData currentData;

    [SerializeField] private float alpha = 0.7f;

    public int CurrentItemID { get; private set; } = -1;

    private bool isPlacing = false;

    void Update()
    {
        if (!gameManager.isBuildMode) return;
        if (!isPlacing) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = gridManager.WorldToGrid(mousePos);

        UpdatePreview(gridPos);

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            TryPlace(gridPos);
        }
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

        currentData = previewObj.GetComponent<Building>().data;

        CurrentItemID = itemID;
        isPlacing = true;
    }

    void UpdatePreview(Vector2Int pos)
    {
        previewObj.transform.position = (Vector2)pos;

        bool canPlace = CanPlace(pos);

        Color color = canPlace ? Color.green : Color.red;
        color.a = alpha;

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

                if (!node.isWalkable) return false;
                if (!chunkManager.IsUnlocked(checkPos)) return false;
                if (node.tileType == TileType.Water) return false;
            }
        }

        return true;
    }

    void TryPlace(Vector2Int pos)
    {
        if (!CanPlace(pos))
        {
            Debug.Log("설치 불가");
            return;
        }

        bool hasMoney = true; // 테스트용 가능

        if (!hasMoney)
        {
            Debug.Log("비용부족");
            return;
        }

        GameObject prefab = System.Array.Find(buildingPrefabs, p => p.name == CurrentItemID.ToString());
        GameObject obj = Instantiate(prefab);

        obj.transform.position = (Vector2)pos;

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

    public Building GetBuilding(int itemID)
    {
        GameObject prefab = System.Array.Find(buildingPrefabs, p => p.name == itemID.ToString());

        if (prefab == null)
        {
            return null;
        }

        return prefab.GetComponent<Building>();
    }

    public void CancelPlacement()
    {
        if (previewObj != null)
            Destroy(previewObj);

        isPlacing = false;
        CurrentItemID = -1;
    }

    public List<BuildingSaveData> SaveBuildings()
    {
        List<BuildingSaveData> list = new List<BuildingSaveData>();

        Building[] allBuildings = FindObjectsOfType<Building>();

        foreach (var building in allBuildings)
        {
            Vector2Int pos = gridManager.WorldToGrid(building.transform.position);

            list.Add(new BuildingSaveData
            {
                itemID = building.itemID,
                x = pos.x,
                y = pos.y,
                extraData = building.Save()
            });
        }

        return list;
    }

    public void LoadBuildings(List<BuildingSaveData> list)
    {
        foreach (var data in list)
        {
            GameObject prefab = System.Array.Find(buildingPrefabs, p => p.name == data.itemID.ToString());

            if (prefab == null)
            {
                Debug.LogError($"프리팹 없음 itemID:{data.itemID}");
                continue;
            }

            GameObject obj = Instantiate(prefab);
            obj.transform.position = new Vector2(data.x, data.y);

            Building buildings = obj.GetComponent<Building>();

            buildings.Load(data.extraData);

            currentData = buildings.data;
            ApplyToGrid(new Vector2Int(data.x, data.y));
        }
    }
}