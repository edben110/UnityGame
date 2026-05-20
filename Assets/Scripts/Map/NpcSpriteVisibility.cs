using UnityEngine;

/// <summary>
/// Control visual de sprites de NPC (opacidad e interacción).
/// </summary>
public static class NpcSpriteVisibility
{
    public const float VisibleAlpha = 1f;
    public const float DeadAlpha = 0f;

    public static void SetOpacity(GameObject root, float alpha)
    {
        if (root == null)
        {
            return;
        }

        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Color color = renderer.color;
            color.a = Mathf.Clamp01(alpha);
            renderer.color = color;
        }
    }

    public static void SetInteractionEnabled(GameObject root, bool enabled)
    {
        if (root == null)
        {
            return;
        }

        NpcInteractable interactable = root.GetComponent<NpcInteractable>();
        if (interactable != null)
        {
            interactable.enabled = enabled;
        }

        Collider2D[] colliders = root.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = enabled;
            }
        }
    }

    public static void ApplyDeathVisual(GameObject root)
    {
        SetOpacity(root, DeadAlpha);
        SetInteractionEnabled(root, false);
    }

    public static void RestoreAliveVisual(GameObject root)
    {
        SetOpacity(root, VisibleAlpha);
        SetInteractionEnabled(root, true);
    }
}
