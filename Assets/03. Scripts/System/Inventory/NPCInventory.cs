using UnityEngine;

public class NPCInventory : InventoryBase
{
    [SerializeField]
    private string npcName;

    [SerializeField]
    private int defaultSize = 20;

    protected override string saveFileName => $"NPCInventory_{npcName}";

    private void Awake()
    {
        Initialized(defaultSize);
    }

    // 게임 종료 시 인벤토리 저장(임시)
    private void OnApplicationQuit()
    {
        SaveInventory();
    }

    // 인벤토리 확장
    public void Expand(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            slots.Add(new InventorySlot(0, 0));
        }

        Debug.Log($"{gameObject.name}의 인벤토리가 {amount}칸 확장됨. 현재 크기 : {slots.Count}");
    }
}
