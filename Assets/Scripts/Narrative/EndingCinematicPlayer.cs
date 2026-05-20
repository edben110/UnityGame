using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Reproduce cinemáticas de finales a pantalla completa.
/// </summary>
public class EndingCinematicPlayer : MonoBehaviour
{
    public static EndingCinematicPlayer Instance { get; private set; }

    public static bool IsPlaying { get; private set; }

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
            IsPlaying = false;
        }
    }

    public static void Play(int endingIndex, Action finishedCallback)
    {
        EnsureInstance();
        Instance.StartPlayback(endingIndex, finishedCallback);
    }

    public static void PlayCredits(Action finishedCallback)
    {
        EnsureInstance();
        Instance.StartPlaybackCredits(finishedCallback);
    }

    private static void EnsureInstance()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject host = new GameObject(nameof(EndingCinematicPlayer));
        host.AddComponent<EndingCinematicPlayer>();
    }

    private void StartPlayback(int endingIndex, Action finishedCallback)
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        ReleaseActiveClip();
        ClearVideoSurface();
        playRoutine = StartCoroutine(PlayRoutine(endingIndex, finishedCallback));
    }

    private void StartPlaybackCredits(Action finishedCallback)
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        ReleaseActiveClip();
        ClearVideoSurface();
        playRoutine = StartCoroutine(PlayCreditsRoutine(finishedCallback));
    }

    private IEnumerator PlayRoutine(int endingIndex, Action finishedCallback)
    {
        IsPlaying = true;
        EnsureUiBuilt();
        onFinished = finishedCallback;

        if (!EndingCinematicCatalog.TryLoadEnding(endingIndex, out VideoClip clip))
        {
            Debug.LogError($"[EndingCinematicPlayer] No se encontró Final_{endingIndex} en Resources/Cinematicas.");
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

    private IEnumerator PlayCreditsRoutine(Action finishedCallback)
    {
        IsPlaying = true;
        EnsureUiBuilt();
        onFinished = finishedCallback;

        if (!EndingCinematicCatalog.TryLoadCredits(out VideoClip clip))
        {
            Debug.LogError("[EndingCinematicPlayer] No se encontró 'Creditos' en Resources/Cinematicas.");
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

        IsPlaying = false;
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

        EndingCinematicCatalog.ReleaseClip(activeClip);
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
        canvas.sortingOrder = 340;

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

        overlayRoot = new GameObject("EndingCinematicRoot");
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
