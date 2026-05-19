using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Crea y gestiona el Canvas fullscreen del nuevo inventario RPG.
/// Estructura:
///   InventoryOverlayCanvas (Canvas, CanvasScaler, GraphicRaycaster)
///     └─ OverlayRoot
///         └─ DarkOverlay (Image negro semiopaco, fullscreen)
///         └─ InventoryContainer (RectTransform centrado, márgenes mínimos)
///             └─ InventoryBackgroundImage (Image con sprite Inventario.png.jpeg, AspectRatioFitter 3:2)
///                 ├─ ZonaGrilla (marcador: grilla de ítems, izquierda)
///                 ├─ ZonaPreview (marcador: preview ítem, derecha superior)
///                 ├─ ZonaDescripcion (marcador: descripción ítem, derecha inferior)
///                 ├─ ZonaMochila (marcador: ícono mochila, esquina inferior izquierda)
///                 └─ ZonaCerrar (marcador: botón X, esquina superior derecha)
///
/// El Canvas inicia DESACTIVADO. Se activa/desactiva externamente.
/// NO implementa lógica de slots, preview, pausa ni input.
/// Las zonas son SOLO marcadores estructurales para futuras tareas.
/// </summary>
public class InventoryOverlayCanvas : MonoBehaviour
{
    public static InventoryOverlayCanvas Instance { get; private set; }

    [Header("Referencias (auto-generadas)")]
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private RectTransform inventoryContainer;
    [SerializeField] private Image inventoryBackgroundImage;

    [Header("Zonas funcionales (marcadores estructurales)")]
    [SerializeField] private RectTransform zonaGrilla;
    [SerializeField] private RectTransform zonaPreview;
    [SerializeField] private RectTransform zonaDescripcion;
    [SerializeField] private RectTransform zonaMochila;
    [SerializeField] private RectTransform zonaCerrar;

