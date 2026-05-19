using UnityEngine;

/// <summary>
/// Mantiene el panel de intro con anclas a pantalla completa para que sea visible y editable en Scene View.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class MenuIntroUiLayout : MonoBehaviour
{
    [SerializeField] private bool stretchOnValidate = true;

    private void OnValidate()
    {
        if (!stretchOnValidate)
        {
            return;
        }

        StretchFullScreen(transform as RectTransform);

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name is "TutorialView" or "PrologueView")
            {
                StretchFullScreen(child as RectTransform);
            }
        }
    }

    public static void StretchFullScreen(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }
}
