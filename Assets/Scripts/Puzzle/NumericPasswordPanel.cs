using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Panel de contraseña numérica para Door_ToSecurityRoom.
/// 
/// DISEÑO:
/// - NO crea números visuales (ya existen en el sprite del fondo).
/// - Crea hitboxes invisibles (BoxCollider2D) sobre cada número del fondo.
/// - El jugador clickea los números en secuencia.
/// - Al completar la secuencia correcta (4-7-2-9), desbloquea la puerta.
/// - Si la secuencia es incorrecta, muestra diálogo y resetea.
///
/// CONTRASEÑA: 4-7-2-9 (del archivo.txt / flujoHistoria.txt)
/// Origen narrativo: Fecha de la carta del padre (4 de julio de 1929)
///   "4" = día, "7" = mes (julio), "2-9" = año (29)
///
/// CONFIGURACIÓN EN INSPECTOR:
/// - correctPassword: la secuencia correcta (default: 4,7,2,9)
/// - digitPositions: posiciones de cada número 1-9 en el fondo
/// - hitboxSize: tamaño de cada hitbox invisible
/// - doorToUnlock: referencia al DoorTrigger que se desbloquea
/// </summary>
public class NumericPasswordPanel : MonoBehaviour
{
    [Header("Contraseña")]
    [Tooltip("Secuencia correcta de dígitos (del archivo.txt: 4-7-2-9)")]
    [SerializeField] private int[] correctPassword = new int[] { 4, 7, 2, 9 };

    [Header("Layout del teclado numérico")]
    [Tooltip("Posiciones locales de cada dígito 1-9 sobre el fondo. Index 0 = dígito 1, Index 8 = dígito 9")]
    [SerializeField] private Vector2[] digitPositions = new Vector2[]
    {
        // Layout estándar 3x3:
        //  1  2  3
        //  4  5  6
        //  7  8  9
        new Vector2(-1.0f,  1.0f),  // 1
        new Vector2( 0.0f,  1.0f),  // 2
        new Vector2( 1.0f,  1.0f),  // 3
        new Vector2(-1.0f,  0.0f),  // 4
        new Vector2( 0.0f,  0.0f),  // 5
        new Vector2( 1.0f,  0.0f),  // 6
        new Vector2(-1.0f, -1.0f),  // 7
        new Vector2( 0.0f, -1.0f),  // 8
        new Vector2( 1.0f, -1.0f),  // 9
    };

    [Header("Hitbox")]
    [Tooltip("Tamaño de cada hitbox invisible sobre los números")]
    [SerializeField] private Vector2 hitboxSize = new Vector2(0.45f, 0.45f);
    [Tooltip("Padre de los botones (BG_SecureDoor). Si está vacío, se busca el fondo de secureDoor.")]
    [SerializeField] private Transform digitHitboxParent;

    [Header("Puerta a desbloquear")]
    [Tooltip("Flag que se setea al resolver la contraseña (la puerta lo lee)")]
    [SerializeField] private string unlockFlag = "SecurityRoom.Unlocked";

    [Header("Feedback")]
    [SerializeField] private string wrongPasswordMessage = "Parece que no es la combinación correcta...";
    [SerializeField] private string correctPasswordMessage = "El mecanismo hace click. La puerta se ha desbloqueado.";

    [Header("Audio (opcional)")]
    [SerializeField] private AudioClip digitClickSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip correctSound;

    private List<int> currentInput = new List<int>();
    private bool isUnlocked;
    private bool panelIsShowing;
    private GameObject[] digitHitboxes;
    private AudioSource audioSource;

    public bool IsUnlocked => isUnlocked;
    public bool IsPanelShowing => panelIsShowing;
    public int RequiredDigitCount => correctPassword != null ? correctPassword.Length : 0;

    public event Action PasswordCorrect;
    public event Action PasswordWrong;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        if (StoryState.Instance != null && StoryState.Instance.HasFlag(unlockFlag))
        {
            isUnlocked = true;
            DisableHitboxes();
            Debug.Log("[NumericPassword] Ya desbloqueado previamente.");
            return;
        }

