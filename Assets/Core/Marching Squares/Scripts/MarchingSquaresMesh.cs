using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingSquaresMesh : ProceduralMesh
{
    [Header("Mesh")]
    public bool build3D = false;
    public int squareSizeX = 10;
    public int squareSizeY = 10;
    public float squareSize = 1f;
    public float scale = 1;
    public float height = 1;
    [Range(0, 1)]
    public float threshold = 0.5f;
    public bool squareLerp = false;

    private MarchingSquare[] squaresMap;

    protected override void Generate()
    {
        base.Generate();

        int length = squareSizeX * squareSizeY;
        float halfSize = squareSize * 0.5f;
        squaresMap = new MarchingSquare[length];
        ProceduralMeshPart main = new ProceduralMeshPart();

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
                if (build3D)
                {
                    squaresMap[index].BuildToMesh3D(main, height);
                }
                else 
                {
                    squaresMap[index].BuildToMesh2D(main);
                }
            }
        }

        main.FillArrays();
        mesh.vertices = main.Vertices;
        mesh.uv = main.UVs;
        mesh.triangles = main.Triangles;
        mesh.colors = main.Colors;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}
