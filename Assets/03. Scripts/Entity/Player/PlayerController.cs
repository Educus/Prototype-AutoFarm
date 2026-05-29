using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Pathfinder pathfinder;

    private Coroutine moveCoroutine;

    public float speed = 5f;

    private Vector2Int currentGridPos;

    void Start()
    {
        currentGridPos = gridManager.WorldToGrid(transform.position);
    }

    public void Move(List<Node> path, System.Action onComplete = null)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveAlongPath(path, onComplete));
    }

    IEnumerator MoveAlongPath(List<Node> path, System.Action onComplete)
    {
        foreach (Node node in path)
        {
            Vector3 target =
                new Vector3(node.x + 0.5f, node.y, 0);

            while (Vector3.Distance(
                transform.position,
                target) > 0.05f)
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        target,
                        speed * Time.deltaTime
                    );

                yield return null;
            }
        }

        onComplete?.Invoke();

            // foreach (Node node in path)
            // {
            //     Vector3 target = new Vector3(node.x + 0.5f, node.y, 0);
            // 
            //     while (Vector3.Distance(transform.position, target) > 0.05f)
            //     {
            //         transform.position = Vector3.MoveTowards(
            //             transform.position,
            //             target,
            //             speed * Time.deltaTime
            //         );
            // 
            //         yield return null;
            //     }
            // 
            //     currentGridPos = new Vector2Int(node.x, node.y);
            // }
    }


    public Vector2Int GetCurrentGridPos()
    {
        return currentGridPos;
    }
}