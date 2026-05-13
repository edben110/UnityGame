using UnityEngine;
using UnityEngine.UI;

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

    [Header("Debug (TEMPORAL - eliminar en producción)")]
    [Tooltip("Activa imágenes de color tenue en las zonas para verificar posiciones.")]
    [SerializeField] private bool showDebugZones = true;

    [Header("Sprite del inventario")]
    [Tooltip("Ruta del sprite en Resources/ (sin extensión). Unity ignora extensiones en Resources.Load.")]
    [SerializeField] private string inventorySpritePath = "Sprites/Inventario.png";

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

    /// <summary>Indica si el overlay del inventario está visible.</summary>
    public bool IsOpen => overlayRoot != null && overlayRoot.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUI();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>Muestra el inventario fullscreen.</summary>
    public void Show()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(true);
        }
    }

    /// <summary>Oculta el inventario fullscreen.</summary>
    public void Hide()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }
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

    private void BuildUI()
    {
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
        // Ajuste: anchorMax.x 0.57→0.61 para cubrir casillas del lado derecho faltantes.
        // Altura sin cambios. No invade panel derecho (separador ~0.68).
        zonaGrilla = CreateZoneMarker("ZonaGrilla", parent,
            anchorMin: new Vector2(0.05f, 0.20f),
            anchorMax: new Vector2(0.61f, 0.88f),
            debugColor: new Color(0.2f, 0.6f, 1f, 0.15f) // Azul tenue
        );

        // --- ZonaPreview: Panel derecho superior, para imagen grande del ítem ---
        // Ajuste: anchorMax.y 0.88→0.85, anchorMax.x 0.95→0.93 (ligeramente más pequeña
        // para encajar dentro del marco decorativo del panel superior derecho)
        zonaPreview = CreateZoneMarker("ZonaPreview", parent,
            anchorMin: new Vector2(0.69f, 0.50f),
            anchorMax: new Vector2(0.93f, 0.85f),
            debugColor: new Color(0.2f, 1f, 0.2f, 0.15f) // Verde tenue
        );

        // --- ZonaDescripcion: Panel derecho inferior, para nombre y descripción ---
        // Ajuste: desplazada 0.04 hacia abajo (Min.y 0.18→0.14, Max.y 0.42→0.38)
        // Misma altura total (24%), mejor alineada con marco ornamental inferior.
        zonaDescripcion = CreateZoneMarker("ZonaDescripcion", parent,
            anchorMin: new Vector2(0.69f, 0.14f),
            anchorMax: new Vector2(0.95f, 0.38f),
            debugColor: new Color(1f, 1f, 0.2f, 0.15f) // Amarillo tenue
        );

        // --- ZonaMochila: Esquina inferior izquierda, sobre ícono de mochila ---
        // Ajuste fino: anchorMax.x 0.15→0.22 (más ancho para futuro contador/texto/ícono)
        zonaMochila = CreateZoneMarker("ZonaMochila", parent,
            anchorMin: new Vector2(0.03f, 0.08f),
            anchorMax: new Vector2(0.22f, 0.18f),
            debugColor: new Color(1f, 0.5f, 0f, 0.15f) // Naranja tenue
        );

        // --- ZonaCerrar: Esquina superior derecha, sobre botón X del sprite ---
        zonaCerrar = CreateZoneMarker("ZonaCerrar", parent,
            anchorMin: new Vector2(0.92f, 0.90f),
            anchorMax: new Vector2(0.99f, 0.98f),
            debugColor: new Color(1f, 0.2f, 0.2f, 0.15f) // Rojo tenue
        );
    }

    /// <summary>
    /// Crea un GameObject vacío con RectTransform como marcador de zona.
    /// Opcionalmente agrega una Image de debug con color tenue (TEMPORAL).
    /// </summary>
    private RectTransform CreateZoneMarker(string zoneName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color debugColor)
    {
        GameObject zoneObj = new GameObject(zoneName);
        zoneObj.transform.SetParent(parent, false);

        RectTransform rect = zoneObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);

        // --- DEBUG TEMPORAL: Imagen de color tenue para verificar posiciones ---
        // Eliminar estos componentes en producción o desactivar showDebugZones
        if (showDebugZones)
        {
            zoneObj.AddComponent<CanvasRenderer>();
            Image debugImage = zoneObj.AddComponent<Image>();
            debugImage.color = debugColor;
            debugImage.raycastTarget = false;
        }

        return rect;
    }

    private static void StretchToFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
