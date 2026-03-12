using UnityEngine;

public abstract class StatusBase : EntityBase
{
    [SerializeField] protected float maxHp = 0;
    [SerializeField] protected float hp = 0;
    [SerializeField] protected float speed = 3.0f;
    [SerializeField] protected int invenNum = 0;

    public virtual void ChangeHp(float value)
    {
        hp -= value;
    }
    public virtual float OnSpeed()
    {
        return speed;
    }
    public virtual int OnInvenNum()
    {
        return invenNum;
    }
}
