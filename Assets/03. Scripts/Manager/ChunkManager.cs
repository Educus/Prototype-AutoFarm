using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    // 没农 包府
    // 没农 农扁 唱吝俊 荐沥
    public int ChunkSize = 100;
    private HashSet<Vector2Int> unlockedChunks = new HashSet<Vector2Int>();

    private void Start()
    {
        // 檬扁 没农 攫遏 (0,0)
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
