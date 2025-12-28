using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGenerator : ValueGenerator
{
    //public float radius = 16;

    public override ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float cubeSize, Vector3 worldCenter)
    {
        valueShader.SetFloat("radius", marchingCubesMesh.numPointsPerAxis * cubeSize * 0.5f);

        return base.Generate(pointsBuffer, numPointsPerAxis, cubeSize, worldCenter);
    }
}
