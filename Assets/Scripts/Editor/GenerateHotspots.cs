using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class GenerateHotspots
{
    // Solo hotspots que NO existen aún en la escena
    // Cap 1: completo (4/4)
    // Cap 2: faltan 2 (nota tablón y archivador — sin sprite, están en el fondo)
    // Cap 3: completo (5/5)
    // Cap 4: faltan todos (5)
    // Cap 5: faltan todos (5)
    private static readonly (string id, string chapter, string conversation, string sprite, float x, float y, bool consume, string itemId, string itemName)[] hotspotsToCreate = new[]
    {
        // ═══ CAP 2 — ESTUDIO (faltantes, sin sprite — objetos dibujados en el fondo) ═══
        ("chapter2_estudio_nota_tablon", "chapter2", "chapter2_estudio_nota_tablon", null, 2.5f, 1.5f, false, "", ""),
        ("chapter2_estudio_archivador",  "chapter2", "chapter2_estudio_archivador",  null, -3.0f, -1.0f, false, "", ""),

        // ═══ CAP 4 — GALERÍA (explorar_galeria del .py) ═══
        // Cuadro inacabado de 5 personas
        ("chapter4_cuadro_cinco",     "chapter4", "chapter4_cuadro_cinco",     "cuadro5",                        -2.5f, 1.0f, false, "", ""),
        // Cuadro abstracto con código 4-7-2-9
        ("chapter4_cuadro_codigo",    "chapter4", "chapter4_cuadro_codigo",    "cuadro_clave-removebg-preview",   2.5f, 1.0f, false, "codigo_4729", "Código 4-7-2-9"),
        // Mancha de sangre cerca de la puerta trasera
        ("chapter4_mancha_sangre",    "chapter4", "chapter4_mancha_sangre",    "Mancha_sangre-removebg-preview",  3.5f, -2.5f, false, "", ""),
        // Carpeta con evidencia (entre lienzos)
        ("chapter4_carpeta_evidencia","chapter4", "chapter4_carpeta_evidencia","Carpeta-removebg-preview",       -3.5f, -1.5f, true, "carpeta_evidencia", "Carpeta con evidencia"),
        // Fonógrafo con grabación de Simón (sin sprite, está en el fondo)
        ("chapter4_fonografo",        "chapter4", "chapter4_fonografo",        null,                              0.0f, 0.5f, false, "", ""),

        // ═══ CAP 5 — SALA DE VIGILANCIA (explorar_sala_camaras del .py) ═══
        // Nota con tinta roja de advertencia
        ("chapter5_nota_roja",        "chapter5", "chapter5_nota_roja",        "Nota_roja-removebg-preview",     -2.5f, 1.5f, false, "", ""),
        // Maletín negro (contiene relicario)
        ("chapter5_maletin",          "chapter5", "chapter5_maletin",          null,                              0.0f, -1.5f, true, "relicario_plata", "Relicario de plata"),
        // Mapa actualizado con ubicación de Simón
        ("chapter5_mapa_actualizado", "chapter5", "chapter5_mapa_actualizado", "Mapa_dibujado",                   2.5f, 1.0f, false, "", ""),
        // Mirilla central (sin sprite, está en el fondo)
        ("chapter5_mirilla",          "chapter5", "chapter5_mirilla",          null,                              0.0f, 2.0f, false, "", ""),
        // Cilindros de fonógrafo (sin sprite, está en el fondo)
        ("chapter5_cilindros",        "chapter5", "chapter5_cilindros",        null,                             -1.5f, -0.5f, false, "", ""),
    };

    [MenuItem("Tools/Generar Hotspots Faltantes (Cap 2, 4, 5)")]
    public static void Generate()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject hotsPots = GameObject.Find("HotsPots");
        if (hotsPots == null)
        {
            hotsPots = new GameObject("HotsPots");
            hotsPots.transform.position = Vector3.zero;
        }

        int created = 0;
        int skipped = 0;

        foreach (var hs in hotspotsToCreate)
        {
            if (FindHotspotById(hs.id) != null)
            {
                skipped++;
                continue;
            }

            GameObject obj = new GameObject($"HS_{hs.id}");
            obj.transform.SetParent(hotsPots.transform);
            obj.transform.localPosition = new Vector3(hs.x, hs.y, 0f);

            var collider = obj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1.2f, 1.2f);

            if (!string.IsNullOrWhiteSpace(hs.sprite))
            {
                string spritePath = $"Assets/Sprites/{hs.sprite}.png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite != null)
                {
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = 5;
                    float maxDim = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
                    if (maxDim > 0)
                    {
                        float scale = 1.2f / maxDim;
                        obj.transform.localScale = new Vector3(scale, scale, 1f);
                    }
                }
            }

            var hotspot = obj.AddComponent<MapHotspot>();
            SerializedObject so = new SerializedObject(hotspot);
            SetString(so, "hotspotId", hs.id);
            SetString(so, "requiredChapterId", hs.chapter);
            SetString(so, "conversationId", hs.conversation);
            SetString(so, "startNodeId", "start");
            SetBool(so, "consumeAfterUse", hs.consume);
            SetBool(so, "showDebugMarkerInGame", string.IsNullOrWhiteSpace(hs.sprite));
            if (!string.IsNullOrWhiteSpace(hs.itemId))
                SetString(so, "grantItemId", hs.itemId);
            if (!string.IsNullOrWhiteSpace(hs.itemName))
                SetString(so, "grantItemDisplayName", hs.itemName);
            so.ApplyModifiedPropertiesWithoutUndo();

            created++;
            Debug.Log($"[Hotspot] Creado: {hs.id} | Cap: {hs.chapter} | Sprite: {hs.sprite ?? "collider invisible"}");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Hotspots Generados",
            $"Creados: {created}\nSaltados (ya existían): {skipped}\n\n" +
            "Cap 2: nota tablón + archivador (collider invisible)\n" +
            "Cap 4: 5 hotspots (cuadro5, cuadro_clave, mancha, carpeta, fonógrafo)\n" +
            "Cap 5: 5 hotspots (nota_roja, maletín, mapa, mirilla, cilindros)",
            "OK");
    }

    private static GameObject FindHotspotById(string id)
    {
        MapHotspot[] all = Object.FindObjectsByType<MapHotspot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MapHotspot hs in all)
        {
            SerializedObject so = new SerializedObject(hs);
            var prop = so.FindProperty("hotspotId");
            if (prop != null && prop.stringValue == id)
                return hs.gameObject;
        }
        return null;
    }

    private static void SetString(SerializedObject so, string propName, string value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.stringValue = value;
    }

    private static void SetBool(SerializedObject so, string propName, bool value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.boolValue = value;
    }
}
