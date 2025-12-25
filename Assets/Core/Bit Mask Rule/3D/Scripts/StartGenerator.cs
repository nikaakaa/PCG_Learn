using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGenerator : MonoBehaviour
{
    private void Start()
    {
        GridMap.Instance.CreateMap();
        GenerateLevel();
    }

    private void GenerateLevel() 
    {
        Vector3Int gridCount = GridMap.Instance.gridCount;
        for (int z = 0; z < gridCount.z; z++)
        {
            for (int x = 0; x < gridCount.x; x++)
            {
                int index = GridMap.Instance.CoordToIndex(x, 0, z, gridCount);
                GridMap.Instance.GetGrid(index).SetActive(true);
            }
        }
    }
}
