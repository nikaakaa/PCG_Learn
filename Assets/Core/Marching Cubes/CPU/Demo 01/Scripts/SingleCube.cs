using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes01
{
    public class SingleCube : ProceduralMesh
    {
        public float size = 1;

        public bool[] cornerPointValue = new bool[8] { true, true, true, true, true, true, true, true };

        public bool lerp;

        [Range(0, 1)]
        public float threshold = 0.5f;

        public GizmosSettings gizmosSettings;

        private MarchingCube marchingCube;

        protected override void Generate()
        {
            base.Generate();

            Vector3[] corners;
            corners = new Vector3[8];
            corners[0] = size * new Vector3(-0.5f, -0.5f, 0.5f);
            corners[1] = size * new Vector3(0.5f, -0.5f, 0.5f);
            corners[2] = size * new Vector3(0.5f, -0.5f, -0.5f);
            corners[3] = size * new Vector3(-0.5f, -0.5f, -0.5f);
            corners[4] = size * new Vector3(-0.5f, 0.5f, 0.5f);
            corners[5] = size * new Vector3(0.5f, 0.5f, 0.5f);
            corners[6] = size * new Vector3(0.5f, 0.5f, -0.5f);
            corners[7] = size * new Vector3(-0.5f, 0.5f, -0.5f);
            float[] values = new float[8];
            for (int i = 0; i < 8; i++)
            {
                if (lerp)
                {
                    values[i] = cornerPointValue[i] ? Random.Range(0, threshold) : Random.Range(threshold, 1);
                }
                else
                {
                    values[i] = cornerPointValue[i] ? 0 : 1;
                }

            }
            marchingCube = new MarchingCube(Vector3.zero, corners, values);

            ProceduralMeshPart main = new ProceduralMeshPart();
            marchingCube.BuildInMesh(main, threshold, lerp);
            main.FillArrays();
            mesh.vertices = main.Vertices;
            mesh.uv = main.UVs;
            mesh.triangles = main.Triangles;
            mesh.colors = main.Colors;
            mesh.RecalculateNormals();
        }

        private void OnDrawGizmos()
        {
            if (marchingCube != null)
            {
                marchingCube.DrawCube(transform.position, size * Vector3.one, gizmosSettings);
            }
        }
    }
}