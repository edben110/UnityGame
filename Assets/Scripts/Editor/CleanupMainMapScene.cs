using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CleanupMainMapScene
{
    [MenuItem("Tools/Limpiar Objetos Sueltos de MainMapScene")]
    public static void Cleanup()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        int removed = 0;

        // Buscar y eliminar BG_Lobby_Placeholder (fondo suelto que no controla RoomManager)
        GameObject placeholder = GameObject.Find("BG_Lobby_Placeholder");
        if (placeholder != null)
        {
            Object.DestroyImmediate(placeholder);
            removed++;
            Debug.Log("Eliminado: BG_Lobby_Placeholder");
        }

        // Buscar cualquier otro objeto con "BG_" que esté suelto (no hijo de Backgrounds)
        GameObject bgParent = GameObject.Find("Backgrounds");
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            if (!obj.name.StartsWith("BG_")) continue;
            if (bgParent != null && obj.transform.IsChildOf(bgParent.transform)) continue;
            if (obj.transform.parent != null && obj.transform.parent.name == "Backgrounds") continue;

            // Es un BG_ suelto fuera de Backgrounds
            Debug.Log($"Eliminado objeto suelto: {obj.name} (padre: {(obj.transform.parent != null ? obj.transform.parent.name : "root")})");
            Object.DestroyImmediate(obj);
            removed++;
        }

        // Verificar que los fondos dentro de Backgrounds estén en posición (0,0,0)
        if (bgParent != null)
        {
            for (int i = 0; i < bgParent.transform.childCount; i++)
            {
                Transform child = bgParent.transform.GetChild(i);
                if (child.localPosition != Vector3.zero)
                {
                    Debug.Log($"Corregida posición de {child.name}: {child.localPosition} -> (0,0,0)");
                    child.localPosition = Vector3.zero;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Limpieza Completa",
            $"Se eliminaron {removed} objetos sueltos.\n" +
            "Fondos verificados en posición (0,0,0).\n\n" +
            "Dale Play para probar.",
            "OK");
    }
}
