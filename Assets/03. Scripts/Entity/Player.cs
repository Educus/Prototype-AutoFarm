using UnityEngine;
using UnityEngine.AI;

public class Player : EntityBase
{
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

    void Update()
    {
        
    }
}