        panelIsShowing = false;
        EnsureDoorColliderEnabled();
    }

    /// <summary>
    /// Muestra el panel de contraseña activando los hitboxes y haciéndolo visible.
    /// Desactiva el collider del padre (DoorTrigger) para que los hitboxes reciban clicks.
    /// Llamado por DoorTrigger cuando el jugador interactúa con Door_ToSecurityRoom.
    /// </summary>
    public void ShowPanel()
    {
        if (isUnlocked)
        {
            Debug.Log("[NumericPassword] Panel ya desbloqueado, no se muestra.");
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        SetDoorColliderEnabled(false);
        currentInput.Clear();
        CreateDigitHitboxes();

        panelIsShowing = true;
        Debug.Log($"[NumericPassword] Teclado activo. Introduce {RequiredDigitCount} dígitos (ej. 4-7-2-9).");
    }

    /// <summary>
    /// Oculta el panel y reactiva el collider del DoorTrigger.
    /// </summary>
    public void HidePanel()
    {
        panelIsShowing = false;
        DisableHitboxes();
        EnsureDoorColliderEnabled();
        Debug.Log("[NumericPassword] Panel ocultado. Collider de puerta reactivado.");
    }

    public static NumericPasswordPanel GetActivePanel()
    {
        NumericPasswordPanel[] panels = FindObjectsByType<NumericPasswordPanel>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < panels.Length; i++)
        {
            if (panels[i] != null && panels[i].IsPanelShowing)
            {
                return panels[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Crea hitboxes invisibles sobre cada posición de dígito.
    /// Cada hitbox tiene un DigitButton component que reporta clicks.
    /// </summary>
    private Transform ResolveDigitHitboxParent()
    {
        if (digitHitboxParent != null)
        {
            return digitHitboxParent;
        }

        if (RoomManager.Instance != null)
        {
            Transform secureDoorBg = RoomManager.Instance.GetRoomBackgroundTransform("secureDoor");
            if (secureDoorBg != null)
            {
                return secureDoorBg;
            }
        }

        GameObject bg = GameObject.Find("BG_SecureDoor");
        return bg != null ? bg.transform : transform;
    }

    private void CreateDigitHitboxes()
    {
        DestroyDigitHitboxes();

        Transform parent = ResolveDigitHitboxParent();
        digitHitboxes = new GameObject[9];

        for (int i = 0; i < 9; i++)
        {
            int digitValue = i + 1; // 1-9
            Vector2 pos = i < digitPositions.Length ? digitPositions[i] : Vector2.zero;

            GameObject hitbox = new GameObject($"DigitHitbox_{digitValue}");
            hitbox.transform.SetParent(parent, false);
            hitbox.transform.localPosition = new Vector3(pos.x, pos.y, 0f);

            BoxCollider2D col = hitbox.AddComponent<BoxCollider2D>();
            col.size = hitboxSize;
            col.isTrigger = false;

            DigitButton button = hitbox.AddComponent<DigitButton>();
            button.Initialize(digitValue, this);

            digitHitboxes[i] = hitbox;
        }

        Debug.Log("[NumericPassword] 9 hitboxes creados sobre el teclado numérico del fondo.");
    }

    /// <summary>
    /// Llamado por DigitButton cuando el jugador clickea un número.
    /// </summary>
    public void OnDigitPressed(int digit)
    {
        if (isUnlocked || !panelIsShowing)
        {
            return;
        }

        if (digit < 1 || digit > 9 || currentInput.Count >= RequiredDigitCount)
        {
            return;
        }

        currentInput.Add(digit);
        Debug.Log($"[NumericPassword] Dígito {currentInput.Count}/{RequiredDigitCount}: {digit}");

        // Feedback sonoro
        if (digitClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(digitClickSound);
        }

        // Verificar si ya tiene suficientes dígitos
        if (currentInput.Count >= RequiredDigitCount)
        {
            ValidatePassword();
        }
    }

    /// <summary>
    /// Valida la secuencia ingresada contra la contraseña correcta.
    /// </summary>
    private void ValidatePassword()
    {
        bool isCorrect = currentInput.Count == RequiredDigitCount;

        for (int i = 0; isCorrect && i < correctPassword.Length; i++)
        {
            if (currentInput[i] != correctPassword[i])
            {
                isCorrect = false;
            }
        }

        if (isCorrect)
        {
            OnPasswordCorrect();
        }
        else
        {
            OnPasswordWrong();
        }
    }

    private void OnPasswordCorrect()
    {
        isUnlocked = true;
        panelIsShowing = false;

        // Persistir el desbloqueo
        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(unlockFlag, true);
        }

        EnsureDoorColliderEnabled();

        // Feedback
        if (correctSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(correctPasswordMessage);
        }

        // Desactivar hitboxes (ya no se necesitan)
        DisableHitboxes();

        Debug.Log("[NumericPassword] ★ CONTRASEÑA CORRECTA. Puerta desbloqueada. Collider padre reactivado.");
        PasswordCorrect?.Invoke();
    }

    private void OnPasswordWrong()
    {
        // Feedback
        if (wrongSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(wrongPasswordMessage);
        }

        // Reset automático
        currentInput.Clear();

        Debug.Log("[NumericPassword] Contraseña incorrecta. Secuencia reseteada.");
        PasswordWrong?.Invoke();
    }

    /// <summary>
    /// Resetea la secuencia manualmente (botón limpiar).
    /// </summary>
    public void ClearInput()
    {
        currentInput.Clear();
        Debug.Log("[NumericPassword] Input limpiado manualmente.");
    }

    private void DisableHitboxes()
    {
        DestroyDigitHitboxes();
    }

    private void DestroyDigitHitboxes()
    {
        if (digitHitboxes == null)
        {
            return;
        }

        for (int i = 0; i < digitHitboxes.Length; i++)
        {
            if (digitHitboxes[i] != null)
            {
                Destroy(digitHitboxes[i]);
            }
        }

        digitHitboxes = null;
    }

    /// <summary>
    /// API para testing: forzar desbloqueo sin contraseña.
    /// Solo usar en modo debug/testing.
    /// </summary>
    public void ForceUnlock()
    {
        if (isUnlocked) return;
        OnPasswordCorrect();
        Debug.Log("[NumericPassword] FORZADO: Puerta desbloqueada sin contraseña (testing).");
    }

    private void SetDoorColliderEnabled(bool enabled)
    {
        BoxCollider2D doorCollider = GetComponent<BoxCollider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = enabled;
        }
    }

    private void EnsureDoorColliderEnabled()
    {
        SetDoorColliderEnabled(true);
    }
}
