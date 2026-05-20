using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Carga los videos de finales desde Resources/Cinematicas (Final_1, Final_2, Final_3).
/// </summary>
public static class EndingCinematicCatalog
{
    public const string Final1Resource = "Final_1";
    public const string Final2Resource = "Final_2";
    public const string Final3Resource = "Final_3";

    public static bool TryLoadEnding(int endingIndex, out VideoClip clip)
    {
        clip = null;
        string resourceName = endingIndex switch
        {
            1 => Final1Resource,
            2 => Final2Resource,
            3 => Final3Resource,
            _ => null
        };

        if (string.IsNullOrEmpty(resourceName))
        {
            return false;
        }

        clip = Resources.Load<VideoClip>($"Cinematicas/{resourceName}");
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
