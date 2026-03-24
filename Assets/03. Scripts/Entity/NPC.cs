using UnityEngine;


public class UpgPerk
{
    // 메인 인벤토리 레벨
    int mainInvLv = 0;
    // 서브 인벤토리 레벨
    int subInvLv = 0;
    // 스피드 레벨
    int speedLv = 0;
    // ???
    int efficLv = 0;
}
public class NPC : StatusBase
{
    // NPC 고유 ID
    [SerializeField] private int npcID;
    // NPC 보유 물 양
    [SerializeField] private int maxWater;
    [SerializeField] private int water;

    public override string GetStatus()
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract()
    {
        Debug.Log($"{entityName} 상호작용");
    }

    public override void SetStatus()
    {
        throw new System.NotImplementedException();
    }
}
