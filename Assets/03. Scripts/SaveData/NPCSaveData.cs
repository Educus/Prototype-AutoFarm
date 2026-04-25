using System;
using UnityEngine;

[Serializable]
public class NPCSaveData
{
    public string id;
    public string entityName;

    public Vector3 position;

    public int water;
    public int maxWater;

    public InventorySaveData mainInventory;
    public InventorySaveData subInventory;
    public InventorySaveData upgradeInventory;
}