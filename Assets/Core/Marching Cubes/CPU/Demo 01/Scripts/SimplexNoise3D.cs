using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes01
{
    public class SimplexNoise3D : ProceduralMesh
    {
        [Header("Marching Cubes")]
        public Vector3Int mapSize = new Vector3Int(8, 8, 8);
        public Vector3 cubeSize = new Vector3Int(1, 1, 1);
        public float noiseScale = 10;
        [Range(-1, 1)] public float threshold = 0;
        public bool lerp = true;

        protected override void Generate()
        {
            base.Generate();

            ProceduralMeshPart main = new ProceduralMeshPart();
            Vector3 halfSize = cubeSize * 0.5f;

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    for (int z = 0; z < mapSize.z; z++)
                    {
                        MarchingCube marchingCube = CreateMarchingCube(new Vector3(x * cubeSize.x, y * cubeSize.y, z * cubeSize.z), halfSize);
                        marchingCube.BuildInMesh(main, threshold, lerp);
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
                values[i] = SimplexNoise.Generate3D(corner[i].x * noiseScale, corner[i].y * noiseScale, corner[i].z *  noiseScale);
            }

            return new MarchingCube(centerPoint, corner, values);
        }
    }
}