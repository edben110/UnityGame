using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Coloca la UI de intro bajo el Canvas de MenuScene para editarla en Scene View.
/// </summary>
public static class SetupMenuNewGameIntro
{
    private const string MenuScenePath = "Assets/Scenes/MenuScene.unity";
    private const int UiLayer = 5;

    [MenuItem("Tools/Menu/Configurar intro nueva partida (Tutorial + Prólogo)")]
    public static void Setup()
    {
        Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

        Canvas sceneCanvas = Object.FindFirstObjectByType<Canvas>();
        if (sceneCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No hay Canvas en MenuScene.", "OK");
            return;
        }

        ConfigureCanvasForSceneEditing(sceneCanvas);

        NewGameIntroPresenter presenter = Object.FindFirstObjectByType<NewGameIntroPresenter>(FindObjectsInactive.Include);
        GameObject presenterRoot;
        if (presenter == null)
        {
            presenterRoot = new GameObject("NewGameIntroPresenter");
            Undo.RegisterCreatedObjectUndo(presenterRoot, "Create NewGameIntroPresenter");
            presenter = Undo.AddComponent<NewGameIntroPresenter>(presenterRoot);
        }
        else
        {
            presenterRoot = presenter.gameObject;
        }

        RectTransform uiRoot = FindOrCreateUiRoot(sceneCanvas.transform, presenterRoot.transform);

        NewGameIntroPresenter.IntroScreenView tutorialView = BuildOrGetView(
            uiRoot,
            "TutorialView",
            new Color(0f, 0f, 0f, 1f),
            null,
            "Tutorial",
            true);

        Sprite prologueSprite = LoadPrologoSprite();
        NewGameIntroPresenter.IntroScreenView prologueView = BuildOrGetView(
            uiRoot,
            "PrologueView",
            Color.white,
            prologueSprite,
            string.Empty,
            false);

        DialogueRunner runner = uiRoot.GetComponentInChildren<DialogueRunner>(true);
        DialoguePanelUI panel = uiRoot.GetComponentInChildren<DialoguePanelUI>(true);
        GameObject dialogueRoot = uiRoot.Find("DialoguePanelRoot")?.gameObject;

        if (runner == null || panel == null)
        {
            MenuDialogueUiFactory.MenuDialogueSystem dialogue = MenuDialogueUiFactory.Create(uiRoot);
            runner = dialogue.Runner;
            panel = dialogue.Panel;
            dialogueRoot = uiRoot.Find("DialoguePanelRoot")?.gameObject;
        }

        Button continueBtn = FindContinueButton(uiRoot);
        if (continueBtn == null)
        {
            continueBtn = MenuDialogueUiFactory.CreateOverlayContinueButton(uiRoot);
            continueBtn.name = "ContinueButton";
        }

        SetUiLayerRecursive(uiRoot, UiLayer);
        StretchHierarchy(uiRoot);

        if (uiRoot.GetComponent<MenuIntroUiLayout>() == null)
        {
            uiRoot.gameObject.AddComponent<MenuIntroUiLayout>();
        }

        dialogueRoot?.transform.SetAsLastSibling();
        continueBtn.transform.SetAsLastSibling();
        uiRoot.SetAsLastSibling();

        SamplePrologueChapter1Builder builder = runner.GetComponent<SamplePrologueChapter1Builder>();
        if (builder == null)
        {
            builder = Undo.AddComponent<SamplePrologueChapter1Builder>(runner.gameObject);
        }

        SerializedObject so = new SerializedObject(presenter);
        so.FindProperty("uiRoot").objectReferenceValue = uiRoot;
        so.FindProperty("tutorialView").FindPropertyRelative("root").objectReferenceValue = tutorialView.root;
        so.FindProperty("tutorialView").FindPropertyRelative("background").objectReferenceValue = tutorialView.background;
        so.FindProperty("tutorialView").FindPropertyRelative("title").objectReferenceValue = tutorialView.title;
        so.FindProperty("prologueView").FindPropertyRelative("root").objectReferenceValue = prologueView.root;
        so.FindProperty("prologueView").FindPropertyRelative("background").objectReferenceValue = prologueView.background;
        so.FindProperty("prologueView").FindPropertyRelative("title").objectReferenceValue = prologueView.title;
        so.FindProperty("dialoguePanelRoot").objectReferenceValue = dialogueRoot;
        so.FindProperty("overlayContinueButton").objectReferenceValue = continueBtn;
        so.FindProperty("dialogueRunner").objectReferenceValue = runner;
        so.FindProperty("dialoguePanel").objectReferenceValue = panel;
        so.FindProperty("dialogueBuilder").objectReferenceValue = builder;
        so.ApplyModifiedPropertiesWithoutUndo();

        tutorialView.root.SetActive(true);
        prologueView.root.SetActive(false);
        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(true);
        }

        continueBtn.gameObject.SetActive(true);
        uiRoot.gameObject.SetActive(true);
        presenterRoot.SetActive(true);

