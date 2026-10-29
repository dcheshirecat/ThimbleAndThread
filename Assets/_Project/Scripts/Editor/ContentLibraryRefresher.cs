#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// Keeps Data/ContentLibrary.asset in sync with the data assets, so new customers, items,
/// supplies and patterns show up in the game without touching code or scenes.
/// Also available as Tools > Thimble & Thread > Refresh Content Library.
public class ContentLibraryRefresher : AssetPostprocessor
{
    public const string LibraryPath = "Assets/_Project/Data/ContentLibrary.asset";
    const string DataRoot = "Assets/_Project/Data";

    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (var list in new[] { imported, deleted, moved, movedFrom })
            foreach (var path in list)
                if (path.StartsWith(DataRoot) && path.EndsWith(".asset") && path != LibraryPath)
                {
                    EditorApplication.delayCall += () => Refresh();   // after the import finishes
                    return;
                }
    }

    [MenuItem("Tools/Thimble & Thread/Refresh Content Library")]
    public static ContentLibrary Refresh()
    {
        var lib = AssetDatabase.LoadAssetAtPath<ContentLibrary>(LibraryPath);
        if (lib == null)
        {
            lib = ScriptableObject.CreateInstance<ContentLibrary>();
            AssetDatabase.CreateAsset(lib, LibraryPath);
        }
        lib.customers = Find<CustomerDef>();
        lib.items = Find<CraftItemDef>();
        lib.supplies = Find<SupplyDef>();
        lib.patterns = Find<PatternDef>();
        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        return lib;
    }

    static T[] Find<T>() where T : Object
    {
        var paths = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { DataRoot }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        paths.Sort();   // stable order, so the asset only changes when content does
        var result = new List<T>();
        foreach (var p in paths)
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(p);
            if (a != null) result.Add(a);
        }
        return result.ToArray();
    }
}
#endif