    [Header("Configuración visual")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private int sortingOrder = 100;



    [Header("Sprite del inventario")]
    [Tooltip("Ruta del sprite en Resources/ (sin extensión). Unity ignora extensiones en Resources.Load.")]
    [SerializeField] private string inventorySpritePath = "Sprites/Inventario.png";

    [Header("Grilla de Slots")]
    [Tooltip("Prefab del slot de inventario. Si es null, se carga desde Resources/Prefabs/InventorySlot.")]
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private int gridColumns = 8;
    [SerializeField] private int gridRows = 6;
    [SerializeField] private Vector2 cellSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 cellSpacing = new Vector2(4f, 4f);
    [SerializeField] private RectOffset gridPadding;

    [Header("Offset fino de la grilla")]
    [Tooltip("Desplazamiento horizontal adicional (px). Positivo = mueve la grilla a la derecha.")]
    [SerializeField] private float gridOffsetX = 8f;
    [Tooltip("Desplazamiento vertical adicional (px). Positivo = mueve la grilla hacia abajo.")]
    [SerializeField] private float gridOffsetY = 8f;

    [Header("Preview del ítem seleccionado")]
    [Tooltip("Imagen UI para mostrar el preview grande del ítem seleccionado. Se crea automáticamente dentro de ZonaPreview si es null.")]
    [SerializeField] private Image previewItemImage;

    [Header("Seleccion de item")]
    [Tooltip("Botón para seleccionar el item activo desde el inventario.")]
    [SerializeField] private Button selectItemButton;
    [SerializeField] private TextMeshProUGUI selectItemButtonText;
    [SerializeField] private string selectItemButtonLabel = "Seleccionar";

    [Header("Textos de ZonaDescripcion")]
    [Tooltip("TextMeshPro para el nombre del ítem seleccionado. Se crea automáticamente dentro de ZonaDescripcion si es null.")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [Tooltip("TextMeshPro para la descripción del ítem seleccionado. Se crea automáticamente dentro de ZonaDescripcion si es null.")]
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    [Header("Contador de ítems (ZonaMochila)")]
    [Tooltip("TextMeshPro que muestra el contador X / 48 dentro de ZonaMochila. Se crea automáticamente si es null.")]
    [SerializeField] private TextMeshProUGUI inventoryCountText;

    [Header("Botón HUD Toggle Inventario")]
    [Tooltip("Botón persistente en pantalla para abrir/cerrar el inventario. Se crea automáticamente si es null.")]
    [SerializeField] private Button btnToggleInventory;

    /// <summary>Índice del slot actualmente seleccionado. -1 = ninguno.</summary>
    private int selectedSlotIndex = -1;
    private string selectedSlotItemId = string.Empty;
    private List<string> currentItemsCache = new List<string>();

    /// <summary>Referencia al contenedor central donde se colocará el sprite del inventario.</summary>
    public RectTransform Container => inventoryContainer;

    /// <summary>Referencia a la imagen de fondo del inventario.</summary>
    public Image BackgroundImage => inventoryBackgroundImage;

    /// <summary>Zona de la grilla de ítems (izquierda).</summary>
    public RectTransform ZonaGrilla => zonaGrilla;

    /// <summary>Zona de preview del ítem (derecha superior).</summary>
    public RectTransform ZonaPreview => zonaPreview;

    /// <summary>Zona de descripción del ítem (derecha inferior).</summary>
    public RectTransform ZonaDescripcion => zonaDescripcion;

    /// <summary>Zona del ícono de mochila (esquina inferior izquierda).</summary>
    public RectTransform ZonaMochila => zonaMochila;

    /// <summary>Zona del botón cerrar (esquina superior derecha).</summary>
    public RectTransform ZonaCerrar => zonaCerrar;

    /// <summary>Lista de todos los slots instanciados en la grilla (0..47).</summary>
    public IReadOnlyList<InventorySlotUI> Slots => slots;

    /// <summary>Indica si el overlay del inventario está visible.</summary>
    public bool IsOpen => overlayRoot != null && overlayRoot.activeSelf;

    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();

    /// <summary>Indica si ya se completó la construcción inicial de la grilla.</summary>
    private bool gridReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Inicializar RectOffset aquí en vez de como campo de clase,
        // porque Unity no permite llamar set_left desde un constructor de MonoBehaviour.
        if (gridPadding == null)
        {
            gridPadding = new RectOffset(8, 4, 8, 4);
        }

        BuildUI();

        // --- Diagnóstico temporal: verificar estado de zonaGrilla antes de BuildInventoryGrid ---
        if (zonaGrilla == null)
        {
            Debug.LogWarning("InventoryOverlayCanvas [DIAG]: zonaGrilla es NULL antes de BuildInventoryGrid(). " +
                "BuildZonas() no la asignó correctamente o inventoryBackgroundImage falló.");
        }
        else
        {
            Debug.Log($"InventoryOverlayCanvas [DIAG]: zonaGrilla encontrado: '{zonaGrilla.name}', " +
                $"parent: '{(zonaGrilla.parent != null ? zonaGrilla.parent.name : "NULL")}', " +
                $"position: {zonaGrilla.anchoredPosition}, size: {zonaGrilla.rect.size}");
        }

        BuildInventoryGrid();
        SubscribeToInventory();
        Hide();
    }

    private void OnEnable()
    {
        SubscribeToInventory();
        // Refrescar al activarse por si el inventario cambió mientras estaba oculto
        if (gridReady)
        {
            RefreshGrid();
        }
        RefreshInventoryCount();
    }

    private void OnDisable()
    {
        // NO desuscribirse aquí: el overlay se oculta via overlayRoot.SetActive(false),
        // no desactivando el GameObject. Pero si el GameObject se desactiva externamente,
        // mantenemos la suscripción para que al reactivarse tenga datos frescos.
        // La desuscripción real ocurre en OnDestroy.
    }

    private void Update()
    {
        // Toggle inventario con tecla I (New Input System, consistente con ClickManager)
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (!GameInputBlocker.IsBlocked)
            {
                Toggle();
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromInventory();

        if (btnToggleInventory != null)
        {
            btnToggleInventory.onClick.RemoveAllListeners();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private bool subscribedToInventory;

    private void SubscribeToInventory()
    {
        if (!subscribedToInventory)
        {
            InventoryState.Changed += OnInventoryChanged;
            subscribedToInventory = true;
        }
    }

    private void UnsubscribeFromInventory()
    {
        if (subscribedToInventory)
        {
            InventoryState.Changed -= OnInventoryChanged;
            subscribedToInventory = false;
        }
    }

    /// <summary>Muestra el inventario fullscreen.</summary>
    public void Show()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(true);
        }

        // Al abrir: limpiar selección previa para estado inicial limpio
        ClearSelection();

        // Refrescar al abrir para mostrar estado actual
        if (gridReady)
        {
            RefreshGrid();
        }

        // Actualizar contador inmediatamente al abrir
        RefreshInventoryCount();
    }

    /// <summary>Oculta el inventario fullscreen.</summary>
    public void Hide()
    {
        CloseActivePuzzleIfOpen();

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }

        // Restaurar cursor al cerrar el inventario (por si estaba en modo mano sobre un slot)
        CursorManager.SetDefault();
    }

    /// <summary>Alterna visibilidad del inventario.</summary>
    public void Toggle()
    {
        if (IsOpen)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private static void CloseActivePuzzleIfOpen()
    {
        if (Acertijo2PuzzleService.Instance != null && Acertijo2PuzzleService.Instance.IsOpen)
        {
            Acertijo2PuzzleService.Instance.ClosePuzzle();
            return;
        }

        if (AcertijoPuzzleService.Instance != null && AcertijoPuzzleService.Instance.IsOpen)
        {
            AcertijoPuzzleService.Instance.ClosePuzzle();
        }
    }

    private void BuildUI()
    {
        // --- Guard: Si ya existe la jerarquía construida, no recrear ---
        // Esto previene duplicación de objetos y NullReferenceException por referencias huérfanas.
        if (overlayRoot != null)
        {
            // La UI ya fue construida (posiblemente serializada desde una ejecución previa).
            // Verificar que zonaGrilla siga siendo válida.
            if (zonaGrilla != null)
            {
                Debug.Log("InventoryOverlayCanvas: BuildUI() omitido — jerarquía ya existe.");

                return;
            }
            else
            {
                // overlayRoot existe pero zonaGrilla no — estado inconsistente.
                // Destruir y reconstruir.
                DestroyImmediate(overlayRoot);
                overlayRoot = null;
                inventoryBackgroundImage = null;
                inventoryContainer = null;
                zonaGrilla = null;
            }
        }

        // --- Canvas Root ---
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        // --- Canvas Scaler (consistente con el Canvas principal del proyecto) ---
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // --- Graphic Raycaster ---
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // --- Overlay Root (contenedor que se activa/desactiva) ---
        overlayRoot = new GameObject("OverlayRoot");
        overlayRoot.transform.SetParent(transform, false);

        RectTransform overlayRootRect = overlayRoot.AddComponent<RectTransform>();
        StretchToFill(overlayRootRect);

        // --- Dark Overlay (fondo negro semiopaco) ---
        GameObject darkOverlayObj = new GameObject("DarkOverlay");
        darkOverlayObj.transform.SetParent(overlayRoot.transform, false);

        RectTransform darkRect = darkOverlayObj.AddComponent<RectTransform>();
        StretchToFill(darkRect);

        darkOverlayObj.AddComponent<CanvasRenderer>();
        darkOverlay = darkOverlayObj.AddComponent<Image>();
        darkOverlay.color = overlayColor;
        darkOverlay.raycastTarget = true;

        // --- Inventory Container (contenedor central) ---
        GameObject containerObj = new GameObject("InventoryContainer");
        containerObj.transform.SetParent(overlayRoot.transform, false);

        inventoryContainer = containerObj.AddComponent<RectTransform>();
        // Centrado, ocupa ~96% del espacio (margen 2% por lado) para maximizar tamaño
        // sin que el sprite se salga de pantalla
        inventoryContainer.anchorMin = new Vector2(0.02f, 0.02f);
        inventoryContainer.anchorMax = new Vector2(0.98f, 0.98f);
        inventoryContainer.offsetMin = Vector2.zero;
        inventoryContainer.offsetMax = Vector2.zero;

        // --- Inventory Background Image (sprite visual del inventario) ---
        BuildInventoryBackgroundImage();

        // --- Zonas funcionales (marcadores estructurales) ---
        BuildZonas();

        // --- Preview Image dentro de ZonaPreview ---
        BuildPreviewImage();

        // --- Botón Seleccionar dentro de ZonaPreview ---
        BuildSelectButton();

        // --- Textos de nombre y descripción dentro de ZonaDescripcion ---
        BuildDescriptionTexts();

        // --- Contador de ítems dentro de ZonaMochila ---
        BuildInventoryCountText();

    }

    /// <summary>
    /// Crea el Image de preview grande dentro de ZonaPreview.
    /// Centrado, preserveAspect activado, inicialmente vacío/transparente.
    /// </summary>
    private void BuildPreviewImage()
    {
        if (zonaPreview == null)
        {
            Debug.LogWarning("InventoryOverlayCanvas: ZonaPreview es null. No se puede crear PreviewItemImage.");
            return;
        }

        // Si ya existe (serializado o de ejecución previa), no recrear
        if (previewItemImage != null)
        {
            return;
        }

        // Buscar si ya existe un hijo llamado PreviewItemImage
        Transform existing = zonaPreview.Find("PreviewItemImage");
        if (existing != null)
        {
            previewItemImage = existing.GetComponent<Image>();
            if (previewItemImage != null)
            {
                return;
            }
        }

        GameObject previewObj = new GameObject("PreviewItemImage");
        previewObj.transform.SetParent(zonaPreview, false);

        RectTransform previewRect = previewObj.AddComponent<RectTransform>();
        // Centrado dentro de ZonaPreview con margen del 10% por lado
        previewRect.anchorMin = new Vector2(0.10f, 0.10f);
        previewRect.anchorMax = new Vector2(0.90f, 0.90f);
        previewRect.offsetMin = Vector2.zero;
        previewRect.offsetMax = Vector2.zero;
        previewRect.pivot = new Vector2(0.5f, 0.5f);

        previewObj.AddComponent<CanvasRenderer>();
        previewItemImage = previewObj.AddComponent<Image>();
        previewItemImage.sprite = null;
        previewItemImage.color = new Color(1f, 1f, 1f, 0f); // Transparente al inicio
        previewItemImage.preserveAspect = true;
        previewItemImage.raycastTarget = false;
        previewItemImage.enabled = true; // Siempre habilitado, controlamos visibilidad con alpha/sprite
    }

    private void BuildSelectButton()
    {
        if (zonaPreview == null)
        {
            Debug.LogWarning("InventoryOverlayCanvas: ZonaPreview es null. No se puede crear el boton Seleccionar.");
            return;
        }

        if (selectItemButton != null)
        {
            return;
        }

        Transform existing = zonaPreview.Find("SelectItemButton");
        if (existing != null)
        {
            selectItemButton = existing.GetComponent<Button>();
            if (selectItemButton != null)
            {
                selectItemButtonText = selectItemButton.GetComponentInChildren<TextMeshProUGUI>(true);
                return;
            }
        }

        GameObject buttonObj = new GameObject("SelectItemButton");
        buttonObj.transform.SetParent(zonaPreview, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.22f, 0.02f);
        buttonRect.anchorMax = new Vector2(0.78f, 0.18f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);

        buttonObj.AddComponent<CanvasRenderer>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.28f, 0.22f, 0.18f, 0.9f);
        buttonImage.raycastTarget = true;

        selectItemButton = buttonObj.AddComponent<Button>();
        selectItemButton.transition = Selectable.Transition.None;
        selectItemButton.onClick.RemoveAllListeners();
        selectItemButton.onClick.AddListener(OnSelectButtonPressed);

        if (buttonObj.GetComponent<CursorHoverUI>() == null)
        {
            buttonObj.AddComponent<CursorHoverUI>();
        }

        GameObject labelObj = new GameObject("SelectItemButtonText");
        labelObj.transform.SetParent(buttonObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        StretchToFill(labelRect);

        selectItemButtonText = labelObj.AddComponent<TextMeshProUGUI>();
        selectItemButtonText.text = selectItemButtonLabel;
        selectItemButtonText.alignment = TextAlignmentOptions.Center;
        selectItemButtonText.fontSize = 16f;
        selectItemButtonText.color = new Color(0.92f, 0.82f, 0.55f, 1f);
        selectItemButtonText.raycastTarget = false;

        TMP_FontAsset rpgFont = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");
        if (rpgFont == null && TMP_Settings.instance != null)
        {
            rpgFont = TMP_Settings.defaultFontAsset;
        }
        if (rpgFont != null)
        {
            selectItemButtonText.font = rpgFont;
        }

        selectItemButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Crea los TextMeshProUGUI para nombre y descripción dentro de ZonaDescripcion.
    /// Estilo RPG: fuente Cinzel-Bold, colores cálidos sobre fondo oscuro.
    /// Inician vacíos (sin placeholders).
    /// </summary>
    private void BuildDescriptionTexts()
    {
        if (zonaDescripcion == null)
        {
            Debug.LogWarning("InventoryOverlayCanvas: ZonaDescripcion es null. No se pueden crear textos de descripción.");
            return;
        }

        // Cargar fuente RPG (Cinzel-Bold SDF) — misma que usa el resto del proyecto
        TMP_FontAsset rpgFont = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");
        if (rpgFont == null && TMP_Settings.instance != null)
        {
            rpgFont = TMP_Settings.defaultFontAsset;
        }

        // --- ItemNameText: parte superior de ZonaDescripcion ---
        if (itemNameText == null)
        {
            Transform existingName = zonaDescripcion.Find("ItemNameText");
            if (existingName != null)
            {
                itemNameText = existingName.GetComponent<TextMeshProUGUI>();
            }
        }

        if (itemNameText == null)
        {
            GameObject nameObj = new GameObject("ItemNameText");
            nameObj.transform.SetParent(zonaDescripcion, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            // Ocupa la parte superior (~35% de la zona)
            nameRect.anchorMin = new Vector2(0.05f, 0.65f);
            nameRect.anchorMax = new Vector2(0.95f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            nameRect.pivot = new Vector2(0.5f, 0.5f);

            itemNameText = nameObj.AddComponent<TextMeshProUGUI>();
            itemNameText.text = string.Empty;
            itemNameText.fontSize = 18f;
            itemNameText.fontStyle = FontStyles.Bold;
            itemNameText.color = new Color(0.92f, 0.82f, 0.55f, 1f); // Dorado cálido RPG
            itemNameText.alignment = TextAlignmentOptions.Center;
            itemNameText.enableWordWrapping = true;
            itemNameText.overflowMode = TextOverflowModes.Ellipsis;
            itemNameText.raycastTarget = false;

            if (rpgFont != null)
            {
                itemNameText.font = rpgFont;
            }
        }

        // --- ItemDescriptionText: parte inferior de ZonaDescripcion ---
        if (itemDescriptionText == null)
        {
            Transform existingDesc = zonaDescripcion.Find("ItemDescriptionText");
            if (existingDesc != null)
            {
                itemDescriptionText = existingDesc.GetComponent<TextMeshProUGUI>();
            }
        }

        if (itemDescriptionText == null)
        {
            GameObject descObj = new GameObject("ItemDescriptionText");
            descObj.transform.SetParent(zonaDescripcion, false);

            RectTransform descRect = descObj.AddComponent<RectTransform>();
            // Ocupa la parte inferior (~60% de la zona)
            descRect.anchorMin = new Vector2(0.05f, 0.05f);
            descRect.anchorMax = new Vector2(0.95f, 0.62f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            descRect.pivot = new Vector2(0.5f, 0.5f);

            itemDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
            itemDescriptionText.text = string.Empty;
            itemDescriptionText.fontSize = 13f;
            itemDescriptionText.fontStyle = FontStyles.Normal;
            itemDescriptionText.color = new Color(0.78f, 0.75f, 0.70f, 1f); // Gris claro pergamino
            itemDescriptionText.alignment = TextAlignmentOptions.TopLeft;
            itemDescriptionText.enableWordWrapping = true;
            itemDescriptionText.overflowMode = TextOverflowModes.Ellipsis;
            itemDescriptionText.raycastTarget = false;

            if (rpgFont != null)
            {
                itemDescriptionText.font = rpgFont;
            }
        }
    }

    /// <summary>
    /// Crea el TextMeshProUGUI del contador de ítems dentro de ZonaMochila.
    /// Formato: "0 / 48". Estilo RPG coherente con el inventario.
    /// Posicionado a la derecha del ícono de mochila para no invadirlo.
    /// </summary>
    private void BuildInventoryCountText()
    {
        if (zonaMochila == null)
        {
            Debug.LogWarning("InventoryOverlayCanvas: ZonaMochila es null. No se puede crear InventoryCountText.");
            return;
        }

        // Si ya existe (serializado o de ejecución previa), no recrear
        if (inventoryCountText != null) return;

        // Buscar si ya existe un hijo llamado InventoryCountText
        Transform existing = zonaMochila.Find("InventoryCountText");
        if (existing != null)
        {
            inventoryCountText = existing.GetComponent<TextMeshProUGUI>();
            if (inventoryCountText != null)
            {
                RefreshInventoryCount();
                return;
            }
        }

        // Cargar fuente RPG (Cinzel-Bold SDF)
        TMP_FontAsset rpgFont = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");
        if (rpgFont == null && TMP_Settings.instance != null)
        {
            rpgFont = TMP_Settings.defaultFontAsset;
        }

        GameObject countObj = new GameObject("InventoryCountText");
        countObj.transform.SetParent(zonaMochila, false);

        RectTransform countRect = countObj.AddComponent<RectTransform>();
        // Posicionado en la mitad derecha de ZonaMochila (junto al ícono de mochila)
        countRect.anchorMin = new Vector2(0.35f, 0.05f);
        countRect.anchorMax = new Vector2(0.95f, 0.95f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;
        countRect.pivot = new Vector2(0.5f, 0.5f);

        inventoryCountText = countObj.AddComponent<TextMeshProUGUI>();
        inventoryCountText.text = "0 / 48";
        inventoryCountText.fontSize = 22f;
        inventoryCountText.fontStyle = FontStyles.Bold;
        inventoryCountText.color = new Color(0.90f, 0.85f, 0.70f, 1f); // Beige cálido RPG
        inventoryCountText.alignment = TextAlignmentOptions.MidlineLeft;
        inventoryCountText.enableWordWrapping = false;
        inventoryCountText.overflowMode = TextOverflowModes.Overflow;
        inventoryCountText.raycastTarget = false;

        if (rpgFont != null)
        {
            inventoryCountText.font = rpgFont;
        }

        // Valor inicial correcto
        RefreshInventoryCount();
    }



    private void BuildInventoryBackgroundImage()
    {
        GameObject imageObj = new GameObject("InventoryBackgroundImage");
        imageObj.transform.SetParent(inventoryContainer, false);

        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        // Stretch para llenar el contenedor, el AspectRatioFitter controlará el tamaño real
        StretchToFill(imageRect);

        imageObj.AddComponent<CanvasRenderer>();
        inventoryBackgroundImage = imageObj.AddComponent<Image>();
        inventoryBackgroundImage.raycastTarget = false;
        inventoryBackgroundImage.preserveAspect = true;

        // Cargar el sprite del inventario
        Sprite inventorySprite = LoadInventorySprite();

        if (inventorySprite != null)
        {
            inventoryBackgroundImage.sprite = inventorySprite;
            inventoryBackgroundImage.type = Image.Type.Simple;
            inventoryBackgroundImage.color = Color.white;

            // AspectRatioFitter para mantener proporciones sin deformar
            // El sprite es 1536x1024 → ratio = 1.5 (3:2)
            AspectRatioFitter fitter = imageObj.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = (float)inventorySprite.texture.width / inventorySprite.texture.height;
        }
        else
        {
            Debug.LogWarning(
                "InventoryOverlayCanvas: No se pudo cargar el sprite del inventario. " +
                "Verifica que 'Inventario.png.jpeg' esté en Assets/Sprites/ y movido a Resources/Sprites/ " +
                "o asignado manualmente en el Inspector.");
            // Placeholder visual para debug
            inventoryBackgroundImage.color = new Color(0.2f, 0.1f, 0.1f, 0.5f);
        }
    }

    private Sprite LoadInventorySprite()
    {
        // El archivo se llama "Inventario.png.jpeg" — nombre inusual con doble extensión.
        // Unity en Resources.Load ignora extensiones, pero el nombre base puede variar.
        // Probamos múltiples rutas para máxima compatibilidad.

        string[] pathsToTry = new string[]
        {
            inventorySpritePath,                // "Sprites/Inventario.png"
            "Sprites/Inventario.png.jpeg",      // nombre completo (Unity puede ignorar .jpeg)
            "Sprites/Inventario",               // sin extensión
        };

        foreach (string path in pathsToTry)
        {
            // Intento como Sprite directamente
            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }

            // Intento como array de sub-sprites (Multiple mode)
            Sprite[] allSprites = Resources.LoadAll<Sprite>(path);
            if (allSprites != null && allSprites.Length > 0)
            {
                return allSprites[0];
            }

            // Intento como Texture2D y crear sprite manualmente
            Texture2D tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        return null;
    }

    // ==================== ZONAS FUNCIONALES ====================

    /// <summary>
    /// Crea los GameObjects vacíos que sirven como marcadores/layout para las zonas
    /// funcionales del inventario. Posicionados sobre las áreas correspondientes del sprite.
    /// SOLO estructural — sin lógica, botones, textos ni scripts funcionales.
    /// </summary>
    private void BuildZonas()
    {
        RectTransform parent = inventoryBackgroundImage.GetComponent<RectTransform>();

        // --- ZonaGrilla: Parte izquierda, cubre el área de casillas del sprite ---
        // Ajuste: anchorMax.x 0.61→0.65 para cubrir casillas faltantes del lado derecho.
        // Altura sin cambios. No invade panel derecho (separador ~0.68).
        zonaGrilla = CreateZoneMarker("ZonaGrilla", parent,
            anchorMin: new Vector2(0.05f, 0.20f),
            anchorMax: new Vector2(0.65f, 0.88f)
        );

        // --- ZonaPreview: Panel derecho superior, para imagen grande del ítem ---
        // Ajuste: movida +0.02 en X y +0.03 en Y (misma proporción en Min y Max)
        // para reubicarse dentro del marco decorativo superior derecho.
        // Tamaño idéntico: ancho 0.24, alto 0.35 (sin cambios).
        zonaPreview = CreateZoneMarker("ZonaPreview", parent,
            anchorMin: new Vector2(0.71f, 0.53f),
            anchorMax: new Vector2(0.95f, 0.88f)
        );

        // --- ZonaDescripcion: Panel derecho inferior, para nombre y descripción ---
        // Ajuste: reducido ancho horizontal simétrico (+0.04 en Min.x, -0.04 en Max.x).
        // Misma altura total (24%), centrada dentro del marco ornamental inferior.
        zonaDescripcion = CreateZoneMarker("ZonaDescripcion", parent,
            anchorMin: new Vector2(0.73f, 0.14f),
            anchorMax: new Vector2(0.91f, 0.38f)
        );

        // --- ZonaMochila: Esquina inferior izquierda, sobre ícono de mochila ---
        // Ajuste fino: anchorMax.x 0.15→0.22 (más ancho para futuro contador/texto/ícono)
        zonaMochila = CreateZoneMarker("ZonaMochila", parent,
            anchorMin: new Vector2(0.03f, 0.08f),
            anchorMax: new Vector2(0.22f, 0.18f)
        );

        // --- ZonaCerrar: Esquina superior derecha, sobre botón X del sprite ---
        zonaCerrar = CreateZoneMarker("ZonaCerrar", parent,
            anchorMin: new Vector2(0.92f, 0.90f),
            anchorMax: new Vector2(0.99f, 0.98f)
        );

        // --- Hacer ZonaCerrar funcional como botón de cierre ---
        SetupCloseButton(zonaCerrar);
    }

    /// <summary>
    /// Crea un GameObject vacío con RectTransform como marcador de zona.
    /// </summary>
    private RectTransform CreateZoneMarker(string zoneName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject zoneObj = new GameObject(zoneName);
        zoneObj.transform.SetParent(parent, false);

        RectTransform rect = zoneObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);

        return rect;
    }



    /// <summary>
    /// Configura ZonaCerrar como un botón funcional que cierra el inventario.
    /// Agrega Image (transparente, raycastTarget=true) y Button con listener a Hide().
    /// </summary>
    private void SetupCloseButton(RectTransform zone)
    {
        if (zone == null) return;

        GameObject zoneObj = zone.gameObject;

        // Asegurar que tiene Image con raycastTarget=true para capturar clics.
        // Si ya existe una Image (debug), solo activar raycastTarget.
        // Si no existe, crear una transparente.
        Image img = zoneObj.GetComponent<Image>();
        if (img == null)
        {
            if (zoneObj.GetComponent<CanvasRenderer>() == null)
            {
                zoneObj.AddComponent<CanvasRenderer>();
            }
            img = zoneObj.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // Completamente transparente
        }
        img.raycastTarget = true;

        // Agregar Button si no existe
        Button btn = zoneObj.GetComponent<Button>();
        if (btn == null)
        {
            btn = zoneObj.AddComponent<Button>();
        }

        // Transición None para que no altere el color del sprite subyacente
        btn.transition = Selectable.Transition.None;

        // Conectar onClick a Hide()
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Hide);

        // Cursor hover: cambiar a mano al pasar sobre el botón X
        if (zoneObj.GetComponent<CursorHoverUI>() == null)
        {
            zoneObj.AddComponent<CursorHoverUI>();
        }
    }

    private static void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    // ==================== SELECCIÓN Y PREVIEW ====================

    /// <summary>
    /// Callback invocado cuando el jugador hace clic en un slot.
    /// Comportamiento elegido para slots vacíos: LIMPIAR preview y deseleccionar.
    /// Esto mantiene un estado visual consistente y predecible.
    /// </summary>
    private void HandleSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            return;
        }

        InventorySlotUI clickedSlot = slots[slotIndex];
        if (clickedSlot == null)
        {
            return;
        }

        // Si el slot NO tiene ítem: limpiar selección y preview
        if (!clickedSlot.HasItem)
        {
            ClearSelection();
            return;
        }

        string clickedItemId = GetItemIdForSlot(slotIndex);
        if (InventoryUsableItems.IsUsable(clickedItemId))
        {
            InventoryUsableItems.TryUse(clickedItemId);
            return;
        }

        // Si se hace clic en el slot ya seleccionado: deseleccionar (toggle)
        if (selectedSlotIndex == slotIndex)
        {
            ClearSelection();
            return;
        }

        // Deseleccionar slot anterior
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
        {
            InventorySlotUI previousSlot = slots[selectedSlotIndex];
            if (previousSlot != null)
            {
                previousSlot.SetSelected(false);
            }
        }

        // Seleccionar nuevo slot
        selectedSlotIndex = slotIndex;
        clickedSlot.SetSelected(true);

        // Actualizar preview con el sprite del ítem
        UpdatePreviewImage(clickedSlot.CurrentSprite);

        // Actualizar textos de nombre y descripción
        UpdateDescriptionTexts(slotIndex);

        string itemId = GetItemIdForSlot(slotIndex);
        UpdateSelectButtonState(!string.IsNullOrWhiteSpace(itemId), itemId);
    }

    /// <summary>
    /// Limpia la selección actual: deselecciona el slot y vacía el preview y textos.
    /// </summary>
    private void ClearSelection()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
        {
            InventorySlotUI previousSlot = slots[selectedSlotIndex];
            if (previousSlot != null)
            {
                previousSlot.SetSelected(false);
            }
        }

        selectedSlotIndex = -1;
        selectedSlotItemId = string.Empty;
        UpdatePreviewImage(null);
        ClearDescriptionTexts();
        UpdateSelectButtonState(false, string.Empty);
    }

    /// <summary>
    /// Actualiza la imagen de preview grande en ZonaPreview.
    /// Si sprite es null, el preview se muestra vacío/transparente.
    /// </summary>
    private void UpdatePreviewImage(Sprite sprite)
    {
        if (previewItemImage == null)
        {
            return;
        }

        previewItemImage.sprite = sprite;
        previewItemImage.color = sprite != null
            ? Color.white
            : new Color(1f, 1f, 1f, 0f); // Transparente cuando vacío
    }

    /// <summary>
    /// Actualiza los textos de nombre y descripción en ZonaDescripcion
    /// a partir del ítem en el slot indicado.
    /// Obtiene el itemId desde InventoryState y busca la definición en InventoryCatalog.
    /// </summary>
    private void UpdateDescriptionTexts(int slotIndex)
    {
        List<string> currentItems = currentItemsCache ?? InventoryState.GetItems();
        InventoryCatalog catalog = InventoryCatalog.Instance;

        if (slotIndex < 0 || slotIndex >= currentItems.Count || catalog == null)
        {
            ClearDescriptionTexts();
            return;
        }

        string itemId = currentItems[slotIndex];
        InventoryNarrativeDefaults.EnsureItemRegistered(itemId);

        if (catalog.TryGet(itemId, out InventoryItemDefinition definition) && definition != null)
        {
            if (itemNameText != null)
            {
                itemNameText.text = !string.IsNullOrWhiteSpace(definition.displayName)
                    ? definition.displayName
                    : InventoryNarrativeDefaults.GetDefaultDisplayName(itemId);
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = !string.IsNullOrWhiteSpace(definition.description)
                    ? definition.description
                    : InventoryNarrativeDefaults.GetDefaultDescription(itemId);
            }
        }
        else
        {
            if (itemNameText != null)
            {
                itemNameText.text = InventoryNarrativeDefaults.GetDefaultDisplayName(itemId);
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = InventoryNarrativeDefaults.GetDefaultDescription(itemId);
            }
        }
    }

    /// <summary>
    /// Limpia los textos de nombre y descripción en ZonaDescripcion.
    /// </summary>
    private void ClearDescriptionTexts()
    {
        if (itemNameText != null)
        {
            itemNameText.text = string.Empty;
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = string.Empty;
        }
    }

    private void UpdateSelectButtonState(bool visible, string itemId)
    {
        if (selectItemButton == null)
        {
            return;
        }

        selectedSlotItemId = visible ? itemId : string.Empty;
        selectItemButton.gameObject.SetActive(visible);
    }

    private void OnSelectButtonPressed()
    {
        if (string.IsNullOrWhiteSpace(selectedSlotItemId))
        {
            return;
        }

        InventoryState.SetSelectedItem(selectedSlotItemId);
    }

    private string GetItemIdForSlot(int slotIndex)
    {
        List<string> items = currentItemsCache ?? InventoryState.GetItems();
        if (slotIndex < 0 || slotIndex >= items.Count)
        {
            return string.Empty;
        }

        return items[slotIndex];
    }

    // ==================== CONTADOR DE ÍTEMS (ZONA MOCHILA) ====================

    /// <summary>Capacidad máxima del inventario.</summary>
    private const int MaxInventoryCapacity = 48;

    /// <summary>
    /// Actualiza el texto del contador de ítems en ZonaMochila.
    /// Formato: "X / 48" donde X = InventoryState.GetItems().Count.
    /// </summary>
    private void RefreshInventoryCount()
    {
        if (inventoryCountText == null) return;

        int currentCount = InventoryState.GetItems().Count;
        inventoryCountText.text = $"{currentCount} / {MaxInventoryCapacity}";
    }

    // ==================== SINCRONIZACIÓN CON INVENTARIO ====================

    /// <summary>
    /// Callback del evento InventoryState.Changed.
    /// Refresca la grilla visual y el contador cuando el inventario cambia.
    /// </summary>
    private void OnInventoryChanged()
    {
        if (gridReady)
        {
            RefreshGrid();
        }
        RefreshInventoryCount();
    }

    /// <summary>
    /// Lee los ítems actuales desde InventoryState y actualiza cada slot de la grilla.
    /// Slots con ítem: muestra el sprite obtenido desde InventoryCatalog.
    /// Slots vacíos: llama SetItem(null) para ocultar el ícono.
    /// Respeta el orden devuelto por InventoryState.GetItems().
    /// Si el slot seleccionado pierde su ítem, limpia la selección.
    /// </summary>
    public void RefreshGrid()
    {
        if (slots == null || slots.Count == 0)
        {
            return;
        }

        currentItemsCache = InventoryState.GetItems();
        InventoryCatalog catalog = InventoryCatalog.Instance;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotUI slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            if (i < currentItemsCache.Count)
            {
                string itemId = currentItemsCache[i];
                InventoryNarrativeDefaults.EnsureItemRegistered(itemId);

                Sprite icon = null;
                if (catalog != null && catalog.TryGet(itemId, out InventoryItemDefinition definition))
                {
                    icon = definition != null ? definition.icon : null;
                }

                if (icon == null)
                {
                    icon = InventoryProvisionalIcons.GetForItem(itemId);
                }

                slot.SetItem(icon);
            }
            else
            {
                slot.SetItem(null);
            }
        }

        // Si el slot seleccionado ya no tiene ítem, limpiar selección
        if (selectedSlotIndex >= 0)
        {
            if (selectedSlotIndex >= slots.Count || !slots[selectedSlotIndex].HasItem)
            {
                ClearSelection();
            }
            else
            {
                // Actualizar preview por si el sprite cambió
                UpdatePreviewImage(slots[selectedSlotIndex].CurrentSprite);
                UpdateSelectButtonState(true, GetItemIdForSlot(selectedSlotIndex));
            }
        }
    }

    // ==================== GRILLA DE SLOTS ====================

    /// <summary>
    /// Configura el GridLayoutGroup en ZonaGrilla e instancia 48 slots vacíos.
    /// Cada slot recibe su slotIndex (0..47) y queda alineado automáticamente por el layout.
    /// </summary>
    private void BuildInventoryGrid()
    {
        // Si zonaGrilla no fue asignada por BuildZonas() (ej: error previo, referencia perdida),
        // intentar encontrarla en la jerarquía antes de abortar.
        if (zonaGrilla == null && inventoryBackgroundImage != null)
        {
            Transform found = inventoryBackgroundImage.transform.Find("ZonaGrilla");
            if (found != null)
            {
                zonaGrilla = found as RectTransform;
            }
        }

        if (zonaGrilla == null)
        {
            Debug.LogError("InventoryOverlayCanvas: ZonaGrilla es null. No se puede generar la grilla.");
            return;
        }

        // --- Configurar GridLayoutGroup ---
        GridLayoutGroup grid = zonaGrilla.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = zonaGrilla.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = gridColumns;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.cellSize = cellSize;
        grid.spacing = cellSpacing;
        grid.padding = gridPadding;

        // --- Cargar prefab ---
        GameObject prefab = inventorySlotPrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Prefabs/InventorySlot");
        }

        // Si no se encontró en Resources ni en Inspector, intentar cargar desde cualquier ruta conocida
        if (prefab == null)
        {
            // Fallback: buscar en la escena si hay algún InventorySlot desactivado como template
            InventorySlotUI existingSlot = FindObjectOfType<InventorySlotUI>(true);
            if (existingSlot != null)
            {
                prefab = existingSlot.gameObject;
            }
        }

        if (prefab == null)
        {
            Debug.LogWarning(
                "InventoryOverlayCanvas: No se encontró el prefab InventorySlot en Resources ni en Inspector. " +
                "Generando slots por código como fallback.");
            GenerateSlotsFallback(grid);
            return;
        }

        // --- Limpiar slots previos (por si se ejecuta más de una vez) ---
        ClearExistingSlots();

        // --- Instanciar slots ---
        int totalSlots = gridColumns * gridRows; // 8 * 6 = 48
        Color borderColor = new Color(0.92f, 0.78f, 0.40f, 1.0f); // Dorado
        float borderThickness = 2f;

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slotObj = Instantiate(prefab, zonaGrilla);
            slotObj.name = $"Slot_{i:D2}";
            slotObj.transform.localScale = Vector3.one;

            // --- Neutralizar SelectionHighlight legacy del prefab ---
            NeutralizeSelectionHighlight(slotObj);

            // --- Agregar 4 bordes reales de selección ---
            GameObject borderTopObj = CreateBorderObject("BorderTop", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(0f, -borderThickness),
                offsetMax: Vector2.zero);

            GameObject borderBottomObj = CreateBorderObject("BorderBottom", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 0f),
                offsetMin: Vector2.zero,
                offsetMax: new Vector2(0f, borderThickness));

