using UnityEngine;
using UnityEngine.Tilemaps;

public class MapTileSet
{
    public Tile GrassA { get; private set; }
    public Tile GrassB { get; private set; }
    public Tile GrassC { get; private set; }
    public Tile WallCornerBL { get; private set; }
    public Tile WallCornerBR { get; private set; }
    public Tile WallCornerTL { get; private set; }
    public Tile WallCornerTR { get; private set; }
    public Tile WallEdgeB { get; private set; }
    public Tile WallEdgeT { get; private set; }
    public Tile WallEdgeL { get; private set; }
    public Tile WallEdgeR { get; private set; }

    public bool IsValid =>
        GrassA != null && WallCornerBL != null && WallEdgeB != null;

    public static MapTileSet Load()
    {
        var set = new MapTileSet();
        set.GrassA = LoadTile("Map/Tiles/Grass_A");
        set.GrassB = LoadTile("Map/Tiles/Grass_B");
        set.GrassC = LoadTile("Map/Tiles/Grass_C");
        set.WallCornerBL = LoadTile("Map/Tiles/Wall_Corner_BL");
        set.WallCornerBR = LoadTile("Map/Tiles/Wall_Corner_BR");
        set.WallCornerTL = LoadTile("Map/Tiles/Wall_Corner_TL");
        set.WallCornerTR = LoadTile("Map/Tiles/Wall_Corner_TR");
        set.WallEdgeB = LoadTile("Map/Tiles/Wall_Edge_B");
        set.WallEdgeT = LoadTile("Map/Tiles/Wall_Edge_T");
        set.WallEdgeL = LoadTile("Map/Tiles/Wall_Edge_L");
        set.WallEdgeR = LoadTile("Map/Tiles/Wall_Edge_R");
        return set;
    }

    public Tile PickGrass(int tileX, int tileY)
    {
        int pick = Mathf.Abs(tileX * 73856093 ^ tileY * 19349663) % 3;
        if (pick == 1)
            return GrassB != null ? GrassB : GrassA;
        if (pick == 2)
            return GrassC != null ? GrassC : GrassA;
        return GrassA;
    }

    public Tile PickWall(int tileX, int tileY, int width, int height)
    {
        bool onBottom = tileY == 0;
        bool onTop = tileY == height - 1;
        bool onLeft = tileX == 0;
        bool onRight = tileX == width - 1;

        if (onBottom && onLeft)
            return WallCornerBL;
        if (onBottom && onRight)
            return WallCornerBR;
        if (onTop && onLeft)
            return WallCornerTL;
        if (onTop && onRight)
            return WallCornerTR;
        if (onBottom)
            return WallEdgeB;
        if (onTop)
            return WallEdgeT;
        if (onLeft)
            return WallEdgeL;
        if (onRight)
            return WallEdgeR;

        return WallEdgeT;
    }

    static Tile LoadTile(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogWarning($"[MapTileSet] Missing sprite: Resources/{resourcePath}");
            return null;
        }

        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        return tile;
    }
}
