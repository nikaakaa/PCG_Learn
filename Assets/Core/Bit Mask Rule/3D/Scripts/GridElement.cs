using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GridElement : MonoBehaviour
{
    private BoxCollider box;
    private Renderer render;

    [ReadOnly] public Vector3Int coord;
    [ReadOnly] public PointElement[] corners;

    public Vector3 Size { get { return box.size; } }
    public bool IsEnable { get; set; }

    public void Initialize(int x, int y, int z, float height)
    {
        box = GetComponent<BoxCollider>();
        box.size = new Vector3(1, height, 1);
        transform.localScale = new Vector3(1, height, 1);
        render = GetComponent<Renderer>();
        render.enabled = false;

        gameObject.name = "Element_" + y + "_" + z + "_" + x;
        coord = new Vector3Int(x, y, z);
        SetActive(false, false);
    }

    public void SetCorners()
    {
        corners = new PointElement[8];
        corners[0] = GridMap.Instance.GetPoint(coord);
        corners[1] = GridMap.Instance.GetPoint(coord + new Vector3Int(1, 0, 0));
        corners[2] = GridMap.Instance.GetPoint(coord + new Vector3Int(0, 0, 1));
        corners[3] = GridMap.Instance.GetPoint(coord + new Vector3Int(1, 0, 1));    
        corners[4] = GridMap.Instance.GetPoint(coord + new Vector3Int(0, 1, 0));
        corners[5] = GridMap.Instance.GetPoint(coord + new Vector3Int(1, 1, 0));
        corners[6] = GridMap.Instance.GetPoint(coord + new Vector3Int(0, 1, 1));
        corners[7] = GridMap.Instance.GetPoint(coord + new Vector3Int(1, 1, 1));

        Vector3 halfSize = box.size * 0.5f;
        corners[0].transform.position = transform.position - halfSize;
        corners[1].transform.position = transform.position + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        corners[2].transform.position = transform.position + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        corners[3].transform.position = transform.position + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        corners[4].transform.position = transform.position + new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        corners[5].transform.position = transform.position + new Vector3(halfSize.x, halfSize.y, halfSize.z);
        corners[6].transform.position = transform.position + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        corners[7].transform.position = transform.position + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
    }

    public void SetActive(bool isActive, bool updateCorner = true)
    {
        box.enabled = isActive;
        IsEnable = isActive;

        if (updateCorner) 
        {
            for (int i = 0; i < 8; i++)
            {
                corners[i].UpdateBitMask();
            }
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, Size);
    }
}
