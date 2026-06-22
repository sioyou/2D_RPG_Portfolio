using UnityEngine;

public class MapCollision
{
    readonly int _mapId;
    readonly float _originX;
    readonly float _originY;
    readonly float _tileSize;
    readonly int _width;
    readonly int _height;
    readonly bool[] _blocked;

    public int MapId => _mapId;
    public bool IsLoaded => _width > 0 && _height > 0;

    public MapCollision(MapCollisionData data)
    {
        _mapId = data.mapId;
        _originX = data.originX;
        _originY = data.originY;
        _tileSize = data.tileSize > 0f ? data.tileSize : 1f;
        _width = data.width;
        _height = data.height;

        int expectedSize = data.width * data.height;
        _blocked = new bool[expectedSize];

        for (int i = 0; i < expectedSize && i < data.cells.Length; i++)
            _blocked[i] = data.cells[i] == Define.MAP_TOOL_WALL;
    }

    public bool IsWalkable(float worldX, float worldY)
    {
        if (IsLoaded == false)
            return false;

        WorldToTile(worldX, worldY, out int tileX, out int tileY);
        return IsTileWalkable(tileX, tileY);
    }

    public bool IsWalkableWithRadius(float worldX, float worldY, float radius)
    {
        if (IsLoaded == false)
            return false;

        if (IsInsideWorldBounds(worldX, worldY, radius) == false)
            return false;

        if (radius <= 0f)
            return IsWalkable(worldX, worldY);

        float minX = worldX - radius;
        float maxX = worldX + radius;
        float minY = worldY - radius;
        float maxY = worldY + radius;

        int minTileX = Mathf.FloorToInt((minX - _originX) / _tileSize);
        int maxTileX = Mathf.FloorToInt((maxX - _originX) / _tileSize);
        int minTileY = Mathf.FloorToInt((minY - _originY) / _tileSize);
        int maxTileY = Mathf.FloorToInt((maxY - _originY) / _tileSize);

        minTileX = Mathf.Max(minTileX, 0);
        minTileY = Mathf.Max(minTileY, 0);
        maxTileX = Mathf.Min(maxTileX, _width - 1);
        maxTileY = Mathf.Min(maxTileY, _height - 1);

        if (minTileX > maxTileX || minTileY > maxTileY)
            return false;

        for (int ty = minTileY; ty <= maxTileY; ty++)
        {
            for (int tx = minTileX; tx <= maxTileX; tx++)
            {
                if (IsTileWalkable(tx, ty) == false)
                    return false;
            }
        }

        return true;
    }

    public Vector2 ResolveMove(float fromX, float fromY, float toX, float toY, float radius)
    {
        if (IsLoaded == false)
            return new Vector2(fromX, fromY);

        if (IsWalkableWithRadius(toX, toY, radius))
            return new Vector2(toX, toY);

        if (IsWalkableWithRadius(toX, fromY, radius))
            return new Vector2(toX, fromY);

        if (IsWalkableWithRadius(fromX, toY, radius))
            return new Vector2(fromX, toY);

        return new Vector2(fromX, fromY);
    }

    public bool IsBlockedTile(int tileX, int tileY)
    {
        return IsTileWalkable(tileX, tileY) == false;
    }

    public float OriginX => _originX;
    public float OriginY => _originY;
    public float TileSize => _tileSize;
    public int Width => _width;
    public int Height => _height;

    bool IsInsideWorldBounds(float worldX, float worldY, float radius)
    {
        float minX = _originX + radius;
        float maxX = _originX + _width * _tileSize - radius;
        float minY = _originY + radius;
        float maxY = _originY + _height * _tileSize - radius;

        if (minX > maxX || minY > maxY)
            return IsWalkable(worldX, worldY);

        return worldX >= minX && worldX <= maxX && worldY >= minY && worldY <= maxY;
    }

    bool IsTileWalkable(int tileX, int tileY)
    {
        if (tileX < 0 || tileY < 0 || tileX >= _width || tileY >= _height)
            return false;

        int index = tileY * _width + tileX;
        if (index < 0 || index >= _blocked.Length)
            return false;

        return _blocked[index] == false;
    }

    void WorldToTile(float worldX, float worldY, out int tileX, out int tileY)
    {
        tileX = Mathf.FloorToInt((worldX - _originX) / _tileSize);
        tileY = Mathf.FloorToInt((worldY - _originY) / _tileSize);
    }
}
