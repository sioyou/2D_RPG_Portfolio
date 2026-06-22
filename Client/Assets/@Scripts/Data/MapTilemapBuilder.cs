using UnityEngine;
using UnityEngine.Tilemaps;

public static class MapTilemapBuilder
{
    public static GameObject Build(MapCollisionData data, MapTileSet tileSet)
    {
        var mapRoot = new GameObject("@MapRoot");
        var grid = mapRoot.AddComponent<Grid>();
        grid.cellSize = new Vector3(data.tileSize, data.tileSize, 0f);
        mapRoot.transform.position = new Vector3(data.originX, data.originY, 0f);

        var groundGo = new GameObject("Ground");
        groundGo.transform.SetParent(mapRoot.transform, false);
        var groundMap = groundGo.AddComponent<Tilemap>();
        var groundRenderer = groundGo.AddComponent<TilemapRenderer>();
        groundRenderer.sortingOrder = 0;

        var wallGo = new GameObject("Wall");
        wallGo.transform.SetParent(mapRoot.transform, false);
        var wallMap = wallGo.AddComponent<Tilemap>();
        var wallRenderer = wallGo.AddComponent<TilemapRenderer>();
        wallRenderer.sortingOrder = 1;

        bool useSprites = tileSet != null && tileSet.IsValid;
        Tile fallbackGround = useSprites ? null : CreateColorTile(new Color(0.45f, 0.55f, 0.40f));
        Tile fallbackWall = useSprites ? null : CreateColorTile(new Color(0.35f, 0.28f, 0.22f));

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                int index = y * data.width + x;
                bool isWall = index < data.cells.Length && data.cells[index] == Define.MAP_TOOL_WALL;

                if (isWall)
                {
                    Tile wallTile = useSprites
                        ? tileSet.PickWall(x, y, data.width, data.height)
                        : fallbackWall;
                    wallMap.SetTile(cell, wallTile);
                }
                else
                {
                    Tile groundTile = useSprites
                        ? tileSet.PickGrass(x, y)
                        : fallbackGround;
                    groundMap.SetTile(cell, groundTile);
                }
            }
        }

        return mapRoot;
    }

    static Tile CreateColorTile(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        texture.filterMode = FilterMode.Point;

        var sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        return tile;
    }
}
