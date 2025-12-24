using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Transform center;

    public float rotateSpeed = 50;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(center.position, Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
