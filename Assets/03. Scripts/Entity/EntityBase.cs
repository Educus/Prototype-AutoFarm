using UnityEngine;

public abstract class EntityBase : MonoBehaviour, ILeftInteractable
{
    [SerializeField] protected string entityName;

    public abstract int ClickPriority { get; }
    public abstract ClickTarget Target { get; }

    public virtual void SetName(string name)
    {
        entityName = name;
    }
    public virtual string GetName()
    {
        return entityName;
    }
    public virtual void OnLInteract()
    {
        Debug.Log($"{entityName} interact");
    }
}
