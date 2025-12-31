using Sirenix.OdinInspector;
using UnityEngine;

public class MarchingCubesGen_GPU : MonoBehaviour
{
    private Mesh mesh;
    const int threadGroupSize = 8;
    public ComputeShader CSShader;
    public int numPointsPerAxis = 64;
    public float cubeSize = 1;
    public float isoLevel = 0;
    [Tooltip("是否使用插值计算顶点位置（勾选=精确插值，不勾选=边中点）")]
    public bool isLerp = true;
    [SerializeReference]
    public MyValueGen value;

    private ComputeBuffer pointsBuffer;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triangleCountBuffer;
    private void InitializeMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;

        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }
    public void OnValidate()
    {
        // 防止编辑器启动时 OnValidate 调用导致的空引用
        if (mesh == null)
        {
            InitializeMesh();
        }
        Generate();
    }

    [Button("Generate")]
    public void Generate()
    {
        // 确保 mesh 已初始化
        if (mesh == null)
        {
            InitializeMesh();
        }
        CreateBuffers();
        CreateMesh();
        if (!Application.isPlaying)
        {
            ReleaseBuffers();
        }
    }
    private void CreateBuffers()
    {
        int numsPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numsVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numsVoxels * 5;

        if (!Application.isPlaying || pointsBuffer == null || numsPoints != pointsBuffer.count)
        {
            if (Application.isPlaying)
            {
                ReleaseBuffers();
            }
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer(numsPoints, sizeof(float) * 4);
            triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }

    }
    private void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            triangleBuffer = null;
        }
        if (pointsBuffer != null)
        {
            pointsBuffer.Release();
            pointsBuffer = null;
        }
        if (triangleCountBuffer != null)
        {
            triangleCountBuffer.Release();
            triangleCountBuffer = null;
        }
    }
    /// <summary>
    /// 确保对象禁用或销毁时释放 GPU 资源
    /// </summary>
    private void OnDisable()
    {
        ReleaseBuffers();
    }
    struct Triangle
    {
#pragma warning disable 649 // disable unassigned variable warning
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }
    private void CreateMesh()
    {
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)threadGroupSize);
        // Calculate value
        value.Generate(pointsBuffer, numPointsPerAxis, cubeSize, transform.position);
        // Marching Cubes 
        triangleBuffer.SetCounterValue(0);
        CSShader.SetBuffer(0, "points", pointsBuffer);
        CSShader.SetBuffer(0, "triangles", triangleBuffer);
        CSShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        CSShader.SetFloat("isoLevel", isoLevel);
        CSShader.SetInt("isLerp", isLerp ? 1 : 0);
        CSShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
        // Get number of triangles in the triangle buffer
        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = { 0 };
        triangleCountBuffer.GetData(triangleCountArray);
        int numTris = triangleCountArray[0];
        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        triangleBuffer.GetData(tris, 0, 0, numTris);
        // Construct Mesh
        var vertices = new Vector3[numTris * 3];
        var triangles = new int[numTris * 3];
        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                triangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    public void ValueUpdate()
    {
        // 防止编辑器启动时 OnValidate 调用导致的空引用
        if (mesh == null)
        {
            InitializeMesh();
        }
        Generate();
    }
}