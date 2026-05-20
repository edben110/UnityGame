using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class AnxietySystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text anxietyLabel;
    [SerializeField] private TMP_Text npcStatusText;
    [SerializeField] private Image anxietyOverlay;

    [Header("Audio")]
    [SerializeField] private AudioSource heartbeatSource;

    [Header("Postproceso URP")]
    [SerializeField] private Volume volume;

    [Header("Intensidad")]
    [SerializeField, Range(0f, 1f)] private float overlayAlphaMax = 0.4f;
    [SerializeField] private Color overlayColor = new Color(0.45f, 0f, 0f, 1f);

    private Vignette vignette;
    private ChromaticAberration chromatic;

    private void Awake()
    {
        TryResolveNpcStatusText();
    }

    private void Start()
    {
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out chromatic);
        }

        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged += OnStateChanged;
            OnStateChanged();
        }

        HideOverlayImage();
        ClearNpcStatusText();
    }

    private void OnDestroy()
    {
        if (StoryState.Instance != null)
        {
            StoryState.Instance.StateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged()
    {
        float normalized = StoryState.Instance != null ? StoryState.Instance.Anxiety / 100f : 0f;

        if (anxietyLabel != null)
        {
            anxietyLabel.text = $"Ansiedad: {Mathf.RoundToInt(normalized * 100f)}";
        }

        if (heartbeatSource != null)
        {
            heartbeatSource.volume = Mathf.Lerp(0f, 0.8f, normalized);
            heartbeatSource.pitch = Mathf.Lerp(0.95f, 1.18f, normalized);

            if (heartbeatSource.volume > 0.02f)
            {
                if (!heartbeatSource.isPlaying)
                {
                    heartbeatSource.Play();
                }
            }
            else if (heartbeatSource.isPlaying)
            {
                heartbeatSource.Stop();
            }
        }

        if (vignette != null)
        {
            vignette.intensity.Override(Mathf.Lerp(0.18f, 0.5f, normalized));
            vignette.smoothness.Override(Mathf.Lerp(0.45f, 0.75f, normalized));
        }

        if (chromatic != null)
        {
            chromatic.intensity.Override(Mathf.Lerp(0f, 0.35f, normalized));
        }
    }

    public void ShowVerificationOverlay(float normalizedIntensity)
    {
        ShowVerificationOverlay(normalizedIntensity, null);
    }

    public void ShowVerificationOverlay(float normalizedIntensity, string statusMessage)
    {
        float clampedIntensity = Mathf.Clamp01(normalizedIntensity);
        HideOverlayImage();
        TryResolveNpcStatusText();

        if (npcStatusText == null)
        {
            return;
        }

        int anxietyPercent = Mathf.RoundToInt(clampedIntensity * 100f);
        npcStatusText.gameObject.SetActive(true);
        npcStatusText.text = string.IsNullOrWhiteSpace(statusMessage)
            ? $"Ansiedad: {anxietyPercent}/100"
            : statusMessage;
        npcStatusText.color = EvaluateAnxietyTextColor(clampedIntensity);
    }

    public void HideVerificationOverlay()
    {
        HideOverlayImage();
        ClearNpcStatusText();
    }

    private void TryResolveNpcStatusText()
    {
        if (npcStatusText != null)
        {
            return;
        }

        GameObject statusObject = GameObject.Find("NpcStatusText");
        if (statusObject != null)
        {
            npcStatusText = statusObject.GetComponent<TMP_Text>();
        }
    }

    private void HideOverlayImage()
    {
        if (anxietyOverlay != null)
        {
            anxietyOverlay.gameObject.SetActive(false);
        }
    }

    private void ClearNpcStatusText()
    {
        if (npcStatusText == null)
        {
            return;
        }

        npcStatusText.text = string.Empty;
    }

    private Color EvaluateAnxietyTextColor(float normalizedIntensity)
    {
        Color low = new Color(0.88f, 0.86f, 0.8f, 1f);
        Color high = overlayColor;
        high.a = 1f;
        return Color.Lerp(low, high, normalizedIntensity);
    }
}
