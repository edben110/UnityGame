using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController
{
    public bool TryLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneController recibio un nombre de escena vacio.");
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Escena no registrada en Build Settings: {sceneName}");
            return false;
        }

        SceneManager.LoadScene(sceneName);
        return true;
    }
}
