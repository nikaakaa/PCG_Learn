using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Marching Cubes 算法练习
/// 
/// ============================================================
///                     算法概述
/// ============================================================
/// 
/// Marching Cubes 是一种从标量场（Scalar Field）中提取等值面的算法。
/// 
/// 核心思想：
/// 1. 将空间划分为网格，每个格点存储一个"密度值"
/// 2. 遍历每个立方体（由 8 个相邻格点组成）
/// 3. 根据 8 个顶点的密度值与阈值的关系，确定配置类型（共 256 种）
/// 4. 查表获取需要生成的三角形
/// 5. 在边上插值计算精确的顶点位置
/// 
/// ============================================================
///                   立方体顶点编号
/// ============================================================
/// 
///        4--------5
///       /|       /|
///      / |      / |
///     7--------6  |
///     |  0-----|--1
///     | /      | /
///     |/       |/
///     3--------2
/// 
/// </summary>
public class MyMarchingCubes : MonoBehaviour
{
    #region 配置参数

    [Title("网格设置")]
    [Tooltip("网格分辨率（每个轴的格子数）")]
    [SerializeField] private Vector3Int gridSize = new Vector3Int(16, 16, 16);

    [Tooltip("每个格子的大小")]
    [SerializeField] private float cellSize = 1f;

    [Title("算法参数")]
    [Tooltip("等值面阈值：密度值小于此值的点被视为'内部'")]
    [SerializeField, Range(0f, 1f)] private float threshold = 0.5f;

    [Tooltip("是否使用插值（关闭则顶点在边中点）")]
    [SerializeField] private bool useInterpolation = true;

    [Title("标量场类型")]
    [SerializeField] private ScalarFieldType fieldType = ScalarFieldType.Sphere;

    [ShowIf("fieldType", ScalarFieldType.Sphere)]
    [SerializeField] private float sphereRadius = 6f;

    [ShowIf("fieldType", ScalarFieldType.Noise)]
    [SerializeField] private float noiseScale = 0.1f;

    #endregion

    #region 运行时数据

    // 网格点数据：存储每个采样点的位置和密度值
    private float[,,] densityField;

    // 生成的 Mesh 数据
    private List<Vector3> vertices;
    private List<int> triangles;

    // 组件引用
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    #endregion

    public enum ScalarFieldType
    {
        Sphere,     // 球体
        Noise,      // 噪声
        Custom      // 自定义
    }

    #region Unity 生命周期

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

    #endregion

    #region 主入口

