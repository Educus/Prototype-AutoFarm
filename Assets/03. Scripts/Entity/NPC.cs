using UnityEngine;

public class NPC : StatusBase
{
    public override string GetStatus()
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract()
    {
        Debug.Log($"{entityName} 상호작용");
    }
}
