using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class StorageBuilding : BuildingBase
{
    public Inventory unified;

    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Storage;

    }

    private void Start()
    {
        InitializeInventories();

        Debug.Log(id);
    }

    private void InitializeInventories()
    {
        Debug.Log($"{id} µî·Ï");

        unified = GetComponent<Inventory>();
        unified.id = id;
        unified.type = InventoryType.Unified;

        if (unified.slots.Count == 0)
            unified.Initialize(30);

        DataManager.Instance.InventoryManager.Register(unified);
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
        UIStorageManagement.Instance.TargetBuilding(id);
        UIStorageManagement.Instance.BuildingInv();
    }
}
