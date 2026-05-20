using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Panel de contraseña numérica para Door_ToSecurityRoom.
/// Botones en escena (DigitButton). Contraseña: 4-7-2-9. Valida al 4.º dígito.
/// </summary>
public class NumericPasswordPanel : MonoBehaviour
{
    private static readonly Vector2 HitboxSize = new Vector2(0.3f, 0.3f);
    private static readonly Vector2[] DigitPositions =
    {
        new Vector2(2.67f, 0.1f),    // 1
        new Vector2(3.02f, 0.1f),    // 2
        new Vector2(3.37f, 0.1f),    // 3
        new Vector2(2.67f, -0.26f),  // 4
        new Vector2(3.02f, -0.26f),  // 5
        new Vector2(3.37f, -0.26f),  // 6
        new Vector2(2.67f, -0.65f),  // 7
        new Vector2(3.02f, -0.65f),  // 8
        new Vector2(3.37f, -0.65f),  // 9
    };

    [Header("Contraseña")]
    [SerializeField] private int[] correctPassword = new int[] { 4, 7, 2, 9 };

    [Header("Botones en escena")]
    [SerializeField] private DigitButton[] sceneDigitButtons;

    [Header("Input")]
    [Tooltip("Tiempo mínimo entre pulsaciones para evitar doble registro.")]
    [SerializeField] private float inputCooldownSeconds = 0.15f;
    [Tooltip("Tiempo de bloqueo extra mientras se valida la secuencia.")]
    [SerializeField] private float validationLockSeconds = 0.25f;

    [Header("Puerta")]
    [SerializeField] private string unlockFlag = "SecurityRoom.Unlocked";

    [Header("Feedback")]
    [SerializeField] private string wrongPasswordMessage = "Parece que no es la combinación correcta...";
    [SerializeField] private string correctPasswordMessage = "El mecanismo hace click. La puerta se ha desbloqueado.";

    [Header("Audio (opcional)")]
    [SerializeField] private AudioClip digitClickSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip correctSound;

    private readonly List<int> currentInput = new List<int>();
    private bool isUnlocked;
    private bool panelIsShowing;
    private bool isInputLocked;
    private int lastAcceptedInputFrame = -1;
    private float nextInputAllowedTime;
    private AudioSource audioSource;
    private Coroutine inputUnlockRoutine;

    public bool IsUnlocked => isUnlocked;
    public bool IsPanelShowing => panelIsShowing;
    public bool CanAcceptDigitInput =>
        panelIsShowing && !isUnlocked && !isInputLocked && Time.time >= nextInputAllowedTime;
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

