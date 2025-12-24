using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquare
{
    public Vector3 centerPoint;
    public MarchingSquaresPoint[] cornerPoints; //0-LB,1-LT,2-RT,3-RB
    public Vector3[] midPoints; //0-ML,1-MT,2-MR,3-MB
    public float threshold = 0.5f;

    public MarchingSquare(Vector3 center, float scale, float threshold, float halfSize, bool squareLerp) 
    {
        centerPoint = center;
        this.threshold = threshold;
        cornerPoints = new MarchingSquaresPoint[4];
        Vector3 lbPos = center + halfSize * new Vector3(-1, 0, -1);
        Vector3 ltPos = center + halfSize * new Vector3(-1, 0, 1);
        Vector3 rtPos = center + halfSize * new Vector3(1, 0, 1);
        Vector3 rbPos = center + halfSize * new Vector3(1, 0, -1);
        cornerPoints[0] = new MarchingSquaresPoint(lbPos, 1 - Mathf.PerlinNoise(lbPos.x * scale, lbPos.z * scale));
        cornerPoints[1] = new MarchingSquaresPoint(ltPos, 1 - Mathf.PerlinNoise(ltPos.x * scale, ltPos.z * scale));
        cornerPoints[2] = new MarchingSquaresPoint(rtPos, 1 - Mathf.PerlinNoise(rtPos.x * scale, rtPos.z * scale));
        cornerPoints[3] = new MarchingSquaresPoint(rbPos, 1 - Mathf.PerlinNoise(rbPos.x * scale, rbPos.z * scale));
        midPoints = new Vector3[4];
        if (squareLerp)
        {
            //错误的平滑
            //float lbTolt = cornerPoints[0].value / (cornerPoints[0].value + cornerPoints[1].value);
            //midPoints[0] = Vector3.Lerp(ltPos, lbPos, lbTolt);
            //float ltTort = cornerPoints[1].value / (cornerPoints[1].value + cornerPoints[2].value);
            //midPoints[1] = Vector3.Lerp(rtPos, ltPos, ltTort);
            //float rtTorb = cornerPoints[2].value / (cornerPoints[2].value + cornerPoints[3].value);
            //midPoints[2] = Vector3.Lerp(rbPos, rtPos, rtTorb);
            //float rbTolb = cornerPoints[3].value / (cornerPoints[3].value + cornerPoints[0].value);
            //midPoints[3] = Vector3.Lerp(lbPos, rbPos, rbTolb);

            //正确的平滑
            midPoints[0] = InterpolateEdgePosition(threshold, cornerPoints[0].pos, cornerPoints[0].value, cornerPoints[1].pos, cornerPoints[1].value);
            midPoints[1] = InterpolateEdgePosition(threshold, cornerPoints[1].pos, cornerPoints[1].value, cornerPoints[2].pos, cornerPoints[2].value);
            midPoints[2] = InterpolateEdgePosition(threshold, cornerPoints[2].pos, cornerPoints[2].value, cornerPoints[3].pos, cornerPoints[3].value);
            midPoints[3] = InterpolateEdgePosition(threshold, cornerPoints[3].pos, cornerPoints[3].value, cornerPoints[0].pos, cornerPoints[0].value);
        }
        else 
        {
            midPoints[0] = center + halfSize * new Vector3(-1, 0, 0); 
            midPoints[1] = center + halfSize * new Vector3(0, 0, 1);
            midPoints[2] = center + halfSize * new Vector3(1, 0, 0); 
            midPoints[3] = center + halfSize * new Vector3(0, 0, -1);
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

    public void DrawLines(Vector3 center, GizmosSettings debugSettings) 
    {
        if (debugSettings.debugLine) 
        {
            int bitMask = GetBitMaskValue();
            Gizmos.color = debugSettings.lineColor;
            switch (bitMask)
            {
                case 0:
                    break;
                case 1:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[3]);
                    break;
                case 2:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[1]);
                    break;
                case 3:
                    Gizmos.DrawLine(center + midPoints[1], center + midPoints[3]);
                    break;
                case 4:
                    Gizmos.DrawLine(center + midPoints[1], center + midPoints[2]);
                    break;
                case 5:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[1]);
                    Gizmos.DrawLine(center + midPoints[2], center + midPoints[3]);
                    break;
                case 6:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[2]);
                    break;
                case 7:
                    Gizmos.DrawLine(center + midPoints[2], center + midPoints[3]);
                    break;
                case 8:
                    Gizmos.DrawLine(center + midPoints[2], center + midPoints[3]);
                    break;
                case 9:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[2]);
                    break;
                case 10:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[3]);
                    Gizmos.DrawLine(center + midPoints[1], center + midPoints[2]);
                    break;
                case 11:
                    Gizmos.DrawLine(center + midPoints[1], center + midPoints[2]);
                    break;
                case 12:
                    Gizmos.DrawLine(center + midPoints[1], center + midPoints[3]);
                    break;
                case 13:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[1]);
                    break;
                case 14:
                    Gizmos.DrawLine(center + midPoints[0], center + midPoints[3]);
                    break;
                case 15:
                    break;
            }
        }
    }

    public void DrawPoints(Vector3 center, GizmosSettings debugSettings) 
    {
        if (debugSettings.debugCenterPoint) 
        {
            Gizmos.color = debugSettings.centerPointColor;
            Gizmos.DrawSphere(centerPoint + center, debugSettings.centerPointSize);
        }
        if (debugSettings.debugCornerPoint) 
        { 
            for (int i = 0; i < cornerPoints.Length; i++)
            {
                if (cornerPoints[i].value > threshold) 
                {
                    if (debugSettings.debugCornerPointAlpha)
                    {
                        Gizmos.color = debugSettings.cornerPointColor * cornerPoints[i].value;
                    }
                    else
                    {
                        Gizmos.color = debugSettings.cornerPointColor;
                    }
                    Gizmos.DrawSphere(cornerPoints[i].pos + center, debugSettings.cornerPointSize);
                }
            }
        }
        if (debugSettings.debugMidPoint) 
        {
            Gizmos.color = debugSettings.midPointColor;
            for (int i = 0; i < midPoints.Length; i++)
            {
                Gizmos.DrawSphere(midPoints[i] + center, debugSettings.midPointSize);
            }
        }
    }

    public void BuildToMesh2D(ProceduralMeshPart meshPart) 
    {
        int bitMask = GetBitMaskValue();
        switch (bitMask)
        {
            case 0:
                break;
            case 1:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3]);
                break;
            case 2:
                meshPart.AddTriangle(cornerPoints[1].pos, midPoints[1], midPoints[0]);
                break;
            case 3:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[3]);
                break;
            case 4:
                meshPart.AddTriangle(cornerPoints[2].pos, midPoints[2], midPoints[1]);
                break;
            case 5:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3]);
                meshPart.AddQuad(midPoints[3], midPoints[0], midPoints[1], midPoints[2]);
                meshPart.AddTriangle(midPoints[1], cornerPoints[2].pos, midPoints[2]);
                break;
            case 6:
                meshPart.AddQuad(midPoints[0], cornerPoints[1].pos, cornerPoints[2].pos, midPoints[2]);
                break;
            case 7:
                meshPart.AddPentagon(cornerPoints[2].pos, midPoints[2], midPoints[3], cornerPoints[0].pos, cornerPoints[1].pos);
                break;
            case 8:
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3]);
                break;
            case 9:
                meshPart.AddQuad(cornerPoints[0].pos, midPoints[0], midPoints[2], cornerPoints[3].pos);
                break;
            case 10:
                meshPart.AddTriangle(midPoints[0], cornerPoints[1].pos, midPoints[1]);
                meshPart.AddQuad(midPoints[0], midPoints[1], midPoints[2], midPoints[3]);
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3]);
                break;
            case 11:
                meshPart.AddPentagon(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[2], cornerPoints[3].pos);
                break;
            case 12:
                meshPart.AddQuad(midPoints[3], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos);
                break;
            case 13:
                meshPart.AddPentagon(cornerPoints[0].pos, midPoints[0], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos);
                break;
            case 14:
                meshPart.AddPentagon(cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos, midPoints[3], midPoints[0]);
                break;
            case 15:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos);
                break;
        }
    }

    public void BuildToMesh3D(ProceduralMeshPart meshPart, float height)
    {
        int bitMask = GetBitMaskValue();
        switch (bitMask)
        {
            case 0:
                break;
            case 1:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3], height);
                break;
            case 2:
                meshPart.AddTriangle(cornerPoints[1].pos, midPoints[1], midPoints[0], height);
                break;
            case 3:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[3], height);
                break;
            case 4:
                meshPart.AddTriangle(cornerPoints[2].pos, midPoints[2], midPoints[1], height);
                break;
            case 5:
                meshPart.AddTriangle(cornerPoints[0].pos, midPoints[0], midPoints[3], height);
                meshPart.AddQuad(midPoints[3], midPoints[0], midPoints[1], midPoints[2], height);
                meshPart.AddTriangle(midPoints[1], cornerPoints[2].pos, midPoints[2], height);
                break;
            case 6:
                meshPart.AddQuad(midPoints[0], cornerPoints[1].pos, cornerPoints[2].pos, midPoints[2], height);
                break;
            case 7:
                meshPart.AddPentagon(cornerPoints[2].pos, midPoints[2], midPoints[3], cornerPoints[0].pos, cornerPoints[1].pos, height);
                break;
            case 8:
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3], height);
                break;
            case 9:
                meshPart.AddQuad(cornerPoints[0].pos, midPoints[0], midPoints[2], cornerPoints[3].pos, height);
                break;
            case 10:
                meshPart.AddTriangle(midPoints[0], cornerPoints[1].pos, midPoints[1], height);
                meshPart.AddQuad(midPoints[0], midPoints[1], midPoints[2], midPoints[3], height);
                meshPart.AddTriangle(midPoints[2], cornerPoints[3].pos, midPoints[3], height);
                break;
            case 11:
                meshPart.AddPentagon(cornerPoints[0].pos, cornerPoints[1].pos, midPoints[1], midPoints[2], cornerPoints[3].pos, height);
                break;
            case 12:
                meshPart.AddQuad(midPoints[3], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos, height);
                break;
            case 13:
                meshPart.AddPentagon(cornerPoints[0].pos, midPoints[0], midPoints[1], cornerPoints[2].pos, cornerPoints[3].pos, height);
                break;
            case 14:
                meshPart.AddPentagon(cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos, midPoints[3], midPoints[0], height);
                break;
            case 15:
                meshPart.AddQuad(cornerPoints[0].pos, cornerPoints[1].pos, cornerPoints[2].pos, cornerPoints[3].pos, height);
                break;
        }
    }

    private int GetBitMaskValue() 
    {
        int lbValue = cornerPoints[0].value > threshold ? 1 : 0;
        int ltValue = cornerPoints[1].value > threshold ? 2 : 0;
        int rtValue = cornerPoints[2].value > threshold ? 4 : 0;
        int rbValue = cornerPoints[3].value > threshold ? 8 : 0;
        return lbValue + ltValue + rtValue + rbValue;
    }
}

public class MarchingSquaresPoint 
{
    public Vector3 pos;
    public float value;

    public MarchingSquaresPoint(Vector3 pos, float value)
    {
        this.pos = pos;
        this.value = value;
    }
}
