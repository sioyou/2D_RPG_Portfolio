#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MapEditorTools
{
    [MenuItem("Tools/Map/Sync Collision Data To Resources")]
    public static void SyncCollisionData()
    {
        if (MapDataSync.TrySyncToResources())
            AssetDatabase.Refresh();

        Debug.Log("[MapEditorTools] Synced MapCollisions.json to Resources.");
    }

    [MenuItem("Tools/Map/Setup Room1 Tilemap In Scene")]
    public static void SetupRoom1TilemapInScene()
    {
        TextAsset mapAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MapDataSync.ResourcesRelativePath);
        if (mapAsset == null)
        {
            SyncCollisionData();
            mapAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(MapDataSync.ResourcesRelativePath);
        }

        if (mapAsset == null)
        {
            Debug.LogError("[MapEditorTools] MapCollisions.json not found in Resources.");
            return;
        }

        MapCollisionData data = MapCollisionDataLoader.LoadFromJson(mapAsset.text);
        if (data == null)
        {
            Debug.LogError("[MapEditorTools] Failed to parse map data.");
            return;
        }

        Transform existing = GameObject.Find("@MapRoot")?.transform;
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        MapTileSet tileSet = MapTileSet.Load();
        if (tileSet.IsValid == false)
            Debug.LogWarning("[MapEditorTools] Tile sprites missing under Resources/Map/Tiles.");

        MapTilemapBuilder.Build(data, tileSet.IsValid ? tileSet : null);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[MapEditorTools] Room1 tilemap created in active scene.");
    }
}
#endif
