using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public List<InventorySaveData> inventories;
    public List<NPCSaveData> npcs;
    public CurrencySaveData currency;
}