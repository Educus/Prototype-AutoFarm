using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

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

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2Int end = gridManager.WorldToGrid(mouse);

            List<Node> path = pathfinder.FindPath(currentGridPos, end);

            if (path != null && path.Count > 0)
                Move(path);
        }
    }

    void Move(List<Node> path)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveAlongPath(path));
    }
    
    IEnumerator MoveAlongPath(List<Node> path)
    {
        foreach (Node node in path)
        {
            Vector3 target = new Vector3(node.x + 0.5f, node.y, 0);

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            currentGridPos = new Vector2Int(node.x, node.y);
        }
    }
}