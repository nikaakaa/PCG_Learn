using System.Collections.Generic;
using UnityEngine;

public class ChunkMgrVC : MonoBehaviour
{
    [Header("配置")]
    [SerializeReference] public ChunkData chunkData = new ChunkData();
    public int viewDistance = 3;  // 视距（以 Chunk 为单位）
    [Header("依赖")]
    public Transform playerTrans;
    public GameObject chunkViewPrefab;

    [Header("Compute Shader")]
    public ComputeShader marchingCubesShader;
    public ComputeShader valueGenShader;
    public float noiseScale = 0.05f;

    [Header("运行时")]
    public Dictionary<Vector3Int, ChunkView> previousChunkViews = new();
    public Dictionary<Vector3Int, ChunkView> currentChunkViews = new();

    private Vector3Int lastPlayerCoor = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

    private void Awake()
    {
    }

    public void Update()
    {
        UpdateChunks();
    }

    public void PushChunkView(Vector3Int coor)
    {
        if (currentChunkViews.TryGetValue(coor, out var existChunkView))
        {
            existChunkView.Hide();
            previousChunkViews.Add(coor, existChunkView);
            currentChunkViews.Remove(coor);
            return;
        }
    }
    public ChunkView GetChunkView(Vector3Int coor)
    {
        if (currentChunkViews.TryGetValue(coor, out var chunkView))
        {
            return chunkView;
        }
        if (previousChunkViews.TryGetValue(coor, out chunkView))
        {
            currentChunkViews.Add(coor, chunkView);
            previousChunkViews.Remove(coor);
            chunkView.Show();
            return chunkView;
        }
        // 创建新的 Chunk
        var go = Instantiate(chunkViewPrefab, Vector3.zero, Quaternion.identity);
        go.name = $"Chunk_{coor.x}_{coor.y}_{coor.z}";
        chunkView = go.GetComponent<ChunkView>();
        chunkView.Initialize(marchingCubesShader, valueGenShader, noiseScale);  // 初始化
        currentChunkViews.Add(coor, chunkView);
        return chunkView;
    }
    public void UpdateChunks()
    {
        // Update Frustum Planes
        if (Camera.main != null) planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // Culling (Runs every frame)
        foreach (var kv in currentChunkViews)
        {
            bool visible = CanShowChunk(kv.Key);
            if (kv.Value.gameObject.activeSelf != visible)
                kv.Value.gameObject.SetActive(visible);
        }

        if (playerTrans == null || chunkData == null || chunkViewPrefab == null) return;
        Vector3Int playerCoor = WorldToCoor(playerTrans.position);

        // Unload logic (Only when player moves)
        if (playerCoor != lastPlayerCoor)
        {
            lastPlayerCoor = playerCoor;
            List<Vector3Int> toUnload = new List<Vector3Int>();
            foreach (var kv in currentChunkViews)
            {
                Vector3Int coor = kv.Key;
                if (Mathf.Abs(coor.x - playerCoor.x) > viewDistance ||
                    Mathf.Abs(coor.y - playerCoor.y) > viewDistance ||
                    Mathf.Abs(coor.z - playerCoor.z) > viewDistance)
                {
                    toUnload.Add(coor);
                }
            }
            foreach (var coor in toUnload) PushChunkView(coor);
        }

        // Load logic (Runs every frame to support Frustum Culling updates)
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    Vector3Int coor = playerCoor + new Vector3Int(x, y, z);
                    if (currentChunkViews.ContainsKey(coor)) continue;
                    if (!CanShowChunk(coor)) continue;

                    ChunkView chunkView = GetChunkView(coor);
                    Vector3 center = CoorToWorld(coor);
                    chunkView.UpdateMesh(chunkData.isoLevel, chunkData.numPointsPerAxis, chunkData.Spacing, center);
                }
            }
        }
    }
    private Plane[] planes;
    public bool CanShowChunk(Vector3Int coor)
    {
        if (planes == null) return true;
        Vector3 min = CoorToWorld(coor);
        Vector3 size = Vector3.one * chunkData.chunkSize;
        Vector3 center = min + size * 0.5f;
        Bounds bounds = new Bounds(center, size);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
    public void Release()
    {
        foreach (var kv in currentChunkViews)
        {
            kv.Value.chunkMCMap?.Release();
            if (Application.isPlaying)
                Destroy(kv.Value.gameObject);
            else
                DestroyImmediate(kv.Value.gameObject);
        }
        currentChunkViews.Clear();

        foreach (var kv in previousChunkViews)
        {
            kv.Value.chunkMCMap?.Release();
            if (Application.isPlaying)
                Destroy(kv.Value.gameObject);
            else
                DestroyImmediate(kv.Value.gameObject);
        }
        previousChunkViews.Clear();
    }

    private void OnDisable()
    {
        Release();
    }
    public Vector3Int WorldToCoor(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / chunkData.chunkSize);
        int y = Mathf.FloorToInt(worldPos.y / chunkData.chunkSize);
        int z = Mathf.FloorToInt(worldPos.z / chunkData.chunkSize);
        return new Vector3Int(x, y, z);
    }
    public Vector3 CoorToWorld(Vector3Int coor)
    {
        float x = coor.x * chunkData.chunkSize;
        float y = coor.y * chunkData.chunkSize;
        float z = coor.z * chunkData.chunkSize;
        return new Vector3(x, y, z);
    }
}