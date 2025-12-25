using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MarchingCubes03
{
    public class MarchingCubes : ProceduralMesh
    {
        [Header("Marching Cubes")]
        public Vector3Int mapSize = new Vector3Int(8, 8, 8);
        public Vector3 cubeSize = new Vector3Int(1, 1, 1);
        public float threshold = 0;
        public bool lerp;
        public bool repeat; //是否要重复加入顶点
        public bool rebuildNormal; //是否要重建顶点法线

        public bool showCubesBound;

        private MarchingCubePoint[,,] pointMap;
        private List<Vector3> vertices;
        private List<int> triangles;

        protected Vector3 center;
        protected Vector3 halfSize;
        protected float minSize;

        [Button("Generate")]
        protected override void Generate()
        {
            base.Generate();

            center = new Vector3(mapSize.x * cubeSize.x, mapSize.y * cubeSize.y, mapSize.z * cubeSize.z) * 0.5f;
            halfSize = center;
            minSize = Mathf.Min(halfSize.x, halfSize.y, halfSize.z);

            pointMap = new MarchingCubePoint[mapSize.x + 1, mapSize.y + 1, mapSize.z + 1];
            vertices = new List<Vector3>();
            triangles = new List<int>();

            for (int x = 0; x <= mapSize.x; x++)
            {
                for (int z = 0; z <= mapSize.z; z++)
                {
                    for (int y = 0; y <= mapSize.y; y++)
                    {
                        Vector3 pos = new Vector3(x * cubeSize.x, y * cubeSize.y, z * cubeSize.z);
                        float value = CalculateValue(pos);
                        pointMap[x, y, z] = new MarchingCubePoint(pos, value);
                    }
                }
            }

            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    for (int z = 0; z < mapSize.z; z++)
                    {
                        BuildMesh(new MarchingCubePoint[8]
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

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            if (rebuildNormal)
            {
                mesh.SetNormals(RebuildNormals());
            }
            else 
            {
                mesh.RecalculateNormals();
            }
            mesh.RecalculateTangents();
        }

        protected virtual float CalculateValue(Vector3 pos) 
        {
            return 0;
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
            if (!repeat) 
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (vertices[i] == ver)
                    {
                        return i;
                    }
                }
            }

            vertices.Add(ver);
            return vertices.Count - 1;
        }

        private Vector3[] RebuildNormals()
        {
            Dictionary<VertexKey, Vector3> vertexNormals = new Dictionary<VertexKey, Vector3>();
            for (int i = 0; i < triangles.Count; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                Vector3 e1 = vertices[b] - vertices[a];
                Vector3 e2 = vertices[c] - vertices[a];
                Vector3 triNormal = Vector3.Cross(e1, e2).normalized;
                float triArea = MathUtility.CalculateTriangleArea(vertices[a], vertices[b], vertices[c]);

                VertexKey aKey = new VertexKey(vertices[a]);
                if (!vertexNormals.ContainsKey(aKey))
                {
                    vertexNormals.Add(aKey, Vector3.zero);
                }
                vertexNormals[aKey] += triNormal * triArea;

                VertexKey bKey = new VertexKey(vertices[b]);
                if (!vertexNormals.ContainsKey(bKey))
                {
                    vertexNormals.Add(bKey, Vector3.zero);
                }
                vertexNormals[bKey] += triNormal * triArea;

                VertexKey cKey = new VertexKey(vertices[c]);
                if (!vertexNormals.ContainsKey(cKey))
                {
                    vertexNormals.Add(cKey, Vector3.zero);
                }
                vertexNormals[cKey] += triNormal * triArea;
            }

            Vector3[] verticeNormals = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++) 
            {
                VertexKey key = new VertexKey(vertices[i]);
                verticeNormals[i] = vertexNormals[key].normalized;
            }

            return verticeNormals;
        }

        private struct VertexKey
        {
            private readonly long _x;
            private readonly long _y;
            private readonly long _z;

            //Change this if you require a different precision.
            private const int Tolerance = 100000;

            public VertexKey(Vector3 position)
            {
                _x = (long)(Mathf.Round(position.x * Tolerance));
                _y = (long)(Mathf.Round(position.y * Tolerance));
                _z = (long)(Mathf.Round(position.z * Tolerance));
            }

            public override bool Equals(object obj)
            {
                var key = (VertexKey)obj;
                return _x == key._x && _y == key._y && _z == key._z;
            }

            public override int GetHashCode()
            {
                return (_x * 7 ^ _y * 13 ^ _z * 27).GetHashCode();
            }
        }

        private void OnDrawGizmos()
        {
            if (showCubesBound) 
                Gizmos.DrawWireCube(transform.position + center, halfSize * 2);
        }
    }
}

