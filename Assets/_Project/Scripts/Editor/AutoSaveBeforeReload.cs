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
            if (!EditorApplication.isPlaying)
                EditorSceneManager.SaveOpenScenes();
        };
    }
}
#endif
