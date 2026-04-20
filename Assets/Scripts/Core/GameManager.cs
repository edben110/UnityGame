using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuracion de capitulos")]
    [SerializeField] private List<ChapterDefinition> chapters = new List<ChapterDefinition>();
    [SerializeField] private string prologueSceneName = "PrologueScene";
    [SerializeField] private string saveFileName = "savegame.json";

    private SaveSystem saveSystem;
    private SceneController sceneController;
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

        List<string> chapterIds = GetValidChapterIds();
        saveSystem = new SaveSystem(new JsonSaveRepository(saveFileName), prologueSceneName, chapterIds);
        sceneController = new SceneController();
        currentSaveData = saveSystem.LoadOrDefault();

        EnsureDefaultUnlockState();
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

            if (currentSaveData.chapterUnlocks.TryGetValue(chapter.id, out bool isUnlocked) && isUnlocked)
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

        saveSystem.DeleteSave();
        currentSaveData = saveSystem.CreateNewSave();

        ChapterDefinition firstChapter = GetFirstValidChapter();
        if (firstChapter != null)
        {
            currentSaveData.chapterUnlocks[firstChapter.id] = true;
        }

        currentSaveData.lastSceneName = prologueSceneName;

        bool saved = saveSystem.Save(currentSaveData);
        if (!saved)
        {
            Debug.LogWarning("No se pudo persistir nuevo juego, pero se continuara con estado en memoria.");
        }

        sceneController.TryLoadScene(prologueSceneName);
    }

    public void ContinueGame()
    {
        if (!CanContinue())
        {
            Debug.LogWarning("No hay partida para continuar.");
            return;
        }

        currentSaveData = saveSystem.LoadOrDefault();

        if (!sceneController.TryLoadScene(currentSaveData.lastSceneName))
        {
            sceneController.TryLoadScene(prologueSceneName);
        }
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

        if (!currentSaveData.chapterUnlocks.TryGetValue(chapterId, out bool unlocked) || !unlocked)
        {
            Debug.LogWarning($"Capitulo bloqueado: {chapterId}");
            return;
        }

        ChapterDefinition chapter = GetChapterById(chapterId);
        if (chapter == null)
        {
            Debug.LogError($"No se encontro capitulo configurado: {chapterId}");
            return;
        }

        currentSaveData.lastSceneName = chapter.sceneName;
        saveSystem.Save(currentSaveData);
        sceneController.TryLoadScene(chapter.sceneName);
    }

    public void UnlockChapter(string chapterId)
    {
        if (string.IsNullOrWhiteSpace(chapterId))
        {
            return;
        }

        if (currentSaveData == null)
        {
            currentSaveData = saveSystem.LoadOrDefault();
        }

        currentSaveData.chapterUnlocks[chapterId] = true;
        saveSystem.Save(currentSaveData);
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

    private ChapterDefinition GetFirstValidChapter()
    {
        foreach (ChapterDefinition chapter in chapters)
        {
            if (chapter != null && chapter.IsValid())
            {
                return chapter;
            }
        }

        return null;
    }

    private List<string> GetValidChapterIds()
    {
        List<string> chapterIds = new List<string>();
        foreach (ChapterDefinition chapter in chapters)
        {
            if (chapter == null || !chapter.IsValid())
            {
                continue;
            }

            chapterIds.Add(chapter.id);
        }

        return chapterIds;
    }

    private void EnsureDefaultUnlockState()
    {
        if (currentSaveData == null)
        {
            return;
        }

        ChapterDefinition firstChapter = GetFirstValidChapter();
        if (firstChapter == null)
        {
            Debug.LogError("No hay capitulos validos configurados en GameManager.");
            return;
        }

        if (!currentSaveData.chapterUnlocks.ContainsKey(firstChapter.id))
        {
            currentSaveData.chapterUnlocks[firstChapter.id] = true;
            saveSystem.Save(currentSaveData);
        }
    }
}
