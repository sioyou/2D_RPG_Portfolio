#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class MapDataAutoSync
{
    static MapDataAutoSync()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
            return;

        MapDataSync.TrySyncToResources();
        AssetDatabase.Refresh();
    }
}
#endif
