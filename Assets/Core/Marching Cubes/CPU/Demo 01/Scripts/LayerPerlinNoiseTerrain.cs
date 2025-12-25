using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes01
{
    public class LayerPerlinNoiseTerrain : ProceduralMesh
    {
        [Header("Marching Cubes")]
        public Vector3Int mapSize = new Vector3Int(8, 8, 8);
        public Vector3 cubeSize = new Vector3Int(1, 1, 1);
        public bool lerp;

        [Header("Layer Perlin Noise")]
        public Vector2 layerPerlinHeightRange = new Vector2(-1, 1);
        [Min(0)] public float cellSize = 1;
        [Min(0)] public int octaves = 4;
        [Min(0)] public float roughness = 3;
        [Min(0)] public float persistance = 0.4f;

        protected override void Generate()
        {
            base.Generate();

            ProceduralMeshPart main = new ProceduralMeshPart();
            Vector3 halfSize = cubeSize * 0.5f;

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    for (int y = 0; y < mapSize.y; y++)
                    {
                        MarchingCube cube = CreateMarchingCube(new Vector3(x * cubeSize.x, y * cubeSize.y, z * cubeSize.z), halfSize);
                        cube.BuildInMesh(main, 0, lerp);
                    }
                }
            }

            main.FillArrays();
            mesh.vertices = main.Vertices;
            mesh.uv = main.UVs;
            mesh.triangles = main.Triangles;
            mesh.colors = main.Colors;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        protected MarchingCube CreateMarchingCube(Vector3 centerPoint, Vector3 halfSize)
        {
            Vector3[] corner;
            float[] values;

            corner = new Vector3[8];
            corner[0] = centerPoint + new Vector3(-1 * halfSize.x, -1 * halfSize.y, 1 * halfSize.z);
            corner[1] = centerPoint + new Vector3(1 * halfSize.x, -1 * halfSize.y, 1 * halfSize.z);
            corner[2] = centerPoint + new Vector3(1 * halfSize.x, -1 * halfSize.y, -1 * halfSize.z);
            corner[3] = centerPoint + new Vector3(-1 * halfSize.x, -1 * halfSize.y, -1 * halfSize.z);
            corner[4] = centerPoint + new Vector3(-1 * halfSize.x, 1 * halfSize.y, 1 * halfSize.z);
            corner[5] = centerPoint + new Vector3(1 * halfSize.x, 1 * halfSize.y, 1 * halfSize.z);
            corner[6] = centerPoint + new Vector3(1 * halfSize.x, 1 * halfSize.y, -1 * halfSize.z);
            corner[7] = centerPoint + new Vector3(-1 * halfSize.x, 1 * halfSize.y, -1 * halfSize.z);

            values = new float[8];
            for (int i = 0; i < 8; i++)
            {
                float height = GetHeight01(corner[i].x, corner[i].z, layerPerlinHeightRange) * mapSize.y * cubeSize.y;
                values[i] = corner[i].y - height;
            }
            return new MarchingCube(centerPoint, corner, values);
        }

        private float GetHeight01(float x, float z, Vector2 heightRange)
        {
            return Mathf.InverseLerp(heightRange.x, heightRange.y, LayerPerlinHeight(new Vector2(x, z) * cellSize));
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
