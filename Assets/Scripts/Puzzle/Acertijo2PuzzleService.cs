using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Abre Acertijo2Scene desde el inventario y entrega GalleryKey al resolver el puzzle deslizante.
/// </summary>
public class Acertijo2PuzzleService : MonoBehaviour
{
    public const string PuzzleBoxItemId = "puzzle_box_2";
    public const string GalleryPuzzleSolvedFlag = "puzzle.gallery_box.solved";
    public const string Acertijo2SceneName = "Acertijo2Scene";

    public static Acertijo2PuzzleService Instance { get; private set; }

    public static event Action PuzzleOpened;
    public static event Action PuzzleClosed;
    public static event Action PuzzleCompleted;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static bool IsGalleryPuzzleSolved()
    {
        if (InventoryState.HasItem(KeyType.GalleryKey.ToString()))
        {
            return true;
        }

        return StoryState.Instance != null && StoryState.Instance.HasFlag(GalleryPuzzleSolvedFlag);
    }

    public bool CanOpenFromInventory()
    {
        if (!InventoryState.HasItem(PuzzleBoxItemId))
        {
            return false;
        }

        return !IsGalleryPuzzleSolved();
    }

    public void OpenFromInventory()
    {
        if (!CanOpenFromInventory())
        {
            ShowSystemMessage(IsGalleryPuzzleSolved()
                ? "La caja ya fue abierta. La llave de la galería está en tu inventario."
                : "No tienes la caja puzzle en el inventario.");
            return;
        }

        if (isOpen)
        {
            return;
        }

        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.Hide();
        }

        PuzzleReturnContext.RememberCurrentScene();
        StartCoroutine(LoadPuzzleSceneRoutine());
    }

    public void NotifyPuzzleCompleted()
    {
        if (IsGalleryPuzzleSolved())
        {
            ClosePuzzle();
            return;
        }

        GrantGalleryKeyReward();
        PuzzleCompleted?.Invoke();
        ClosePuzzle();
    }

    public void ClosePuzzle()
    {
        if (!isOpen)
        {
            return;
        }

        StartCoroutine(UnloadPuzzleSceneRoutine());
    }

    private IEnumerator LoadPuzzleSceneRoutine()
    {
        if (!Application.CanStreamedLevelBeLoaded(Acertijo2SceneName))
        {
            Debug.LogError($"Acertijo2PuzzleService: '{Acertijo2SceneName}' no está en Build Settings.");
            ShowSystemMessage("No se pudo abrir el acertijo (escena no registrada).");
            yield break;
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(Acertijo2SceneName, LoadSceneMode.Additive);
        if (load == null)
        {
            yield break;
        }

        while (!load.isDone)
        {
            yield return null;
        }

        Scene puzzleScene = SceneManager.GetSceneByName(Acertijo2SceneName);
        if (puzzleScene.IsValid())
        {
            SceneManager.SetActiveScene(puzzleScene);
            EnsureSceneBootstrap(puzzleScene);
        }

        isOpen = true;
        PuzzleOpened?.Invoke();
    }

    private IEnumerator UnloadPuzzleSceneRoutine()
    {
        Scene puzzleScene = SceneManager.GetSceneByName(Acertijo2SceneName);
        if (puzzleScene.IsValid() && puzzleScene.isLoaded)
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(puzzleScene);
            if (unload != null)
            {
                while (!unload.isDone)
                {
                    yield return null;
                }
            }
        }

        isOpen = false;
        PuzzleClosed?.Invoke();
        PuzzleReturnContext.RestoreReturnScene();
    }

    public static void GrantGalleryKeyReward()
    {
        InventoryState.RemoveItem(PuzzleBoxItemId);

        KeyType keyType = KeyType.GalleryKey;
        string itemId = keyType.ToString();

        if (InventoryCatalog.Instance == null)
        {
            GameObject catalogHost = new GameObject("InventoryCatalog");
            catalogHost.AddComponent<InventoryCatalog>();
        }

        InventoryNarrativeDefaults.EnsureItemRegistered(itemId);
        bool added = InventoryState.AddItem(itemId);

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(GalleryPuzzleSolvedFlag, true);
            StoryState.Instance.SetFlag($"KeyPickedUp_{keyType}", true);
        }

        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.RefreshGrid();
        }

        string displayName = KeyTypeDisplayNames.GetDisplayName(keyType);
        string message = added
            ? $"Has resuelto el acertijo y obtenido {displayName}."
            : $"Ya tenías {displayName} en el inventario.";
        ShowSystemMessage(message);
    }

    private static void EnsureSceneBootstrap(Scene puzzleScene)
    {
        if (!puzzleScene.IsValid())
        {
            return;
        }

        foreach (GameObject root in puzzleScene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<Acertijo2SceneBootstrap>(true) != null)
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

            if (canvas != null)
            {
                if (canvas.GetComponent<Acertijo2SceneBootstrap>() == null)
                {
                    canvas.gameObject.AddComponent<Acertijo2SceneBootstrap>();
                }

                return;
            }
        }
    }

    private static void ShowSystemMessage(string message)
    {
        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(message);
        }
        else
        {
            Debug.Log($"[Acertijo2Puzzle] {message}");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(Acertijo2PuzzleService));
        host.AddComponent<Acertijo2PuzzleService>();
    }
}
