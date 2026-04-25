using UnityEngine;

public class BuildingInteraction : EntityBase
{
    public override void OnInteract()
    {
        Debug.Log($"{entityName} 상호작용");
    }
}
