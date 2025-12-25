using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes03
{
    public class MarchingCubesSphere : MarchingCubes
    {
        protected override float CalculateValue(Vector3 pos)
        {
            return Vector3.SqrMagnitude(pos - center) - minSize * minSize;
        }
    }
}