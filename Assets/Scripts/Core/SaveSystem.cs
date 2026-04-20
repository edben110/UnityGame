using UnityEngine;
using System.Collections.Generic;

public class SaveSystem
{
    private readonly ISaveRepository repository;
    private readonly string fallbackSceneName;
    private readonly List<string> defaultChapterIds;

    public SaveSystem(ISaveRepository repository, string fallbackSceneName, List<string> defaultChapterIds)
    {
        this.repository = repository;
        this.fallbackSceneName = fallbackSceneName;
        this.defaultChapterIds = defaultChapterIds ?? new List<string>();
    }

    public bool HasSave()
    {
        return repository != null && repository.Exists();
    }

    public SaveData CreateNewSave()
    {
        SaveData data = new SaveData
        {
            lastSceneName = fallbackSceneName,
            basicProgress = 0
        };

        data.EnsureValid(fallbackSceneName, defaultChapterIds);
        return data;
    }

    public SaveData LoadOrDefault()
    {
        if (repository == null)
        {
            Debug.LogError("SaveSystem sin repositorio. Se usara estado por defecto.");
            return CreateNewSave();
        }

        if (!repository.Exists())
        {
            Debug.Log("No existe partida guardada todavia. Se usaran valores por defecto.");
            return CreateNewSave();
        }

        if (!repository.TryLoad(out SaveData loaded) || loaded == null)
        {
            Debug.LogWarning("Fallo al cargar guardado. Se usaran valores por defecto.");
            return CreateNewSave();
        }

        loaded.SyncAfterLoad();
        loaded.EnsureValid(fallbackSceneName, defaultChapterIds);
        return loaded;
    }

    public bool Save(SaveData data)
    {
        if (repository == null || data == null)
        {
            Debug.LogError("No se puede guardar: repositorio o data nulos.");
            return false;
        }

        data.EnsureValid(fallbackSceneName, defaultChapterIds);
        data.SyncBeforeSave();
        return repository.TrySave(data);
    }

    public bool DeleteSave()
    {
        if (repository == null)
        {
            return false;
        }

        return repository.Delete();
    }
}
