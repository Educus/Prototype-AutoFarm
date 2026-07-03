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
    private Node[,] gridBuilding;

    [SerializeField] private ChunkManager chunkManager;
    public ChunkManager ChunkManager { get; private set; }
    public Pathfinder Pathfinder { get; private set; }

    public static GridManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        ChunkManager = chunkManager;
        Pathfinder = GetComponent<Pathfinder>();

        GenerateGrid();
    }

    // Grid 생성
    void GenerateGrid()
    {
        grid = new Node[width, height];
        gridBuilding = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 기본은 이동 가능
                grid[x, y] = new Node(x, y, true, TileType.Ground);
                gridBuilding[x, y] = new Node(x, y, true, TileType.Ground);
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
    public Node GetBuildingNode(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
            return null;
        return gridBuilding[x, y];
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

    // 갈 수 없는 곳일때 가장 가까운 이동 가능 좌표 찾기 (BFS)
    public Vector2Int FindNearestWalkable(Vector2Int target)
    {
        // 목표가 이동 가능하면 그대로 반환
        if (IsWalkable(target.x, target.y))
            return target;

        Queue<Vector2Int> queue = new();
        HashSet<Vector2Int> visited = new();

        queue.Enqueue(target);
        visited.Add(target);

        Vector2Int[] dirs =
        {
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up
    };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (var dir in dirs)
            {
                Vector2Int next = current + dir;

                if (visited.Contains(next))
                    continue;

                visited.Add(next);

                if (IsWalkable(next.x, next.y))
                    return next;

                if (GetNode(next.x, next.y) != null)
                    queue.Enqueue(next);
            }
        }

        return target;
    }

    // NPC 이동 시, 이동 불가 좌표일 경우 가장 가까운 이동 가능 좌표를 찾는 함수 (반경 탐색)
    public Vector2Int GetNearestWalkablePosition(Vector2Int center)
    {
        List<Vector2Int> candidates = new();

        for (int radius = 1; radius <= Mathf.Max(width, height); radius++)
        {
            candidates.Clear();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius &&
                        Mathf.Abs(y) != radius)
                        continue;

                    Vector2Int pos = center + new Vector2Int(x, y);

                    if (IsWalkable(pos.x, pos.y))
                    {
                        candidates.Add(pos);
                    }
                }
            }

            // 현재 반경에서 하나라도 찾았다면
            if (candidates.Count > 0)
            {
                Vector2Int best = candidates[0];

                foreach (var pos in candidates)
                {
                    if (pos.y < best.y)
                    {
                        best = pos;
                    }
                }

                return best;
            }
        }

        // 못 찾으면 원래 위치 반환
        return center;
    }

    // 이동 불가 좌표일 경우 가장 가까운 이동 가능 좌표를 찾는 함수 (BFS)
    public Vector2Int GetNearestWalkableFrom(Vector2Int start)
    {
        if (IsWalkable(start.x, start.y))
            return start;

        Queue<Vector2Int> queue = new();
        HashSet<Vector2Int> visited = new();

        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] dirs =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (var dir in dirs)
            {
                Vector2Int next = current + dir;

                if (visited.Contains(next))
                    continue;

                visited.Add(next);

                if (IsWalkable(next.x, next.y))
                    return next;

                if (GetNode(next.x, next.y) != null)
                    queue.Enqueue(next);
            }
        }

        return start;
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
    public void SetBuildingBlocked(int x, int y, bool blocked)
    {
        Node node = GetBuildingNode(x, y);

        if (node == null)
            return;

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

    // 설치 가능 여부 검사
    public bool CanPlaceBuilding(int x, int y)
    {
        Node node = GetBuildingNode(x, y);

        if (node == null)
            return false;

        return node.isWalkable;
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


