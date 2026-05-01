using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotspotItemPanelUI : MonoBehaviour
{
    public static HotspotItemPanelUI Instance { get; private set; }

    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject previewContainer;
    [SerializeField] private GameObject previewDetailsRoot;
    [SerializeField] private Image previewImage;
    [SerializeField] private TMP_Text previewDescriptionText;
    [SerializeField] private Button viewButton;
    [SerializeField] private Button pickButton;
    [SerializeField] private Button closeButton;

    private Sprite currentSprite;
    private string currentDescription;
    private Action onPick;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (viewButton != null)
        {
            viewButton.onClick.AddListener(HandleViewPressed);
        }

        if (pickButton != null)
        {
            pickButton.onClick.AddListener(HandlePickPressed);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (viewButton != null)
        {
            viewButton.onClick.RemoveListener(HandleViewPressed);
        }

        if (pickButton != null)
        {
            pickButton.onClick.RemoveListener(HandlePickPressed);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show(string hotspotName, string itemName, string itemDescription, Sprite itemSprite, Action pickAction)
    {
        onPick = pickAction;
        currentSprite = itemSprite;
        currentDescription = itemDescription ?? string.Empty;

        if (titleText != null)
        {
            string readableName = string.IsNullOrWhiteSpace(itemName) ? "Objeto" : itemName;
            titleText.text = string.IsNullOrWhiteSpace(hotspotName)
                ? readableName
                : $"{hotspotName}: {readableName}";
        }

        if (statusText != null)
        {
            statusText.text = "Elige una opcion: visualizar o recoger.";
        }

        SetPreviewDetailsVisible(false);
        if (previewImage != null)
        {
            previewImage.enabled = false;
        }

        if (previewDescriptionText != null)
        {
            previewDescriptionText.text = string.Empty;
        }

        if (root != null)
        {
            root.SetActive(true);
        }
    }

    public void Hide()
    {
        onPick = null;
        currentSprite = null;
        currentDescription = string.Empty;

        SetPreviewDetailsVisible(false);

        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private void HandleViewPressed()
    {
        SetPreviewDetailsVisible(true);

        if (previewImage != null)
        {
            previewImage.sprite = currentSprite;
            previewImage.enabled = currentSprite != null;
        }

        if (previewDescriptionText != null)
        {
            previewDescriptionText.text = string.IsNullOrWhiteSpace(currentDescription)
                ? "Sin descripcion para este objeto."
                : currentDescription;
        }

        if (statusText != null)
        {
            statusText.text = "Vista previa cargada.";
        }
    }

    private void HandlePickPressed()
    {
        onPick?.Invoke();
    }

    private void SetPreviewDetailsVisible(bool visible)
    {
        if (previewDetailsRoot != null)
        {
            previewDetailsRoot.SetActive(visible);
            return;
        }

        // Si no hay contenedor dedicado de detalles, evitamos ocultar el previewContainer
        // para no desaparecer botones que puedan estar dentro de el.
        if (!visible)
        {
            return;
        }

        if (previewContainer != null)
        {
            previewContainer.SetActive(true);
        }
    }
}
