using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Marching Cubes 算法 - 参考答案
/// 
/// 这是 MyMarchingCubes.cs 的完整实现版本
/// 在你完成练习后可以对照检查
/// </summary>
public class MyMarchingCubesAnswer : MonoBehaviour
{
    #region 配置参数

    [Title("网格设置")]
    [SerializeField] private Vector3Int gridSize = new Vector3Int(16, 16, 16);
    [SerializeField] private float cellSize = 1f;

    [Title("算法参数")]
    [SerializeField, Range(0f, 1f)] private float threshold = 0.5f;
    [SerializeField] private bool useInterpolation = true;

    [Title("标量场类型")]
    [SerializeField] private ScalarFieldType fieldType = ScalarFieldType.Sphere;

    [ShowIf("fieldType", ScalarFieldType.Sphere)]
    [SerializeField] private float sphereRadius = 6f;

    [ShowIf("fieldType", ScalarFieldType.Noise)]
    [SerializeField] private float noiseScale = 0.1f;

    #endregion

    private float[,,] densityField;
    private List<Vector3> vertices;
    private List<int> triangles;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // 网格偏移量（用于居中）
    private Vector3 gridOffset;

    public enum ScalarFieldType { Sphere, Noise, Custom }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
    }

    [Button("生成 Mesh", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
    public void Generate()
    {
        // 确保组件已初始化（编辑器模式下 Awake 可能未调用）
        EnsureComponents();

        vertices = new List<Vector3>();
        triangles = new List<int>();

        SampleDensityField();
        MarchAllCubes();
        BuildMesh();

        Debug.Log($"[MarchingCubes] 生成完成: {vertices.Count} 顶点, {triangles.Count / 3} 三角形");
    }

    private void EnsureComponents()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
                meshRenderer.material = new Material(Shader.Find("Standard"));
            }
        }
    }

    private void SampleDensityField()
    {
        int sizeX = gridSize.x + 1;
        int sizeY = gridSize.y + 1;
        int sizeZ = gridSize.z + 1;

        densityField = new float[sizeX, sizeY, sizeZ];

        // 计算偏移量使模型居中
        gridOffset = new Vector3(
            gridSize.x * cellSize * 0.5f,
            gridSize.y * cellSize * 0.5f,
            gridSize.z * cellSize * 0.5f
        );

        // center 用于球体等对称形状的密度计算
        Vector3 center = gridOffset;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    Vector3 worldPos = new Vector3(x, y, z) * cellSize;
                    densityField[x, y, z] = CalculateDensity(worldPos, center);
                }
            }
        }
    }

    private float CalculateDensity(Vector3 position, Vector3 center)
    {
        switch (fieldType)
        {
            case ScalarFieldType.Sphere:
                float distance = Vector3.Distance(position, center);
                return distance / sphereRadius;

            case ScalarFieldType.Noise:
                float noise = Mathf.PerlinNoise(
                    position.x * noiseScale,
                    position.z * noiseScale
                );
                float heightFactor = position.y / (gridSize.y * cellSize);
                return heightFactor + (1 - noise) * 0.5f;

            default:
                return 0f;
        }
    }

    private void MarchAllCubes()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    ProcessCube(x, y, z);
                }
            }
        }
    }

    /// <summary>
    /// 处理单个立方体 - 完整实现
    /// </summary>
    private void ProcessCube(int x, int y, int z)
    {
        // ============================================================
        // 答案 1: 获取立方体 8 个顶点的位置和密度值
        // ============================================================
        Vector3[] cubeCorners = new Vector3[8];
        float[] cubeValues = new float[8];

        // v0: (x, y, z+1) - 底面左前
        cubeCorners[0] = new Vector3(x, y, z + 1) * cellSize - gridOffset;
        cubeValues[0] = densityField[x, y, z + 1];

        // v1: (x+1, y, z+1) - 底面右前
        cubeCorners[1] = new Vector3(x + 1, y, z + 1) * cellSize - gridOffset;
        cubeValues[1] = densityField[x + 1, y, z + 1];

        // v2: (x+1, y, z) - 底面右后
        cubeCorners[2] = new Vector3(x + 1, y, z) * cellSize - gridOffset;
        cubeValues[2] = densityField[x + 1, y, z];

        // v3: (x, y, z) - 底面左后
        cubeCorners[3] = new Vector3(x, y, z) * cellSize - gridOffset;
        cubeValues[3] = densityField[x, y, z];

        // v4: (x, y+1, z+1) - 顶面左前
        cubeCorners[4] = new Vector3(x, y + 1, z + 1) * cellSize - gridOffset;
        cubeValues[4] = densityField[x, y + 1, z + 1];

        // v5: (x+1, y+1, z+1) - 顶面右前
        cubeCorners[5] = new Vector3(x + 1, y + 1, z + 1) * cellSize - gridOffset;
        cubeValues[5] = densityField[x + 1, y + 1, z + 1];

        // v6: (x+1, y+1, z) - 顶面右后
        cubeCorners[6] = new Vector3(x + 1, y + 1, z) * cellSize - gridOffset;
        cubeValues[6] = densityField[x + 1, y + 1, z];

        // v7: (x, y+1, z) - 顶面左后
        cubeCorners[7] = new Vector3(x, y + 1, z) * cellSize - gridOffset;
        cubeValues[7] = densityField[x, y + 1, z];

        // ============================================================
        // 答案 2: 计算 cubeIndex
        // ============================================================
        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cubeValues[i] < threshold)
            {
                cubeIndex |= (1 << i);
            }
        }

        // 如果 cubeIndex 为 0 或 255，表示全在外部或全在内部，无需生成三角形
        if (cubeIndex == 0 || cubeIndex == 255)
            return;

        // ============================================================
        // 答案 3: 根据 cubeIndex 查表生成三角形
        // ============================================================
        int[] triangleEdges = MarchingCubesLookupTable.triangulation[cubeIndex];

        for (int i = 0; triangleEdges[i] != -1; i += 3)
        {
            // 获取三条边的索引
            int edgeA = triangleEdges[i];
            int edgeB = triangleEdges[i + 1];
            int edgeC = triangleEdges[i + 2];

            // 边 A 的两个端点
            int a0 = MarchingCubesLookupTable.cornerIndexAFromEdge[edgeA];
            int b0 = MarchingCubesLookupTable.cornerIndexBFromEdge[edgeA];

            // 边 B 的两个端点
            int a1 = MarchingCubesLookupTable.cornerIndexAFromEdge[edgeB];
            int b1 = MarchingCubesLookupTable.cornerIndexBFromEdge[edgeB];

            // 边 C 的两个端点
            int a2 = MarchingCubesLookupTable.cornerIndexAFromEdge[edgeC];
            int b2 = MarchingCubesLookupTable.cornerIndexBFromEdge[edgeC];

            // 计算三角形顶点（在边上插值）
            Vector3 vertA = InterpolateVertex(
                cubeCorners[a0], cubeValues[a0],
                cubeCorners[b0], cubeValues[b0]
            );
            Vector3 vertB = InterpolateVertex(
                cubeCorners[a1], cubeValues[a1],
                cubeCorners[b1], cubeValues[b1]
            );
            Vector3 vertC = InterpolateVertex(
                cubeCorners[a2], cubeValues[a2],
                cubeCorners[b2], cubeValues[b2]
            );

            // 添加三角形
            AddTriangle(vertA, vertB, vertC);
        }
    }

    /// <summary>
    /// 边插值 - 完整实现
    /// </summary>
    private Vector3 InterpolateVertex(Vector3 p1, float v1, Vector3 p2, float v2)
    {
        if (!useInterpolation)
        {
            return (p1 + p2) * 0.5f;
        }

        // ============================================================
        // 答案 4: 线性插值实现
        // ============================================================

        // 边界情况处理
        if (Mathf.Approximately(threshold, v1)) return p1;
        if (Mathf.Approximately(threshold, v2)) return p2;
        if (Mathf.Approximately(v1, v2)) return p1;

        // 线性插值
        float t = (threshold - v1) / (v2 - v1);
        return p1 + t * (p2 - p1);
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(index);
        triangles.Add(index + 2);
        triangles.Add(index + 1);
    }

    private void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "MarchingCubes Mesh";

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    [Title("调试")]
    [SerializeField] private bool showGizmos = false;
    [SerializeField] private bool showOnlyInsidePoints = true;

    private void OnDrawGizmos()
    {
        if (!showGizmos || densityField == null) return;

        for (int x = 0; x <= gridSize.x; x++)
        {
            for (int y = 0; y <= gridSize.y; y++)
            {
                for (int z = 0; z <= gridSize.z; z++)
                {
                    float value = densityField[x, y, z];
                    bool isInside = value < threshold;

                    if (showOnlyInsidePoints && !isInside) continue;

                    Gizmos.color = isInside ? Color.green : Color.red;
                    Vector3 pos = transform.position + new Vector3(x, y, z) * cellSize;
                    Gizmos.DrawSphere(pos, cellSize * 0.1f);
                }
            }
        }
    }
}
