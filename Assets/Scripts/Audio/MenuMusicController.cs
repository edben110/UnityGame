using UnityEngine;

/// <summary>
/// Controla la música exclusiva del menú principal.
/// Se destruye automáticamente al cambiar de escena (NO usa DontDestroyOnLoad).
/// </summary>
public class MenuMusicController : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = true;

        if (audioSource.clip == null)
        {
            audioSource.clip = Resources.Load<AudioClip>("Audio/MusicaInicio");
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
