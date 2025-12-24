using UnityEngine;
using UnityEngine.Tilemaps;

public class BitMaskDemo : MonoBehaviour
{
    public Tilemap tileMap;
    public TileBase[] tiles;
    public TextAsset bitMaskConfig;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            CreateTile(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButtonDown(1))
        {
            DestroyMap(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearMap();
        }
    }

    private void CreateTile(Vector3 worldPos) 
    {
        Vector3Int selectedTile = tileMap.WorldToCell(worldPos);
        tileMap.SetTile(selectedTile, tiles[0]);
        SetRule();
    }

    private void DestroyMap(Vector3 worldPos)
    {
        Vector3Int selectedTile = tileMap.WorldToCell(worldPos);
        tileMap.SetTile(selectedTile, null);
        SetRule();
    }

    [ContextMenu("SetRule")]
    private void SetRule()
    {
        BitMaskRuleTileMap.SetRule(tileMap, tiles, bitMaskConfig);
    }

    [ContextMenu("ClearMap")]
    private void ClearMap() 
    {
        tileMap.ClearAllTiles();
    }
}
