using UnityEngine;
using UnityEngine.AI;

public class Player : StatusBase
{
    public float moveSpeed = 3f;

    NavMeshAgent agent;

    IInteractable interactTarget;
    bool isInteracting;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
