using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool extensible para configurar escenas narrativas.
/// 
/// CAPACIDADES:
/// 1. Crear/actualizar hotspots con diálogos, items, y configuración de capítulo
/// 2. Crear/actualizar puertas con llaves requeridas y transiciones
/// 3. Añadir sistemas singleton (NpcDeliverySystem, Chapter5ValidationGate)
/// 4. Actualizar diálogos de objetos existentes sin recrearlos
/// 5. Verificar configuración actual de la escena
///
/// USO FUTURO:
/// - Para añadir nuevos hotspots: agregar entradas al array correspondiente
/// - Para actualizar diálogos: usar UpdateDialogueForHotspot() o el menú
/// - Para nuevas puertas: agregar entradas a doorsToCreate
/// 
/// MENÚ: Tools > Narrativa > [opciones]
/// </summary>
public class SceneNarrativeConfigurator : EditorWindow
{
    private Vector2 scrollPos;
    private string targetScene = "Assets/Scenes/MainMapScene.unity";
    private bool showHotspots = true;
    private bool showDoors = true;
    private bool showSystems = true;
    private bool showDialogueUpdater = true;

    // ═══════════════════════════════════════════════════════════════
    //  DATOS DE CONFIGURACIÓN — HOTSPOTS
    // ═══════════════════════════════════════════════════════════════

    [Serializable]
    private struct HotspotConfig
    {
        public string id;
        public string chapter;
        public string conversationId;
        public string spriteName;
        public float x;
        public float y;
        public bool consumeAfterUse;
        public bool hideAfterPickup;
        public string grantItemId;
        public string grantItemDisplayName;
        public string grantItemDescription;
        public string setFlagOnInteract;
        public string parentName;
    }

    private static readonly HotspotConfig[] chapter4Hotspots = new HotspotConfig[]
    {
        new HotspotConfig
        {
            id = "chapter4_mirilla_central",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_mirilla_central",
            spriteName = null,
            x = 0.0f, y = 2.0f,
            consumeAfterUse = false,
            hideAfterPickup = false,
            grantItemId = "",
            grantItemDisplayName = "",
            grantItemDescription = "",
            setFlagOnInteract = "chapter4.mirilla.seen",
            parentName = "HotsPots"
        },
        new HotspotConfig
        {
            id = "chapter4_maletin_negro",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_maletin_negro",
            spriteName = null,
            x = 1.5f, y = -1.5f,
            consumeAfterUse = true,
            hideAfterPickup = true,
            grantItemId = "relicario_lucas",
            grantItemDisplayName = "Relicario de Lucas",
            grantItemDescription = "Un relicario de plata con la inscripcion: 'Para Lucas. Siempre.'",
            setFlagOnInteract = "chapter4.maletin.opened",
            parentName = "HotsPots"
        },
        new HotspotConfig
        {
            id = "chapter4_caja_puzzle_llave",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_caja_puzzle_llave",
            spriteName = null,
            x = -2.0f, y = -1.0f,
            consumeAfterUse = true,
            hideAfterPickup = true,
            grantItemId = "SingleUseKey",
            grantItemDisplayName = "Llave Desgastada",
            grantItemDescription = "Una llave vieja y desgastada. Solo resistira un uso mas.",
            setFlagOnInteract = "chapter4.single_use_key.found",
            parentName = "HotsPots"
        },
        new HotspotConfig
        {
            id = "chapter4_cilindros_fonografo",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_cilindros",
            spriteName = null,
            x = -1.5f, y = -0.5f,
            consumeAfterUse = false,
            hideAfterPickup = false,
            grantItemId = "",
            grantItemDisplayName = "",
            grantItemDescription = "",
            setFlagOnInteract = "chapter4.cilindros.heard",
            parentName = "HotsPots"
        },
        new HotspotConfig
        {
            id = "chapter4_nota_roja",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_nota_roja",
            spriteName = null,
            x = 3.0f, y = 1.5f,
            consumeAfterUse = false,
            hideAfterPickup = false,
            grantItemId = "",
            grantItemDisplayName = "",
            grantItemDescription = "",
            setFlagOnInteract = "chapter4.nota_roja.read",
            parentName = "HotsPots"
        },
        new HotspotConfig
        {
            id = "chapter4_mapa_actualizado",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_mapa",
            spriteName = null,
            x = 2.5f, y = 1.0f,
            consumeAfterUse = false,
            hideAfterPickup = false,
            grantItemId = "",
            grantItemDisplayName = "",
            grantItemDescription = "",
            setFlagOnInteract = "chapter4.mapa.seen",
            parentName = "HotsPots"
        },
        new HotspotConfig
        {
            id = "chapter4_diario_final",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_diario_final",
            spriteName = null,
            x = -3.0f, y = 0.5f,
            consumeAfterUse = true,
            hideAfterPickup = true,
            grantItemId = "diario_final",
            grantItemDisplayName = "Diario de Investigacion",
            grantItemDescription = "El diario final de Simon. Contiene informacion sobre su investigacion secreta.",
            setFlagOnInteract = "chapter4.diario.found",
            parentName = "HotsPots"
        },
        new HotspotConfig
        {
            id = "chapter4_maquinaria",
            chapter = "chapter4",
            conversationId = "chapter4_hotspot_maquinaria",
            spriteName = null,
            x = 3.5f, y = -0.5f,
            consumeAfterUse = false,
            hideAfterPickup = false,
            grantItemId = "",
            grantItemDisplayName = "",
            grantItemDescription = "",
            setFlagOnInteract = "",
            parentName = "HotsPots"
        }
    };

