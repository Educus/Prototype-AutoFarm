using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Queue<Vector2> path = new Queue<Vector2>();
    private IInteractable interactTarget;

    private bool isMoving = false;

    public void MoveTo(Vector2 targetPos, IInteractable target)
    {
        interactTarget = target;

        path = PathFinder.FindPath(transform.position, targetPos);

        StopAllCoroutines();
        StartCoroutine(IEMoveRoutine());

    }

    IEnumerator IEMoveRoutine()
    {
        isMoving = true;

        while (path.Count > 0)
        {
            Vector2 nextPos = path.Dequeue();

            while ((Vector2)transform.position != nextPos)
            {
                transform.position = Vector2.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                
                yield return null;
            }
        }

        isMoving = false;

        if (interactTarget != null)
        {
            interactTarget.Interact(player);
            interactTarget = null;
        }
    }

}
