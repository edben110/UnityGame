using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Copia Final_1/2/3 a Resources/Cinematicas y asegura EndingFlowController en MainMapScene.
/// </summary>
public static class SetupEndingCinematics
{
    private const string MainMapScenePath = "Assets/Scenes/MainMapScene.unity";
    private const string SourceFolder = "Assets/Cinematicas";
    private const string TargetFolder = "Assets/Resources/Cinematicas";

    private static readonly string[] EndingFiles = { "Final_1.mp4", "Final_2.mp4", "Final_3.mp4" };

    [MenuItem("Tools/Narrativa/Configurar finales (videos + EndingFlowController)")]
    public static void Configure()
    {
        EnsureTargetFolder();
        int copied = CopyEndingVideos();
        EnsureEndingFlowControllerInScene();

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Finales",
            $"Videos copiados/actualizados: {copied}/3\n" +
            "EndingFlowController en MainMapScene.\n\n" +
            "Flujo:\n" +
            "- Door_ToKidNappedSimon → Final_1 (algún NPC vivo) o Final_2 (todos muertos)\n" +
            "- Door_ToEmptyRoom + Door_ToKillerBunker → Final_3",
            "OK");
    }

    private static void EnsureTargetFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources/Cinematicas"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Cinematicas");
        }
    }

    private static int CopyEndingVideos()
    {
        int copied = 0;

        for (int i = 0; i < EndingFiles.Length; i++)
        {
            string fileName = EndingFiles[i];
            string sourcePath = Path.Combine(SourceFolder, fileName).Replace('\\', '/');
            string targetPath = Path.Combine(TargetFolder, fileName).Replace('\\', '/');

            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"[SetupEndingCinematics] No encontrado: {sourcePath}");
                continue;
            }

            File.Copy(sourcePath, targetPath, true);
            copied++;
            Debug.Log($"[SetupEndingCinematics] Copiado {fileName} → Resources/Cinematicas/");
        }

        return copied;
    }

    private static void EnsureEndingFlowControllerInScene()
    {
        var scene = EditorSceneManager.OpenScene(MainMapScenePath, OpenSceneMode.Single);

        if (Object.FindAnyObjectByType<EndingFlowController>() == null)
        {
            GameObject host = new GameObject("EndingFlowController");
            host.AddComponent<EndingFlowController>();
            Debug.Log("[SetupEndingCinematics] EndingFlowController creado en escena.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
