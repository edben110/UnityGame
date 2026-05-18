using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enlaza Acertijo2Scene con Acertijo2PuzzleService y añade botón cerrar.
/// </summary>
public class Acertijo2SceneBootstrap : MonoBehaviour
{
    [SerializeField] private SlidingPuzzleController puzzleController;

    private void Awake()
    {
        if (puzzleController == null)
        {
            puzzleController = FindFirstObjectByType<SlidingPuzzleController>();
        }

        if (puzzleController != null)
        {
            puzzleController.PuzzleCompleted -= OnPuzzleCompleted;
            puzzleController.PuzzleCompleted += OnPuzzleCompleted;
        }

        EnsureCloseButton();
    }

    private void OnDestroy()
    {
        if (puzzleController != null)
        {
            puzzleController.PuzzleCompleted -= OnPuzzleCompleted;
        }
    }

    private void OnPuzzleCompleted()
    {
        if (Acertijo2PuzzleService.Instance != null)
        {
            Acertijo2PuzzleService.Instance.NotifyPuzzleCompleted();
        }
    }

    private void EnsureCloseButton()
    {
        if (transform.Find("BtnCerrarAcertijo") != null)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        GameObject buttonObj = new GameObject("BtnCerrarAcertijo");
        buttonObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.92f, 0.92f);
        rect.anchorMax = new Vector2(0.99f, 0.99f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        buttonObj.AddComponent<CanvasRenderer>();
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.15f, 0.12f, 0.85f);
        image.raycastTarget = true;

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(CloseWithoutReward);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(buttonObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text label = labelObj.AddComponent<Text>();
        label.text = "X";
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.92f, 0.82f, 0.55f, 1f);
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 22;
        label.raycastTarget = false;
    }

    private static void CloseWithoutReward()
    {
        if (Acertijo2PuzzleService.Instance != null)
        {
            Acertijo2PuzzleService.Instance.ClosePuzzle();
        }
    }
}
