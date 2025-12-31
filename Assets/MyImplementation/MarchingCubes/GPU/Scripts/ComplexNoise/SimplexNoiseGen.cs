using System;
using UnityEngine;

/// <summary>
/// Simplex Noise 地形密度场生成器
/// 使用分形噪声生成起伏地形
/// </summary>
[Serializable]
public class SimplexNoiseGen : MyValueGen
{
    // === 噪声参数 ===
    /// <summary>噪声缩放系数，值越小地形越平缓</summary>
    public float noiseScale = 0.05f;

    /// <summary>噪声强度，控制地形起伏幅度</summary>
    public float noiseStrength = 10f;

    /// <summary>地板基准高度（Y轴）</summary>
    public float floorHeight = 0f;

    // === 分形参数 ===
    /// <summary>分形层数，越多细节越丰富</summary>
    [Range(1, 8)]
    public int octaves = 4;

    /// <summary>每层振幅衰减比例</summary>
    [Range(0f, 1f)]
    public float persistence = 0.5f;

    /// <summary>每层频率增长比例</summary>
    [Range(1f, 4f)]
    public float lacunarity = 2f;

    public override ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float cubeSize, Vector3 worldCenter)
    {
        // 设置噪声参数
        valueShader.SetFloat("noiseScale", noiseScale);
        valueShader.SetFloat("noiseStrength", noiseStrength);
        valueShader.SetFloat("floorHeight", floorHeight);

        // 设置分形参数
        valueShader.SetInt("octaves", octaves);
        valueShader.SetFloat("persistence", persistence);
        valueShader.SetFloat("lacunarity", lacunarity);

        return base.Generate(pointsBuffer, numPointsPerAxis, cubeSize, worldCenter);
    }
}
