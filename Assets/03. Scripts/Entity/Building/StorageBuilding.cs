using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class StorageBuilding : BuildingBase
{
    public Inventory unified;

    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Storage;

        if (inventory != null)
        {
            inventory.type = InventoryType.Unified;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        Debug.Log($"{id} 등록");

        // Inventory 연결
        if (inventory == null)
        {
            Debug.LogError($"Inventory missing : {gameObject.name}");
            return;
        }

        // Inventory 설정
        inventory.id = id;
        inventory.type = InventoryType.Unified;

        // 처음 생성 시만 슬롯 생성
        if (inventory.slots == null || inventory.slots.Count == 0)
        {
            inventory.Initialize(30);
        }

        // Inventory 등록
        DataManager.Instance.InventoryManager.Register(inventory);
    }

    public override string GetJsonData()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadJsonData(string json)
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract(int itemId)
    {
        UIStorageManagement.Instance.TargetBuilding(id);
        UIStorageManagement.Instance.BuildingInv();
    }
}
