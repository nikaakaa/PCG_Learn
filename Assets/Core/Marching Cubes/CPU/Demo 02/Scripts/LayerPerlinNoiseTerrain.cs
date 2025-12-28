using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes02
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

            // init point
            Vector3[,,] corners = new Vector3[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
            float[,,] values = new float[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
            for (int x = 0; x <= mapSize.x; x++)
            {
                for (int z = 0; z <= mapSize.z; z++)
                {
                    for (int y = 0; y <= mapSize.y; y++)
                    {
                        Vector3 corner = new Vector3(x * cubeSize.x, y * cubeSize.y, z * cubeSize.z);
                        corners[x, y, z] = corner;
                        float value = corner.y - GetHeight01(corner.x, corner.z, layerPerlinHeightRange) * mapSize.y * cubeSize.y;
                        values[x, y, z] = value;
                    }
                }
            }

            // build cube
            ProceduralMeshPart main = new ProceduralMeshPart();
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int z = 0; z < mapSize.z; z++)
                {
                    for (int y = 0; y < mapSize.y; y++)
                    {
                        Vector3[] cubeCorners = new Vector3[8];
                        cubeCorners[0] = corners[x, y, z + 1];        
                        cubeCorners[1] = corners[x + 1, y, z + 1];    
                        cubeCorners[2] = corners[x + 1, y, z];        
                        cubeCorners[3] = corners[x, y, z];            
                        cubeCorners[4] = corners[x, y + 1, z + 1];    
                        cubeCorners[5] = corners[x + 1, y + 1, z + 1];
                        cubeCorners[6] = corners[x + 1, y + 1, z];    
                        cubeCorners[7] = corners[x, y + 1, z];

                        float[] cubeValues = new float[8];
                        cubeValues[0] = values[x, y, z + 1];
                        cubeValues[1] = values[x + 1, y, z + 1];
                        cubeValues[2] = values[x + 1, y, z];
                        cubeValues[3] = values[x, y, z];
                        cubeValues[4] = values[x, y + 1, z + 1];
                        cubeValues[5] = values[x + 1, y + 1, z + 1];
                        cubeValues[6] = values[x + 1, y + 1, z];
                        cubeValues[7] = values[x, y + 1, z];

                        MarchingCube cube = new MarchingCube(cubeCorners, cubeValues);
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