    // ═══════════════════════════════════════════════════════════════
    //  DATOS DE CONFIGURACIÓN — PUERTAS
    // ═══════════════════════════════════════════════════════════════

    [Serializable]
    private struct DoorConfig
    {
        public string name;
        public string targetRoomId;
        public string requiredChapterId;
        public string requiredFlag;
        public KeyType[] requiredKeys;
        public int requiredNpcTalkCount;
        public bool triggersChapterTransition;
        public string transitionToChapterId;
        public float x;
        public float y;
        public float sizeX;
        public float sizeY;
        public string lockedMessage;
        public string parentName;
    }

    private static readonly DoorConfig[] chapter5Doors = new DoorConfig[]
    {
        new DoorConfig
        {
            name = "Door_ToNorthStreet",
            targetRoomId = "ala_norte",
            requiredChapterId = "chapter4",
            requiredFlag = "",
            requiredKeys = new KeyType[] { KeyType.SingleUseKey },
            requiredNpcTalkCount = 1,
            triggersChapterTransition = true,
            transitionToChapterId = "chapter5",
            x = 0.0f, y = 3.0f,
            sizeX = 2.0f, sizeY = 1.5f,
            lockedMessage = "La puerta al Ala Norte esta cerrada. Necesito la llave desgastada.",
            parentName = "Doors"
        },
        new DoorConfig
        {
            name = "Door_ToEmptyRoom",
            targetRoomId = "empty_room",
            requiredChapterId = "chapter5",
            requiredFlag = "",
            requiredKeys = new KeyType[] { KeyType.SingleUseKey },
            requiredNpcTalkCount = 0,
            triggersChapterTransition = false,
            transitionToChapterId = "",
            x = -3.0f, y = 0.0f,
            sizeX = 1.5f, sizeY = 3.0f,
            lockedMessage = "Esta puerta esta cerrada con llave.",
            parentName = "Doors"
        },
        new DoorConfig
        {
            name = "Door_ToKidnappedSimon",
            targetRoomId = "simon_captive",
            requiredChapterId = "chapter5",
            requiredFlag = "",
            requiredKeys = new KeyType[] { KeyType.SingleUseKey },
            requiredNpcTalkCount = 0,
            triggersChapterTransition = false,
            transitionToChapterId = "",
            x = 0.0f, y = 0.0f,
            sizeX = 1.5f, sizeY = 3.0f,
            lockedMessage = "Esta puerta esta cerrada con llave.",
            parentName = "Doors"
        },
        new DoorConfig
        {
            name = "Door_ToKillerBunker",
            targetRoomId = "killer_bunker",
            requiredChapterId = "chapter5",
            requiredFlag = "",
            requiredKeys = new KeyType[] { KeyType.SingleUseKey },
            requiredNpcTalkCount = 0,
            triggersChapterTransition = false,
            transitionToChapterId = "",
            x = 3.0f, y = 0.0f,
            sizeX = 1.5f, sizeY = 3.0f,
            lockedMessage = "Esta puerta esta cerrada con llave.",
            parentName = "Doors"
        }
    };

    // ═══════════════════════════════════════════════════════════════
    //  MENÚ PRINCIPAL
    // ═══════════════════════════════════════════════════════════════

