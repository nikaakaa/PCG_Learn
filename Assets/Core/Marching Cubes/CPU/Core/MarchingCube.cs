using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCube
{
    public Vector3 center;

    public Vector3[] corner;

    public float[] values;

    public MarchingCube(Vector3 center, Vector3[] corner, float[] values)
    {
        this.center = center;
        this.corner = corner;
        this.values = values;
    }

    public MarchingCube(Vector3[] corner, float[] values)
    {
        Vector3 c = Vector3.zero;
        for (int i = 0; i < corner.Length; i++) 
        {
            c += corner[i];
        }
        c /= corner.Length;
        this.center = c;
        this.corner = corner;
        this.values = values;
    }

    public void BuildInMesh(ProceduralMeshPart meshPart, float threshold, bool lerp)
    {
        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (values[i] < threshold) 
            {
                cubeIndex |= 1 << i;
            }
        }

        // Create triangles for current cube configuration
        for (int i = 0; MarchingCubesTable.triangulation[cubeIndex][i] != -1; i += 3)
        {
            // Get indices of corner points A and B for each of the three edges of the cube that need to be joined to form the triangle.
            int a0 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[cubeIndex][i]];
            int b0 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[cubeIndex][i]];
            int a1 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 1]];
            int b1 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 1]];
            int a2 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 2]];
            int b2 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 2]];

            //错误的平滑
            //Vector3 vertexA = lerp ? Vector3.Lerp(corner[a0], corner[b0], values[a0] / (values[a0] + values[b0])) : (corner[a0] + corner[b0]) * 0.5f;
            //Vector3 vertexB = lerp ? Vector3.Lerp(corner[a1], corner[b1], values[a1] / (values[a1] + values[b1])) : (corner[a1] + corner[b1]) * 0.5f;
            //Vector3 vertexC = lerp ? Vector3.Lerp(corner[a2], corner[b2], values[a2] / (values[a2] + values[b2])) : (corner[a2] + corner[b2]) * 0.5f;

            //正确的平滑
            Vector3 vertexA = lerp ? InterpolateEdgePosition(threshold, corner[a0], values[a0], corner[b0], values[b0]) : (corner[a0] + corner[b0]) * 0.5f;
            Vector3 vertexB = lerp ? InterpolateEdgePosition(threshold, corner[a1], values[a1], corner[b1], values[b1]) : (corner[a1] + corner[b1]) * 0.5f;
            Vector3 vertexC = lerp ? InterpolateEdgePosition(threshold, corner[a2], values[a2], corner[b2], values[b2]) : (corner[a2] + corner[b2]) * 0.5f;

            meshPart.AddTriangle(vertexA, vertexC, vertexB);
        }
    }

    private Vector3 InterpolateEdgePosition(float threshold, Vector3 vertex1, float value1, Vector3 vertex2, float value2)
    {
        Vector3 pointOnEdge = Vector3.zero;

        if (Mathf.Approximately(threshold - value1, 0) == true) return vertex1;
        if (Mathf.Approximately(threshold - value2, 0) == true) return vertex2;
        if (Mathf.Approximately(value1 - value2, 0) == true) return vertex1;

        float mu = (threshold - value1) / (value2 - value1);
        pointOnEdge.x = vertex1.x + mu * (vertex2.x - vertex1.x);
        pointOnEdge.y = vertex1.y + mu * (vertex2.y - vertex1.y);
        pointOnEdge.z = vertex1.z + mu * (vertex2.z - vertex1.z);

        return pointOnEdge;
    }

    public void DrawCube(Vector3 worldPos, Vector3 size, GizmosSettings gizmosSettings) 
    {
        if (gizmosSettings.debugCornerPoint) 
        {
            for (int i = 0; i < corner.Length; i++)
            {
                if (gizmosSettings.debugCornerPointAlpha)
                {
                    Gizmos.color = new Color(values[i], values[i], values[i], 1);
                }
                else 
                {
                    Gizmos.color = gizmosSettings.cornerPointColor * 0.5f + gizmosSettings.cornerPointColor / (i + 1);
                }
                Gizmos.DrawSphere(worldPos + corner[i], gizmosSettings.cornerPointSize);
            }
        }

        Gizmos.color = gizmosSettings.lineColor;
        Gizmos.DrawWireCube(worldPos, size);
    }
}
