using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class MarchingSquaresMap
{
    public int cellSize = 1;
    public int width = 10;
    public int length = 10;
    public int height = 2;
    public bool build3D = false;
    public bool isLerp = true;
    public float threshold = 0.5f;
    public float scale = 0.1f;  // 噪声缩放（越小越平滑，越大越细碎）
    public Vector2 noiseOffset = new Vector2(1000, 1000);  // 噪声偏移（打破中心对称）
    public List<MarchingSquarePoint> points = new();
    public List<MarchingSquare> squares = new();

    // Mesh 数据
    public List<Vector3> vertices = new();
    public List<int> triangles = new();

    public void GenerateMap(Vector3 mapCenter)
    {
        ClearMap();
        GeneratePoints(mapCenter);
        Debug.Log($"[MarchingSquares] Points generated: {points.Count}");

        GenerateSquares(mapCenter);
        Debug.Log($"[MarchingSquares] Squares generated: {squares.Count}");

        GenerateTroughValue();
        int totalEdgeVerts = 0;
        foreach (var sq in squares) totalEdgeVerts += sq.edgeVertices.Count;
        Debug.Log($"[MarchingSquares] Total edge vertices: {totalEdgeVerts}");

        GenerateMesh();
        Debug.Log($"[MarchingSquares] Vertices: {vertices.Count}, Triangles: {triangles.Count}");
    }
    private void GeneratePoints(Vector3 mapCenter)
    {
        points.Clear();

        // 计算点的数量
        int pointsX = width / cellSize + 1;   // x 方向点数
        int pointsZ = length / cellSize + 1;  // z 方向点数

        float startX = mapCenter.x - width / 2f;
        float startZ = mapCenter.z - length / 2f;

        // 按 x（列）为外层，z（行）为内层生成点
        for (int ix = 0; ix < pointsX; ix++)
        {
            for (int iz = 0; iz < pointsZ; iz++)
            {
                float x = startX + ix * cellSize;
                float z = startZ + iz * cellSize;
                float value = Mathf.PerlinNoise(x * scale + noiseOffset.x, z * scale + noiseOffset.y);
                points.Add(new MarchingSquarePoint(
                    new Vector3(x, mapCenter.y, z), value));
            }
        }
    }
    public void ClearMap()
    {
        points.Clear();
        squares.Clear();
        vertices.Clear();
        triangles.Clear();
    }

    private void GenerateSquares(Vector3 mapCenter)
    {
        squares.Clear();

        // 点的数量
        int pointsX = width / cellSize + 1;   // x 方向点数（列数）
        int pointsZ = length / cellSize + 1;  // z 方向点数（每列的点数）

        // 方格数 = 点数 - 1
        int squaresX = pointsX - 1;
        int squaresZ = pointsZ - 1;

        for (int ix = 0; ix < squaresX; ix++)
        {
            for (int iz = 0; iz < squaresZ; iz++)
            {
                // 当前列和下一列的起始索引
                int currentColStart = ix * pointsZ;
                int nextColStart = (ix + 1) * pointsZ;

                // 四个角点
                // z 小的是 bottom，z 大的是 top
                MarchingSquarePoint bottomLeft = points[currentColStart + iz];
                MarchingSquarePoint topLeft = points[currentColStart + iz + 1];
                MarchingSquarePoint bottomRight = points[nextColStart + iz];
                MarchingSquarePoint topRight = points[nextColStart + iz + 1];

                Vector3 centerPos = new Vector3(
                    (bottomLeft.position.x + bottomRight.position.x) / 2,
                    mapCenter.y,
                    (bottomLeft.position.z + topLeft.position.z) / 2);

                squares.Add(new MarchingSquare(
                    centerPos,
                    topLeft,
                    topRight,
                    bottomLeft,
                    bottomRight));
            }
        }
    }
    private void GenerateTroughValue()
    {
        // 调试：统计 state 分布
        int[] stateCounts = new int[16];
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < squares.Count; i++)
        {
            MarchingSquare square = squares[i];
            square.state = 0;
            square.edgeVertices.Clear();

            // 记录最小最大值
            foreach (var pt in square.points)
            {
                if (pt.value < minValue) minValue = pt.value;
                if (pt.value > maxValue) maxValue = pt.value;
            }

            if (square.points[0].value > threshold) square.state += 1;  // topLeft
            if (square.points[1].value > threshold) square.state += 2;  // topRight
            if (square.points[2].value > threshold) square.state += 4;  // bottomLeft
            if (square.points[3].value > threshold) square.state += 8;  // bottomRight

            stateCounts[square.state]++;

            // 边的定义：
            // top    = topLeft(0) ↔ topRight(1)
            // left   = topLeft(0) ↔ bottomLeft(2)
            // right  = topRight(1) ↔ bottomRight(3)
            // bottom = bottomLeft(2) ↔ bottomRight(3)

            switch (square.state)
            {
                case 0: // 全空
                    break;

                case 1: // 只有 topLeft
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    break;

                case 2: // 只有 topRight
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    break;

                case 3: // topLeft + topRight（上半边）
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    break;

                case 4: // 只有 bottomLeft
                    square.edgeVertices.Add(square.points[2].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    break;

                case 5: // topLeft + bottomLeft（左半边）
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    square.edgeVertices.Add(square.points[2].position);
                    break;

                case 6: // topRight + bottomLeft（对角）
                    // 三角形1: topRight
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    // 三角形2: bottomLeft
                    square.edgeVertices.Add(square.points[2].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    break;

                case 7: // topLeft + topRight + bottomLeft（缺右下）
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    square.edgeVertices.Add(square.points[2].position);
                    break;

                case 8: // 只有 bottomRight
                    square.edgeVertices.Add(square.points[3].position);
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    break;

                case 9: // topLeft + bottomRight（对角）
                    // 三角形1: topLeft
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    // 三角形2: bottomRight
                    square.edgeVertices.Add(square.points[3].position);
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    break;

                case 10: // topRight + bottomRight（右半边）
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(square.points[3].position);
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    break;

                case 11: // topLeft + topRight + bottomRight（缺左下）
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(square.points[3].position);
                    square.edgeVertices.Add(LerpEdge(square.points[2], square.points[3])); // bottom 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    break;

                case 12: // bottomLeft + bottomRight（下半边）
                    square.edgeVertices.Add(square.points[2].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    square.edgeVertices.Add(square.points[3].position);
                    break;

                case 13: // topLeft + bottomLeft + bottomRight（缺右上）
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    square.edgeVertices.Add(LerpEdge(square.points[1], square.points[3])); // right 边
                    square.edgeVertices.Add(square.points[3].position);
                    square.edgeVertices.Add(square.points[2].position);
                    break;

                case 14: // topRight + bottomLeft + bottomRight（缺左上）
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(square.points[3].position);
                    square.edgeVertices.Add(square.points[2].position);
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[2])); // left 边
                    square.edgeVertices.Add(LerpEdge(square.points[0], square.points[1])); // top 边
                    break;

                case 15: // 全满
                    square.edgeVertices.Add(square.points[0].position);
                    square.edgeVertices.Add(square.points[1].position);
                    square.edgeVertices.Add(square.points[3].position);
                    square.edgeVertices.Add(square.points[2].position);
                    break;
            }
        }

        // 输出调试信息
        Debug.Log($"[MarchingSquares] Noise value range: {minValue:F3} ~ {maxValue:F3}, threshold: {threshold}");
        string stateInfo = "";
        for (int s = 0; s < 16; s++)
        {
            if (stateCounts[s] > 0) stateInfo += $"state{s}:{stateCounts[s]} ";
        }
        Debug.Log($"[MarchingSquares] State distribution: {stateInfo}");
    }

    /// <summary>
    /// 在两点之间根据阈值进行线性插值
    /// </summary>
    private Vector3 LerpEdge(MarchingSquarePoint p1, MarchingSquarePoint p2)
    {
        if (!isLerp)
        {
            return (p1.position + p2.position) / 2f;
        }

        if (Mathf.Abs(p2.value - p1.value) < 0.0001f)
        {
            return (p1.position + p2.position) / 2f;
        }

        float t = (threshold - p1.value) / (p2.value - p1.value);
        t = Mathf.Clamp01(t);

        return new Vector3(
            Mathf.Lerp(p1.position.x, p2.position.x, t),
            Mathf.Lerp(p1.position.y, p2.position.y, t),
            Mathf.Lerp(p1.position.z, p2.position.z, t)
        );
    }
    private void DrawAllSquares()
    {
        vertices.Clear();
        triangles.Clear();

        for (int i = 0; i < squares.Count; i++)
        {
            DrawOneSquare(squares[i]);
        }
    }

    private void DrawOneSquare(MarchingSquare square)
    {
        if (square.edgeVertices.Count < 3) return;

        // 根据 state 选择正确的多边形处理方式
        // 对角情况 (case 6, 9) 是两个独立三角形

        switch (square.state)
        {
            case 0:
                break;

            case 1:
            case 2:
            case 4:
            case 8: // 三角形
                if (build3D)
                    AddTriangle3D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2]);
                else
                    AddTriangle2D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2]);
                break;

            case 3:
            case 5:
            case 10:
            case 12: // 四边形
                if (build3D)
                    AddQuad3D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2], square.edgeVertices[3]);
                else
                    AddQuad2D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2], square.edgeVertices[3]);
                break;

            case 6:
            case 9: // 对角：两个独立三角形
                if (build3D)
                {
                    AddTriangle3D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2]);
                    AddTriangle3D(square.edgeVertices[3], square.edgeVertices[4], square.edgeVertices[5]);
                }
                else
                {
                    AddTriangle2D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2]);
                    AddTriangle2D(square.edgeVertices[3], square.edgeVertices[4], square.edgeVertices[5]);
                }
                break;

            case 7:
            case 11:
            case 13:
            case 14: // 五边形
                if (build3D)
                    AddPentagon3D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2],
                                  square.edgeVertices[3], square.edgeVertices[4]);
                else
                    AddPentagon2D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2],
                                  square.edgeVertices[3], square.edgeVertices[4]);
                break;

            case 15: // 全满四边形
                if (build3D)
                    AddQuad3D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2], square.edgeVertices[3]);
                else
                    AddQuad2D(square.edgeVertices[0], square.edgeVertices[1], square.edgeVertices[2], square.edgeVertices[3]);
                break;
        }
    }

    #region 2D 模式

    private void AddTriangle2D(Vector3 a, Vector3 b, Vector3 c)
    {
        int startIndex = vertices.Count;
        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
        triangles.Add(startIndex);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);
    }

    private void AddQuad2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        AddTriangle2D(a, b, c);
        AddTriangle2D(a, c, d);
    }

    private void AddPentagon2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        AddTriangle2D(a, b, c);
        AddTriangle2D(a, c, d);
        AddTriangle2D(a, d, e);
    }

    #endregion

    #region 3D 模式

    private void AddTriangle3D(Vector3 a, Vector3 b, Vector3 c)
    {
        // 顶面
        AddTriangle2D(a + Vector3.up * height, b + Vector3.up * height, c + Vector3.up * height);
        // 底面（反向）
        AddTriangle2D(c, b, a);
        // 侧面
        AddSideQuad(a, b);
        AddSideQuad(b, c);
        AddSideQuad(c, a);
    }

    private void AddQuad3D(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // 顶面
        AddQuad2D(a + Vector3.up * height, b + Vector3.up * height, c + Vector3.up * height, d + Vector3.up * height);
        // 底面（反向）
        AddQuad2D(d, c, b, a);
        // 侧面
        AddSideQuad(a, b);
        AddSideQuad(b, c);
        AddSideQuad(c, d);
        AddSideQuad(d, a);
    }

    private void AddPentagon3D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
    {
        // 顶面
        AddPentagon2D(a + Vector3.up * height, b + Vector3.up * height, c + Vector3.up * height,
                      d + Vector3.up * height, e + Vector3.up * height);
        // 底面（反向）
        AddPentagon2D(e, d, c, b, a);
        // 侧面
        AddSideQuad(a, b);
        AddSideQuad(b, c);
        AddSideQuad(c, d);
        AddSideQuad(d, e);
        AddSideQuad(e, a);
    }

    #endregion

    /// <summary>
    /// 添加侧面四边形（两个三角形）
    /// </summary>
    private void AddSideQuad(Vector3 bottom1, Vector3 bottom2)
    {
        Vector3 top1 = bottom1 + Vector3.up * height;
        Vector3 top2 = bottom2 + Vector3.up * height;
        // 侧面四边形：bottom1 -> bottom2 -> top2 -> top1
        AddTriangle2D(bottom1, bottom2, top2);
        AddTriangle2D(bottom1, top2, top1);
    }

    /// <summary>
    /// 生成并返回 Mesh 对象
    /// </summary>
    private Mesh GenerateMesh()
    {
        DrawAllSquares();

        Mesh mesh = new Mesh();

        // 使用 32 位索引格式，支持超过 65535 个顶点
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}

[System.Serializable]
public class MarchingSquare
{
    public Vector3 CenterPos;
    public int state;
    public List<MarchingSquarePoint> points = new();
    public List<Vector3> edgeVertices = new();

    public MarchingSquare(Vector3 centerPos,
                          MarchingSquarePoint topLeft,
                          MarchingSquarePoint topRight,
                          MarchingSquarePoint bottomLeft,
                          MarchingSquarePoint bottomRight)
    {
        CenterPos = centerPos;
        points.Add(topLeft);
        points.Add(topRight);
        points.Add(bottomLeft);
        points.Add(bottomRight);
    }
}
[System.Serializable]
public class MarchingSquarePoint
{
    public Vector3 position;
    public float value;

    public MarchingSquarePoint(Vector3 position, float value)
    {
        this.position = position;
        this.value = value;
    }
}
