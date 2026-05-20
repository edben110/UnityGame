using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Abre AcertijoScene de forma aditiva desde el inventario y entrega BedroomKey al resolver.
/// </summary>
public class AcertijoPuzzleService : MonoBehaviour
{
    public const string PuzzleBoxItemId = "puzzle_box";
    public const string BedroomPuzzleSolvedFlag = "puzzle.bedroom_box.solved";
    public const string AcertijoSceneName = "AcertijoScene";

    public static AcertijoPuzzleService Instance { get; private set; }

    public static event Action PuzzleOpened;
    public static event Action PuzzleClosed;
    public static event Action PuzzleCompleted;

    [SerializeField] private bool isOpen;

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

    public static bool IsBedroomPuzzleSolved()
    {
        if (InventoryState.HasItem(KeyType.BedroomKey.ToString()))
        {
            return true;
        }

        return StoryState.Instance != null && StoryState.Instance.HasFlag(BedroomPuzzleSolvedFlag);
    }

    public bool CanOpenFromInventory()
    {
        if (!InventoryState.HasItem(PuzzleBoxItemId))
        {
            return false;
        }

        return !IsBedroomPuzzleSolved();
    }

    public void OpenFromInventory()
    {
        if (!CanOpenFromInventory())
        {
            ShowSystemMessage(IsBedroomPuzzleSolved()
                ? "La caja ya fue abierta. La llave está en tu inventario."
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
        if (IsBedroomPuzzleSolved())
        {
            ClosePuzzle();
            return;
        }

        GrantBedroomKeyReward();
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
        if (!Application.CanStreamedLevelBeLoaded(AcertijoSceneName))
        {
            Debug.LogError($"AcertijoPuzzleService: '{AcertijoSceneName}' no está en Build Settings.");
            ShowSystemMessage("No se pudo abrir el acertijo (escena no registrada).");
            yield break;
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(AcertijoSceneName, LoadSceneMode.Additive);
        if (load == null)
        {
            yield break;
        }

        while (!load.isDone)
        {
            yield return null;
        }

        Scene puzzleScene = SceneManager.GetSceneByName(AcertijoSceneName);
        if (puzzleScene.IsValid())
        {
            SceneManager.SetActiveScene(puzzleScene);
            PuzzleAdditiveSceneUtility.FinalizeLoadedScene(puzzleScene, typeof(AcertijoSceneBootstrap));
        }

        isOpen = true;
        PuzzleOpened?.Invoke();
    }

    private IEnumerator UnloadPuzzleSceneRoutine()
    {
        Scene puzzleScene = SceneManager.GetSceneByName(AcertijoSceneName);
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

    private static void GrantBedroomKeyReward()
    {
        InventoryState.RemoveItem(PuzzleBoxItemId);

        KeyType keyType = KeyType.BedroomKey;
        string itemId = keyType.ToString();
        string displayName = KeyTypeDisplayNames.GetDisplayName(keyType);

        if (InventoryCatalog.Instance == null)
        {
            GameObject catalogHost = new GameObject("InventoryCatalog");
            catalogHost.AddComponent<InventoryCatalog>();
        }

        InventoryNarrativeDefaults.EnsureItemRegistered(itemId);
        bool added = InventoryState.AddItem(itemId);

        if (ItemSoundManager.Instance != null)
            ItemSoundManager.Instance.PlayItemSound();

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(BedroomPuzzleSolvedFlag, true);
            StoryState.Instance.SetFlag($"KeyPickedUp_{keyType}", true);
        }

        if (InventoryOverlayCanvas.Instance != null)
        {
            InventoryOverlayCanvas.Instance.RefreshGrid();
        }

        string message = added
            ? $"Has resuelto el acertijo y obtenido {displayName}."
            : $"Ya tenías {displayName} en el inventario.";
        ShowSystemMessage(message);
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
            Debug.Log($"[AcertijoPuzzle] {message}");
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(AcertijoPuzzleService));
        host.AddComponent<AcertijoPuzzleService>();
    }
}
