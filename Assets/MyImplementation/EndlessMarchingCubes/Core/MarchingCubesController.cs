using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 这个是controller层
/// </summary>
public class ChunkMarchingCubesMap
{
    public ChunkInfo chunkInfo;
    [SerializeReference] public ValueGenerator valueGenerator;
    public List<Vector3> vertices = new();
    public List<int> triangleIndices = new();
    public ComputeShader marchingCubesShader;
    public ComputeBuffer triangleBuffer;
    public ComputeBuffer triangleCountBuffer;

    public void Generate(float isoLevel, int numPointsPerAxis, float cubeSize, Vector3 center)
    {
        GenerateValue(isoLevel, numPointsPerAxis, cubeSize, center);
        GenerateMesh(isoLevel, numPointsPerAxis, cubeSize, center);
        GetDataFromGPU();
    }
    public void Release()
    {
        valueGenerator?.pointsBuffer?.Release();
        triangleBuffer?.Release();
        triangleCountBuffer?.Release();
    }
    private void GenerateValue(float isoLevel, int numCountPerAxis, float cubeSize, Vector3 center)
    {
        valueGenerator.Generate(isoLevel, numCountPerAxis, cubeSize, center);
    }
    private void GenerateMesh(float isoLevel, int numCountPerAxis, float cubeSize, Vector3 center)
    {
        int numGroupsAxisCount = Mathf.CeilToInt((numCountPerAxis - 1) / 8.0f);
        int maxTriangleCount = (numCountPerAxis - 1) * (numCountPerAxis - 1) * (numCountPerAxis - 1) * 5;
        triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 9, ComputeBufferType.Append);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        marchingCubesShader.SetInt("numPointsPerAxis", numCountPerAxis);  // 匹配 Shader 变量名
        marchingCubesShader.SetFloat("cubeSize", cubeSize);
        marchingCubesShader.SetFloat("isoLevel", isoLevel);
        marchingCubesShader.SetFloats("chunkCenter", new float[] { center.x, center.y, center.z });
        marchingCubesShader.SetBuffer(0, "pointsBuffer", valueGenerator.pointsBuffer);
        marchingCubesShader.SetBuffer(0, "triangleBuffer", triangleBuffer);
        triangleBuffer.SetCounterValue(0);
        marchingCubesShader.Dispatch(0, numGroupsAxisCount, numGroupsAxisCount, numGroupsAxisCount);
    }
    private void GetDataFromGPU()
    {
        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        int[] triCountArray = { 0 };
        triangleCountBuffer.GetData(triCountArray);
        int triCount = triCountArray[0];

        Triangle[] triangleArray = new Triangle[triCount];
        triangleBuffer.GetData(triangleArray, 0, 0, triCount);

        vertices.Clear();
        triangleIndices.Clear();
        for (int i = 0; i < triCount; i++)
        {
            Triangle tri = triangleArray[i];
            int baseIndex = vertices.Count;
            vertices.Add(tri.v0);
            vertices.Add(tri.v1);
            vertices.Add(tri.v2);
            triangleIndices.Add(baseIndex);
            triangleIndices.Add(baseIndex + 1);
            triangleIndices.Add(baseIndex + 2);
        }
    }

}

/// <summary>
/// 这个序列化是为了多态序列化组合
/// </summary>
[Serializable]
public abstract class ValueGenerator
{
    public ComputeShader ValueGenShader;
    public ComputeBuffer pointsBuffer;
    public abstract void Generate(float isoLevel, int numCountPerAxis, float cubeSize, Vector3 worldOffset);
}
[Serializable]
public class SimplexNoiseGen : ValueGenerator
{
    public float scale;
    public override void Generate(float isoLevel, int numCountPerAxis, float cubeSize, Vector3 worldOffset)
    {

        int numPointCount = numCountPerAxis * numCountPerAxis * numCountPerAxis;
        if (pointsBuffer == null || pointsBuffer.count != numPointCount)
        {
            if (pointsBuffer != null)
            {
                pointsBuffer.Release();
            }
            pointsBuffer = new ComputeBuffer(numPointCount, sizeof(float) * 4);
        }
        int numGroupsAxisCount = Mathf.CeilToInt(numCountPerAxis / 8.0f);
        ValueGenShader.SetFloat("scale", scale);
        ValueGenShader.SetFloat("cubeSize", cubeSize);
        ValueGenShader.SetInt("numPointsPerAxis", numCountPerAxis);  // 匹配 Shader 变量名
        ValueGenShader.SetVector("center", worldOffset);  // 匹配 Shader 变量名
        ValueGenShader.SetBuffer(0, "pointsBuffer", pointsBuffer);
        ValueGenShader.Dispatch(0, numGroupsAxisCount, numGroupsAxisCount, numGroupsAxisCount);
    }
}