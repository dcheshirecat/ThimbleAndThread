#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AutoSaveBeforeReload
{
    static AutoSaveBeforeReload()
    {
        AssemblyReloadEvents.beforeAssemblyReload += () =>
        {
            if (EditorApplication.isPlaying) return;
            // Only save scenes that already have a file; an untitled scene would open a Save As dialog.
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if (scene.isDirty && !string.IsNullOrEmpty(scene.path))
                    EditorSceneManager.SaveScene(scene);
            }
        };
    }
}
#endif
