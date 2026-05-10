using UnityEngine;

/// <summary>
/// Placeholder de puzzle para bloquear el acceso a una llave o puerta.
/// Cuando el puzzle real sea implementado, el desarrollador llamará OnPuzzleSolved().
/// 
/// FLUJO:
///   1. El jugador interactúa con este hotspot.
///   2. Si el puzzle no ha sido resuelto, se muestra un mensaje de que algo bloquea el acceso.
///   3. Cuando el puzzle se resuelve (OnPuzzleSolved), se activa la llave/puerta enlazada.
///   4. Se marca un flag en StoryState para persistir el estado.
///
/// USO DESDE OTRO SCRIPT:
///   PuzzleGatePlaceholder gate = FindObjectOfType<PuzzleGatePlaceholder>();
///   gate.OnPuzzleSolved();  // Esto libera la llave automáticamente.
/// </summary>
public class PuzzleGatePlaceholder : Interactable, IPuzzleGate
{
    [Header("Identidad del Puzzle")]
    [SerializeField] private string puzzleId = "puzzle_placeholder";
    [TextArea(2, 4)]
    [SerializeField] private string lockedMessage = "Hay algo aquí que no puedes resolver todavía.";
    [TextArea(2, 4)]
    [SerializeField] private string solvedMessage = "El mecanismo cede. Algo se ha desbloqueado.";

    [Header("Recompensa al Resolver")]
    [Tooltip("GameObject de la llave que se activa al resolver el puzzle (puede ser null).")]
    [SerializeField] private GameObject rewardKeyObject;
    [Tooltip("Flag que se setea en StoryState al resolver.")]
    [SerializeField] private string solvedFlag;

    [Header("Visual Placeholder")]
    [SerializeField] private Color placeholderColor = new Color(0.8f, 0.2f, 0.9f, 0.5f);

    private bool solved;

    public string PuzzleId => puzzleId;
    public bool IsSolved => solved;

    private void Start()
    {
        // Restaurar estado persistido
        if (StoryState.Instance != null && !string.IsNullOrWhiteSpace(solvedFlag))
        {
            solved = StoryState.Instance.HasFlag(solvedFlag);
        }

        if (solved && rewardKeyObject != null)
        {
            rewardKeyObject.SetActive(true);
        }

        EnsurePlaceholderVisual();
    }

    public override void Interact()
    {
        base.Interact();

        if (solved)
        {
            ShowMessage(solvedMessage);
            return;
        }

        ShowMessage(lockedMessage);
    }

    /// <summary>
    /// Llamar externamente cuando el puzzle sea resuelto.
    /// Activa la recompensa y persiste el estado.
    /// </summary>
    public void OnPuzzleSolved()
    {
        if (solved)
        {
            return;
        }

        solved = true;
        Debug.Log($"[PuzzleGatePlaceholder] Puzzle '{puzzleId}' resuelto!");

        if (!string.IsNullOrWhiteSpace(solvedFlag) && StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(solvedFlag, true);
        }

        if (rewardKeyObject != null)
        {
            rewardKeyObject.SetActive(true);
            Debug.Log($"[PuzzleGatePlaceholder] Recompensa activada: {rewardKeyObject.name}");
        }

        ShowMessage(solvedMessage);
    }

    private static void ShowMessage(string message)
    {
        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(message);
        }
        else
        {
            Debug.Log($"[PuzzleGate] {message}");
        }
    }

    private void EnsurePlaceholderVisual()
    {
        if (GetComponent<SpriteRenderer>() != null)
        {
            return;
        }

        // Crear un cuadrado visual de placeholder si no hay sprite
        SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
        Rect rect = new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height);
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, rect, new Vector2(0.5f, 0.5f), 100f);
        sr.color = placeholderColor;
        sr.sortingOrder = 120;
        transform.localScale = new Vector3(0.8f, 0.8f, 1f);
    }
}
