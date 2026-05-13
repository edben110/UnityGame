using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AssignKeySprite
{
    [MenuItem("Tools/Asignar Sprite de Llaves a Door_ToStudio")]
    public static void Assign()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject door = GameObject.Find("Key_StudioKey");
        if (door == null)
        {
            // Fallback al nombre anterior
            door = GameObject.Find("Door_ToStudio");
        }
        if (door == null)
        {
            Debug.LogError("No se encontró Key_StudioKey ni Door_ToStudio en la escena.");
            EditorUtility.DisplayDialog("Error", "No se encontró el objeto de la llave.", "OK");
            return;
        }

        string spritePath = "Assets/Sprites/llaves.png";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            Debug.LogError($"No se encontró el sprite: {spritePath}");
            EditorUtility.DisplayDialog("Error", "No se encontró llaves.png en Assets/Sprites/.", "OK");
            return;
        }

        SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = door.AddComponent<SpriteRenderer>();
        }

        sr.sprite = sprite;
        sr.sortingOrder = 50; // Muy por encima del fondo

        // No cambiar la escala si ya tiene un tamaño definido por el usuario
        // Solo asignar el sprite
        Debug.Log($"Sprite asignado. SortingOrder: {sr.sortingOrder}, Scale: {door.transform.localScale}");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Sprite 'llaves.png' asignado a Door_ToStudio.");
        EditorUtility.DisplayDialog("Listo", "Sprite de llaves asignado a Door_ToStudio.", "OK");
    }
}
