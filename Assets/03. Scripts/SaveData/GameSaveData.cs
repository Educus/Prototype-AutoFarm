using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int version = 1;

    public PlayerSaveData player;           // 플레이어의 위치
    public PlayerDataSaveData playerData;   // 플레이어의 재화, 키 설정, 북마크
    public TimeSaveData time;
    public List<Vector2Int> unlockedChunks;

    public MarketSaveData market;
    public List<ItemCountEntry> shopCart;

    public List<BuildingSaveData> buildings;
    public List<InventorySaveData> inventories;
    public List<AnimalSaveData> animals;
    public List<NPCSaveData> npcs;
}