using UnityEngine;

public class MapManager
{
    public const string MapDataFileName = "MapCollisions.json";
    public const string ResourcesMapDataPath = "Data/MapCollisions";

    MapCollision _collision;
    GameObject _mapRoot;
    int _loadedMapId;

    public MapCollision Collision => _collision;
    public bool IsReady => _collision != null && _collision.IsLoaded;
    public int LoadedMapId => _loadedMapId;

    public bool Init(int mapId)
    {
        Clear();

        GameObject existing = GameObject.Find("@MapRoot");
        if (existing != null)
            Object.Destroy(existing);

        if (MapDataSync.TrySyncToResources() == false)
            Debug.LogWarning("[MapManager] Map collision data sync skipped or failed.");

        TextAsset mapAsset = Resources.Load<TextAsset>(ResourcesMapDataPath);
        if (mapAsset == null)
        {
            Debug.LogError($"[MapManager] Failed to load Resources/{ResourcesMapDataPath}.json");
            return false;
        }

        MapCollisionData data = MapCollisionDataLoader.LoadFromJson(mapAsset.text);
        if (data == null)
        {
            Debug.LogError("[MapManager] Map collision data is empty.");
            return false;
        }

        if (data.mapId != mapId)
            Debug.LogWarning($"[MapManager] Loaded mapId={data.mapId}, requested mapId={mapId}");

        _collision = new MapCollision(data);
        if (_collision.IsLoaded == false)
        {
            Debug.LogError($"[MapManager] Invalid map collision data. mapId={data.mapId}");
            _collision = null;
            return false;
        }

        _loadedMapId = data.mapId;

        MapTileSet tileSet = MapTileSet.Load();
        if (tileSet.IsValid == false)
            Debug.LogWarning("[MapManager] Map tile sprites missing. Using fallback colors.");

        _mapRoot = MapTilemapBuilder.Build(data, tileSet.IsValid ? tileSet : null);
        return true;
    }

    public Vector2 ResolveMove(Vector2 from, Vector2 to, float radius)
    {
        if (_collision == null)
            return from;

        return _collision.ResolveMove(from.x, from.y, to.x, to.y, radius);
    }

    public void Clear()
    {
        _collision = null;
        _loadedMapId = 0;

        if (_mapRoot != null)
        {
            Object.Destroy(_mapRoot);
            _mapRoot = null;
        }
    }
}
