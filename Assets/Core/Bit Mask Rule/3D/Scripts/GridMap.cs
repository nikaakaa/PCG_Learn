using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour
{
    private static GridMap instance;
    public static GridMap Instance { get { return instance; } }

    public Vector3Int gridCount = new Vector3Int(5, 10, 5);
    public GridElement gridPrefab;
    public PointElement pointPrefab;
    public Material buildingMaterial;
    public GameObject moduleObj;
    public AudioClip audioClip;

    private Dictionary<string, Mesh> modules = new Dictionary<string, Mesh>();

    private Transform gridsTF;
    public int totalGridCount { get; set; }
    private GridElement[] grids;

    private Vector3Int pointCount;
    private Transform pointsTF;
    private int totalPointCount;
    private PointElement[] points;

    private Camera cam;

    private void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    /// <summary> 创建格子和点 </summary>
    public void CreateMap()
    {
        // 记录所有模块
        foreach (Transform child in moduleObj.transform)
        {
            modules.Add(child.name, child.GetComponent<MeshFilter>().sharedMesh);
        }

        // 地基
        float floorHeight = 0.25f; // 第0层
        float basementHeight = 1.5f - floorHeight * 0.5f; // 第1层
        float gridHeight = 1;

        // 所有的格子
        gridsTF = new GameObject("Grids").transform;
        gridsTF.SetParent(transform);
        totalGridCount = gridCount.x * gridCount.y * gridCount.z;
        grids = new GridElement[totalGridCount];
        for (int y = 0; y < gridCount.y; y++)
        {
            float yPos = y;
            if (y == 0)
                gridHeight = floorHeight;
            else if (y == 1) 
            {
                gridHeight = basementHeight;
                yPos = (floorHeight + basementHeight) * 0.5f;
            }
            else
                gridHeight = 1;

            for (int z = 0; z < gridCount.z; z++)
            {
                for (int x = 0; x < gridCount.x; x++)
                {
                    int index = CoordToIndex(x, y, z, gridCount);
                    Vector3 pos = new Vector3(x, yPos, z) - 0.5f * new Vector3(gridCount.x - 1, 0, gridCount.z - 1);
                    GridElement grid = Instantiate(gridPrefab, pos, Quaternion.identity, gridsTF);
                    grid.Initialize(x, y, z, gridHeight);
                    grids[index] = grid;
                }
            }
        }

        // 所有的顶点
        pointCount = gridCount + Vector3Int.one;
        pointsTF = new GameObject("Points").transform;
        pointsTF.SetParent(transform);
        totalPointCount = (gridCount.x + 1) * (gridCount.y + 1) * (gridCount.z + 1);
        points = new PointElement[totalPointCount];
        for (int y = 0; y < pointCount.y; y++)
        {
            for (int z = 0; z < pointCount.z; z++)
            {
                for (int x = 0; x < pointCount.x; x++)
                {
                    int index = CoordToIndex(x, y, z, pointCount);
                    PointElement point = Instantiate(pointPrefab, pointsTF);
                    point.Initialize(x, y, z, buildingMaterial);
                    points[index] = point;
                }
            }
        }

        // 标记每个格子的顶点
        for (int i = 0; i < totalGridCount; i++) 
        {
            grids[i].SetCorners();
        }
    }

    /// <summary> 在当前选中的格子的一个面上创建 </summary>
    public void AddGrid(GridElement element, int face) 
    {
        Vector3Int coord = element.coord;
        int index = -1;
        switch (face)
        {
            case 1: //前
                if (coord.z < gridCount.z - 1)
                {
                    coord += new Vector3Int(0, 0, 1);
                    index = CoordToIndex(coord, gridCount);
                }
                break;
            case 2: //后
                if (coord.z > 0)
                {
                    coord += new Vector3Int(0, 0, -1);
                    index = CoordToIndex(coord, gridCount);
                }
                break;
            case 3: //左
                if (coord.x > 0)
                {
                    coord += new Vector3Int(-1, 0, 0);
                    index = CoordToIndex(coord, gridCount);
                }
                break;
            case 4: //右
                if (coord.x < gridCount.x - 1)
                {
                    coord += new Vector3Int(1, 0, 0);
                    index = CoordToIndex(coord, gridCount);
                }
                break;
            case 5: //上
                if (coord.y < gridCount.y - 1)
                {
                    coord += new Vector3Int(0, 1, 0);
                    index = CoordToIndex(coord, gridCount);
                }
                break;
            case 6: //下
                if (coord.y > 0)
                {
                    coord += new Vector3Int(0, -1, 0);
                    index = CoordToIndex(coord, gridCount);
                }
                break;
        }

        if (index > 0) 
        {
            grids[index].SetActive(true);
            PlayMusic();
        }       
    }

    /// <summary> 移除在当前选中的格子 </summary>
    public void RemoveGrid(GridElement element)
    {
        if (element.coord.y > 0) 
        {
            element.SetActive(false);
            PlayMusic();
        }   
    }

    /// <summary> 对格子进行操作的音乐 </summary>
    public void PlayMusic() 
    {
        AudioSource.PlayClipAtPoint(audioClip, cam.transform.position);
    }

    /// <summary> 根据坐标获取格子 </summary>
    public GridElement GetGrid(Vector3Int coord) 
    {
        int index = CoordToIndex(coord, gridCount);
        if(IsInRange(coord, gridCount))
        {
            return grids[index];
        }
        return null;
    }

    /// <summary> 根据索引获取格子 </summary>
    public GridElement GetGrid(int index)
    {
        if (index >= 0 && index < totalGridCount) 
        { 
            return grids[index];
        }
        return null;
    }

    /// <summary> 根据坐标获取顶点 </summary>
    public PointElement GetPoint(Vector3Int coord)
    {
        int index = CoordToIndex(coord, pointCount);
        if (IsInRange(coord, pointCount))
        {
            return points[index];
        }
        return null;
    }

    /// <summary> 根据顶点值获取模型 </summary>
    public Mesh GetPointMesh(int bitmask, int level)
    {
        Mesh result;

        if (level > 1 && level < gridCount.y)
        {
            if (modules.TryGetValue(bitmask.ToString(), out result))
            {
                return result;
            }

        }

        else if (level == 0)
        {
            if (modules.TryGetValue(0 + "_" + bitmask.ToString(), out result))
            {
                return result;
            }
        }

        else if (level == 1)
        {
            if (modules.TryGetValue(1 + "_" + bitmask.ToString(), out result))
            {
                return result;
            }
        }

        else if (level == gridCount.y)
        {
            if (modules.TryGetValue(2 + "_" + bitmask.ToString(), out result))
            {
                return result;
            }
        }

        return null;
    }

    public int CoordToIndex(int x, int y, int z, Vector3Int count)
    {
        return y * count.z * count.x + z * count.x + x;
    }

    public int CoordToIndex(Vector3Int coord, Vector3Int count)
    {
        return coord.y * count.z * count.x + coord.z * count.x + coord.x;
    }

    public bool IsInRange(Vector3 index, Vector3 range) 
    {
        return index.x >= 0 && index.x < range.x 
            && index.y >= 0 && index.y < range.y 
            && index.z >= 0 && index.z < range.z;
    }
}
