using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestPointTest : MonoBehaviour
{
    public Collider col;

    private Vector3 closestPoint;

    private void OnDrawGizmos()
    {
        if (col != null) 
        {
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            closestPoint = col.ClosestPoint(transform.position);
            Gizmos.DrawWireSphere(closestPoint, 0.1f);
            Gizmos.DrawLine(transform.position, closestPoint);
        }
    }
}
