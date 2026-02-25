using UnityEngine;

public class PlayerInventory : InventoryBase
{
    private const int fixed_size = 8;

    protected override string saveFileName => "PlayerInventory";

    private void Awake()
    {
        Initialized(fixed_size);
    }

    // 게임 종료 시 인벤토리 저장(임시)
    private void OnApplicationQuit()
    {
        SaveInventory();
    }
}
