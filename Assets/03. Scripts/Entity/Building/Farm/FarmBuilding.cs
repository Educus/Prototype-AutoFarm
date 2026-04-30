using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FarmBuilding : BuildingBase
{
    public List<FarmTile> tiles = new List<FarmTile>();

    // 농장 타일 갯수
    private void Start()
    {
        type = BuildingType.Farm;

        if (tiles.Count == 0)
        {
            for (int i = 0; i < 9; i++) tiles.Add(new FarmTile());
        }

        TimeManager.Instance.onDayEvent += NextDay;
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
        return true;
    }
    // 물 주기
    public void Water(FarmTile tile)
    {
        if (tile.hasCrop)
            tile.Water();
    }
    public void WaterAll()
    {
        foreach (var tile in tiles)
        {
            tile.Water();
        }
    }
    public int TryHarvest(FarmTile tile)
    {
        if (!tile.IsReady())
            return 0;

        return tile.Harvest();
    }
    #endregion

    // 스크립트 내 실행
    // 다음날 물 초기화 및 성장
    private void NextDay()
    {
        foreach (var tile in tiles)
        {
            tile.Grow();
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
}
