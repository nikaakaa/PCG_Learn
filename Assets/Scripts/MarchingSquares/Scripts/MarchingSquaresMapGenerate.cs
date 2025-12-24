using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingSquaresMapGenerate : MonoBehaviour
{
    [Header("地图配置")]
    public MarchingSquaresMap marchingSquaresMap = new MarchingSquaresMap();

    [Header("自动刷新")]
    [SerializeField] private bool autoRefresh = true;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        Generate();
    }

    private void OnValidate()
    {
        if (autoRefresh)
        {
            // 延迟调用，避免在 OnValidate 中直接修改序列化数据
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    Generate();
                }
            };
#endif
        }
    }

    [Button("生成地图")]
    public void Generate()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();


        // 生成地图数据（使用本地坐标，中心点为原点）
        marchingSquaresMap.GenerateMap(Vector3.zero);

        // 生成 Mesh
        if (mesh == null)
        {
            mesh = new Mesh { name = "MarchingSquares_Mesh" };
        }
        else
        {
            mesh.Clear();
        }

        mesh.vertices = marchingSquaresMap.vertices.ToArray();
        mesh.triangles = marchingSquaresMap.triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        if (marchingSquaresMap.points == null || marchingSquaresMap.points.Count == 0)
            return;

        // 绘制采样点
        foreach (var point in marchingSquaresMap.points)
        {
            Gizmos.color = point.value > marchingSquaresMap.threshold ? Color.white : Color.black;
            Gizmos.DrawSphere(point.position, 0.1f);
        }

        // 绘制等值线边界
        Gizmos.color = Color.green;
        foreach (var square in marchingSquaresMap.squares)
        {
            if (square.edgeVertices.Count >= 2)
            {
                for (int i = 0; i < square.edgeVertices.Count; i++)
                {
                    Vector3 start = square.edgeVertices[i];
                    Vector3 end = square.edgeVertices[(i + 1) % square.edgeVertices.Count];
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (mesh != null && !Application.isPlaying)
        {
            DestroyImmediate(mesh);
        }
    }
}