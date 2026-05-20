using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Crea los 9 botones de contraseña (DigitButton) bajo BG_SecureDoor y los enlaza a Door_ToSecurityRoom.
/// Menú: Tools/Puzzle/Configurar botones puerta seguridad
/// </summary>
public static class SetupSecurityPasswordButtons
{
    private const string MainMapScenePath = "Assets/Scenes/MainMapScene.unity";
    private const string KeypadParentName = "SecurityPasswordKeypad";
    private static readonly Vector2 HitboxSize = new Vector2(0.3f, 0.3f);
    private static readonly Vector2[] DefaultDigitPositions =
    {
        new Vector2(4.27f, 0.35f),
        new Vector2(4.67f, 0.35f),
        new Vector2(5.1f, 0.35f),
        new Vector2(4.27f, -0.01f),
        new Vector2(4.67f, -0.01f),
        new Vector2(5.1f, -0.01f),
        new Vector2(4.27f, -0.4f),
        new Vector2(4.67f, -0.4f),
        new Vector2(5.1f, -0.4f),
    };

    [MenuItem("Tools/Puzzle/Configurar botones puerta seguridad")]
    public static void Configure()
    {
        var scene = EditorSceneManager.OpenScene(MainMapScenePath, OpenSceneMode.Single);

        GameObject door = GameObject.Find("Door_ToSecurityRoom");
        if (door == null)
        {
            Debug.LogError("SetupSecurityPasswordButtons: no se encontró Door_ToSecurityRoom.");
            return;
        }

        NumericPasswordPanel panel = door.GetComponent<NumericPasswordPanel>();
        DoorTrigger doorTrigger = door.GetComponent<DoorTrigger>();
        if (panel == null)
        {
            Debug.LogError("SetupSecurityPasswordButtons: Door_ToSecurityRoom no tiene NumericPasswordPanel.");
            return;
        }

        GameObject secureBg = GameObject.Find("BG_SecureDoor");
        if (secureBg == null)
        {
            Debug.LogError("SetupSecurityPasswordButtons: no se encontró BG_SecureDoor.");
            return;
        }

        Transform keypadParent = secureBg.transform.Find(KeypadParentName);
        if (keypadParent == null)
        {
            GameObject keypadRoot = new GameObject(KeypadParentName);
            keypadRoot.transform.SetParent(secureBg.transform, false);
            keypadRoot.transform.localPosition = Vector3.zero;
            keypadRoot.transform.localRotation = Quaternion.identity;
            keypadRoot.transform.localScale = Vector3.one;
            keypadParent = keypadRoot.transform;
        }

        SerializedObject panelSo = new SerializedObject(panel);
        Vector2[] positions = DefaultDigitPositions;

        List<DigitButton> buttons = new List<DigitButton>(9);
        for (int digit = 1; digit <= 9; digit++)
        {
            string childName = $"DigitButton_{digit}";
            Transform existing = keypadParent.Find(childName);
            GameObject buttonObj = existing != null ? existing.gameObject : new GameObject(childName);
            if (existing == null)
            {
                buttonObj.transform.SetParent(keypadParent, false);
            }

            Vector2 pos = digit - 1 < positions.Length ? positions[digit - 1] : Vector2.zero;
            buttonObj.transform.localPosition = new Vector3(pos.x, pos.y, 0f);

            BoxCollider2D col = buttonObj.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = buttonObj.AddComponent<BoxCollider2D>();
            }

            col.size = HitboxSize;
            col.isTrigger = false;
            col.enabled = false;

            DigitButton digitButton = buttonObj.GetComponent<DigitButton>();
            if (digitButton == null)
            {
                digitButton = buttonObj.AddComponent<DigitButton>();
            }

            SerializedObject digitSo = new SerializedObject(digitButton);
            digitSo.FindProperty("digitValue").intValue = digit;
            digitSo.FindProperty("parentPanel").objectReferenceValue = panel;
            digitSo.ApplyModifiedPropertiesWithoutUndo();

            buttons.Add(digitButton);
        }

        SerializedProperty buttonsProp = panelSo.FindProperty("sceneDigitButtons");
        buttonsProp.arraySize = buttons.Count;
        for (int i = 0; i < buttons.Count; i++)
        {
            buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
        }

        panelSo.ApplyModifiedPropertiesWithoutUndo();

        if (doorTrigger != null)
        {
            SerializedObject doorSo = new SerializedObject(doorTrigger);
            doorSo.FindProperty("securityPasswordPanel").objectReferenceValue = panel;
            doorSo.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog(
            "Puerta de seguridad",
            "Listo:\n" +
            "- 9 DigitButton bajo BG_SecureDoor/SecurityPasswordKeypad\n" +
            "- parentPanel enlazado a Door_ToSecurityRoom\n" +
            "- DoorTrigger.securityPasswordPanel enlazado\n\n" +
            "Contraseña: 4-7-2-9 (valida tras 4 clics).",
            "OK");
    }

}
