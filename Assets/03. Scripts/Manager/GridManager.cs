using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    // 타일의 종류
    // Ground = 통과 가능, Block = 막힘, Water = 물(이후 낚시 컨텐츠)
    Ground,
    Block,
    Water
}

public class Node
{
    public bool isWalkable;
    public TileType tileType;
    public Vector2Int pos;

    public Node(bool walkable, Vector2Int pos)
    {
        isWalkable = walkable;
        this.pos = pos;
        tileType = TileType.Ground;
    }
}

public class GridManager : MonoBehaviour
{
    public Dictionary<Vector2Int, Node> grid = new Dictionary<Vector2Int, Node>();

    public Node GetNode(int x, int y)
    {
        Vector2Int pos = new Vector2Int(x, y);

        if (!grid.ContainsKey(pos))
        { 
            grid[pos] = new Node(true, pos);
        }

        return grid[pos];
    }

    public void SetBlocked(int x, int y, bool blocked)
    {
        Node node = GetNode(x, y);
        node.isWalkable = !blocked;
        node.tileType = blocked ? TileType.Block : TileType.Ground;
    }

    public void SetWater(int x, int y)
    {
        Node node = GetNode(x, y);
        node.isWalkable = false;
        node.tileType = TileType.Water;
    }

    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
    }
}


