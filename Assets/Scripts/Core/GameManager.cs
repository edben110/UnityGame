using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuracion de capitulos")]
    [SerializeField] private List<ChapterDefinition> chapters = new List<ChapterDefinition>();
    [SerializeField] private string saveFileName = "savegame.json";

    private SaveSystem saveSystem;
    private SaveData currentSaveData;

    public IReadOnlyList<ChapterDefinition> Chapters => chapters;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        string fallbackChapter = GetFirstValidChapterId();
        saveSystem = new SaveSystem(new JsonSaveRepository(saveFileName), fallbackChapter);
        currentSaveData = saveSystem.LoadOrDefault();
    }

    public bool CanContinue()
    {
        return saveSystem != null && saveSystem.HasSave();
    }

    public List<ChapterDefinition> GetUnlockedChapters()
    {
        List<ChapterDefinition> unlocked = new List<ChapterDefinition>();
        if (currentSaveData == null)
        {
            return unlocked;
        }

        foreach (ChapterDefinition chapter in chapters)
        {
            if (chapter == null || !chapter.IsValid())
            {
                continue;
            }

            if (currentSaveData.unlockedChapterIds.Contains(chapter.id))
            {
                unlocked.Add(chapter);
            }
        }

        return unlocked;
    }

    public void StartNewGame()
    {
        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem no inicializado.");
            return;
        }

        currentSaveData = saveSystem.CreateNewSave();
        bool saved = saveSystem.Save(currentSaveData);
        if (!saved)
        {
            Debug.LogWarning("No se pudo persistir nuevo juego, pero se continuara con estado en memoria.");
        }

        LoadCurrentChapterScene();
    }

    public void ContinueGame()
    {
        if (!CanContinue())
        {
            Debug.LogWarning("No hay partida para continuar.");
            return;
        }

        currentSaveData = saveSystem.LoadOrDefault();
        LoadCurrentChapterScene();
    }

    public void LoadChapterById(string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            Debug.LogError("chapterId vacio.");
            return;
        }

        if (currentSaveData == null)
        {
            currentSaveData = saveSystem.LoadOrDefault();
        }

        if (!currentSaveData.unlockedChapterIds.Contains(chapterId))
        {
            Debug.LogWarning($"Capitulo bloqueado: {chapterId}");
            return;
        }

        currentSaveData.currentChapterId = chapterId;
        saveSystem.Save(currentSaveData);
        LoadCurrentChapterScene();
    }

    public void UpdateBasicProgress(int value)
    {
        if (currentSaveData == null)
        {
            currentSaveData = saveSystem.LoadOrDefault();
        }

        currentSaveData.basicProgress = Mathf.Max(0, value);
        saveSystem.Save(currentSaveData);
    }

    public void SaveDecision(string key, string value)
    {
        if (currentSaveData == null)
        {
            currentSaveData = saveSystem.LoadOrDefault();
        }

        currentSaveData.SetDecision(key, value);
        saveSystem.Save(currentSaveData);
    }

    public string GetDecision(string key, string defaultValue = "")
    {
        if (currentSaveData == null)
        {
            currentSaveData = saveSystem.LoadOrDefault();
        }

        return currentSaveData.GetDecision(key, defaultValue);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void LoadCurrentChapterScene()
    {
        ChapterDefinition chapter = GetChapterById(currentSaveData.currentChapterId);
        if (chapter == null)
        {
            Debug.LogError("No se encontro capitulo valido para cargar.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(chapter.sceneName))
        {
            Debug.LogError($"Escena no registrada en Build Settings: {chapter.sceneName}");
            return;
        }

        SceneManager.LoadScene(chapter.sceneName);
    }

    private ChapterDefinition GetChapterById(string chapterId)
    {
        foreach (ChapterDefinition chapter in chapters)
        {
            if (chapter != null && chapter.id == chapterId)
            {
                return chapter;
            }
        }

        return null;
    }

    private string GetFirstValidChapterId()
    {
        foreach (ChapterDefinition chapter in chapters)
        {
            if (chapter != null && chapter.IsValid())
            {
                return chapter.id;
            }
        }

        Debug.LogWarning("No hay capitulos validos configurados. Se usara 'chapter_01'.");
        return "chapter_01";
    }
}