    [Button("生成 Mesh", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 0.4f)]
    public void Generate()
    {
        // 确保组件已初始化（编辑器模式下 Awake 可能未调用）
        EnsureComponents();

        // 初始化数据
        vertices = new List<Vector3>();
        triangles = new List<int>();

        // Step 1: 采样标量场
        SampleDensityField();

        // Step 2: 遍历所有立方体，提取三角形
        MarchAllCubes();

        // Step 3: 构建 Mesh
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

    #endregion

    #region Step 1: 采样标量场

    /// <summary>
    /// 在网格的每个采样点计算密度值
    /// </summary>
    private void SampleDensityField()
    {
        // 注意：网格点数 = 格子数 + 1
        int sizeX = gridSize.x + 1;
        int sizeY = gridSize.y + 1;
        int sizeZ = gridSize.z + 1;

        densityField = new float[sizeX, sizeY, sizeZ];

        // 计算网格中心（用于球体等对称形状）
        Vector3 center = new Vector3(
            gridSize.x * cellSize * 0.5f,
            gridSize.y * cellSize * 0.5f,
            gridSize.z * cellSize * 0.5f
        );

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

    /// <summary>
    /// 计算某个点的密度值
    /// </summary>
    private float CalculateDensity(Vector3 position, Vector3 center)
    {
        switch (fieldType)
        {
            case ScalarFieldType.Sphere:
                // 球体：返回到中心的距离（归一化）
                float distance = Vector3.Distance(position, center);
                return distance / sphereRadius;

            case ScalarFieldType.Noise:
                // 噪声：使用 Perlin 噪声
                float noise = Mathf.PerlinNoise(
                    position.x * noiseScale,
                    position.z * noiseScale
                );
                // 加入高度因素
                float heightFactor = position.y / (gridSize.y * cellSize);
                return heightFactor + (1 - noise) * 0.5f;

            default:
                return 0f;
        }
    }

    #endregion

    #region Step 2: 遍历立方体

    /// <summary>
    /// 遍历所有立方体单元，对每个单元调用 ProcessCube
    /// </summary>
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
    /// 处理单个立方体
    /// 这是算法的核心！
    /// </summary>
    private void ProcessCube(int x, int y, int z)
    {
        // ============================================================
        // TODO 1: 获取立方体 8 个顶点的位置和密度值
        // ============================================================
        // 顶点编号（见类顶部的 ASCII 图）：
        //   v0: (x,   y,   z+1)  底面-左前
        //   v1: (x+1, y,   z+1)  底面-右前
        //   v2: (x+1, y,   z)    底面-右后
        //   v3: (x,   y,   z)    底面-左后
        //   v4: (x,   y+1, z+1)  顶面-左前
        //   v5: (x+1, y+1, z+1)  顶面-右前
        //   v6: (x+1, y+1, z)    顶面-右后
        //   v7: (x,   y+1, z)    顶面-左后

        Vector3[] cubeCorners = new Vector3[8];
        float[] cubeValues = new float[8];

        // 提示：使用 cellSize 计算世界坐标
        // 提示：使用 densityField[x, y, z] 获取密度值

        // ===== 在这里填写代码 =====
        // cubeCorners[0] = ...
        // cubeValues[0] = ...
        // ... 填写全部 8 个顶点



        // ============================================================
        // TODO 2: 计算 cubeIndex（配置索引）
        // ============================================================
        // cubeIndex 是一个 0-255 的整数，表示 256 种配置中的哪一种
        // 
        // 规则：
        // - 如果顶点 i 的密度值 < threshold，则该顶点在"内部"
        // - 将第 i 位设为 1：cubeIndex |= (1 << i)
        // 
        // 例如：如果只有 v0 在内部，则 cubeIndex = 1 (二进制 00000001)
        // 例如：如果 v0 和 v1 都在内部，则 cubeIndex = 3 (二进制 00000011)

        int cubeIndex = 0;

        // ===== 在这里填写代码 =====
        // for (int i = 0; i < 8; i++)
        // {
        //     if (cubeValues[i] < threshold)
        //     {
        //         cubeIndex |= ...
        //     }
        // }



        // ============================================================
        // TODO 3: 根据 cubeIndex 查表生成三角形
        // ============================================================
        // 使用 MarchingCubesLookupTable.triangulation[cubeIndex] 获取需要的边
        // 
        // 三角形表的格式：每 3 个数字代表一个三角形的 3 条边
        // 例如：{0, 8, 3, -1, ...} 表示一个三角形使用边 0、边 8、边 3
        // -1 表示结束
        //
        // 对于每条边，需要：
        // 1. 从 cornerIndexAFromEdge 和 cornerIndexBFromEdge 获取边的两个端点
        // 2. 调用 InterpolateVertex() 计算边上的顶点位置
        // 3. 添加顶点和三角形索引

        int[] triangleEdges = MarchingCubesLookupTable.triangulation[cubeIndex];

        // ===== 在这里填写代码 =====
        // for (int i = 0; triangleEdges[i] != -1; i += 3)
        // {
        //     int edgeA = triangleEdges[i];
        //     int edgeB = triangleEdges[i + 1];
        //     int edgeC = triangleEdges[i + 2];
        //     
        //     // 获取每条边的两个端点索引
        //     int a0 = MarchingCubesLookupTable.cornerIndexAFromEdge[edgeA];
        //     int b0 = MarchingCubesLookupTable.cornerIndexBFromEdge[edgeA];
        //     // ... 对 edgeB 和 edgeC 做同样的操作
        //     
        //     // 计算三角形的三个顶点
        //     Vector3 vertA = InterpolateVertex(
        //         cubeCorners[a0], cubeValues[a0],
        //         cubeCorners[b0], cubeValues[b0]
        //     );
        //     // ... 对其他两个顶点做同样的操作
        //     
        //     // 添加三角形
        //     AddTriangle(vertA, vertB, vertC);
        // }

    }

    #endregion

    #region Step 3: 辅助方法

    /// <summary>
    /// 在边上插值计算顶点位置
    /// </summary>
    /// <param name="p1">边的第一个端点位置</param>
    /// <param name="v1">第一个端点的密度值</param>
    /// <param name="p2">边的第二个端点位置</param>
    /// <param name="v2">第二个端点的密度值</param>
    /// <returns>等值面与边的交点位置</returns>
    private Vector3 InterpolateVertex(Vector3 p1, float v1, Vector3 p2, float v2)
    {
        if (!useInterpolation)
        {
            // 不使用插值时，返回中点
            return (p1 + p2) * 0.5f;
        }

        // ============================================================
        // TODO 4: 实现线性插值
        // ============================================================
        // 目标：找到边上密度值等于 threshold 的点
        // 
        // 线性插值公式：
        //   t = (threshold - v1) / (v2 - v1)
        //   result = p1 + t * (p2 - p1)
        // 
        // 边界处理：
        // - 如果 v1 ≈ threshold，直接返回 p1
        // - 如果 v2 ≈ threshold，直接返回 p2
        // - 如果 v1 ≈ v2（除零保护），返回 p1

        // ===== 在这里填写代码 =====
        // 提示：使用 Mathf.Approximately() 判断近似相等



        return (p1 + p2) * 0.5f; // 临时返回中点，替换为你的实现
    }

    /// <summary>
    /// 添加一个三角形
    /// </summary>
    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        // 注意顶点顺序决定法线朝向（顺时针/逆时针）
        triangles.Add(index);
        triangles.Add(index + 2);  // 交换顺序以翻转法线
        triangles.Add(index + 1);
    }

    #endregion

    #region Step 4: 构建 Mesh

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

    #endregion

    #region 调试可视化

    [Title("调试")]
    [SerializeField] private bool showGizmos = false;
    [SerializeField] private bool showOnlyInsidePoints = true;

    private void OnDrawGizmos()
    {
        if (!showGizmos || densityField == null) return;

        Vector3 center = new Vector3(
            gridSize.x * cellSize * 0.5f,
            gridSize.y * cellSize * 0.5f,
            gridSize.z * cellSize * 0.5f
        );

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

    #endregion
}
