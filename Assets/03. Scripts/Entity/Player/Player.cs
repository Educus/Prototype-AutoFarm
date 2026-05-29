using UnityEngine;
using UnityEngine.AI;

public class Player : StatusBase
{
    public float moveSpeed = 3f;

    IInteractable interactTarget;
    bool isInteracting;

    public Inventory mainInventory;
    public Inventory subInventory;

    void Start()
    {
        GameManager.Instance.player = this;

        mainInventory.Initialize(5);
        subInventory.Initialize(3);
    }

    public override void OnInteract()
    {
    }
}
