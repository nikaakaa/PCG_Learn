using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes03
{
    public class MarchingCubesLayerPerlin : MarchingCubes
    {
        [Header("Layer Perlin Noise")]
        public Vector2 heightRange = new Vector2(-1, 1);
        [Min(0)] public float cellSize = 1;
        public Vector2 offset;
        [Min(0)] public int octaves = 4;
        [Min(0)] public float roughness = 3;
        [Min(0)] public float persistance = 0.4f;

        protected override float CalculateValue(Vector3 pos)
        {
            return pos.y - GetHeight(pos.x, pos.z);
        }

        private float GetHeight(float x, float z)
        {
            return Mathf.InverseLerp(heightRange.x, heightRange.y, LayerPerlinHeight(new Vector2(x, z) * cellSize + offset)) * mapSize.y * cubeSize.y;
        }

        private float LayerPerlinHeight(Vector2 value)
        {
            float noise = 0;
            float frequency = 1;
            float factor = 1;

            for (int i = 0; i < octaves; i++)
            {
                Vector2 layerValue = value * frequency;
                noise = noise + (Mathf.PerlinNoise(layerValue.x, layerValue.y) * 2 - 1) * factor;
                factor *= persistance;
                frequency *= roughness;
            }

            return noise;
        }
    }
}