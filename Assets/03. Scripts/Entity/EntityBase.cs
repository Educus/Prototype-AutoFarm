using UnityEngine;

public abstract class EntityBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected string entityName;
    
    public virtual string GetName()
    {
        return entityName;
    }

    public abstract string GetStatus();

    public virtual void OnInteract(Player player)
    {
        Debug.Log($"{entityName} interacted by {player.GetName()}");
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