        Selection.activeGameObject = tutorialView.background.gameObject;
        EditorGUIUtility.PingObject(tutorialView.background.gameObject);
        SceneView.lastActiveSceneView?.FrameSelected();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[SetupMenuNewGameIntro] UI bajo Canvas lista para editar en Scene View (TutorialView activo).");
    }

    private static RectTransform FindOrCreateUiRoot(Transform sceneCanvas, Transform presenterRoot)
    {
        Transform existing = sceneCanvas.Find(NewGameIntroPresenter.IntroUiRootName);
        if (existing == null)
        {
            existing = presenterRoot.Find("NewGameIntroOverlay");
        }

        if (existing == null)
        {
            existing = presenterRoot.Find(NewGameIntroPresenter.IntroUiRootName);
        }

        GameObject rootObj;
        if (existing != null)
        {
            rootObj = existing.gameObject;
            Undo.RecordObject(rootObj, "Reparent intro UI");
            rootObj.transform.SetParent(sceneCanvas, false);
            rootObj.name = NewGameIntroPresenter.IntroUiRootName;
        }
        else
        {
            rootObj = new GameObject(NewGameIntroPresenter.IntroUiRootName, typeof(RectTransform), typeof(MenuIntroUiLayout));
            Undo.RegisterCreatedObjectUndo(rootObj, "Create NewGameIntroUI");
            rootObj.transform.SetParent(sceneCanvas, false);
        }

        RemoveNestedCanvasComponents(rootObj);

        RectTransform uiRoot = rootObj.GetComponent<RectTransform>();
        MenuIntroUiLayout.StretchFullScreen(uiRoot);
        return uiRoot;
    }

    private static void RemoveNestedCanvasComponents(GameObject rootObj)
    {
        Canvas[] canvases = rootObj.GetComponents<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            Object.DestroyImmediate(canvases[i]);
        }

        CanvasScaler scaler = rootObj.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Object.DestroyImmediate(scaler);
        }

        GraphicRaycaster raycaster = rootObj.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            Object.DestroyImmediate(raycaster);
        }
    }

    private static void ConfigureCanvasForSceneEditing(Canvas sceneCanvas)
    {
        Camera cam = sceneCanvas.worldCamera;
        if (cam == null)
        {
            cam = Object.FindFirstObjectByType<Camera>();
        }

        Undo.RecordObject(sceneCanvas, "Configure menu canvas");
        sceneCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        sceneCanvas.worldCamera = cam;
        sceneCanvas.planeDistance = 10f;

        CanvasScaler scaler = sceneCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            Undo.RecordObject(scaler, "Configure menu canvas scaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static NewGameIntroPresenter.IntroScreenView BuildOrGetView(
        RectTransform uiRoot,
        string viewName,
        Color bgColor,
        Sprite bgSprite,
        string titleText,
        bool showTitle)
    {
        Transform existing = uiRoot.Find(viewName);
        if (existing != null)
        {
            return new NewGameIntroPresenter.IntroScreenView
            {
                root = existing.gameObject,
                background = existing.Find("Background")?.GetComponent<Image>(),
                title = existing.Find("Title")?.GetComponent<TMP_Text>()
            };
        }

        GameObject viewRoot = CreateUiObject(viewName, uiRoot);
        MenuIntroUiLayout.StretchFullScreen(viewRoot.GetComponent<RectTransform>());

        GameObject bgObj = CreateUiObject("Background", viewRoot.transform);
        MenuIntroUiLayout.StretchFullScreen(bgObj.GetComponent<RectTransform>());
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = bgColor;
        bgImage.sprite = bgSprite;
        bgImage.preserveAspect = bgSprite != null;
        bgImage.raycastTarget = false;

        GameObject titleObj = CreateUiObject("Title", viewRoot.transform);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.82f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        TMP_Text title = titleObj.AddComponent<TextMeshProUGUI>();
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 56f;
        title.fontStyle = FontStyles.Bold;
        title.color = new Color(0.92f, 0.84f, 0.62f, 1f);
        title.text = titleText;
        titleObj.SetActive(showTitle);

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/Cinzel-Bold SDF");
        if (font != null)
        {
            title.font = font;
        }

        return new NewGameIntroPresenter.IntroScreenView
        {
            root = viewRoot,
            background = bgImage,
            title = title
        };
    }

    private static Button FindContinueButton(Transform root)
    {
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (button.name == "ContinueButton")
            {
                return button;
            }
        }

        return null;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        obj.layer = UiLayer;
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void SetUiLayerRecursive(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
        {
            SetUiLayerRecursive(root.GetChild(i), layer);
        }
    }

    private static void StretchHierarchy(Transform root)
    {
        if (root is RectTransform rect)
        {
            if (root.name is NewGameIntroPresenter.IntroUiRootName or "TutorialView" or "PrologueView" or "Background")
            {
                MenuIntroUiLayout.StretchFullScreen(rect);
            }
        }

        for (int i = 0; i < root.childCount; i++)
        {
            StretchHierarchy(root.GetChild(i));
        }
    }

    private static Sprite LoadPrologoSprite()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Prologo.png");
        if (assets == null || assets.Length == 0)
        {
            assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Sprites/Prologo.png");
        }

        if (assets != null)
        {
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite && sprite.name == "Prologo_0")
                {
                    return sprite;
                }
            }
        }

        return null;
    }
}
