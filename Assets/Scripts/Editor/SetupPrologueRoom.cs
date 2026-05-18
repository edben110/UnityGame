using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to add a "prologue" room to the RoomManager.
/// Run via menu: Tools > Setup Prologue Room
/// 
/// This creates a BG_Prologue background object and registers it as a room
/// in the RoomManager so the prologue plays on its own background.
/// </summary>
public static class SetupPrologueRoom
{
    [MenuItem("Tools/Setup Prologue Room")]
    public static void Execute()
    {
        // Find or create the Backgrounds parent
        GameObject backgroundsParent = GameObject.Find("Backgrounds");
        if (backgroundsParent == null)
        {
            backgroundsParent = new GameObject("Backgrounds");
            Debug.Log("[SetupPrologueRoom] Created Backgrounds parent.");
        }

        // Find or create BG_Prologue
        Transform existingBg = backgroundsParent.transform.Find("BG_Prologue");
        GameObject bgPrologue;
        if (existingBg != null)
        {
            bgPrologue = existingBg.gameObject;
            Debug.Log("[SetupPrologueRoom] BG_Prologue already exists.");
        }
        else
        {
            bgPrologue = new GameObject("BG_Prologue");
            bgPrologue.transform.SetParent(backgroundsParent.transform, false);
            bgPrologue.transform.localPosition = new Vector3(0f, 100f, 0f); // Offset to not overlap with lobby

            // Add a SpriteRenderer placeholder
            SpriteRenderer sr = bgPrologue.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -10;

            // Try to load the Prologo sprite if it exists
            Sprite prologueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prologo.png");
            if (prologueSprite != null)
            {
                sr.sprite = prologueSprite;
                Debug.Log("[SetupPrologueRoom] Assigned Prologo.png sprite to BG_Prologue.");
            }
            else
            {
                Debug.LogWarning("[SetupPrologueRoom] No Prologo.png found. Assign a sprite manually to BG_Prologue.");
            }

            Debug.Log("[SetupPrologueRoom] Created BG_Prologue.");
        }

        // Find RoomManager and add the prologue room
        RoomManager roomManager = Object.FindAnyObjectByType<RoomManager>();
        if (roomManager == null)
        {
            Debug.LogError("[SetupPrologueRoom] No RoomManager found in scene!");
            return;
        }

        SerializedObject rmSO = new SerializedObject(roomManager);
        SerializedProperty roomsProp = rmSO.FindProperty("rooms");

        // Check if prologue room already exists
        bool alreadyExists = false;
        for (int i = 0; i < roomsProp.arraySize; i++)
        {
            SerializedProperty element = roomsProp.GetArrayElementAtIndex(i);
            string roomId = element.FindPropertyRelative("roomId").stringValue;
            if (roomId == "prologue")
            {
                alreadyExists = true;
                // Update the background reference
                element.FindPropertyRelative("backgroundObject").objectReferenceValue = bgPrologue;
                Debug.Log("[SetupPrologueRoom] Updated existing prologue room entry.");
                break;
            }
        }

        if (!alreadyExists)
        {
            // Insert at position 0 (before lobby)
            roomsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newElement = roomsProp.GetArrayElementAtIndex(0);
            newElement.FindPropertyRelative("roomId").stringValue = "prologue";
            newElement.FindPropertyRelative("displayName").stringValue = "Prólogo";
            newElement.FindPropertyRelative("backgroundObject").objectReferenceValue = bgPrologue;
            newElement.FindPropertyRelative("hotspotsContainer").objectReferenceValue = null;
            newElement.FindPropertyRelative("cameraObject").objectReferenceValue = null;
            Debug.Log("[SetupPrologueRoom] Added prologue room to RoomManager.");
        }

        // Update startingRoomId to "prologue"
        SerializedProperty startProp = rmSO.FindProperty("startingRoomId");
        if (startProp != null)
        {
            startProp.stringValue = "prologue";
            Debug.Log("[SetupPrologueRoom] Set startingRoomId to 'prologue'.");
        }

        rmSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(roomManager);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[SetupPrologueRoom] ✓ Prologue room setup complete. Save the scene (Ctrl+S).");
        EditorUtility.DisplayDialog("Setup Prologue Room",
            "Prologue room configured successfully.\n\n" +
            "• BG_Prologue created/updated\n" +
            "• RoomManager updated with 'prologue' room\n" +
            "• startingRoomId set to 'prologue'\n\n" +
            "Remember to save the scene (Ctrl+S).",
            "OK");
    }
}
