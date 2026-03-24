using UnityEngine;

public class Structure : EntityBase
{
    public override void OnInteract()
    {
        Debug.Log($"{entityName} 상호작용");
    }
    public override string GetStatus()
    {
        throw new System.NotImplementedException();
    }

    public override void SetStatus()
    {
        throw new System.NotImplementedException();
    }
}
