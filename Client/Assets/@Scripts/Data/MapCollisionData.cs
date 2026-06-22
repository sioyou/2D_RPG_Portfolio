using System;
using Newtonsoft.Json;

[Serializable]
public class MapCollisionData
{
    public int mapId;
    public float originX;
    public float originY;
    public float tileSize;
    public int width;
    public int height;
    public string cells;
}

public static class MapCollisionDataLoader
{
    public static MapCollisionData LoadFromJson(string json)
    {
        MapCollisionData[] maps = JsonConvert.DeserializeObject<MapCollisionData[]>(json);
        if (maps == null || maps.Length == 0)
            return null;

        return maps[0];
    }
}
