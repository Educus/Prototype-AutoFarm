using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    private GridManager gridManager;

    private void Awake()
    {
        gridManager = GetComponent<GridManager>();
    }

    public List<Node> FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        // 시작 위치가 막혀있으면 가장 가까운 이동 가능한 위치로 변경
        if (!gridManager.IsWalkable(startPos.x, startPos.y))
        {
            startPos = gridManager.FindNearestWalkable(startPos);
        }

        // 목적지가 막혀있으면 가장 가까운 이동 가능한 위치로 변경
        if (!gridManager.IsWalkable(targetPos.x, targetPos.y))
        {
            targetPos = gridManager.FindNearestWalkable(targetPos);
        }

        if (!gridManager.IsWalkable(startPos.x, startPos.y) ||
            !gridManager.IsWalkable(targetPos.x, targetPos.y))
        {
            return null;
        }

        ResetNodes();

        Node startNode =
            gridManager.GetNode(startPos.x, startPos.y);

        Node targetNode =
            gridManager.GetNode(targetPos.x, targetPos.y);

        if (startNode == null || targetNode == null)
            return null;

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        List<Node> open = new List<Node>();
        HashSet<Node> closed = new HashSet<Node>();

        open.Add(startNode);

        while (open.Count > 0)
        {
            Node current = open[0];

            foreach (var n in open)
            {
                if (n.fCost < current.fCost ||
                   (n.fCost == current.fCost && n.hCost < current.hCost))
                    current = n;
            }

            open.Remove(current);
            closed.Add(current);

            if (current == targetNode)
                return Retrace(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (neighbor == null || !neighbor.isWalkable || closed.Contains(neighbor))
                    continue;

                int newCost = current.gCost + GetDistance(current, neighbor);

                if (newCost < neighbor.gCost || !open.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = current;

                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }

        return null;
    }

    private void ResetNodes()
    {
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                Node node =
                    gridManager.GetNode(x, y);

                if (node == null)
                    continue;

                node.gCost = int.MaxValue;
                node.hCost = 0;
                node.parent = null;
            }
        }
    }

    List<Node> Retrace(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> list = new List<Node>();

        int[,] dirs = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

        for (int i = 0; i < 4; i++)
        {
            list.Add(gridManager.GetNode(
                node.x + dirs[i, 0],
                node.y + dirs[i, 1]
            ));
        }

        return list;
    }

    int GetDistance(Node a, Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}