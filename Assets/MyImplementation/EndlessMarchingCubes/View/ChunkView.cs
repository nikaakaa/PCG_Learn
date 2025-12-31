using UnityEngine;

public class ChunkView : MonoBehaviour
{
    [Header("组件")]
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    [Header("运行时")]
    public ChunkMarchingCubesMap chunkMCMap;
    public bool isGenerated = false;

    /// <summary>
    /// 初始化 ChunkView，由 ChunkMgr 调用
    /// </summary>
    public void Initialize(ComputeShader mcShader, ComputeShader vgShader, float scale)
    {
        chunkMCMap = new ChunkMarchingCubesMap
        {
            marchingCubesShader = mcShader,
            valueGenerator = new SimplexNoiseGen
            {
                ValueGenShader = vgShader,
                scale = scale
            }
        };
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void UpdateMesh(float isoLevel, int numPointsPerAxis, float cubeSize, Vector3 center)
    {
        if (isGenerated) return;
        chunkMCMap.Generate(isoLevel, numPointsPerAxis, cubeSize, center);

        // 赋值到 Mesh（加这些）
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = chunkMCMap.vertices.ToArray();
        mesh.triangles = chunkMCMap.triangleIndices.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        isGenerated = true;

    }
}


