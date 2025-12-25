using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes03
{
    public class MarchingCubesSimplexNoise : MarchingCubes
    {
        public float noiseSclae = 1;

        protected override float CalculateValue(Vector3 pos)
        {
            pos = pos * noiseSclae;
            return SimplexNoise.Generate3D(pos.x, pos.y, pos.z);
        }
    }
}