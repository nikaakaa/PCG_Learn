using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointElement : MonoBehaviour
{
	[ReadOnly] public Vector3Int coord;
    [ReadOnly] public GridElement[] nearGrids;
	[ReadOnly] public int bitMaskValue;

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Material material;

	public float speed = 2f;
	private bool animating;
	private bool dissolve;
	private float timer;
	private float timeLength;

    private void Update()
    {
		if (animating) 
		{
			if (timer < timeLength)
			{
				timer += Time.deltaTime;
				if (dissolve)
				{
					material.SetFloat("_DissolveValue", timer * speed);
				}
				else 
				{
					material.SetFloat("_DissolveValue", 1 - timer * speed);
				}
			}
			else 
			{
				timer = 0;
				animating = false;
			}
		}
    }

    public void Initialize(int x, int y, int z, Material material)
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		this.material = new Material(material);
		meshRenderer.material = this.material;
		gameObject.name = "Point_" + y + "_" + z + "_" + x;
		coord = new Vector3Int(x, y, z);
		timeLength = 1f / speed;

		nearGrids = new GridElement[8];
		Vector3Int count = GridMap.Instance.gridCount;
		//UpperNorthEast
		nearGrids[0] = GridMap.Instance.GetGrid(coord);
		//UpperNorthWest
		nearGrids[1] = GridMap.Instance.GetGrid(coord + new Vector3Int(-1, 0, 0));
		//UpperSouthWest
		nearGrids[2] = GridMap.Instance.GetGrid(coord + new Vector3Int(-1, 0, -1));
		//UpperSouthEast
		nearGrids[3] = GridMap.Instance.GetGrid(coord + new Vector3Int(0, 0, -1));
		//LowerNorthEast
		nearGrids[4] = GridMap.Instance.GetGrid(coord + new Vector3Int(0, -1, 0));
		//LowerNorthWest
		nearGrids[5] = GridMap.Instance.GetGrid(coord + new Vector3Int(-1, -1, 0));
		//LowerSouthWest
		nearGrids[6] = GridMap.Instance.GetGrid(coord + new Vector3Int(-1, -1, -1));
		//LowerSouthEast
		nearGrids[7] = GridMap.Instance.GetGrid(coord + new Vector3Int(0, -1, -1));

		UpdateBitMask();
	}

	public void UpdateBitMask() 
	{
		int newBitMaskValue = 0;
		for (int i = 0; i < nearGrids.Length; i++) 
		{
			if (nearGrids[i] != null && nearGrids[i].IsEnable) 
			{
				newBitMaskValue += 1 << i;
			}
		}

		dissolve = newBitMaskValue == 0 && bitMaskValue != 0;
		if (dissolve) //ÏûÊ§
		{
			material.SetFloat("_DissolveValue", 0);
		}
		else //±ä»¯
		{
			material.SetFloat("_DissolveValue", 1);
		}
		timer = 0;
		animating = true;
		
		bitMaskValue = newBitMaskValue;
		meshRenderer.enabled = bitMaskValue != 0;
		meshFilter.mesh = GridMap.Instance.GetPointMesh(bitMaskValue, coord.y);
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(transform.position, 0.1f);
	}
}
