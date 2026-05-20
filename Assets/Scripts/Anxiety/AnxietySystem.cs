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

    private const string NpcAnxietyNumberColor = "#FFFFFF";
    private const string NpcAnxietySuffixColor = "#8A8580";

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
        ShowNpcAnxiety(Mathf.RoundToInt(Mathf.Clamp01(normalizedIntensity) * 100f));
    }

    public void ShowVerificationOverlay(float normalizedIntensity, string statusMessage)
    {
        ShowNpcAnxiety(Mathf.RoundToInt(Mathf.Clamp01(normalizedIntensity) * 100f));
    }

    /// <summary>
    /// Muestra solo el valor de ansiedad del NPC en NpcStatusText (ej. 45/100).
    /// El número va en blanco; el sufijo /100 en gris para contraste.
    /// </summary>
    public void ShowNpcAnxiety(int anxietyPercent)
    {
        HideOverlayImage();
        TryResolveNpcStatusText();

        if (npcStatusText == null)
        {
            return;
        }

        int value = Mathf.Clamp(anxietyPercent, 0, 100);
        npcStatusText.gameObject.SetActive(true);
        npcStatusText.richText = true;
        npcStatusText.text =
            $"<color={NpcAnxietyNumberColor}><b>{value}</b></color><color={NpcAnxietySuffixColor}>/100</color>";
        npcStatusText.color = Color.white;
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
}
