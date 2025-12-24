using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

using BitMaskDictionary = System.Collections.Generic.Dictionary<int, int>;

public class BitMaskRuleTileMap
{
    public static void SetRule(Tilemap tileMap, TileBase[] tiles, TextAsset bitMaskConfig) 
    {
        BitMaskDictionary bitMaskDic = BitMaskConfigReader.Read(bitMaskConfig.text);
        Vector2Int mapSize = (Vector2Int)tileMap.size + Vector2Int.one * 2;
        Vector2Int mapOffsetFromOrigin = Vector2Int.zero - (Vector2Int)tileMap.origin;
        int[,] map = new int[mapSize.x, mapSize.y];

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                if (i == 0 || i == mapSize.x - 1 || j == 0 || j == mapSize.y - 1)
                {
                    map[i, j] = 0;
                    continue;
                }

                if (tileMap.GetTile(new Vector3Int(i - 1 - mapOffsetFromOrigin.x, j - 1 - mapOffsetFromOrigin.y, 0)))
                {
                    map[i, j] = -1;
                }
                else
                {
                    map[i, j] = 0;
                }
            }
        }

        int[,] tmp = new int[mapSize.x, mapSize.y];
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                int value = map[i, j];
                tmp[i, j] = value;
            }
        }

        for (int i = 1; i < mapSize.x - 1; i++)
        {
            for (int j = 1; j < mapSize.y - 1; j++)
            {
                if (map[i, j] != 0)
                {
                    int northTile = map[i, j + 1] == -1 ? 1 : 0;
                    int southTile = map[i, j - 1] == -1 ? 1 : 0;
                    int westTile = map[i - 1, j] == -1 ? 1 : 0;
                    int eastTile = map[i + 1, j] == -1 ? 1 : 0;
                    int north_west_tile = map[i - 1, j + 1] == -1 && northTile == 1 && westTile == 1 ? 1 : 0;
                    int north_east_tile = map[i + 1, j + 1] == -1 && northTile == 1 && eastTile == 1 ? 1 : 0;
                    int south_west_tile = map[i - 1, j - 1] == -1 && southTile == 1 && westTile == 1 ? 1 : 0;
                    int south_east_tile = map[i + 1, j - 1] == -1 && southTile == 1 && eastTile == 1 ? 1 : 0;

                    int tileIndex = northTile * DirectionEight.North + 
                                    westTile * DirectionEight.West + 
                                    southTile * DirectionEight.South + 
                                    eastTile * DirectionEight.East +
                                    north_west_tile * DirectionEight.West_North +
                                    north_east_tile * DirectionEight.East_North +
                                    south_west_tile * DirectionEight.West_South +
                                    south_east_tile * DirectionEight.East_South;
                    tmp[i, j] = 1;
                    tileMap.SetTile(new Vector3Int(i - 1 - mapOffsetFromOrigin.x, j - 1 - mapOffsetFromOrigin.y, 0), tiles[bitMaskDic[tileIndex]]);
                }
            }
        }

        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                int value = tmp[i, j];
                map[i, j] = value;
            }
        }
    }
}

public class DirectionEight
{
    public const int West_North = 1;
    public const int North = 2;
    public const int East_North = 4;
    public const int West = 8;
    public const int East = 16;
    public const int West_South = 32;
    public const int South = 64;
    public const int East_South = 128;
}

public static class BitMaskConfigReader
{
    public static BitMaskDictionary Read(string contents)
    {
        BitMaskDictionary dic = new BitMaskDictionary();
        int key = -1; //主键
        int value = -1;//值
        StringReader reader = new StringReader(contents);
        string line = null;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();//去除空白行
            if (!string.IsNullOrEmpty(line))
            {
                string[] keyValue = line.Split('=');
                key = int.Parse(keyValue[0].Trim());
                value = int.Parse(keyValue[1].Trim(','));
                dic.Add(key, value);
            }
        }
        return dic;
    }
}
