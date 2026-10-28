using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int version = 1;

    public PlayerSaveData player;
    public TimeSaveData time;
    public List<Vector2Int> unlockedChunks;

    public CurrencySaveData currency;
    public MarketSaveData market;
    public List<ItemCountEntry> shopCart;

    public List<BuildingSaveData> buildings;
    public List<InventorySaveData> inventories;
    public List<AnimalSaveData> animals;
    public List<NPCSaveData> npcs;

    public List<KeyCodeSaveData> keyCodes;
}