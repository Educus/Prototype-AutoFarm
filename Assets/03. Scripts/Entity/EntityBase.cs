using UnityEngine;

public abstract class EntityBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected string entityName;
    
    public virtual void SetName(string name)
    {
        entityName = name;
    }
    public virtual string GetName()
    {
        return entityName;
    }
    public virtual void OnInteract()
    {
        Debug.Log($"{entityName} interact");
    }
}
