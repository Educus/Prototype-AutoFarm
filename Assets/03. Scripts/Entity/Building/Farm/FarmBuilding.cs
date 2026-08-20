using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FarmBuilding : BuildingBase
{
    public List<FarmTile> tiles = new List<FarmTile>();
    public List<FarmTileView> tileViews = new List<FarmTileView>();

    // 밭에 작업이 생겼을 때 호출
    public event Action<FarmBuilding> onWorkRequested;

    // NPC가 작업을 수행할 수 있는지 확인
    public bool HasPendingWork()
    {
        return HasHarvestWork() || HasPlantWork() || HasWaterWork();
    }

    #region Unity
        // 농장 타일 갯수
    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Farm;

        if (tiles.Count == 0)
        {
            for (int i = 0; i < 9; i++) 
                tiles.Add(new FarmTile());
        }

        // 초기 상태
        foreach (var tile in tiles)
        {
            tile.Harvest();
        }

        TimeManager.Instance.onMinuteEvent += Growing;

        ViewUpdate();

        if (HasPendingWork())
        {
            RequestWork();
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.onMinuteEvent -= Growing;
        }
    }

    private void LateUpdate()
    {
        ViewUpdate();
    }
    #endregion

    #region 작물 성장
    private void Growing(int minute)
    {
        bool growthChanged = false;

        foreach (var tile in tiles)
        {
            if (!tile.hasCrop)
                continue;

            // 성장 시간 감소
            bool grew = tile.UpdateGrowthTime(minute);

            if (grew)
            {
                growthChanged = true;
            }
        }

        if (growthChanged)
        {
            ViewUpdate();
        }

        // 수확 가능한 작물 여부
        if (HasPendingWork())
        {
            RequestWork();
        }
    }
    #endregion

    #region NPC 작업 확인
    // 수확할 작물이 있는가?
    public bool HasHarvestWork()
    {
        return tiles.Any(t => t.IsReady());
    }

    // 심을 수 있는 땅이 있는가?
    public bool HasPlantWork()
    {
        foreach (var tile in tiles)
        {
            if (tile.CanPlant())
                return true;
        }

        return false;
    }

    // 물을 줄 수 있는 작물이 있는가?
    public bool HasWaterWork()
    {
        return tiles.Any(t =>
        t.hasCrop &&
        !t.watered &&
        !t.IsReady());
    }


    // 실제 작업 대상 가져오기
    // 빈 땅 보유여부
    public List<FarmTile> GetPlantableTiles()
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

    #region RequestWork

    public void RequestWork()
    {
        onWorkRequested?.Invoke(this);
    }

    #endregion


    #region Player, NPC가 실행
    // 씨앗 심기
    public bool TryPlant(FarmTile tile, int seedID)
    {
        if (tile == null)
            return false;

        if (!tile.CanPlant())
            return false;

        tile.Plant(seedID);

        ViewUpdate();

        return true;
    }
    // 물 주기
    public bool TryWater(FarmTile tile)
    {
        if (tile == null)
            return false;

        if (!tile.hasCrop)
            return false;

        if (tile.watered)
            return false;

        tile.Water();

        ViewUpdate();

        return true;
    }
    public void TryWaterAll()
    {
        bool changed = false;

        foreach (var tile in tiles)
        {
            if (!tile.hasCrop)
                continue;

            if (tile.watered)
                continue;

            tile.Water();

            changed = true;
        }

        if (changed)
        {
            ViewUpdate();
        }
    }
    // 수확
    public int TryHarvest(FarmTile tile)
    {
        if (tile == null)
            return 0;

        if (!tile.IsReady())
            return 0;

        int value = tile.Harvest();

        ViewUpdate();

        // 수확 후 씨앗 심기 작업
        RequestWork();

        return value;
    }
    #endregion

    #region 작업 완료 확인
    // 현재 밭에 작업이 남아있는지 확인
    public bool HasAnyWork()
    {
        return HasHarvestWork() || HasPlantWork() || HasWaterWork();
    }
    #endregion

    #region View
    private void ViewUpdate()
    {
        foreach (var tileView in tileViews)
        {
            int index = tileView.index - 1;

            if (index < 0 || index >= tiles.Count)
                continue;

            FarmTile tile = tiles[index];

            int value = tile.watered ? 1 : 0;

            Sprite image = DataManager.Instance.GetCropImage(tile.cropID, tile.growth);

            tileView.UpdateView(value, image);
        }
    }
    #endregion

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
                growth = t.growth,
            })
        };

        return JsonUtility.ToJson(data);
    }
    public override void LoadJsonData(string json)
    {
        var data = JsonUtility.FromJson<FarmBuildingData>(json);

        if (data == null || data.tiles == null)
            return;

        int count = Mathf.Min(
            tiles.Count,
            data.tiles.Count
        );

        for (int i = 0; i < count; i++)
        {
            tiles[i].hasCrop =
                data.tiles[i].hasCrop;

            tiles[i].cropID =
                data.tiles[i].cropID;

            tiles[i].watered =
                data.tiles[i].watered;

            tiles[i].growth =
                data.tiles[i].growth;
        }

        ViewUpdate();

        // 불러온 상태에서 작업이 필요하면
        // NPC에게 알려야 함
        if (HasPendingWork())
        {
            RequestWork();
        }
    }
    #endregion

    // 플레이어가 상호작용
    public override void OnInteract()
    {
        Debug.Log("이거 말고;");
    }
}
