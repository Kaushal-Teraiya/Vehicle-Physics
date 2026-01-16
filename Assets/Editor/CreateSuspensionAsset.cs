#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Editor utilities for creating SuspensionData assets from script
public static class CreateSuspensionAsset
{
    [MenuItem("Assets/Create/Car/Suspension Data (from script)")]
    public static void Create()
    {
        // Create a new asset and place it in the Assets folder. Use a unique path to avoid collisions.
        var asset = ScriptableObject.CreateInstance<SuspensionData>();
        string path = "Assets/NewSuspensionData.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    // Create an asset at a specific relative path (editor API)
    public static SuspensionData CreateAtPath(string relativePath)
    {
        var asset = ScriptableObject.CreateInstance<SuspensionData>();
        string path = relativePath;
        if (string.IsNullOrEmpty(path))
            path = "Assets/NewSuspensionData.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        return asset;
    }
}
#endif
