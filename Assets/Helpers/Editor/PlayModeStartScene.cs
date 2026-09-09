#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Editor Play Mode uses the currently open scene, not Build Settings.
/// If that scene is untitled/empty, Game view is black and only
/// RuntimeInitializeOnLoadMethod scripts (like AOTPreserveHelper) run.
/// Force Play to start from the first enabled Build Settings scene.
/// </summary>
[InitializeOnLoad]
public static class PlayModeStartScene
{
    static PlayModeStartScene()
    {
        SetPlayModeStartScene();
        EditorBuildSettings.sceneListChanged += SetPlayModeStartScene;
    }

    private static void SetPlayModeStartScene()
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled || string.IsNullOrEmpty(scene.path))
                continue;

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            if (sceneAsset == null)
                continue;

            if (EditorSceneManager.playModeStartScene != sceneAsset)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
                Debug.Log($"[StartupDiag] Play Mode start scene set to '{scene.path}'.");
            }
            return;
        }

        EditorSceneManager.playModeStartScene = null;
        Debug.LogWarning("[StartupDiag] No enabled scene in Build Settings. Play Mode will use the currently open scene.");
    }
}
#endif
