using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes05
{
    public class MeshToMarchingCubes : ProceduralMesh
    {
        [Header("Marching Cubes")]
        public Mesh objectMesh;
        public int maxCount = 100;
        public Vector3Int sliceCount = new Vector3Int(8, 8, 8);
        public float threshold = 0.99f;
        public bool lerp;

        private Vector3 cubeSize;
        private Vector3Int cubeCount;
        private MarchingCubePoints[,,] cubes;
        private Vector3Int pointCount;
        private MarchingCubePoint[,,] pointMap;
        private Dictionary<Vector3Int, int> coords;

        private List<Vector3> vertices;
        private List<int> triangles;

        protected override void Generate()
        {
            base.Generate();

            if (objectMesh == null) 
            {
                return;
            }      
            cubeSize = new Vector3(objectMesh.bounds.size.x / sliceCount.x, objectMesh.bounds.size.y / sliceCount.y, objectMesh.bounds.size.z / sliceCount.z);
            cubeCount = sliceCount + Vector3Int.one * 3;
            pointCount = cubeCount + Vector3Int.one;
            pointMap = new MarchingCubePoint[pointCount.x, pointCount.y, pointCount.z];
            cubes = new MarchingCubePoints[cubeCount.x, cubeCount.y, cubeCount.z];

            vertices = new List<Vector3>();
            triangles = new List<int>();

            //points
            for (int x = 0; x < pointCount.x; x++)
            {
                for (int y = 0; y < pointCount.y; y++) 
                {
                    for (int z = 0; z < pointCount.z; z++)
                    {
                        pointMap[x, y, z] = new MarchingCubePoint(new Vector3(x * cubeSize.x, y * cubeSize.y, z * cubeSize.z), 1);
                    }
                }
            }

            //cubes
            for (int x = 0; x < cubeCount.x; x++)
            {
                for (int y = 0; y < cubeCount.y; y++)
                {
                    for (int z = 0; z < cubeCount.z; z++)
                    {
                        cubes[x, y, z] = new MarchingCubePoints(new MarchingCubePoint[8] 
                        {
                            pointMap[x, y, z + 1],
                            pointMap[x + 1, y, z + 1],
                            pointMap[x + 1, y, z],
                            pointMap[x, y, z],
                            pointMap[x, y + 1, z + 1],
                            pointMap[x + 1, y + 1, z + 1],
                            pointMap[x + 1, y + 1, z],
                            pointMap[x, y + 1, z],
                        });
                    }
                }
            }

            //object mesh coord
            coords = new Dictionary<Vector3Int, int>();
            Vector3Int minCoord = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            for (int i = 0; i < objectMesh.vertices.Length; i++) 
            {
                Vector3Int coord = GetCoordFromPos(objectMesh.vertices[i]);
                if (!coords.ContainsKey(coord))
                {
                    coords.Add(coord, 1);
                }
                else 
                {
                    coords[coord] += 1;
                }

                minCoord = Min(minCoord, coord);
            }
            foreach(var keyValue in coords)
            {
                Vector3Int coord = keyValue.Key - minCoord + Vector3Int.one;
                cubes[coord.x, coord.y, coord.z].SetValue(Mathf.Min(0.98f, (float)keyValue.Value / maxCount));
            }

            //mesh
            for (int x = 0; x < cubeCount.x; x++)
            {
                for (int y = 0; y < cubeCount.y; y++)
                {
                    for (int z = 0; z < cubeCount.z; z++)
                    {
                        BuildMesh(cubes[x, y, z].points);
                    }
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents(); 
        }

        private Vector3Int GetCoordFromPos(Vector3 pos) 
        {
            return new Vector3Int(Mathf.RoundToInt(pos.x / cubeSize.x), Mathf.RoundToInt(pos.y / cubeSize.y), Mathf.RoundToInt(pos.z / cubeSize.z));
        }

        private Vector3Int Min(Vector3Int a, Vector3Int b) 
        {
            return new Vector3Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
        }

        private void BuildMesh(MarchingCubePoint[] points)
        {
            int cubeIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (points[i].value < threshold)
                {
                    cubeIndex |= 1 << i;
                }
            }

            // Create triangles for current cube configuration
            for (int i = 0; MarchingCubesTable.triangulation[cubeIndex][i] != -1; i += 3)
            {
                // Get indices of corner points A and B for each of the three edges of the cube that need to be joined to form the triangle.
                int a0 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[cubeIndex][i]];
                int b0 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[cubeIndex][i]];
                int a1 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 1]];
                int b1 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 1]];
                int a2 = MarchingCubesTable.cornerIndexAFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 2]];
                int b2 = MarchingCubesTable.cornerIndexBFromEdge[MarchingCubesTable.triangulation[cubeIndex][i + 2]];

                Vector3 vertexA = lerp ? InterpolateEdgePosition(threshold, points[a0].pos, points[a0].value, points[b0].pos, points[b0].value) : (points[a0].pos + points[b0].pos) * 0.5f;
                Vector3 vertexB = lerp ? InterpolateEdgePosition(threshold, points[a1].pos, points[a1].value, points[b1].pos, points[b1].value) : (points[a1].pos + points[b1].pos) * 0.5f;
                Vector3 vertexC = lerp ? InterpolateEdgePosition(threshold, points[a2].pos, points[a2].value, points[b2].pos, points[b2].value) : (points[a2].pos + points[b2].pos) * 0.5f;

                int triA = AddVertice(vertexA);
                triangles.Add(triA);
                int triC = AddVertice(vertexC);
                triangles.Add(triC);
                int triB = AddVertice(vertexB);
                triangles.Add(triB);
            }
        }

        private Vector3 InterpolateEdgePosition(float threshold, Vector3 vertex1, float value1, Vector3 vertex2, float value2)
        {
            Vector3 pointOnEdge = Vector3.zero;

            if (Mathf.Approximately(threshold - value1, 0) == true) return vertex1;
            if (Mathf.Approximately(threshold - value2, 0) == true) return vertex2;
            if (Mathf.Approximately(value1 - value2, 0) == true) return vertex1;

            float mu = (threshold - value1) / (value2 - value1);
            pointOnEdge.x = vertex1.x + mu * (vertex2.x - vertex1.x);
            pointOnEdge.y = vertex1.y + mu * (vertex2.y - vertex1.y);
            pointOnEdge.z = vertex1.z + mu * (vertex2.z - vertex1.z);

            return pointOnEdge;
        }

        private int AddVertice(Vector3 ver)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i] == ver)
                {
                    return i;
                }
            }

            vertices.Add(ver);
            return vertices.Count - 1;
        }
    }
}