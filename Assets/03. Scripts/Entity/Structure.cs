using UnityEngine;

public class Structure : EntityBase
{
    public override string GetStatus()
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract(Player player)
    {
        Debug.Log($"{entityName} with Player!");
    }
}
