using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TMP_Text label;
    private Image glowImage;
    private Color normalColor;
    private Color hoverColor;
    private float normalSize;
    private float hoverSize;

    public void Init(Color normal, Color hover, float baseFontSize)
    {
        label = GetComponentInChildren<TMP_Text>(true);
        normalColor = normal;
        hoverColor = hover;
        normalSize = baseFontSize;
        hoverSize = baseFontSize + 2;

        // Resplandor redondeado
        GameObject glowObj = new GameObject("Glow", typeof(RectTransform), typeof(Image));
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.SetAsFirstSibling();

        RectTransform glowRect = glowObj.GetComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(-10f, -2f);
        glowRect.offsetMax = new Vector2(10f, 2f);

        glowImage = glowObj.GetComponent<Image>();
        glowImage.color = Color.clear;
        glowImage.raycastTarget = false;

        // Crear textura con bordes redondeados y gradiente
        Texture2D gradTex = CreateRoundedGradient();
        glowImage.sprite = Sprite.Create(
            gradTex,
            new Rect(0, 0, gradTex.width, gradTex.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(16, 16, 16, 16)
        );
        glowImage.type = Image.Type.Sliced;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (label != null)
        {
            label.color = hoverColor;
            label.fontSize = hoverSize;
        }
        if (glowImage != null)
            glowImage.color = new Color(0.55f, 0.04f, 0.04f, 0.30f);

        CursorManager.SetHand();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (label != null)
        {
            label.color = normalColor;
            label.fontSize = normalSize;
        }
        if (glowImage != null)
            glowImage.color = Color.clear;

        CursorManager.SetDefault();
    }

    private void OnDisable()
    {
        // Restaurar cursor si el botón se desactiva mientras el mouse está encima
        CursorManager.SetDefault();
    }

    private static Texture2D CreateRoundedGradient()
    {
        int width = 128;
        int height = 32;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Fade horizontal — fuerte a la izquierda, desvanece a la derecha
                float fx = (float)x / width;
                float alphaH = Mathf.Pow(1f - fx, 2.5f);

                // Fade vertical — desvanece desde el centro hacia arriba y abajo
                float fy = Mathf.Abs((float)y / height - 0.5f) * 2f;
                float alphaV = Mathf.Pow(1f - fy, 2f);

                // Fade en el borde izquierdo tambien (suave)
                float leftFade = Mathf.Clamp01((float)x / 15f);

                float alpha = alphaH * alphaV * leftFade;
                tex.SetPixel(x, y, new Color(1f, 0.12f, 0.08f, alpha));
            }
        }

        tex.Apply();
        return tex;
    }
}
