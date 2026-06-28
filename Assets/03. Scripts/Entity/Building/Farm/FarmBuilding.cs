using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FarmBuilding : BuildingBase
{
    public List<FarmTile> tiles = new List<FarmTile>();
    public List<FarmTileView> tileViews = new List<FarmTileView>();

    // 농장 타일 갯수
    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Farm;

        if (tiles.Count == 0)
        {
            for (int i = 0; i < 9; i++) tiles.Add(new FarmTile());
        }

        foreach (var tile in tiles)
        {
            tile.Harvest();
        }

        TimeManager.Instance.onDayEvent += NextDay;
    }

    private void LateUpdate()
    {
        ViewUpdate();
    }

    #region NPC가 참조하는 영역
    // 빈 땅 보유여부
    public List<FarmTile>GetPlantableTiles()
    {
        return tiles.FindAll(t => t.CanPlant());
    }
    // 물 줬는지 여부
    public List<FarmTile> GetWaterableTiles()
    {
        return tiles.FindAll(t => t.hasCrop && !t.watered);
    }
    // 완성 작물 여부
    public List<FarmTile> GetHarvestableTiles()
    {
        return tiles.FindAll(t => t.IsReady());
    }
    #endregion

    #region NPC가 실행
    // 씨앗 심기
    public bool TryPlant(FarmTile tile, int seedID)
    {
        if (!tile.CanPlant())
            return false;

        tile.Plant(seedID);
        ViewUpdate();
        return true;
    }
    // 물 주기
    public void Water(FarmTile tile)
    {
        if (tile.hasCrop)
        {
            tile.Water();
            ViewUpdate();
        }
    }
    public void WaterAll()
    {
        foreach (var tile in tiles)
        {
            tile.Water();
            ViewUpdate();
        }
    }
    public int TryHarvest(FarmTile tile)
    {
        if (!tile.IsReady())
            return 0;

        int value = tile.Harvest();
        ViewUpdate();

        return value;
    }
    #endregion

    // 스크립트 내 실행
    // 다음날 물 초기화 및 성장
    private void NextDay()
    {
        foreach (var tile in tiles)
        {
            tile.Grow();
            ViewUpdate();
        }
    }

    private void ViewUpdate()
    {
        foreach (var tileView in tileViews)
        {
            int value = tiles[tileView.index - 1].watered ? 1 : 0;
            Sprite image = DataManager.Instance.GetCropImage(tiles[tileView.index - 1].cropID, tiles[tileView.index - 1].growth);

            tileView.UpdateView(value,image);
        }
    }

    #region Save Load
    public override string GetJsonData()
    {
        var data = new FarmBuildingData
        {
            tiles = tiles.ConvertAll(t => new FarmTileSaveData
            {
                hasCrop = t.hasCrop,
                cropID = t.cropID,
                watered = t.watered,
                growth = t.growth
            })
        };

        return JsonUtility.ToJson(data);
    }
    public override void LoadJsonData(string json)
    {
        var data = JsonUtility.FromJson<FarmBuildingData>(json);

        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].hasCrop = data.tiles[i].hasCrop;
            tiles[i].cropID = data.tiles[i].cropID;
            tiles[i].watered = data.tiles[i].watered;
            tiles[i].growth = data.tiles[i].growth;
        }
    }
    #endregion

    // 플레이어가 상호작용
    public override void OnInteract(int itemId)
    {
        Debug.Log("이거 말고;");
    }

    public void OnPlayerInteract(int index, int itemId)
    {
        FarmTile tile = tiles[index - 1];

        Debug.Log($"index : {index}, itemID : {itemId}");

        // 1. 심기
        if (itemId != 0 && DataManager.Instance.itemsData[itemId].itemType == ItemType.Seed && tile.CanPlant())
        {
            bool success = TryPlant(tile, itemId);
            if (success)
            {
                GameManager.Instance.player.subInventory.RemoveItem(itemId, 1);
            }
            return;
        }
        // 2. 물주기
        if (tile.hasCrop && !tile.watered)
        {
            Water(tile);
            return;
        }
        // 3. 수확
        if (tile.IsReady())
        {
            int item = TryHarvest(tile);
            if (item > 0)
            {
                GameManager.Instance.player.AddItemToInventory(item, 1);
            }
            return;
        }
        
    }
}
