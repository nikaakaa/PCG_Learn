using System;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubeMapBase
{
    public Vector3 cubeSize = new Vector3(1, 1, 1);
    public Vector3 mapSize = new Vector3(32, 32, 32);
    public Vector3 mapCenter = Vector3.zero;
    public float threshold = 0.5f;
    public bool isLerp;

    public List<MarchingCubePoint> points = new List<MarchingCubePoint>();
    public List<MarchingCube> marchingCubes = new List<MarchingCube>();

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public void Generate()
    {
        vertices.Clear();
        triangles.Clear();
        GeneratePoints();
        GenerateCubes();
    }
    private void GeneratePoints()
    {
        points.Clear();
        int i = (int)(mapSize.x / cubeSize.x);
        int j = (int)(mapSize.y / cubeSize.y);
        int k = (int)(mapSize.z / cubeSize.z);
        Vector3 offset = new Vector3(i * cubeSize.x, j * cubeSize.y, k * cubeSize.z) * 0.5f;
        //注意是等于
        for (int x = 0; x <= i; x++)
        {
            for (int y = 0; y <= j; y++)
            {
                for (int z = 0; z <= k; z++)
                {
                    Vector3 pointPos = new Vector3(x * cubeSize.x, y * cubeSize.y, z * cubeSize.z) - offset + mapCenter;
                    float pointValue = SetPointValue(pointPos);
                    points.Add(new MarchingCubePoint(pointPos, pointValue));
                }
            }
        }
    }
    protected virtual float SetPointValue(Vector3 position)
    {
        return 1;
    }
    private void GenerateCubes()
    {
        marchingCubes.Clear();
        int countX = (int)(mapSize.x / cubeSize.x);
        int countY = (int)(mapSize.y / cubeSize.y);
        int countZ = (int)(mapSize.z / cubeSize.z);
        int strideY = countZ + 1;
        int strideX = (countY + 1) * (countZ + 1);

        for (int x = 0; x < countX; x++)
        {
            for (int y = 0; y < countY; y++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    // 底面左下角id
                    int id = x * strideX + y * strideY + z;
                    MarchingCubePoint p0 = points[id + 1];
                    MarchingCubePoint p1 = points[id + strideX + 1];
                    MarchingCubePoint p2 = points[id + strideX];
                    MarchingCubePoint p3 = points[id];
                    MarchingCubePoint p4 = points[id + strideY + 1];
                    MarchingCubePoint p5 = points[id + strideY + strideX + 1];
                    MarchingCubePoint p6 = points[id + strideY + strideX];
                    MarchingCubePoint p7 = points[id + strideY];

                    MarchingCube cube = new MarchingCube(
                        (p0.pos + p6.pos) * 0.5f,
                        new List<MarchingCubePoint>()
                        {
                            p0, p1, p2, p3, p4, p5, p6, p7
                        },
                        threshold,
                        isLerp);
                    marchingCubes.Add(cube);

                    // 关键：合并时需要偏移三角形索引
                    int vertexOffset = vertices.Count;
                    vertices.AddRange(cube.vertices);
                    foreach (int t in cube.trangles)
                    {
                        triangles.Add(t + vertexOffset);
                    }
                }
            }
        }
    }
}

[Serializable]
public class MarchingCubeMap_Sphere : MarchingCubeMapBase
{
    public float radius = 5;
    protected override float SetPointValue(Vector3 position)
    {
        return radius - Vector3.Distance(position, mapCenter);
    }
}

[Serializable]
public class MarchingCubeMap_SimplexNoise : MarchingCubeMapBase
{
    public float noiseScale = 0.1f;
    protected override float SetPointValue(Vector3 position)
    {
        return SimplexNoise.Generate3D(
            position.x * noiseScale,
            position.y * noiseScale,
            position.z * noiseScale);
    }
}

[Serializable]
public class MarchingCubeMap_Terrain : MarchingCubeMapBase
{
    public float noiseScale = 0.1f;      // 噪声缩放（越小越平滑）
    public float heightScale = 5f;       // 地形高度范围
    public float baseHeight = 0f;        // 基础地面高度

    protected override float SetPointValue(Vector3 position)
    {
        // 用 2D 噪声（只用 x 和 z）计算地表高度
        float surfaceHeight = SimplexNoise.Generate3D(
            position.x * noiseScale,
            0,  // y 固定为 0，让噪声只根据 xz 变化
            position.z * noiseScale) * heightScale + baseHeight;

        // 低于地表高度的点为正值（实体），高于的为负值（空气）
        return surfaceHeight - position.y;
    }
}





