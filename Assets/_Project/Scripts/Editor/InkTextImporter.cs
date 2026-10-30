#if UNITY_EDITOR
using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

/// Imports .ink dialogue files as plain text, for InkLite to read at runtime.
/// (If the official Ink Unity package is ever added, delete this: that package imports .ink files itself.)
[ScriptedImporter(1, "ink")]
public class InkTextImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var text = new TextAsset(File.ReadAllText(ctx.assetPath));
        ctx.AddObjectToAsset("text", text);
        ctx.SetMainObject(text);
    }
}
#endif
