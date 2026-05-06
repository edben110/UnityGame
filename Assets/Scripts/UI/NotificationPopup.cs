using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sistema de notificaciones en forma de popup modal.
/// Muestra mensaje, imagen (opcional) y botón de confirmación.
/// </summary>
public class NotificationPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Image notificationImage;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private float autoCloseDelay = 3f;

    private Coroutine autoCloseRoutine;
    private static NotificationPopup instance;

    public static NotificationPopup Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<NotificationPopup>();
                if (instance == null)
                {
                    Debug.LogWarning("[NotificationPopup] No instance found in scene!");
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(Close);
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(Close);
        }
    }

    /// <summary>
    /// Muestra una notificación con mensaje e imagen opcional.
    /// </summary>
    public void Show(string message, Sprite icon = null, float displayTime = 0)
    {
        if (popupPanel == null || messageText == null)
        {
            Debug.LogError("[NotificationPopup] popupPanel o messageText no configurados!");
            return;
        }

        // Detener auto-close anterior si existe
        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }

        // Configurar mensaje
        messageText.text = message;
        Debug.Log($"[NotificationPopup] Mostrando: {message}");

        // Configurar imagen
        if (notificationImage != null)
        {
            if (icon != null)
            {
                notificationImage.sprite = icon;
                notificationImage.enabled = true;
            }
            else
            {
                notificationImage.enabled = false;
            }
        }

        // Mostrar panel
        popupPanel.SetActive(true);

        // Auto-cerrar si se especifica tiempo
        if (displayTime > 0)
        {
            autoCloseRoutine = StartCoroutine(AutoCloseRoutine(displayTime));
        }
    }

    public void Close()
    {
        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private IEnumerator AutoCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        autoCloseRoutine = null;
        Close();
    }
}
