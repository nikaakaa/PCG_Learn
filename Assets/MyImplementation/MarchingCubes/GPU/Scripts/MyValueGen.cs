using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class MyValueGen
{
    const int threadGroupSize = 8;

    public ComputeShader valueShader;

    protected List<ComputeBuffer> buffersToRelease;

    public virtual ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float cubeSize, Vector3 worldCenter)
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / (float)threadGroupSize);

        valueShader.SetBuffer(0, "points", pointsBuffer);
        valueShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        valueShader.SetVector("center", worldCenter);
        valueShader.SetFloat("cubeSize", cubeSize);

        valueShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        if (buffersToRelease != null)
        {
            foreach (var buffer in buffersToRelease)
            {
                buffer.Release();
            }
        }

        return pointsBuffer;
    }
}