    [MenuItem("Tools/Narrativa/Configurador de Escena")]
    public static void ShowWindow()
    {
        GetWindow<SceneNarrativeConfigurator>("Configurador Narrativo");
    }

    [MenuItem("Tools/Narrativa/Configurar Cap 4 + Cap 5 (Completo)")]
    public static void ConfigureChapter4And5Full()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        int hotspotsCreated = CreateOrUpdateHotspots(chapter4Hotspots);
        int doorsCreated = CreateOrUpdateDoors(chapter5Doors);
        EnsureSingletonSystems();
        EnsureSecurityRoomDoor();
        EnsureEpilogueSystem();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Configuracion Completa",
            $"Hotspots Cap 4: {hotspotsCreated} creados/actualizados\n" +
            $"Puertas Cap 5: {doorsCreated} creadas/actualizadas\n" +
            $"Sistemas singleton: verificados\n" +
            $"Door_ToSecurityRoom + NumericPasswordPanel: verificados\n" +
            $"EpilogueSystem + EpilogueBuilder: verificados\n\n" +
            "Hotspots creados:\n" +
            "- Mirilla Central\n" +
            "- Maletin Negro (relicario Lucas)\n" +
            "- Caja Puzzle (SingleUseKey)\n" +
            "- Cilindros Fonografo\n" +
            "- Nota Roja\n" +
            "- Mapa Actualizado\n" +
            "- Diario Final\n" +
            "- Maquinaria\n\n" +
            "Puertas creadas:\n" +
            "- Door_ToSecurityRoom (contraseña 4-7-2-9)\n" +
            "- Door_ToNorthStreet (Cap 4 -> Cap 5)\n" +
            "- Door_ToEmptyRoom (Cap 5)\n" +
            "- Door_ToKidnappedSimon (Cap 5)\n" +
            "- Door_ToKillerBunker (Cap 5)",
            "OK");
    }

    [MenuItem("Tools/Narrativa/Verificar Configuracion Actual")]
    public static void VerifyCurrentConfiguration()
    {
        string report = GenerateVerificationReport();
        Debug.Log(report);
        EditorUtility.DisplayDialog("Verificacion de Escena", report, "OK");
    }

    [MenuItem("Tools/Narrativa/Actualizar Dialogos de Hotspots")]
    public static void UpdateAllHotspotDialogues()
    {
        int updated = UpdateDialoguesForAllHotspots(chapter4Hotspots);
        EditorUtility.DisplayDialog("Dialogos Actualizados",
            $"Hotspots actualizados: {updated}\n\n" +
            "Los conversationId han sido sincronizados con la configuracion actual.",
            "OK");
    }

    // ═══════════════════════════════════════════════════════════════
    //  VENTANA DEL EDITOR (GUI)
    // ═══════════════════════════════════════════════════════════════

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("Configurador Narrativo de Escena", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetScene = EditorGUILayout.TextField("Escena objetivo:", targetScene);
        EditorGUILayout.Space();

        // --- Sección Hotspots ---
        showHotspots = EditorGUILayout.Foldout(showHotspots, "Hotspots Cap 4 (Sotano/Sala Seguridad)");
        if (showHotspots)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < chapter4Hotspots.Length; i++)
            {
                var hs = chapter4Hotspots[i];
                bool exists = FindHotspotInScene(hs.id) != null;
                string status = exists ? "[OK]" : "[FALTA]";
                EditorGUILayout.LabelField($"{status} {hs.id}", exists ? EditorStyles.label : EditorStyles.boldLabel);
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Crear/Actualizar Hotspots Cap 4"))
            {
                var scene = EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Single);
                int count = CreateOrUpdateHotspots(chapter4Hotspots);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Configurador] {count} hotspots creados/actualizados.");
            }
        }

        EditorGUILayout.Space();

        // --- Sección Puertas ---
        showDoors = EditorGUILayout.Foldout(showDoors, "Puertas Cap 5 (Ala Norte)");
        if (showDoors)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < chapter5Doors.Length; i++)
            {
                var door = chapter5Doors[i];
                bool exists = FindInScene(door.name) != null;
                string status = exists ? "[OK]" : "[FALTA]";
                string keysStr = door.requiredKeys != null ? string.Join(", ", door.requiredKeys) : "ninguna";
                EditorGUILayout.LabelField($"{status} {door.name} (keys: {keysStr})", exists ? EditorStyles.label : EditorStyles.boldLabel);
            }
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Crear/Actualizar Puertas Cap 5"))
            {
                var scene = EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Single);
                int count = CreateOrUpdateDoors(chapter5Doors);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Configurador] {count} puertas creadas/actualizadas.");
            }
        }

        EditorGUILayout.Space();

        // --- Sección Sistemas ---
        showSystems = EditorGUILayout.Foldout(showSystems, "Sistemas Singleton");
        if (showSystems)
        {
            EditorGUI.indentLevel++;
            bool hasDelivery = UnityEngine.Object.FindAnyObjectByType<NpcDeliverySystem>() != null;
            bool hasGate = UnityEngine.Object.FindAnyObjectByType<Chapter5ValidationGate>() != null;
            EditorGUILayout.LabelField($"{(hasDelivery ? "[OK]" : "[FALTA]")} NpcDeliverySystem");
            EditorGUILayout.LabelField($"{(hasGate ? "[OK]" : "[FALTA]")} Chapter5ValidationGate");
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Asegurar Sistemas Singleton"))
            {
                EnsureSingletonSystems();
                Debug.Log("[Configurador] Sistemas singleton verificados.");
            }
        }

        EditorGUILayout.Space();

        // --- Sección Actualizador de Diálogos ---
        showDialogueUpdater = EditorGUILayout.Foldout(showDialogueUpdater, "Actualizador de Dialogos");
        if (showDialogueUpdater)
        {
            EditorGUILayout.HelpBox(
                "Usa esta seccion para actualizar el conversationId de hotspots existentes " +
                "sin recrearlos. Util cuando cambias dialogos en los Builders.",
                MessageType.Info);

            if (GUILayout.Button("Actualizar Todos los Dialogos"))
            {
                int count = UpdateDialoguesForAllHotspots(chapter4Hotspots);
                Debug.Log($"[Configurador] {count} dialogos actualizados.");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actualizar hotspot individual:", EditorStyles.boldLabel);

            if (GUILayout.Button("Refrescar lista de hotspots en escena"))
            {
                LogAllHotspotsInScene();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // --- Botón principal ---
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
        if (GUILayout.Button("CONFIGURAR TODO (Cap 4 + Cap 5)", GUILayout.Height(40)))
        {
            ConfigureChapter4And5Full();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();

        GUI.backgroundColor = new Color(0.8f, 0.8f, 0.3f);
        if (GUILayout.Button("VERIFICAR CONFIGURACION", GUILayout.Height(30)))
        {
            VerifyCurrentConfiguration();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndScrollView();
    }

    // ═══════════════════════════════════════════════════════════════
    //  CREACIÓN DE HOTSPOTS
    // ═══════════════════════════════════════════════════════════════

    private static int CreateOrUpdateHotspots(HotspotConfig[] configs)
    {
        int count = 0;

        foreach (var config in configs)
        {
            GameObject existing = FindHotspotInScene(config.id);

            if (existing != null)
            {
                // Actualizar configuración existente
                UpdateHotspotConfig(existing, config);
                count++;
                Debug.Log($"[Configurador] Hotspot actualizado: {config.id}");
                continue;
            }

            // Crear nuevo
            GameObject parent = FindOrCreateParent(config.parentName);
            GameObject obj = new GameObject($"HS_{config.id}");
            obj.transform.SetParent(parent.transform);
            obj.transform.localPosition = new Vector3(config.x, config.y, 0f);

            // Collider
            var collider = obj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.2f, 1.2f);

            // Sprite (si existe)
            if (!string.IsNullOrWhiteSpace(config.spriteName))
            {
                Sprite sprite = LoadSprite(config.spriteName);
                if (sprite != null)
                {
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = 5;
                }
            }

            // MapHotspot component
            var hotspot = obj.AddComponent<MapHotspot>();
            UpdateHotspotConfig(obj, config);

            count++;
            Debug.Log($"[Configurador] Hotspot CREADO: {config.id} en ({config.x}, {config.y})");
        }

        return count;
    }

    private static void UpdateHotspotConfig(GameObject obj, HotspotConfig config)
    {
        var hotspot = obj.GetComponent<MapHotspot>();
        if (hotspot == null) return;

        SerializedObject so = new SerializedObject(hotspot);
        SetString(so, "hotspotId", config.id);
        SetString(so, "requiredChapterId", config.chapter);
        SetString(so, "conversationId", config.conversationId);
        SetString(so, "startNodeId", "start");
        SetBool(so, "consumeAfterUse", config.consumeAfterUse);
        SetBool(so, "hideAfterPickup", config.hideAfterPickup);
        SetBool(so, "showDebugMarkerInGame", string.IsNullOrWhiteSpace(config.spriteName));

        if (!string.IsNullOrWhiteSpace(config.grantItemId))
            SetString(so, "grantItemId", config.grantItemId);
        if (!string.IsNullOrWhiteSpace(config.grantItemDisplayName))
            SetString(so, "grantItemDisplayName", config.grantItemDisplayName);
        if (!string.IsNullOrWhiteSpace(config.grantItemDescription))
            SetString(so, "grantItemDescription", config.grantItemDescription);
        if (!string.IsNullOrWhiteSpace(config.setFlagOnInteract))
            SetString(so, "setFlagOnInteract", config.setFlagOnInteract);

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ═══════════════════════════════════════════════════════════════
    //  CREACIÓN DE PUERTAS
    // ═══════════════════════════════════════════════════════════════

    private static int CreateOrUpdateDoors(DoorConfig[] configs)
    {
        int count = 0;

        foreach (var config in configs)
        {
            GameObject existing = FindInScene(config.name);

            if (existing != null)
            {
                UpdateDoorConfig(existing, config);
                count++;
                Debug.Log($"[Configurador] Puerta actualizada: {config.name}");
                continue;
            }

            // Crear nueva
            GameObject parent = FindOrCreateParent(config.parentName);
            GameObject obj = new GameObject(config.name);
            obj.transform.SetParent(parent.transform);
            obj.transform.localPosition = new Vector3(config.x, config.y, 0f);

            // Collider
            var collider = obj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(config.sizeX, config.sizeY);

            // DoorTrigger component
            obj.AddComponent<DoorTrigger>();
            UpdateDoorConfig(obj, config);

            count++;
            Debug.Log($"[Configurador] Puerta CREADA: {config.name} -> {config.targetRoomId}");
        }

        return count;
    }

    private static void UpdateDoorConfig(GameObject obj, DoorConfig config)
    {
        var door = obj.GetComponent<DoorTrigger>();
        if (door == null) return;

        SerializedObject so = new SerializedObject(door);
        SetString(so, "targetRoomId", config.targetRoomId);
        SetString(so, "lockedMessage", config.lockedMessage);

        if (!string.IsNullOrWhiteSpace(config.requiredChapterId))
            SetString(so, "requiredChapterId", config.requiredChapterId);
        if (!string.IsNullOrWhiteSpace(config.requiredFlag))
            SetString(so, "requiredFlag", config.requiredFlag);

        SetInt(so, "requiredNpcTalkCount", config.requiredNpcTalkCount);
        SetBool(so, "triggersChapterTransition", config.triggersChapterTransition);

        if (!string.IsNullOrWhiteSpace(config.transitionToChapterId))
            SetString(so, "transitionToChapterId", config.transitionToChapterId);

        // Configurar requiredKeys array
        if (config.requiredKeys != null && config.requiredKeys.Length > 0)
        {
            var keysProp = so.FindProperty("requiredKeys");
            if (keysProp != null)
            {
                keysProp.arraySize = config.requiredKeys.Length;
                for (int i = 0; i < config.requiredKeys.Length; i++)
                {
                    keysProp.GetArrayElementAtIndex(i).enumValueIndex = (int)config.requiredKeys[i];
                }
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ═══════════════════════════════════════════════════════════════
    //  SISTEMAS SINGLETON
    // ═══════════════════════════════════════════════════════════════

    private static void EnsureSingletonSystems()
    {
        // NpcDeliverySystem
        if (UnityEngine.Object.FindAnyObjectByType<NpcDeliverySystem>() == null)
        {
            GameObject systemsParent = FindOrCreateParent("_Systems");
            GameObject deliveryObj = new GameObject("NpcDeliverySystem");
            deliveryObj.transform.SetParent(systemsParent.transform);
            deliveryObj.AddComponent<NpcDeliverySystem>();
            Debug.Log("[Configurador] NpcDeliverySystem CREADO.");
        }

        // Chapter5ValidationGate
        if (UnityEngine.Object.FindAnyObjectByType<Chapter5ValidationGate>() == null)
        {
            GameObject systemsParent = FindOrCreateParent("_Systems");
            GameObject gateObj = new GameObject("Chapter5ValidationGate");
            gateObj.transform.SetParent(systemsParent.transform);
            gateObj.AddComponent<Chapter5ValidationGate>();
            Debug.Log("[Configurador] Chapter5ValidationGate CREADO.");
        }

        // ProgressionTestHelper (debug)
        if (UnityEngine.Object.FindAnyObjectByType<ProgressionTestHelper>() == null)
        {
            GameObject systemsParent = FindOrCreateParent("_Systems");
            GameObject testObj = new GameObject("ProgressionTestHelper");
            testObj.transform.SetParent(systemsParent.transform);
            testObj.AddComponent<ProgressionTestHelper>();
            Debug.Log("[Configurador] ProgressionTestHelper CREADO.");
        }
    }

    /// <summary>
    /// Asegura que exista Door_ToSecurityRoom con NumericPasswordPanel.
    /// </summary>
    private static void EnsureSecurityRoomDoor()
    {
        // Buscar la puerta
        GameObject doorObj = FindInScene("Door_ToSecurityRoom");
        if (doorObj == null)
        {
            // Crear la puerta en el sótano
            GameObject parent = FindOrCreateParent("Doors");
            doorObj = new GameObject("Door_ToSecurityRoom");
            doorObj.transform.SetParent(parent.transform);
            doorObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            var col = doorObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.0f, 2.5f);

            doorObj.AddComponent<DoorTrigger>();
            Debug.Log("[Configurador] Door_ToSecurityRoom CREADA.");
        }

        // Configurar DoorTrigger
        var door = doorObj.GetComponent<DoorTrigger>();
        if (door == null)
        {
            door = doorObj.AddComponent<DoorTrigger>();
        }

        SerializedObject doorSo = new SerializedObject(door);
        SetString(doorSo, "targetRoomId", "security_room");
        SetString(doorSo, "requiredChapterId", "chapter4");
        SetString(doorSo, "lockedMessage", "La puerta tiene un teclado numerico. Necesito encontrar la combinacion correcta.");
        doorSo.ApplyModifiedPropertiesWithoutUndo();

        // Asegurar NumericPasswordPanel
        var passwordPanel = doorObj.GetComponent<NumericPasswordPanel>();
        if (passwordPanel == null)
        {
            passwordPanel = doorObj.AddComponent<NumericPasswordPanel>();
            Debug.Log("[Configurador] NumericPasswordPanel añadido a Door_ToSecurityRoom.");
        }
    }

    /// <summary>
    /// Asegura que existan EpilogueSystem y EpilogueBuilder.
    /// </summary>
    private static void EnsureEpilogueSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EpilogueSystem>() == null)
        {
            GameObject systemsParent = FindOrCreateParent("_Systems");
            GameObject epilogueObj = new GameObject("EpilogueSystem");
            epilogueObj.transform.SetParent(systemsParent.transform);
            epilogueObj.AddComponent<EpilogueSystem>();
            Debug.Log("[Configurador] EpilogueSystem CREADO.");
        }

        if (UnityEngine.Object.FindAnyObjectByType<EpilogueBuilder>() == null)
        {
            // Buscar DialogueLibrary existente para adjuntar el builder
            DialogueLibrary library = UnityEngine.Object.FindAnyObjectByType<DialogueLibrary>();
            if (library != null)
            {
                var builder = library.gameObject.GetComponent<EpilogueBuilder>();
                if (builder == null)
                {
                    library.gameObject.AddComponent<EpilogueBuilder>();
                    Debug.Log("[Configurador] EpilogueBuilder añadido al GameObject de DialogueLibrary.");
                }
            }
            else
            {
                GameObject systemsParent = FindOrCreateParent("_Systems");
                GameObject builderObj = new GameObject("EpilogueBuilder");
                builderObj.transform.SetParent(systemsParent.transform);
                builderObj.AddComponent<DialogueLibrary>();
                builderObj.AddComponent<EpilogueBuilder>();
                Debug.Log("[Configurador] EpilogueBuilder CREADO con DialogueLibrary propia.");
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  ACTUALIZACIÓN DE DIÁLOGOS (EXTENSIBLE)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Actualiza el conversationId de un hotspot existente sin recrearlo.
    /// Útil cuando se modifican los Builders y se quiere sincronizar la escena.
    /// </summary>
    public static bool UpdateDialogueForHotspot(string hotspotId, string newConversationId)
    {
        GameObject obj = FindHotspotInScene(hotspotId);
        if (obj == null)
        {
            Debug.LogWarning($"[Configurador] No se encontro hotspot con id '{hotspotId}' en la escena.");
            return false;
        }

        var hotspot = obj.GetComponent<MapHotspot>();
        if (hotspot == null) return false;

        SerializedObject so = new SerializedObject(hotspot);
        SetString(so, "conversationId", newConversationId);
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"[Configurador] Dialogo actualizado: {hotspotId} -> {newConversationId}");
        return true;
    }

    /// <summary>
    /// Actualiza el conversationId y el setFlagOnInteract de un hotspot.
    /// </summary>
    public static bool UpdateDialogueForHotspot(string hotspotId, string newConversationId, string newSetFlag)
    {
        GameObject obj = FindHotspotInScene(hotspotId);
        if (obj == null)
        {
            Debug.LogWarning($"[Configurador] No se encontro hotspot con id '{hotspotId}' en la escena.");
            return false;
        }

        var hotspot = obj.GetComponent<MapHotspot>();
        if (hotspot == null) return false;

        SerializedObject so = new SerializedObject(hotspot);
        SetString(so, "conversationId", newConversationId);
        if (!string.IsNullOrWhiteSpace(newSetFlag))
            SetString(so, "setFlagOnInteract", newSetFlag);
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"[Configurador] Dialogo actualizado: {hotspotId} -> {newConversationId} (flag: {newSetFlag})");
        return true;
    }

    /// <summary>
    /// Actualiza los diálogos de todos los hotspots en el array de configuración.
    /// Sincroniza la escena con los datos definidos en este script.
    /// </summary>
    private static int UpdateDialoguesForAllHotspots(HotspotConfig[] configs)
    {
        int updated = 0;
        foreach (var config in configs)
        {
            if (UpdateDialogueForHotspot(config.id, config.conversationId, config.setFlagOnInteract))
            {
                updated++;
            }
        }
        return updated;
    }

    /// <summary>
    /// API pública para actualizar diálogos de cualquier hotspot por ID.
    /// Uso futuro: llamar desde otros scripts de editor o herramientas.
    /// 
    /// Ejemplo:
    ///   SceneNarrativeConfigurator.UpdateHotspotDialogue("chapter4_mirilla_central", "chapter4_hotspot_mirilla_v2");
    /// </summary>
    public static void UpdateHotspotDialogue(string hotspotId, string newConversationId)
    {
        UpdateDialogueForHotspot(hotspotId, newConversationId);
    }

    /// <summary>
    /// API pública para actualizar diálogos de una puerta por nombre.
    /// Uso futuro: cambiar el lockedMessage o targetRoomId de puertas existentes.
    /// </summary>
    public static bool UpdateDoorDialogue(string doorName, string newLockedMessage)
    {
        GameObject obj = FindInScene(doorName);
        if (obj == null)
        {
            Debug.LogWarning($"[Configurador] No se encontro puerta '{doorName}' en la escena.");
            return false;
        }

        var door = obj.GetComponent<DoorTrigger>();
        if (door == null) return false;

        SerializedObject so = new SerializedObject(door);
        SetString(so, "lockedMessage", newLockedMessage);
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log($"[Configurador] Mensaje de puerta actualizado: {doorName}");
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    //  VERIFICACIÓN
    // ═══════════════════════════════════════════════════════════════

    private static string GenerateVerificationReport()
    {
        var lines = new List<string>();
        lines.Add("=== VERIFICACION DE ESCENA ===\n");

        // Hotspots Cap 4
        lines.Add("--- HOTSPOTS CAP 4 ---");
        foreach (var hs in chapter4Hotspots)
        {
            GameObject obj = FindHotspotInScene(hs.id);
            if (obj == null)
            {
                lines.Add($"  [FALTA] {hs.id}");
                continue;
            }

            var hotspot = obj.GetComponent<MapHotspot>();
            if (hotspot == null)
            {
                lines.Add($"  [ERROR] {hs.id} - sin componente MapHotspot");
                continue;
            }

            SerializedObject so = new SerializedObject(hotspot);
            string chapter = GetString(so, "requiredChapterId");
            string conv = GetString(so, "conversationId");
            string item = GetString(so, "grantItemId");
            lines.Add($"  [OK] {hs.id} | cap:{chapter} | conv:{conv} | item:{(string.IsNullOrEmpty(item) ? "-" : item)}");
        }

        // Puertas Cap 5
        lines.Add("\n--- PUERTAS CAP 5 ---");
        foreach (var door in chapter5Doors)
        {
            GameObject obj = FindInScene(door.name);
            if (obj == null)
            {
                lines.Add($"  [FALTA] {door.name}");
                continue;
            }

            var doorComp = obj.GetComponent<DoorTrigger>();
            if (doorComp == null)
            {
                lines.Add($"  [ERROR] {door.name} - sin componente DoorTrigger");
                continue;
            }

            SerializedObject so = new SerializedObject(doorComp);
            string target = GetString(so, "targetRoomId");
            string reqChapter = GetString(so, "requiredChapterId");
            var keysProp = so.FindProperty("requiredKeys");
            int keyCount = keysProp != null ? keysProp.arraySize : 0;
            lines.Add($"  [OK] {door.name} | target:{target} | cap:{reqChapter} | keys:{keyCount}");
        }

        // Sistemas
        lines.Add("\n--- SISTEMAS ---");
        bool hasDelivery = UnityEngine.Object.FindAnyObjectByType<NpcDeliverySystem>() != null;
        bool hasGate = UnityEngine.Object.FindAnyObjectByType<Chapter5ValidationGate>() != null;
        lines.Add($"  {(hasDelivery ? "[OK]" : "[FALTA]")} NpcDeliverySystem");
        lines.Add($"  {(hasGate ? "[OK]" : "[FALTA]")} Chapter5ValidationGate");

        // Resumen
        lines.Add("\n--- RESUMEN ---");
        int totalHotspots = chapter4Hotspots.Length;
        int foundHotspots = 0;
        foreach (var hs in chapter4Hotspots)
            if (FindHotspotInScene(hs.id) != null) foundHotspots++;

        int totalDoors = chapter5Doors.Length;
        int foundDoors = 0;
        foreach (var d in chapter5Doors)
            if (FindInScene(d.name) != null) foundDoors++;

        lines.Add($"  Hotspots: {foundHotspots}/{totalHotspots}");
        lines.Add($"  Puertas: {foundDoors}/{totalDoors}");
        lines.Add($"  Sistemas: {(hasDelivery && hasGate ? "COMPLETO" : "INCOMPLETO")}");

        return string.Join("\n", lines);
    }

    private static void LogAllHotspotsInScene()
    {
        MapHotspot[] all = UnityEngine.Object.FindObjectsByType<MapHotspot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[Configurador] === {all.Length} hotspots en escena ===");
        foreach (var hs in all)
        {
            SerializedObject so = new SerializedObject(hs);
            string id = GetString(so, "hotspotId");
            string chapter = GetString(so, "requiredChapterId");
            string conv = GetString(so, "conversationId");
            Debug.Log($"  {hs.gameObject.name} | id:{id} | cap:{chapter} | conv:{conv}");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  UTILIDADES
    // ═══════════════════════════════════════════════════════════════

    private static GameObject FindHotspotInScene(string hotspotId)
    {
        MapHotspot[] all = UnityEngine.Object.FindObjectsByType<MapHotspot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MapHotspot hs in all)
        {
            SerializedObject so = new SerializedObject(hs);
            var prop = so.FindProperty("hotspotId");
            if (prop != null && prop.stringValue == hotspotId)
                return hs.gameObject;
        }

        // Fallback: buscar por nombre de GameObject
        GameObject byName = FindInScene($"HS_{hotspotId}");
        return byName;
    }

    private static GameObject FindInScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.name == name && t.gameObject.scene.isLoaded)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    private static GameObject FindOrCreateParent(string parentName)
    {
        if (string.IsNullOrWhiteSpace(parentName))
            parentName = "HotsPots";

        GameObject parent = GameObject.Find(parentName);
        if (parent == null)
        {
            parent = new GameObject(parentName);
            parent.transform.position = Vector3.zero;
        }
        return parent;
    }

    private static Sprite LoadSprite(string spriteName)
    {
        if (string.IsNullOrWhiteSpace(spriteName)) return null;

        string[] paths = new string[]
        {
            $"Assets/Sprites/{spriteName}.png",
            $"Assets/Resources/Sprites/{spriteName}.png",
            $"Assets/Sprites/{spriteName}.jpg",
        };

        foreach (string path in paths)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
        }

        return null;
    }

    private static void SetString(SerializedObject so, string propName, string value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.stringValue = value ?? "";
    }

    private static void SetBool(SerializedObject so, string propName, bool value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.boolValue = value;
    }

    private static void SetInt(SerializedObject so, string propName, int value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.intValue = value;
    }

    private static string GetString(SerializedObject so, string propName)
    {
        var prop = so.FindProperty(propName);
        return prop != null ? prop.stringValue : "";
    }
}
