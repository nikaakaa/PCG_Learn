using Sirenix.OdinInspector;
using UnityEngine;

public class MarchingCubesGen : MonoBehaviour
{
    [SerializeReference]
    public MarchingCubeMapBase marchingCubeMapBase;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    public bool autoRefresh = false;

    void Start()
    {

    }
    public void OnValidate()
    {
        if (!autoRefresh) return;
        Generate();
    }
    [Button("Generate")]
    public void Generate()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        marchingCubeMapBase.Generate();
        Mesh mesh = new Mesh();
        mesh.vertices = marchingCubeMapBase.vertices.ToArray();
        mesh.triangles = marchingCubeMapBase.triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}