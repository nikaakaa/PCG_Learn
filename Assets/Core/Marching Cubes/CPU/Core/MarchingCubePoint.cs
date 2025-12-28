using UnityEngine;

/// <summary>
/// Marching Cube 的单个角点数据
/// </summary>
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

/// <summary>
/// Marching Cube 的8个角点集合
/// </summary>
public class MarchingCubePoints
{
    public MarchingCubePoint[] points;

    public MarchingCubePoints(MarchingCubePoint[] points)
    {
        this.points = points;
    }

    /// <summary>
    /// 设置所有角点的值
    /// </summary>
    public void SetValue(float value)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].value = value;
        }
    }
}
