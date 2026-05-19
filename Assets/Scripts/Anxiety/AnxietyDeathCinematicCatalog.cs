using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Resuelve el VideoClip de muerte por personaje (Resources/Cinematicas).
/// </summary>
public static class AnxietyDeathCinematicCatalog
{
    private static readonly Dictionary<string, string> ResourceNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "robert", "Robet_cine" },
            { "ana", "Ana_cine" },
            { "ben", "Ben_cine" },
            { "lisa", "Lisa_cine" },
            { "lucas", "Lucas_cine" }
        };

    public static bool TryLoadClip(string characterId, out VideoClip clip)
    {
        clip = null;
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        string normalizedId = characterId.Trim();
        if (!ResourceNames.TryGetValue(normalizedId, out string resourceName))
        {
            resourceName = normalizedId.Length > 0
                ? $"{char.ToUpper(normalizedId[0])}{normalizedId.Substring(1)}_cine"
                : string.Empty;
        }

        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return false;
        }

        clip = Resources.Load<VideoClip>($"Cinematicas/{resourceName}");
        if (clip != null)
        {
            return true;
        }

        clip = Resources.Load<VideoClip>($"Cinematicas/{resourceName.ToLowerInvariant()}");
        return clip != null;
    }

    public static void ReleaseClip(VideoClip clip)
    {
        if (clip == null)
        {
            return;
        }

        Resources.UnloadAsset(clip);
    }
}
