using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Garantiza que el Canvas de Acertijo2 renderice correctamente en Play Mode.
/// Adjuntar al GameObject Canvas (o se auto-aplica en Awake).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas))]
public class Acertijo2UICanvasSetup : MonoBehaviour
{
    [SerializeField] private Vector2 referenciaResolucion = new Vector2(1920f, 1080f);
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    private void Awake()
    {
        AplicarConfiguracion();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            AplicarConfiguracion();
    }
#endif

    private void AplicarConfiguracion()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        canvas.targetDisplay = 0;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenciaResolucion;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
        }

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;

        Transform fondo = transform.Find("FondoUI");
        if (fondo != null)
        {
            fondo.SetAsFirstSibling();
            RectTransform fondoRt = fondo as RectTransform;
            if (fondoRt != null)
            {
                fondoRt.anchorMin = Vector2.zero;
                fondoRt.anchorMax = Vector2.one;
                fondoRt.offsetMin = Vector2.zero;
                fondoRt.offsetMax = Vector2.zero;
            }
        }
    }
}
