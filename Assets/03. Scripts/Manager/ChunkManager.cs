using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public int chunkSize = 100;

    private HashSet<Vector2Int> unlockedChunks = new HashSet<Vector2Int>();

    [Header("Debug")]
    public bool drawGizmos = true;
    public Color unlockedColor = new Color(0, 0, 1, 0.2f); // 파랑
    public Color lockedColor = new Color(0, 0, 0, 0.4f);   // 검정

    private void Start()
    {
        UnlockChunk(Vector2Int.zero);
        UnlockChunk(new Vector2Int(0,1));
    }

    // 청크 좌표 변환
    public Vector2Int WorldToChunk(Vector2Int gridPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)gridPos.x / chunkSize),
            Mathf.FloorToInt((float)gridPos.y / chunkSize)
        );
    }

    // 사용 가능 여부
    public bool IsUnlocked(Vector2Int gridPos)
    {
        Vector2Int chunk = WorldToChunk(gridPos);
        return unlockedChunks.Contains(chunk);
    }

    // 청크 해금
    public void UnlockChunk(Vector2Int chunkCoord)
    {
        if (unlockedChunks.Contains(chunkCoord)) return;

        unlockedChunks.Add(chunkCoord);
    }

    // Gizmos 시각화
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // 주변 몇 개만 그리기 (성능)
        int range = 3;

        for (int cx = -range; cx <= range; cx++)
        {
            for (int cy = -range; cy <= range; cy++)
            {
                Vector2Int chunkCoord = new Vector2Int(cx, cy);

                bool unlocked = unlockedChunks.Contains(chunkCoord);

                Gizmos.color = unlocked ? unlockedColor : lockedColor;

                Vector3 center = new Vector3(
                    (cx * chunkSize) + chunkSize / 2f,
                    (cy * chunkSize) + chunkSize / 2f,
                    0
                );

                Vector3 size = new Vector3(chunkSize, chunkSize, 0.1f);

                Gizmos.DrawCube(center, size);

                // 테두리
                Gizmos.color = unlocked ? Color.blue : Color.black;
                Gizmos.DrawWireCube(center, size);
            }
        }

        Gizmos.color = new Color(0f, 0f, 1f, 1f); // 진한 파랑

        foreach (var chunk in unlockedChunks)
        {
            DrawChunkBorder(chunk, chunkSize);
        }
    }

    void DrawChunkBorder(Vector2Int chunk, float size)
    {
        float t = 0.2f; // 두께

        Vector3 center = new Vector3(chunk.x * size + size / 2, chunk.y * size + size / 2, 0);

        // 아래
        Gizmos.DrawCube(center + new Vector3(0, -size / 2, 0), new Vector3(size, t, 0));

        // 위
        Gizmos.DrawCube(center + new Vector3(0, size / 2, 0), new Vector3(size, t, 0));

        // 왼쪽
        Gizmos.DrawCube(center + new Vector3(-size / 2, 0, 0), new Vector3(t, size, 0));

        // 오른쪽
        Gizmos.DrawCube(center + new Vector3(size / 2, 0, 0), new Vector3(t, size, 0));
    }
}
/*
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    // 청크 관리
    // 청크 크기 나중에 수정
    public int ChunkSize = 100;
    private HashSet<Vector2Int> unlockedChunks = new HashSet<Vector2Int>();

    private void Start()
    {
        // 초기 청크 언락 (0,0)
        UnlockChunk(Vector2Int.zero);
    }

    public bool IsUnlocked(Vector2Int gridPos)
    {
        Vector2Int chunk = new Vector2Int(gridPos.x / ChunkSize, gridPos.y / ChunkSize);

        return unlockedChunks.Contains(chunk);
    }

    public void UnlockChunk(Vector2Int chunkCoord)
    {
        unlockedChunks.Add(chunkCoord);
    }
}
*/