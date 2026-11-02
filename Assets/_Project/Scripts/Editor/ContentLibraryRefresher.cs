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
    const string DialogueRoot = "Assets/_Project/Dialogue";
    public const string ButtonArtRoot = "Assets/_Project/Art/UI/Buttons";

    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (var list in new[] { imported, deleted, moved, movedFrom })
            foreach (var path in list)
                if (path.StartsWith(DataRoot) && path.EndsWith(".asset") && path != LibraryPath
                    || path.StartsWith(DialogueRoot) && path.EndsWith(".ink")
                    || path.StartsWith(ButtonArtRoot) && path.EndsWith(".png"))
                {
                    EditorApplication.delayCall += () => Refresh();   // after the import finishes
                    return;
                }
    }

    // Hand-lettered buttons: import every PNG in Art/UI/Buttons as a crisp sprite.
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ButtonArtRoot)) return;
        var imp = (TextureImporter)assetImporter;
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
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
        lib.commissions = Find<CommissionDef>();
        lib.skills = Find<SkillDef>();
        lib.cosmetics = Find<CosmeticDef>();
        lib.decor = Find<DecorDef>();
        lib.chapters = Find<ChapterDef>();
        lib.upgrades = Find<UpgradeDef>();
        lib.quests = Find<QuestDef>();
        lib.dialogue = Find<TextAsset>(DialogueRoot);
        lib.buttonArt = Find<Sprite>(ButtonArtRoot);
        var tutorials = Find<TutorialDef>();
        lib.tutorial = tutorials.Length > 0 ? tutorials[0] : null;
        EditorUtility.SetDirty(lib);
        AssetDatabase.SaveAssets();
        return lib;
    }

    static T[] Find<T>(string root = DataRoot) where T : Object
    {
        var paths = new List<string>();
        if (!AssetDatabase.IsValidFolder(root)) return new T[0];
        foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { root }))
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
