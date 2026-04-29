using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupNpcSprites
{
    private static readonly (string npcName, string spriteName)[] npcSprites = new[]
    {
        ("NPC_Ana",    "Ana-removebg-preview"),
        ("NPC_Ben",    "Ben-removebg-preview"),
        ("NPC_Lisa",   "Lisa-removebg-preview (1)"),
        ("NPC_Lucas",  "Lucas-removebg-preview"),
        ("NPC_Robert", "Robert-removebg-preview"),
    };

    [MenuItem("Tools/Asignar Sprites a NPCs")]
    public static void AssignNpcSprites()
    {
        string scenePath = "Assets/Scenes/MainMapScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        int count = 0;

        foreach (var (npcName, spriteName) in npcSprites)
        {
            // Buscar el NPC en la escena
            GameObject npc = FindInScene(npcName);
            if (npc == null)
            {
                Debug.LogWarning($"NPC no encontrado: {npcName}");
                continue;
            }

            // Cargar el sprite
            string spritePath = $"Assets/Sprites/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite no encontrado: {spritePath}");
                continue;
            }

            // Asignar o crear SpriteRenderer
            SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = npc.AddComponent<SpriteRenderer>();
            }

            sr.sprite = sprite;
            sr.sortingOrder = 10; // Por encima de los fondos (-100)

            // Escalar el NPC para que tenga un tamaño razonable en la escena
            float targetHeight = 3f; // Altura deseada en unidades de mundo
            float spriteHeight = sprite.bounds.size.y;
            if (spriteHeight > 0)
            {
                float scale = targetHeight / spriteHeight;
                npc.transform.localScale = new Vector3(scale, scale, 1f);
            }

            count++;
            Debug.Log($"Sprite asignado: {npcName} -> {spriteName} (sortingOrder: 10, escala: {npc.transform.localScale})");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("NPCs Configurados",
            $"Se asignaron sprites a {count} NPCs.\n\n" +
            "Puedes ajustar la posición y escala de cada NPC\n" +
            "seleccionándolo en la jerarquía.",
            "OK");
    }

    private static GameObject FindInScene(string name)
    {
        // Buscar en toda la escena incluyendo objetos inactivos
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
}
