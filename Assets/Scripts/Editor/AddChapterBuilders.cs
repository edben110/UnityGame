using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AddChapterBuilders
{
    [MenuItem("Tools/Agregar Builders de Capítulos 2 y 3")]
    public static void AddBuilders()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Buscar el DialogueSystem
        GameObject dialogueSystem = GameObject.Find("DialogueSystem");
        if (dialogueSystem == null)
        {
            Debug.LogError("No se encontró DialogueSystem en la escena.");
            return;
        }

        // Buscar la DialogueLibrary en el mismo objeto
        DialogueLibrary library = dialogueSystem.GetComponent<DialogueLibrary>();
        if (library == null)
        {
            Debug.LogError("No se encontró DialogueLibrary en DialogueSystem.");
            return;
        }

        int added = 0;

        // Agregar Chapter2Builder si no existe
        var ch2 = dialogueSystem.GetComponent<Chapter2Builder>();
        if (ch2 == null)
        {
            ch2 = dialogueSystem.AddComponent<Chapter2Builder>();
            // Configurar la referencia a la library via SerializedObject
            SerializedObject so = new SerializedObject(ch2);
            var libProp = so.FindProperty("targetLibrary");
            if (libProp != null)
            {
                libProp.objectReferenceValue = library;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            added++;
            Debug.Log("Chapter2Builder agregado al DialogueSystem.");
        }
        else
        {
            Debug.Log("Chapter2Builder ya existe.");
        }

        // Agregar Chapter3Builder si no existe
        var ch3 = dialogueSystem.GetComponent<Chapter3Builder>();
        if (ch3 == null)
        {
            ch3 = dialogueSystem.AddComponent<Chapter3Builder>();
            SerializedObject so = new SerializedObject(ch3);
            var libProp = so.FindProperty("targetLibrary");
            if (libProp != null)
            {
                libProp.objectReferenceValue = library;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            added++;
            Debug.Log("Chapter3Builder agregado al DialogueSystem.");
        }
        else
        {
            Debug.Log("Chapter3Builder ya existe.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Builders Agregados",
            $"Se agregaron {added} builders al DialogueSystem.\n\n" +
            "Al darle Play, los diálogos de Cap 2 y 3 se generarán\n" +
            "automáticamente cuando el jugador llegue a esos capítulos.",
            "OK");
    }
}
