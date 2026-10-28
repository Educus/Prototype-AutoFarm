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

    #region 파괴 규칙
    public override string GetDestroyBlockReason()
    {
        if (isDefaultBuilding)
            return "기본 건물을 파괴할 수 없습니다.";

        if (!IsInventoryEmpty())
            return "창고 안을 비워야 파괴할 수 있습니다.";

        return null;
    }

    private bool IsInventoryEmpty()
    {
        if (inventory == null)
            return true;

        foreach (var slot in inventory.slots)
        {
            if (!slot.IsEmpty())
                return false;
        }

        return true;
    }
    #endregion

    #region Intercation
    // 게임모드
    // None일 때 창고 UI
    // 
    public override void OnNormalInteract()
    {
        Debug.Log("StorageBuilding OnNormalInteract");

        UIStorageManagement.Instance.TargetBuilding(id);
        UIStorageManagement.Instance.BuildingInv();
    }

    public override void OnRInteract()
    {
        Debug.Log("StorageBuilding OnRInteract");
    }

    #endregion
}
