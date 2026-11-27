using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public int chunkWidth = 40;
    public int chunkHeight = 30;

    private HashSet<Vector2Int> unlockedChunks = new HashSet<Vector2Int>();

    [Header("Debug")]
    public bool drawGizmos = true;
    public int gizmoRange = 2;
    public Color unlockedColor = new Color(0, 0, 1, 0.2f); // 파랑
    public Color lockedColor = new Color(0, 0, 0, 0.4f);   // 검정

    private void Start()
    {
        UnlockDefaultChunks();
    }

    private void UnlockDefaultChunks()
    {
        UnlockChunk(Vector2Int.zero);
        UnlockChunk(new Vector2Int(0, 1));
    }

    // 청크 좌표 변환
    public Vector2Int WorldToChunk(Vector2Int gridPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt((float)gridPos.x / chunkWidth),
            Mathf.FloorToInt((float)gridPos.y / chunkHeight)
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

    #region Save / Load
    public List<Vector2Int> GetSaveData()
    {
        return new List<Vector2Int>(unlockedChunks);
    }

    public void Load(List<Vector2Int> chunks)
    {
        unlockedChunks.Clear();

        if (chunks == null || chunks.Count == 0)
        {
            UnlockDefaultChunks();
            return;
        }

        foreach (var chunk in chunks)
            unlockedChunks.Add(chunk);
    }
    #endregion

    // Gizmos 시각화
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // 주변 몇 개만 그리기 (성능)
        int range = gizmoRange;

        for (int cx = -range; cx <= range; cx++)
        {
            for (int cy = -range; cy <= range; cy++)
            {
                Vector2Int chunkCoord = new Vector2Int(cx, cy);

                bool unlocked = unlockedChunks.Contains(chunkCoord);

                Gizmos.color = unlocked ? unlockedColor : lockedColor;

                Vector3 center = new Vector3(
                    (cx * chunkWidth) + chunkWidth / 2f,
                    (cy * chunkHeight) + chunkHeight / 2f,
                    0
                );

                Vector3 size = new Vector3(chunkWidth, chunkHeight, 0.1f);

                Gizmos.DrawCube(center, size);

                // 테두리
                Gizmos.color = unlocked ? Color.blue : Color.black;
                Gizmos.DrawWireCube(center, size);
            }
        }

        Gizmos.color = new Color(0f, 0f, 1f, 1f); // 진한 파랑

        foreach (var chunk in unlockedChunks)
        {
            DrawChunkBorder(chunk);
        }
    }

    void DrawChunkBorder(Vector2Int chunk)
    {
        float t = 0.2f; // 두께

        Vector3 center = new Vector3(chunk.x * chunkWidth + chunkWidth / 2f, chunk.y * chunkHeight + chunkHeight / 2f, 0);

        // 아래
        Gizmos.DrawCube(center + new Vector3(0, -chunkHeight / 2f, 0), new Vector3(chunkWidth, t, 0));

        // 위
        Gizmos.DrawCube(center + new Vector3(0, chunkHeight / 2f, 0), new Vector3(chunkWidth, t, 0));

        // 왼쪽
        Gizmos.DrawCube(center + new Vector3(-chunkWidth / 2f, 0, 0), new Vector3(t, chunkHeight, 0));

        // 오른쪽
        Gizmos.DrawCube(center + new Vector3(chunkWidth / 2f, 0, 0), new Vector3(t, chunkHeight, 0));
    }
}