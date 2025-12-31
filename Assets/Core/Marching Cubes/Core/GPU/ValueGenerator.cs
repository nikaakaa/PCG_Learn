using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueGenerator
{
    const int threadGroupSize = 8;

    public ComputeShader valueShader;

    public MarchingCubesMesh marchingCubesMesh;

    protected List<ComputeBuffer> buffersToRelease;

    private void OnValidate()
    {
        // 防止编辑器启动时 marchingCubesMesh 未赋值导致的空引用
        if (marchingCubesMesh != null)
        {
            marchingCubesMesh.ValueUpdate();
        }
    }

    public virtual ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float cubeSize, Vector3 worldCenter)
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / (float)threadGroupSize);
        // Points buffer is populated inside shader with pos (xyz) + density (w).
        // Set paramaters
        valueShader.SetBuffer(0, "points", pointsBuffer);
        valueShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        valueShader.SetVector("center", worldCenter);
        valueShader.SetFloat("cubeSize", cubeSize);

        // Dispatch shader
        valueShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        if (buffersToRelease != null)
        {
            foreach (var b in buffersToRelease)
            {
                b.Release();
            }
        }

        // Return voxel data buffer so it can be used to generate mesh
        return pointsBuffer;
    }
}
