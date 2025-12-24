using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquaresMap : MonoBehaviour
{
    public GizmosSettings gizmosSettings;

    public int squareSizeX = 10;
    public int squareSizeY = 10;
    public float squareSize = 1f;
    public float scale = 1;
    [Range(0, 1)]
    public float threshold = 0.5f;
    public bool squareLerp = false;

    private MarchingSquare[] squaresMap;

    private void OnValidate()
    {
        CreateMap();
    }

    private void CreateMap() 
    {
        int length = squareSizeX * squareSizeY;
        float halfSize = squareSize * 0.5f;

        squaresMap = new MarchingSquare[length];
        for (int i = 0; i < squareSizeX; i++)
        {
            for (int j = 0; j < squareSizeY; j++)
            {
                int index = i * squareSizeY + j;
                squaresMap[index] = new MarchingSquare(
                    new Vector3(i * squareSize + halfSize, 0, j * squareSize + halfSize),
                    scale,
                    threshold,
                    halfSize, 
                    squareLerp);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (squaresMap != null) 
        {
            for (int i = 0; i < squaresMap.Length; i++)
            {
                squaresMap[i].DrawPoints(transform.position, gizmosSettings);
                squaresMap[i].DrawLines(transform.position, gizmosSettings);
            }
        }
    }
}