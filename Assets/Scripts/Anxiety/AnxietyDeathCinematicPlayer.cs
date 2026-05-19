using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Reproduce la cinemática de muerte por ansiedad a pantalla completa.
/// </summary>
public class AnxietyDeathCinematicPlayer : MonoBehaviour
{
    public static AnxietyDeathCinematicPlayer Instance { get; private set; }

    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private VideoPlayer videoPlayer;

    private RenderTexture renderTexture;
    private VideoClip activeClip;
    private Action onFinished;
    private Coroutine playRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureUiBuilt();
        Hide();
    }

    private void OnDestroy()
    {
        ReleaseActiveClip();
        ReleaseRenderTexture();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void Play(string characterId, Action finishedCallback)
    {
        EnsureInstance();
        Instance.StartPlayback(characterId, finishedCallback);
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(AnxietyDeathCinematicPlayer));
        host.AddComponent<AnxietyDeathCinematicPlayer>();
    }

    private void StartPlayback(string characterId, Action finishedCallback)
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        ReleaseActiveClip();
        ClearVideoSurface();
        playRoutine = StartCoroutine(PlayRoutine(characterId, finishedCallback));
    }

    private IEnumerator PlayRoutine(string characterId, Action finishedCallback)
    {
        EnsureUiBuilt();
        onFinished = finishedCallback;

        if (!AnxietyDeathCinematicCatalog.TryLoadClip(characterId, out VideoClip clip))
        {
            Debug.LogWarning($"[AnxietyDeathCinematicPlayer] No hay video para '{characterId}'. Se omite cinemática.");
            CompletePlayback();
            yield break;
        }

        activeClip = clip;
        ReleaseRenderTexture();
        EnsureRenderTexture();
        ClearRenderTexture();

        overlayRoot.SetActive(true);
        videoImage.texture = renderTexture;
        videoImage.color = Color.white;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.time = 0d;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        CompletePlayback();
    }

    private void CompletePlayback()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
        }

        ReleaseActiveClip();
        ClearVideoSurface();
        ReleaseRenderTexture();
        Hide();

        Action callback = onFinished;
        onFinished = null;
        playRoutine = null;
        callback?.Invoke();

        Resources.UnloadUnusedAssets();
    }

    public void Hide()
    {
        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }
    }

    private void ReleaseActiveClip()
    {
        if (activeClip == null)
        {
            return;
        }

        AnxietyDeathCinematicCatalog.ReleaseClip(activeClip);
        activeClip = null;
    }

    private void ClearVideoSurface()
    {
        if (videoImage != null)
        {
            videoImage.texture = null;
            videoImage.color = Color.black;
        }
    }

    private void ClearRenderTexture()
    {
        if (renderTexture == null)
        {
            return;
        }

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = previous;
    }

    private void EnsureRenderTexture()
    {
        if (renderTexture != null)
        {
            return;
        }

        renderTexture = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
    }

    private void ReleaseRenderTexture()
    {
        if (renderTexture == null)
        {
            return;
        }

        if (videoPlayer != null)
        {
            videoPlayer.targetTexture = null;
        }

        if (videoImage != null && videoImage.texture == renderTexture)
        {
            videoImage.texture = null;
        }

        renderTexture.Release();
        Destroy(renderTexture);
        renderTexture = null;
    }

    private void EnsureUiBuilt()
    {
        if (overlayRoot != null)
        {
            return;
        }

        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 330;

        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        overlayRoot = new GameObject("CinematicRoot");
        overlayRoot.transform.SetParent(transform, false);
        RectTransform rootRect = overlayRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        GameObject imageObj = new GameObject("VideoImage");
        imageObj.transform.SetParent(overlayRoot.transform, false);
        RectTransform imageRect = imageObj.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        videoImage = imageObj.AddComponent<RawImage>();
        videoImage.color = Color.black;

        GameObject playerObj = new GameObject("VideoPlayer");
        playerObj.transform.SetParent(transform, false);
        videoPlayer = playerObj.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
    }
}
