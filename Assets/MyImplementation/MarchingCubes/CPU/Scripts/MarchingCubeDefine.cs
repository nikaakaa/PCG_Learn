using System.Collections.Generic;
using UnityEngine;

public class MarchingCubePoint
{
    public Vector3 pos;
    public float value;

    public MarchingCubePoint(Vector3 pos, float value)
    {
        this.pos = pos;
        this.value = value;
    }
}
public class MarchingCube
{
    public Vector3 position;
    public List<MarchingCubePoint> cornerPoints;
    public List<Vector3> vertices = new();
    public List<int> trangles = new();
    public int state = 0;
    public MarchingCube(Vector3 position, List<MarchingCubePoint> cornerPoints, float threshold, bool isLerp)
    {
        this.position = position;
        this.cornerPoints = cornerPoints;
        state = 0;
        for (int i = 0; i < cornerPoints.Count; i++)
        {
            if (cornerPoints[i].value < threshold)
            {
                state |= 1 << i;
            }
        }
        for (int i = 0; i < MarchingCubesTable.triangulation[state].Length; i += 3)
        {
            if (MarchingCubesTable.triangulation[state][i] == -1)
            {
                break;
            }
            int index0 = MarchingCubesTable.triangulation[state][i];
            int index1 = MarchingCubesTable.triangulation[state][i + 1];
            int index2 = MarchingCubesTable.triangulation[state][i + 2];
            Vector3 v0 = GetEdgeVertex(index0, isLerp, threshold);
            Vector3 v1 = GetEdgeVertex(index1, isLerp, threshold);
            Vector3 v2 = GetEdgeVertex(index2, isLerp, threshold);
            int baseIndex = vertices.Count;
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            trangles.Add(baseIndex);
            trangles.Add(baseIndex + 1);
            trangles.Add(baseIndex + 2);

        }
    }
    public Vector3 GetEdgeVertex(int edgeIndex, bool isLerp, float threshold)
    {
        int cornerAIndex = MarchingCubesTable.cornerIndexAFromEdge[edgeIndex];
        int cornerBIndex = MarchingCubesTable.cornerIndexBFromEdge[edgeIndex];
        MarchingCubePoint cornerA = cornerPoints[cornerAIndex];
        MarchingCubePoint cornerB = cornerPoints[cornerBIndex];
        if (isLerp)
        {
            float t = (threshold - cornerA.value) / (cornerB.value - cornerA.value);
            return Vector3.Lerp(cornerA.pos, cornerB.pos, t);
        }
        else
        {
            return (cornerA.pos + cornerB.pos) * 0.5f;
        }
    }
}