using UnityEngine;

public static class MathUtility
{
    public static float CalculateTriangleArea(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).magnitude * 0.5f;
    }
}
