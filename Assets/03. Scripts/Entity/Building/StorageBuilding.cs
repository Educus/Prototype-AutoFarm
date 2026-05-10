using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class StorageBuilding : BuildingBase
{
    public Inventory unified;


    void Start()
    {
        type = BuildingType.Storage;

        unified = GetComponent<Inventory>();
        unified.type = InventoryType.Unified;
    }

    public override string GetJsonData()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadJsonData(string json)
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract()
    {
        throw new System.NotImplementedException();
    }
}
