using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ConfigureStudioDoor
{
    [MenuItem("Tools/Configurar Puerta del Estudio (Checkpoint Cap 2)")]
    public static void Configure()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Buscar la puerta al estudio
        DoorTrigger[] allDoors = Object.FindObjectsByType<DoorTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        DoorTrigger studioDoor = null;

        foreach (DoorTrigger door in allDoors)
        {
            SerializedObject so = new SerializedObject(door);
            var targetProp = so.FindProperty("targetRoomId");
            if (targetProp != null && (targetProp.stringValue == "estudio" || targetProp.stringValue == "studio"))
            {
                studioDoor = door;
                break;
            }
        }

        if (studioDoor == null)
        {
            // Buscar por nombre
            GameObject doorObj = GameObject.Find("Door_lobby_to_estudio");
            if (doorObj == null)
            {
                doorObj = GameObject.Find("Door_ToStudio");
            }

            if (doorObj != null)
            {
                studioDoor = doorObj.GetComponent<DoorTrigger>();
            }
        }

        if (studioDoor == null)
        {
            Debug.LogError("No se encontró la puerta al estudio en la escena.");
            EditorUtility.DisplayDialog("Error", "No se encontró la puerta al estudio.", "OK");
            return;
        }

        // Configurar la puerta como checkpoint narrativo
        SerializedObject doorSO = new SerializedObject(studioDoor);

        // requiredNpcTalkCount = 1 (mínimo 1 NPC interrogado)
        var npcCountProp = doorSO.FindProperty("requiredNpcTalkCount");
        if (npcCountProp != null)
        {
            npcCountProp.intValue = 1;
        }

        // triggersChapterTransition = true
        var transitionProp = doorSO.FindProperty("triggersChapterTransition");
        if (transitionProp != null)
        {
            transitionProp.boolValue = true;
        }

        // transitionToChapterId = "chapter2"
        var chapterProp = doorSO.FindProperty("transitionToChapterId");
        if (chapterProp != null)
        {
            chapterProp.stringValue = "chapter2";
        }

        doorSO.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"[ConfigureStudioDoor] Puerta '{studioDoor.gameObject.name}' configurada:");
        Debug.Log($"  requiredNpcTalkCount: 1");
        Debug.Log($"  triggersChapterTransition: true");
        Debug.Log($"  transitionToChapterId: chapter2");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Puerta Configurada",
            $"Puerta '{studioDoor.gameObject.name}' configurada como checkpoint:\n\n" +
            "✓ Requiere mínimo 1 NPC interrogado\n" +
            "✓ Dispara transición a Capítulo 2\n" +
            "✓ Si TODOS los NPC fueron interrogados, muestra decisión primero\n\n" +
            "Reglas:\n" +
            "• 0 NPC hablados → puerta BLOQUEADA\n" +
            "• 1+ NPC hablados + llave → acceso + Cap 2\n" +
            "• 5 NPC hablados + llave → decisión + Cap 2",
            "OK");
    }
}
