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
    public int x;
    public int y;

    public bool isWalkable;
    public TileType tileType;

    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;
    public Node parent;

    public Node(int x, int y, bool isWalkable, TileType tileType)
    {
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
        this.tileType = tileType;
    }
}

public class GridManager : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public float cellSize = 1f;

    private Node[,] grid;

    [SerializeField] private ChunkManager chunkManager;

    private void Awake()
    {
        GenerateGrid();
    }

    // Grid 생성
    void GenerateGrid()
    {
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 기본은 이동 가능
                grid[x, y] = new Node(x, y, true, TileType.Ground);
            }
        }
    }

    // Node 가져오기
    public Node GetNode(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return null;

        return grid[x, y];
    }

    // 이동 가능 여부(청크 포함)
    public bool IsWalkable(int x, int y)
    {
        Node node = GetNode(x, y);
        if (node == null) return false;

        if (!node.isWalkable) return false;

        if (!chunkManager.IsUnlocked(new Vector2Int(x, y))) return false;

        return true;
    }

    // 월드 → 그리드 좌표
    public Vector2Int WorldToGrid(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);

        return new Vector2Int(x, y);
    }

    // 그리드 → 월드 좌표
    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x * cellSize, y * cellSize, 0);
    }

    // 캐릭터 위치 수정
    public Vector3 GetCellCenter(int x, int y)
    {
        return new Vector3(
            x * cellSize + cellSize / 2f,
            y * cellSize + cellSize / 2f,
            0
        );
    }

    // 이동 막기 / 풀기
    public void SetBlocked(int x, int y, bool blocked)
    {
        Node node = GetNode(x, y);
        if (node == null) return;

        node.isWalkable = !blocked;
    }

    // 타일 타입 설정 (물 등)
    public void SetTileType(int x, int y, TileType type)
    {
        Node node = GetNode(x, y);
        if (node == null) return;

        node.tileType = type;

        if (type == TileType.Water)
            node.isWalkable = false;
    }

    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = grid[x, y];
                if (node == null) continue;

                Vector3 pos = GridToWorld(x, y);

                Vector3 center = pos + new Vector3(cellSize / 2f, cellSize / 2f, 0);

                // 색상 설정
                if (node.tileType == TileType.Water)
                    Gizmos.color = Color.blue;
                else if (!node.isWalkable)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = Color.white;

                Gizmos.DrawWireCube(center, Vector3.one * cellSize * 0.9f);
                // Gizmos.DrawWireCube(pos, Vector3.one * 0.9f);   // 사각형 테두리만
                // Gizmos.DrawCube(pos, Vector3.one * 0.9f);       // 사각형 체움
            }
        }
    }

    //////////////////////////// 
    //////////////////////////// 
    /*
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
    */
}


