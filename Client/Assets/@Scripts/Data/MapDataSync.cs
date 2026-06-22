using System.IO;
using UnityEngine;

public static class MapDataSync
{
    public const string SourceRelativePath = "Data/MapCollisions.json";
    public const string ResourcesRelativePath = "Assets/Resources/Data/MapCollisions.json";

    public static bool TrySyncToResources()
    {
        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
            return false;

        string sourcePath = Path.GetFullPath(Path.Combine(projectRoot, "..", SourceRelativePath));
        string targetPath = Path.Combine(projectRoot, ResourcesRelativePath);

        if (File.Exists(sourcePath) == false)
        {
            Debug.LogWarning($"[MapDataSync] Source not found: {sourcePath}");
            return File.Exists(targetPath);
        }

        string sourceText = File.ReadAllText(sourcePath);
        string targetText = File.Exists(targetPath) ? File.ReadAllText(targetPath) : string.Empty;
        if (sourceText == targetText)
            return true;

        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
        File.WriteAllText(targetPath, sourceText);
        Debug.Log("[MapDataSync] Synced MapCollisions.json to Resources.");
        return true;
    }
}
