using UnityEngine;

/// <summary>
/// 程序化网格生成基类
/// 提供网格生成的基础框架，子类需重写 Generate 方法实现具体逻辑
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{
    [SerializeField] protected MeshFilter meshFilter;
    [SerializeField] protected MeshRenderer meshRenderer;
    
    protected Mesh mesh;

    /// <summary>
    /// 自动刷新开关：Inspector 参数变化时是否自动重新生成网格
    /// </summary>
    [Header("自动刷新")]
    [SerializeField] protected bool autoRefresh = true;

    protected virtual void Awake()
    {
        InitializeMesh();
    }

    protected virtual void OnValidate()
    {
        if (autoRefresh)
        {
            InitializeMesh();
            Generate();
        }
    }

    /// <summary>
    /// 初始化网格对象
    /// </summary>
    protected void InitializeMesh()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = $"{gameObject.name}_ProceduralMesh" };
        }
        else
        {
            mesh.Clear();
        }
        meshFilter.sharedMesh = mesh;
    }

    /// <summary>
    /// 生成网格的核心方法，子类需重写此方法
    /// </summary>
    protected virtual void Generate()
    {
        // 子类实现具体生成逻辑
    }

    /// <summary>
    /// 手动触发刷新
    /// </summary>
    [ContextMenu("刷新网格")]
    public void Refresh()
    {
        InitializeMesh();
        Generate();
    }

    protected virtual void OnDestroy()
    {
        if (mesh != null && !Application.isPlaying)
        {
            // 编辑器模式下清理 mesh
            DestroyImmediate(mesh);
        }
    }
}
