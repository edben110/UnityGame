using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Controla la música ambiente global. Singleton persistente con DontDestroyOnLoad.
/// Se pausa automáticamente durante cinemáticas y no suena en MenuScene.
/// </summary>
public class AmbientMusicController : MonoBehaviour
{
    public static AmbientMusicController Instance { get; private set; }

    private AudioSource audioSource;
    private bool pausedByCinematic = false;

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

        audioSource.loop = true;
        audioSource.volume = 0.3f;
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = true;

        if (audioSource.clip == null)
        {
            audioSource.clip = Resources.Load<AudioClip>("Audio/SonidoAmbiente");
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        HandleSceneAudio(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneAudio(scene.name);
    }

    private void HandleSceneAudio(string sceneName)
    {
        if (sceneName == "MenuScene")
        {
            audioSource.Stop();
        }
        else
        {
            if (!audioSource.isPlaying && !pausedByCinematic)
            {
                audioSource.Play();
            }
        }
    }

    private void Update()
    {
        bool cinematicActive = IsCinematicPlaying();

        if (cinematicActive && !pausedByCinematic)
        {
            audioSource.Pause();
            pausedByCinematic = true;
        }
        else if (!cinematicActive && pausedByCinematic)
        {
            audioSource.UnPause();
            pausedByCinematic = false;
        }
    }

    private bool IsCinematicPlaying()
    {
        // Detectar EndingCinematicPlayer via su propiedad estática IsPlaying
        if (EndingCinematicPlayer.IsPlaying)
        {
            return true;
        }

        // Detectar AnxietyDeathCinematicPlayer sin modificar su script.
        // Usamos su Instance pública y buscamos el VideoPlayer hijo para verificar si está reproduciendo.
        if (AnxietyDeathCinematicPlayer.Instance != null)
        {
            VideoPlayer vp = AnxietyDeathCinematicPlayer.Instance.GetComponentInChildren<VideoPlayer>();
            if (vp != null && vp.isPlaying)
            {
                return true;
            }
        }

        return false;
    }
}
