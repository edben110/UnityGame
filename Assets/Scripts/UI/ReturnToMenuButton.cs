using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Botón persistente de HUD que permite regresar al menú principal (MenuScene)
/// desde cualquier escena de gameplay. Se auto-construye y usa DontDestroyOnLoad.
/// Se oculta automáticamente cuando ya se está en MenuScene.
/// </summary>
public class ReturnToMenuButton : MonoBehaviour
{
    public static ReturnToMenuButton Instance { get; private set; }

    private Canvas canvas;
    private Button menuButton;
    private const string MenuSceneName = "MenuScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("ReturnToMenuButton");
        go.AddComponent<ReturnToMenuButton>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        bool inMenu = SceneManager.GetActiveScene().name == MenuSceneName;
        if (canvas != null)
        {
            canvas.gameObject.SetActive(!inMenu);
        }
    }

    /// <summary>
    /// Método público invocado por el botón para regresar al menú.
    /// </summary>
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    private void BuildUI()
    {
        // Canvas propio con sorting order alto para estar siempre visible
        GameObject canvasObj = new GameObject("ReturnToMenuCanvas");
        canvasObj.transform.SetParent(transform);

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Botón en esquina superior derecha
        GameObject btnObj = new GameObject("MenuButton");
        btnObj.transform.SetParent(canvasObj.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 1f);
        btnRect.anchorMax = new Vector2(1f, 1f);
        btnRect.pivot = new Vector2(1f, 1f);
        btnRect.anchoredPosition = new Vector2(-20f, -20f);
        btnRect.sizeDelta = new Vector2(140f, 50f);

        // Fondo del botón
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        menuButton = btnObj.AddComponent<Button>();
        menuButton.targetGraphic = btnImage;

        // Colores del botón
        ColorBlock colors = menuButton.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        colors.highlightedColor = new Color(0.65f, 0.08f, 0.08f, 1f);
        colors.pressedColor = new Color(0.5f, 0.05f, 0.05f, 1f);
        colors.selectedColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        menuButton.colors = colors;

        // Texto del botón
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text label = textObj.AddComponent<TextMeshProUGUI>();
        label.text = "MENU";
        label.fontSize = 24;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(0.78f, 0.75f, 0.70f, 1f); // ghostWhite del proyecto

        // Conectar evento
        menuButton.onClick.AddListener(ReturnToMenu);
    }
}
