using UnityEngine;

public class NPCInventory : Inventory
{
    [SerializeField]
    private string npcName;

    [SerializeField]
    private int defaultSize = 20;


    private void Awake()
    {
    }

    // 게임 종료 시 인벤토리 저장(임시)
    private void OnApplicationQuit()
    {
    }

    // 인벤토리 확장
    public void Expand(int amount)
    {

    }
}
