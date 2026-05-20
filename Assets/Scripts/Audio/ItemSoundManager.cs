using UnityEngine;

/// <summary>
/// Singleton que reproduce el sonido de recolección de ítems.
/// Usa PlayOneShot para no interrumpir la música ambiente.
/// </summary>
public class ItemSoundManager : MonoBehaviour
{
    public static ItemSoundManager Instance { get; private set; }

    private AudioSource audioSource;
    private AudioClip itemClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = false;
        audioSource.volume = 0.7f;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;

        // Usar el clip asignado en el AudioSource del Inspector, o cargar desde Resources como fallback
        if (audioSource.clip != null)
        {
            itemClip = audioSource.clip;
        }
        else
        {
            itemClip = Resources.Load<AudioClip>("Audio/Items");
        }
    }

    /// <summary>
    /// Reproduce el sonido de recolección de ítem una sola vez.
    /// </summary>
    public void PlayItemSound()
    {
        if (itemClip != null)
        {
            audioSource.PlayOneShot(itemClip, 0.7f);
        }
        else
        {
            Debug.LogWarning("[ItemSoundManager] No se encontró el clip Audio/Items en Resources.");
        }
    }
}
