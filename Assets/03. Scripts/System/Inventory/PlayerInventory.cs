using UnityEngine;

public class PlayerInventory : Inventory
{
    private const int fixed_size = 8;

    private void Awake()
    {
    }

    // 게임 종료 시 인벤토리 저장(임시)
    private void OnApplicationQuit()
    {
    }
}
