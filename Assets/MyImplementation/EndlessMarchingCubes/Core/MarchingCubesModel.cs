using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ChunkData
{
    public float chunkSize = 16f;
    public int numPointsPerAxis = 32;
    public float isoLevel = 0f;

    /// <summary>
    /// 点间距，动态计算：chunkSize / (numPointsPerAxis - 1)
    /// </summary>
    public float Spacing => chunkSize / (numPointsPerAxis - 1);
}
[Serializable]
public class ChunkInfo
{
    public ChunkData data;
    public Vector3Int coor;
}

/// <summary>
/// 实际与GPU共通存储的数据结构
/// </summary>
public struct Triangle
{
    public Vector3 v0;
    public Vector3 v1;
    public Vector3 v2;
    public Vector3 this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return v0;
                case 1:
                    return v1;
                default:
                    return v2;
            }
        }
    }
}
/// <summary>
/// 给unityMesh使用的数据结构
/// </summary>
public struct ChunkMeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    public ChunkMeshData(Vector3[] vertices, int[] triangles)
    {
        this.vertices = vertices;
        this.triangles = triangles;
    }
}
