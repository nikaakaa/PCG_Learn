using UnityEngine;

/// <summary>
/// 球体密度场生成器
/// 继承自 MyValueGen，负责设置球体半径参数
/// </summary>
public class SphereValueGen : MyValueGen
{
    [Header("球体参数")]
    [Tooltip("球体半径，默认会根据网格尺寸自动计算")]
    public float radius = -1f; // -1 表示自动计算

    public override ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float cubeSize, Vector3 worldCenter)
    {
        // 如果 radius <= 0，则自动计算为网格范围的一半
        float actualRadius = radius > 0 ? radius : numPointsPerAxis * cubeSize * 0.5f;
        
        valueShader.SetFloat("radius", actualRadius);

        return base.Generate(pointsBuffer, numPointsPerAxis, cubeSize, worldCenter);
    }
}
