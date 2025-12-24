using UnityEngine;

/// <summary>
/// Gizmos 调试显示配置
/// 用于控制 Marching Squares 的调试可视化
/// </summary>
[System.Serializable]
public class GizmosSettings
{
    [Header("等值线调试")]
    public bool debugLine = true;
    public Color lineColor = Color.green;

    [Header("中心点调试")]
    public bool debugCenterPoint = false;
    public Color centerPointColor = Color.yellow;
    public float centerPointSize = 0.1f;

    [Header("角点调试")]
    public bool debugCornerPoint = true;
    public Color cornerPointColor = Color.red;
    public float cornerPointSize = 0.1f;
    public bool debugCornerPointAlpha = false;

    [Header("中点调试")]
    public bool debugMidPoint = false;
    public Color midPointColor = Color.blue;
    public float midPointSize = 0.05f;
}