            GameObject borderLeftObj = CreateBorderObject("BorderLeft", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(0f, 1f),
                offsetMin: Vector2.zero,
                offsetMax: new Vector2(borderThickness, 0f));

            GameObject borderRightObj = CreateBorderObject("BorderRight", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(1f, 0f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(-borderThickness, 0f),
                offsetMax: Vector2.zero);

            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.SlotIndex = i;
                // Asignar referencias de bordes via reflection (campos [SerializeField] privados)
                AssignSlotReferences(slotUI,
                    slotUI.ItemIcon,
                    null, // selectionHighlight ya neutralizado, pasar null
                    borderTopObj, borderBottomObj, borderLeftObj, borderRightObj);
                slotUI.ClearSlot();
                slotUI.OnSlotClicked = HandleSlotClicked;
            }

            slots.Add(slotUI);
        }

        // --- Ajustar cellSize automáticamente si se desea que encaje perfecto ---
        // Esto recalcula el cellSize para que los 8x6 slots llenen ZonaGrilla sin overflow.
        AdjustCellSizeToFit(grid);

        Debug.Log($"InventoryOverlayCanvas: Grilla generada — {totalSlots} slots ({gridColumns}x{gridRows}) en ZonaGrilla.");

        gridReady = true;
        RefreshGrid();
    }

    /// <summary>
    /// Recalcula el cellSize del GridLayoutGroup para que los 48 slots (8x6) encajen
    /// perfectamente dentro de ZonaGrilla sin desbordarse.
    /// Siempre delega a una corrutina que espera un frame completo, garantizando que
    /// el AspectRatioFitter y todos los layouts hayan resuelto sus tamaños finales.
    /// </summary>
    private void AdjustCellSizeToFit(GridLayoutGroup grid)
    {
        // Siempre usar corrutina: el AspectRatioFitter (FitInParent) en InventoryBackgroundImage
        // necesita al menos un frame completo de layout para resolver el tamaño real de sus hijos.
        // Canvas.ForceUpdateCanvases() NO resuelve AspectRatioFitter en el mismo frame en Awake().
        StartCoroutine(AdjustCellSizeDeferred(grid));
    }

    /// <summary>
    /// Coroutine: espera un frame completo para que el layout se resuelva,
    /// luego recalcula el cellSize. Si aún no hay tamaño válido, espera un frame adicional.
    /// </summary>
    private IEnumerator AdjustCellSizeDeferred(GridLayoutGroup grid)
    {
        // Esperar un frame completo (yield return null) para que:
        // - CanvasScaler resuelva la escala
        // - AspectRatioFitter calcule el tamaño real de InventoryBackgroundImage
        // - Los anchors de ZonaGrilla se resuelvan en base al padre ya dimensionado
        yield return null;

        // Forzar rebuild para asegurar que los RectTransforms tengan valores actualizados
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(zonaGrilla);

        Rect grillaRect = zonaGrilla.rect;

        // Debug.Log temporal para verificar tamaño real de ZonaGrilla
        Debug.Log($"[DEBUG AdjustCellSize] ZonaGrilla.rect DESPUÉS de yield return null: " +
            $"width={grillaRect.width:F2}, height={grillaRect.height:F2}");

        if (grillaRect.width <= 0 || grillaRect.height <= 0)
        {
            // Segundo intento: esperar otro frame por si el AspectRatioFitter necesita más tiempo
            Debug.LogWarning("InventoryOverlayCanvas: ZonaGrilla.rect inválido tras 1 frame. " +
                $"Valores: {grillaRect.width:F2}x{grillaRect.height:F2}. Esperando un frame adicional...");

            yield return null;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(zonaGrilla);

            grillaRect = zonaGrilla.rect;

            Debug.Log($"[DEBUG AdjustCellSize] ZonaGrilla.rect DESPUÉS de 2do frame: " +
                $"width={grillaRect.width:F2}, height={grillaRect.height:F2}");

            if (grillaRect.width <= 0 || grillaRect.height <= 0)
            {
                Debug.LogError("InventoryOverlayCanvas: ZonaGrilla.rect sigue inválido después de 2 frames. " +
                    $"Valores: {grillaRect.width:F2}x{grillaRect.height:F2}. Verifica anchors y jerarquía UI.");
                yield break;
            }
        }

        ApplyCellSize(grid, grillaRect);
    }

    /// <summary>
    /// Aplica el cálculo final de cellSize al GridLayoutGroup.
    /// Fórmula:
    ///   cellWidth  = floor((ZonaGrilla.width  - padding.left - padding.right  - spacing.x * (columnas - 1)) / columnas)
    ///   cellHeight = floor((ZonaGrilla.height - padding.top  - padding.bottom - spacing.y * (filas - 1))    / filas)
    /// Usa el menor de ambos para mantener slots cuadrados y evitar desbordamiento.
    /// Se aplica Mathf.Floor para garantizar que no haya desbordamiento por redondeo de floats.
    /// Configuración: 8 columnas, 6 filas.
    /// </summary>
    private void ApplyCellSize(GridLayoutGroup grid, Rect grillaRect)
    {
        // --- Calcular padding FINAL incluyendo offsets ---
        // gridOffsetX positivo = más padding-left = grilla se mueve a la derecha.
        // gridOffsetY positivo = más padding-top  = grilla se mueve hacia abajo.
        // Solo se suma al lado izquierdo/superior; el lado derecho/inferior queda intacto
        // para que el cellSize se recalcule con el espacio real disponible.
        int offsetX = Mathf.RoundToInt(gridOffsetX);
        int offsetY = Mathf.RoundToInt(gridOffsetY);

        int finalPaddingLeft   = gridPadding.left + offsetX;
        int finalPaddingRight  = gridPadding.right;
        int finalPaddingTop    = gridPadding.top + offsetY;
        int finalPaddingBottom = gridPadding.bottom;

        // --- Calcular espacio disponible con el padding final (offsets incluidos) ---
        float availableWidth = grillaRect.width
            - finalPaddingLeft
            - finalPaddingRight
            - (cellSpacing.x * (gridColumns - 1));

        float availableHeight = grillaRect.height
            - finalPaddingTop
            - finalPaddingBottom
            - (cellSpacing.y * (gridRows - 1));

        float cellWidth = availableWidth / gridColumns;
        float cellHeight = availableHeight / gridRows;

        // Usar el menor para mantener slots cuadrados sin desbordamiento.
        // Mathf.Floor evita que fracciones de pixel causen que la última columna se desborde.
        float side = Mathf.Floor(Mathf.Min(cellWidth, cellHeight));

        Debug.Log($"[DEBUG ApplyCellSize] ZonaGrilla size: {grillaRect.width:F2} x {grillaRect.height:F2}\n" +
            $"  Padding final (con offset): L={finalPaddingLeft}, R={finalPaddingRight}, T={finalPaddingTop}, B={finalPaddingBottom}\n" +
            $"  Offset aplicado: X={offsetX}, Y={offsetY}\n" +
            $"  Spacing: x={cellSpacing.x}, y={cellSpacing.y}\n" +
            $"  Available: width={availableWidth:F2}, height={availableHeight:F2}\n" +
            $"  CellWidth={cellWidth:F2}, CellHeight={cellHeight:F2}\n" +
            $"  Final side (floored): {side:F1}");

        if (side > 0)
        {
            // Aplicar padding final AL GridLayoutGroup (offsets ya incluidos)
            grid.padding = new RectOffset(
                finalPaddingLeft,
                finalPaddingRight,
                finalPaddingTop,
                finalPaddingBottom
            );

            grid.cellSize = new Vector2(side, side);
            cellSize = new Vector2(side, side);

            Debug.Log($"InventoryOverlayCanvas: cellSize ajustado a ({side:F1}, {side:F1}). " +
                $"ZonaGrilla: {grillaRect.width:F1}x{grillaRect.height:F1}, " +
                $"Padding base: ({gridPadding.left},{gridPadding.right},{gridPadding.top},{gridPadding.bottom}), " +
                $"Offset: ({gridOffsetX},{gridOffsetY}), " +
                $"Padding final: ({finalPaddingLeft},{finalPaddingRight},{finalPaddingTop},{finalPaddingBottom}), " +
                $"Spacing: ({cellSpacing.x},{cellSpacing.y}), " +
                $"Columnas: {gridColumns}, Filas: {gridRows}.");
        }
        else
        {
            Debug.LogError($"InventoryOverlayCanvas: cellSize calculado es <= 0 ({side}). " +
                $"Algo está mal con las dimensiones de ZonaGrilla o los valores de padding/spacing.");
        }
    }

    /// <summary>
    /// Elimina todos los slots hijos de ZonaGrilla y limpia la lista interna.
    /// </summary>
    private void ClearExistingSlots()
    {
        if (slots != null)
        {
            slots.Clear();
        }

        // Validación robusta: ReferenceEquals verifica null real de C#,
        // el operador == de Unity verifica objetos destruidos.
        if (ReferenceEquals(zonaGrilla, null) || zonaGrilla == null) return;

        Transform grillaTransform = zonaGrilla.transform;
        for (int i = grillaTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = grillaTransform.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Genera los 48 slots por código si el prefab no está disponible.
    /// Replica la estructura del prefab InventorySlot:
    ///   InventorySlot (Image fondo transparente, InventorySlotUI)
    ///     ├─ ItemIcon (Image, preserveAspect, transparente, desactivado)
    ///     ├─ SelectionHighlight (Image transparente, raycastTarget=false) [legacy, inerte]
    ///     ├─ BorderTop (Image dorado, 2px alto, DESACTIVADO)
    ///     ├─ BorderBottom (Image dorado, 2px alto, DESACTIVADO)
    ///     ├─ BorderLeft (Image dorado, 2px ancho, DESACTIVADO)
    ///     └─ BorderRight (Image dorado, 2px ancho, DESACTIVADO)
    /// La selección se muestra activando/desactivando los 4 bordes reales.
    /// </summary>
    private void GenerateSlotsFallback(GridLayoutGroup grid)
    {
        int totalSlots = gridColumns * gridRows;
        Color borderColor = new Color(0.92f, 0.78f, 0.40f, 1.0f); // Dorado
        float borderThickness = 2f;

        for (int i = 0; i < totalSlots; i++)
        {
            // --- Root: InventorySlot ---
            GameObject slotObj = new GameObject($"Slot_{i:D2}");
            slotObj.transform.SetParent(zonaGrilla, false);
            slotObj.transform.localScale = Vector3.one;

            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(80f, 80f);

            slotObj.AddComponent<CanvasRenderer>();
            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0f, 0f, 0f, 0f); // Transparente: deja ver las casillas del sprite RPG
            slotBg.raycastTarget = true;
            slotBg.sprite = null;

            // --- ItemIcon ---
            GameObject iconObj = new GameObject("ItemIcon");
            iconObj.transform.SetParent(slotObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.12f, 0.12f);
            iconRect.anchorMax = new Vector2(0.88f, 0.88f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            iconObj.AddComponent<CanvasRenderer>();
            Image itemIcon = iconObj.AddComponent<Image>();
            itemIcon.sprite = null;
            itemIcon.color = new Color(1f, 1f, 1f, 0f);
            itemIcon.raycastTarget = false;
            itemIcon.preserveAspect = true;
            itemIcon.enabled = false;

            // --- SelectionHighlight (legacy, inerte — color transparente, sin Outline) ---
            GameObject highlightObj = new GameObject("SelectionHighlight");
            highlightObj.transform.SetParent(slotObj.transform, false);

            RectTransform highlightRect = highlightObj.AddComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;

            highlightObj.AddComponent<CanvasRenderer>();
            Image highlightImage = highlightObj.AddComponent<Image>();
            highlightImage.color = new Color(0f, 0f, 0f, 0f); // Completamente transparente
            highlightImage.raycastTarget = false;

            highlightObj.SetActive(false); // Inerte, no se usa para la selección visual

            // --- 4 Bordes reales de selección ---
            // Cada borde es un GameObject hijo con Image dorado, posicionado como barra en un lado.
            // Se activan/desactivan en SetSelected(bool).

            // BorderTop: barra horizontal en la parte superior
            GameObject borderTopObj = CreateBorderObject("BorderTop", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(0f, -borderThickness),
                offsetMax: Vector2.zero);

            // BorderBottom: barra horizontal en la parte inferior
            GameObject borderBottomObj = CreateBorderObject("BorderBottom", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 0f),
                offsetMin: Vector2.zero,
                offsetMax: new Vector2(0f, borderThickness));

            // BorderLeft: barra vertical en el lado izquierdo
            GameObject borderLeftObj = CreateBorderObject("BorderLeft", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(0f, 1f),
                offsetMin: Vector2.zero,
                offsetMax: new Vector2(borderThickness, 0f));

            // BorderRight: barra vertical en el lado derecho
            GameObject borderRightObj = CreateBorderObject("BorderRight", slotObj.transform, borderColor, borderThickness,
                anchorMin: new Vector2(1f, 0f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(-borderThickness, 0f),
                offsetMax: Vector2.zero);

            // --- InventorySlotUI ---
            InventorySlotUI slotUI = slotObj.AddComponent<InventorySlotUI>();
            slotUI.SlotIndex = i;

            // Asignar referencias privadas via reflection (campos [SerializeField])
            AssignSlotReferences(slotUI, itemIcon, highlightObj, borderTopObj, borderBottomObj, borderLeftObj, borderRightObj);

            slotUI.ClearSlot();
            slotUI.OnSlotClicked = HandleSlotClicked;
            slots.Add(slotUI);
        }

        AdjustCellSizeToFit(grid);
        Debug.Log($"InventoryOverlayCanvas: Grilla generada (fallback) — {totalSlots} slots ({gridColumns}x{gridRows}) en ZonaGrilla.");

        gridReady = true;
        RefreshGrid();
    }

    /// <summary>
    /// Neutraliza el SelectionHighlight legacy de un slot instanciado desde prefab.
    /// Desactiva el GameObject, elimina el Outline, y fuerza color completamente transparente.
    /// Esto garantiza que nunca se muestre visualmente aunque algo lo active por error.
    /// </summary>
    private static void NeutralizeSelectionHighlight(GameObject slotObj)
    {
        Transform highlightTransform = slotObj.transform.Find("SelectionHighlight");
        if (highlightTransform == null) return;

        GameObject highlightObj = highlightTransform.gameObject;

        // Desactivar el GameObject
        highlightObj.SetActive(false);

        // Eliminar el Outline que causa el cuadrado sólido dorado
        Outline outline = highlightObj.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }

        // Forzar Image a completamente transparente sin sprite
        Image img = highlightObj.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0f, 0f, 0f, 0f);
            img.sprite = null;
            img.enabled = false;
        }
    }

    /// <summary>
    /// Crea un GameObject de borde (barra de 2px) como hijo del slot.
    /// Usado para los 4 lados del marco de selección dorado.
    /// El objeto se crea DESACTIVADO.
    /// </summary>
    private static GameObject CreateBorderObject(string name, Transform parent, Color color, float thickness,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject borderObj = new GameObject(name);
        borderObj.transform.SetParent(parent, false);

        RectTransform rect = borderObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        borderObj.AddComponent<CanvasRenderer>();
        Image img = borderObj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        borderObj.SetActive(false); // Inicia desactivado, se activa en SetSelected(true)

        return borderObj;
    }

    /// <summary>
    /// Asigna las referencias privadas de InventorySlotUI usando reflection.
    /// Necesario cuando se genera por código sin prefab serializado.
    /// Incluye los 4 bordes de selección además del ícono y highlight legacy.
    /// </summary>
    private static void AssignSlotReferences(InventorySlotUI slotUI, Image itemIcon, GameObject selectionHighlight,
        GameObject borderTop, GameObject borderBottom, GameObject borderLeft, GameObject borderRight)
    {
        System.Type type = typeof(InventorySlotUI);
        System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        System.Reflection.FieldInfo iconField = type.GetField("itemIcon", flags);
        System.Reflection.FieldInfo highlightField = type.GetField("selectionHighlight", flags);
        System.Reflection.FieldInfo borderTopField = type.GetField("borderTop", flags);
        System.Reflection.FieldInfo borderBottomField = type.GetField("borderBottom", flags);
        System.Reflection.FieldInfo borderLeftField = type.GetField("borderLeft", flags);
        System.Reflection.FieldInfo borderRightField = type.GetField("borderRight", flags);

        if (iconField != null)
            iconField.SetValue(slotUI, itemIcon);

        if (highlightField != null)
            highlightField.SetValue(slotUI, selectionHighlight);

        if (borderTopField != null)
            borderTopField.SetValue(slotUI, borderTop);

        if (borderBottomField != null)
            borderBottomField.SetValue(slotUI, borderBottom);

        if (borderLeftField != null)
            borderLeftField.SetValue(slotUI, borderLeft);

        if (borderRightField != null)
            borderRightField.SetValue(slotUI, borderRight);
    }
}
