using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Recuerda la escena activa antes de abrir un acertijo aditivo y la restaura al cerrar.
/// </summary>
public static class PuzzleReturnContext
{
    private static string returnSceneName;

    public static void RememberCurrentScene()
    {
        Scene active = SceneManager.GetActiveScene();
        if (!active.IsValid())
        {
            return;
        }

        string name = active.name;
        if (name == AcertijoPuzzleService.AcertijoSceneName || name == Acertijo2PuzzleService.Acertijo2SceneName)
        {
            return;
        }

        returnSceneName = name;
    }

    public static void RestoreReturnScene()
    {
        if (string.IsNullOrWhiteSpace(returnSceneName))
        {
            return;
        }

        Scene returnScene = SceneManager.GetSceneByName(returnSceneName);
        if (returnScene.IsValid() && returnScene.isLoaded)
        {
            SceneManager.SetActiveScene(returnScene);
        }

        returnSceneName = null;
    }
}
