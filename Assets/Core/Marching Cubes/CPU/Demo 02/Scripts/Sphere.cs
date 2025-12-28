using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes02
{
    public class Sphere : ProceduralMesh
    {
        [Header("Marching Cubes")]
        public float radius = 10;

        public bool lerp;

        protected override void Generate()
        {
            base.Generate();

            Vector3Int mapSize = Mathf.CeilToInt(radius * 2 + 1) * Vector3Int.one;

            // init point
            Vector3[,,] corners = new Vector3[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
            float[,,] values = new float[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
            for (int x = 0; x <= mapSize.x; x++)
            {
                for (int z = 0; z <= mapSize.z; z++)
                {
                    for (int y = 0; y <= mapSize.y; y++)
                    {
                        Vector3 corner = new Vector3(x, y, z);
                        corners[x, y, z] = corner;
                        float value = Vector3.SqrMagnitude(corner - radius * Vector3.one);
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
                        cube.BuildInMesh(main, radius * radius, lerp);
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
    }
}