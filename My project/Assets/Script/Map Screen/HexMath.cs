using UnityEngine;

public static class HexMath
{
    // axial 방향 (진짜 hex 표준)
    public static Vector2Int[] EvenRowDirs =
    {
        new Vector2Int(+1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
    };

    public static Vector2Int[] OddRowDirs =
    {
        new Vector2Int(+1, 0),
        new Vector2Int(+1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(+1, 1),
    };

    // axial hex -> UI (pointy top 기준)
    public static Vector2 HexToUI(int col, int row, float size)
    {
        float x = size * (Mathf.Sqrt(3f) * (col + (row % 2 == 0 ? 0f : 0.5f)));
        float y = size * (3f / 2f * row);

        return new Vector2(x, -y);
    }

    public static int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dq = a.x - b.x;
        int dr = a.y - b.y;

        return (Mathf.Abs(dq) +
                Mathf.Abs(dr) +
                Mathf.Abs(dq + dr)) / 2;
    }
}