        BindSceneDigitButtons();
        ApplySceneButtonLayout();
    }

    private void Start()
    {
        if (StoryState.Instance != null && StoryState.Instance.HasFlag(unlockFlag))
        {
            isUnlocked = true;
            SetDigitButtonsInteractable(false);
            return;
        }

        panelIsShowing = false;
        EnsureDoorColliderEnabled();
        SetDigitButtonsInteractable(false);
    }

    public void ShowPanel()
    {
        if (isUnlocked)
        {
            return;
        }

        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        if (!HasSceneDigitButtons())
        {
            Debug.LogError("[NumericPassword] No hay DigitButton en escena. Asigna sceneDigitButtons.");
            return;
        }

        ReleaseInputLockImmediate();
        SetDoorColliderEnabled(false);
        currentInput.Clear();
        BindSceneDigitButtons();
        ApplySceneButtonLayout();
        SetDigitButtonsInteractable(true);
        panelIsShowing = true;
        Debug.Log($"[NumericPassword] Teclado activo. Introduce {RequiredDigitCount} dígitos (4-7-2-9).");
    }

    public void HidePanel()
    {
        panelIsShowing = false;
        ReleaseInputLockImmediate();
        SetDigitButtonsInteractable(false);
        EnsureDoorColliderEnabled();
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
    /// Registra un solo dígito por interacción. Devuelve false si el input fue rechazado (doble clic / bloqueo).
    /// </summary>
    public bool TryRegisterDigit(int digit)
    {
        if (!CanAcceptDigitInput)
        {
            return false;
        }

        if (digit < 1 || digit > 9 || currentInput.Count >= RequiredDigitCount)
        {
            return false;
        }

        // Un solo dígito por frame de Unity.
        if (Time.frameCount == lastAcceptedInputFrame)
        {
            return false;
        }

        LockInputUntil(Time.time + inputCooldownSeconds);
        lastAcceptedInputFrame = Time.frameCount;

        currentInput.Add(digit);
        Debug.Log($"[NumericPassword] Dígito {currentInput.Count}/{RequiredDigitCount}: {digit} | Secuencia: {GetInputPreview()}");

        if (digitClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(digitClickSound);
        }

        if (currentInput.Count >= RequiredDigitCount)
        {
            LockInputUntil(Time.time + validationLockSeconds);
            ValidatePassword();
        }

        return true;
    }

    private string GetInputPreview()
    {
        if (currentInput.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("-", currentInput);
    }

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
        isInputLocked = true;

        if (StoryState.Instance != null)
        {
            StoryState.Instance.SetFlag(unlockFlag, true);
        }

        EnsureDoorColliderEnabled();

        if (correctSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(correctPasswordMessage);
        }

        SetDigitButtonsInteractable(false);
        Debug.Log("[NumericPassword] Contraseña correcta (4-7-2-9). Puerta desbloqueada.");
        PasswordCorrect?.Invoke();
    }

    private void OnPasswordWrong()
    {
        if (wrongSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        DialoguePanelUI panel = DialoguePanelUI.Instance;
        if (panel != null)
        {
            panel.ShowSystemMessage(wrongPasswordMessage);
        }

        currentInput.Clear();
        lastAcceptedInputFrame = -1;
        panelIsShowing = true;
        SetDigitButtonsInteractable(true);
        LockInputUntil(Time.time + inputCooldownSeconds);
        Debug.Log("[NumericPassword] Contraseña incorrecta. Secuencia reseteada.");
        PasswordWrong?.Invoke();
    }

    public void ClearInput()
    {
        currentInput.Clear();
        lastAcceptedInputFrame = -1;
    }

    public void ForceUnlock()
    {
        if (isUnlocked)
        {
            return;
        }

        OnPasswordCorrect();
    }

    private void ApplySceneButtonLayout()
    {
        if (!HasSceneDigitButtons())
        {
            return;
        }

        for (int i = 0; i < sceneDigitButtons.Length; i++)
        {
            DigitButton button = sceneDigitButtons[i];
            if (button == null)
            {
                continue;
            }

            int index = button.DigitValue - 1;
            if (index >= 0 && index < DigitPositions.Length)
            {
                Transform t = button.transform;
                t.localPosition = new Vector3(DigitPositions[index].x, DigitPositions[index].y, 0f);
            }

            BoxCollider2D col = button.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = HitboxSize;
                col.offset = Vector2.zero;
            }
        }
    }

    private void BindSceneDigitButtons()
    {
        if (sceneDigitButtons == null || sceneDigitButtons.Length == 0)
        {
            return;
        }

        for (int i = 0; i < sceneDigitButtons.Length; i++)
        {
            DigitButton button = sceneDigitButtons[i];
            if (button != null)
            {
                button.BindToPanel(this);
            }
        }
    }

    private bool HasSceneDigitButtons()
    {
        return sceneDigitButtons != null && sceneDigitButtons.Length > 0;
    }

    private void SetDigitButtonsInteractable(bool interactable)
    {
        if (!HasSceneDigitButtons())
        {
            return;
        }

        for (int i = 0; i < sceneDigitButtons.Length; i++)
        {
            DigitButton button = sceneDigitButtons[i];
            if (button == null)
            {
                continue;
            }

            BoxCollider2D col = button.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.enabled = interactable && !isUnlocked;
            }
        }
    }

    private void LockInputUntil(float unlockTime)
    {
        isInputLocked = true;
        nextInputAllowedTime = unlockTime;

        if (inputUnlockRoutine != null)
        {
            StopCoroutine(inputUnlockRoutine);
        }

        float delay = Mathf.Max(0f, unlockTime - Time.time);
        if (delay > 0f)
        {
            inputUnlockRoutine = StartCoroutine(ReleaseInputLockAfter(delay));
        }
        else
        {
            isInputLocked = false;
        }
    }

    private IEnumerator ReleaseInputLockAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInputLocked = false;
        inputUnlockRoutine = null;
    }

    private void ReleaseInputLockImmediate()
    {
        if (inputUnlockRoutine != null)
        {
            StopCoroutine(inputUnlockRoutine);
            inputUnlockRoutine = null;
        }

        isInputLocked = false;
        nextInputAllowedTime = 0f;
        lastAcceptedInputFrame = -1;
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
