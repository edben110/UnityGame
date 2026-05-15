using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemEntryUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private InventoryItemDragHandler dragHandler;
    [SerializeField] private GameObject actionsRoot;
    [SerializeField] private Button viewButton;
    [SerializeField] private Button selectButton;

    private string itemId;
    private Action<string> onEntryClicked;
    private Action<string> onViewPressed;
    private Action<string> onSelectPressed;
    private Button rootButton;
    private Image rootImage;

    private void Awake()
    {
        if (GetComponentInParent<ScrollRect>() == null)
        {
            gameObject.SetActive(false);
            return;
        }

        EnsureRootClickTarget();
    }

    public string ItemId => itemId;

    public void Setup(
        string itemId,
        string displayName,
        Sprite icon,
        Action<string> entryClickAction,
        Action<string> viewAction,
        Action<string> selectAction)
    {
        this.itemId = itemId;
        onEntryClicked = entryClickAction;
        onViewPressed = viewAction;
        onSelectPressed = selectAction;

        if (labelText != null)
        {
            labelText.text = displayName;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = 12f;
            labelText.fontSizeMax = labelText.fontSize > 0f ? labelText.fontSize : 20f;
            labelText.overflowMode = TextOverflowModes.Ellipsis;
            labelText.raycastTarget = false;
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.transform.SetAsFirstSibling();
        }

        if (dragHandler != null)
        {
            dragHandler.Configure(itemId, icon);
        }

        if (labelText != null)
        {
            labelText.transform.SetSiblingIndex(1);
        }

        EnsureActionButtons();
        SetExpanded(false);

        if (actionsRoot != null)
        {
            actionsRoot.SetActive(false);
        }

        if (viewButton != null)
        {
            viewButton.onClick.RemoveAllListeners();
            viewButton.onClick.AddListener(() => onViewPressed?.Invoke(this.itemId));
            ConfigureButtonVisual(viewButton, "Ver");
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelectPressed?.Invoke(this.itemId));
            ConfigureButtonVisual(selectButton, "Seleccionar");
        }

        // Añadir LayoutElement al entry root para mejor spacing
        LayoutElement layout = GetComponent<LayoutElement>();
        if(layout == null)
        {
            layout = gameObject.AddComponent<LayoutElement>();
        }
        layout.preferredHeight = 80f;
        layout.minHeight = 70f;
        layout.preferredWidth = -1f;
        layout.minWidth = 0f;
        layout.flexibleWidth = 1f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (rootButton == null)
        {
            onEntryClicked?.Invoke(itemId);
        }
    }

    public void SetExpanded(bool expanded)
    {
        if (actionsRoot != null)
        {
            actionsRoot.SetActive(expanded);
        }
    }

    public void SetSelected(bool selected)
    {
        Image bg = GetComponent<Image>();
        if (bg != null)
        {
            bg.color = selected ? new Color(0.74f, 0.88f, 0.74f, 0.2f) : new Color(1f, 1f, 1f, 0f);
        }
    }

    private void EnsureActionButtons()
    {
        if (actionsRoot == null)
        {
            GameObject rootObject = new GameObject("Actions", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rootObject.transform.SetParent(transform, false);
            actionsRoot = rootObject;

            RectTransform rootRect = rootObject.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.45f, 0.05f);
            rootRect.anchorMax = new Vector2(0.98f, 0.35f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = rootObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.spacing = 8f;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
        }

        HorizontalLayoutGroup existingLayout = actionsRoot.GetComponent<HorizontalLayoutGroup>();
        if (existingLayout != null)
        {
            existingLayout.padding = new RectOffset(0, 0, 0, 0);
            existingLayout.childAlignment = TextAnchor.MiddleRight;
            existingLayout.spacing = 8f;
            existingLayout.childForceExpandWidth = false;
            existingLayout.childForceExpandHeight = false;
            existingLayout.childControlWidth = true;
            existingLayout.childControlHeight = true;
        }

        actionsRoot.SetActive(false);

        if (viewButton == null)
        {
            viewButton = CreateActionButton("VerBtn", "Ver");
        }

        if (viewButton != null)
        {
            viewButton.transform.SetParent(actionsRoot.transform, false);
            // Ajusta tamaño dinámico según el contenido
            LayoutElement viewLayout = viewButton.GetComponent<LayoutElement>();
            if(viewLayout == null) viewLayout = viewButton.gameObject.AddComponent<LayoutElement>();
            viewLayout.preferredWidth = -1; // Auto
            viewLayout.preferredHeight = 34f;
        }

        if (selectButton == null)
        {
            selectButton = CreateActionButton("SeleccionarBtn", "Seleccionar");
        }

        if (selectButton != null)
        {
            selectButton.transform.SetParent(actionsRoot.transform, false);
            // Ajusta tamaño dinámico según el contenido
            LayoutElement selectLayout = selectButton.GetComponent<LayoutElement>();
            if(selectLayout == null) selectLayout = selectButton.gameObject.AddComponent<LayoutElement>();
            selectLayout.preferredWidth = -1; // Auto
            selectLayout.preferredHeight = 34f;
        }

        Transform strayCanvas = actionsRoot.transform.Find("Canvas");
        if (strayCanvas != null)
        {
            for (int i = 0; i < strayCanvas.childCount; i++)
            {
                strayCanvas.GetChild(i).SetParent(actionsRoot.transform, false);
            }

            Destroy(strayCanvas.gameObject);
        }
    }

    private Button CreateActionButton(string objectName, string label)
    {
        if (actionsRoot == null)
        {
            return null;
        }

        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(actionsRoot.transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(110f, 34f);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 34f;
        layoutElement.minHeight = 30f;
        layoutElement.preferredWidth = -1; // Auto
        layoutElement.minWidth = 70f;

        Image background = buttonObject.GetComponent<Image>();
        // Botones blancos para mejor legibilidad
        background.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();

        GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 24f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = 24f;
        text.color = Color.black;
        text.raycastTarget = false;

        return button;
    }

    private void EnsureRootClickTarget()
    {
        rootButton = GetComponent<Button>();
        rootImage = GetComponent<Image>();

        if (rootImage == null)
        {
            rootImage = gameObject.AddComponent<Image>();
        }

        rootImage.color = new Color(1f, 1f, 1f, 0f);
        rootImage.raycastTarget = true;

        if (rootButton == null)
        {
            rootButton = gameObject.AddComponent<Button>();
        }

        rootButton.transition = Selectable.Transition.None;
        rootButton.targetGraphic = rootImage;
        rootButton.onClick.RemoveAllListeners();
        rootButton.onClick.AddListener(() => onEntryClicked?.Invoke(itemId));
    }

    private static void ConfigureButtonVisual(Button button, string label)
    {
        if (button == null)
        {
            return;
        }

        LayoutElement layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = button.gameObject.AddComponent<LayoutElement>();
        }

        // Altura fija, ancho adaptable
        layoutElement.preferredHeight = 34f;
        layoutElement.minHeight = 30f;
        layoutElement.preferredWidth = -1; // Auto según contenido
        layoutElement.minWidth = 70f;

        Image background = button.GetComponent<Image>();
        if (background != null)
        {
            // Botones blancos consistentes
            background.color = Color.white;
        }

        // Asegurar que el texto sea dinámico
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if(buttonText != null)
        {
            buttonText.text = label;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = 14f;
            buttonText.fontSizeMax = 24f;
            buttonText.color = Color.black;
            buttonText.alignment = TextAlignmentOptions.Center;
        }
    }
}
