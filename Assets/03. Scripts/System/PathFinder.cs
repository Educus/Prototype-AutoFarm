using UnityEngine;
using System.Collections.Generic;

public static class PathFinder
{
    public static Queue<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        // 길찾기 알고리즘 구현 예정
        // 현 직선이동
        Queue<Vector2> path = new Queue<Vector2>();

        path.Enqueue(end);

        return path;
    }
}
