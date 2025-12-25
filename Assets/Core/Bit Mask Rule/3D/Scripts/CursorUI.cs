using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorUI : MonoBehaviour
{
    private Camera cam;
    private RectTransform rtf;
    private Canvas canvas;
    [SerializeField] private RectTransform nagZ;
    [SerializeField] private RectTransform posZ;

    private GridElement currentElement;

    private void Awake()
    {
        cam = Camera.main;
        rtf = GetComponent<RectTransform>();
        rtf.sizeDelta = Vector2.zero;
        canvas = GetComponent<Canvas>();
    }

    private void Update()
    {
        RayCast();
        CheckRemoveGrid();
    }

    /// <summary> 当前鼠标悬浮的格子 </summary>
    private void RayCast()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GridElement element = hit.transform.GetComponent<GridElement>();
            if (element != null)
            {
                SetActive(true, element);
                return;
            }
        }
        SetActive(false, null);
    }

    /// <summary> 悬浮在格子上时显示 </summary>
    private void SetActive(bool isHit, GridElement element)
    {
        canvas.enabled = isHit;
        currentElement = element;

        if (isHit)
        {
            rtf.position = element.transform.position;
            rtf.sizeDelta = new Vector2(element.Size.x, element.Size.y);
            nagZ.localPosition = new Vector3(nagZ.localPosition.x, nagZ.localPosition.y, -element.Size.z * 0.5f);
            posZ.localPosition = new Vector3(posZ.localPosition.x, posZ.localPosition.y, element.Size.z * 0.5f);
        }
    }

    /// <summary> 右键删除格子 </summary>
    private void CheckRemoveGrid() 
    {
        if (currentElement != null)
        {
            if (Input.GetMouseButtonDown(1)) 
            {
                GridMap.Instance.RemoveGrid(currentElement);
            }
        }
    }

    /// <summary> 左键删除格子 </summary>
    public void CheckAddGrid(int face) 
    {
        if (currentElement != null) 
        {
            GridMap.Instance.AddGrid(currentElement, face);
        }
    }
}
