using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Preparación común para escenas de acertijo cargadas de forma aditiva desde el inventario.
/// </summary>
public static class PuzzleAdditiveSceneUtility
{
    public const int PuzzleCanvasSortingOrder = 150;

    public static void FinalizeLoadedScene(Scene puzzleScene, Type bootstrapComponentType)
    {
        if (!puzzleScene.IsValid())
        {
            return;
        }

        foreach (GameObject root in puzzleScene.GetRootGameObjects())
        {
            foreach (Acertijo2RenderSetup renderSetup in root.GetComponentsInChildren<Acertijo2RenderSetup>(true))
            {
                renderSetup.enabled = false;
            }

            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                PrepareOverlayCanvas(canvas);
            }
        }

        EnsureBootstrapComponent(puzzleScene, bootstrapComponentType);
    }

    public static void PrepareOverlayCanvas(Canvas canvas)
    {
        if (canvas == null)
        {
            return;
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        canvas.overrideSorting = true;
        canvas.sortingOrder = PuzzleCanvasSortingOrder;
        canvas.targetDisplay = 0;

        RectTransform rect = canvas.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        Acertijo2UICanvasSetup canvasSetup = canvas.GetComponent<Acertijo2UICanvasSetup>();
        if (canvasSetup != null)
        {
            canvasSetup.ApplyRuntimeConfiguration();
        }
    }

    private static void EnsureBootstrapComponent(Scene puzzleScene, Type bootstrapComponentType)
    {
        if (bootstrapComponentType == null)
        {
            return;
        }

        foreach (GameObject root in puzzleScene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren(bootstrapComponentType, true) != null)
            {
                return;
            }
        }

        foreach (GameObject root in puzzleScene.GetRootGameObjects())
        {
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = root.GetComponentInChildren<Canvas>(true);
            }

            if (canvas != null && canvas.GetComponent(bootstrapComponentType) == null)
            {
                canvas.gameObject.AddComponent(bootstrapComponentType);
                return;
            }
        }
    }
}
