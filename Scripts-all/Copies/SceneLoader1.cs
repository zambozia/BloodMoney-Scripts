using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public static class SceneLoader
{
    /// <summary>
    /// Loads a scene by name.
    /// </summary>
    /// <param name="sceneName">The exact name of the scene as added in Build Settings.</param>
    public static void LoadScene(string sceneName)
    {
        if (IsSceneInBuildSettings(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            UnityEngine.Debug.LogError($"Scene '{sceneName}' is not listed in Build Settings. Please add it there.");
        }
    }

    /// <summary>
    /// Reloads the currently active scene.
    /// </summary>
    public static void ReloadCurrentScene()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    /// <summary>
    /// Quits the game (has no effect in editor).
    /// </summary>
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }

    /// <summary>
    /// Checks if a scene is listed in the build settings.
    /// </summary>
    private static bool IsSceneInBuildSettings(string sceneName)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name == sceneName)
                return true;
        }
        return false;
    }
}
