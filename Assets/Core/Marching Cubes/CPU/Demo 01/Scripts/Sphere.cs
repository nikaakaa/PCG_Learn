using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes01
{
    public class Sphere : ProceduralMesh
    {
        [Header("Marching Cubes")]
        public float radius = 10;

        public bool lerp;

        protected override void Generate()
        {
            base.Generate();

            int mapSize = Mathf.CeilToInt(radius * 2 + 1);
            float radius2 = radius * radius;
            ProceduralMeshPart main = new ProceduralMeshPart();

            for (int x = 0; x < mapSize; x++)
            {
                for (int y = 0; y < mapSize; y++)
                {
                    for (int z = 0; z < mapSize; z++)
                    {
                        CreateMarchingCube(new Vector3(x, y, z)).BuildInMesh(main, radius2, lerp);
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

        protected MarchingCube CreateMarchingCube(Vector3 centerPoint)
        {
            Vector3[] corner;
            float[] values;

            corner = new Vector3[8];
            corner[0] = centerPoint + new Vector3(-0.5f, -0.5f, 0.5f);
            corner[1] = centerPoint + new Vector3(0.5f, -0.5f, 0.5f);
            corner[2] = centerPoint + new Vector3(0.5f, -0.5f, -0.5f);
            corner[3] = centerPoint + new Vector3(-0.5f, -0.5f, -0.5f);
            corner[4] = centerPoint + new Vector3(-0.5f, 0.5f, 0.5f);
            corner[5] = centerPoint + new Vector3(0.5f, 0.5f, 0.5f);
            corner[6] = centerPoint + new Vector3(0.5f, 0.5f, -0.5f);
            corner[7] = centerPoint + new Vector3(-0.5f, 0.5f, -0.5f);

            values = new float[8];
            for (int i = 0; i < 8; i++)
            {
                values[i] = Vector3.SqrMagnitude(corner[i] - radius * Vector3.one);
            }

            return new MarchingCube(centerPoint, corner, values);
        }
    }
}
