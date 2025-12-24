using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquares 
{
    
}

public class MarchingSquarePoint
{
    public Vector3 position;
    public float value;

    public MarchingSquarePoint(Vector3 position, float value)
    {
        this.position = position;
        this.value = value;
    }   
}
