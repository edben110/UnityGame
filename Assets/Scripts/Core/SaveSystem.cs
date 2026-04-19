using UnityEngine;

public class SaveSystem
{
    private readonly ISaveRepository repository;
    private readonly string fallbackChapterId;

    public SaveSystem(ISaveRepository repository, string fallbackChapterId)
    {
        this.repository = repository;
        this.fallbackChapterId = fallbackChapterId;
    }

    public bool HasSave()
    {
        return repository != null && repository.Exists();
    }

    public SaveData CreateNewSave()
    {
        SaveData data = new SaveData
        {
            currentChapterId = fallbackChapterId,
            basicProgress = 0
        };

        data.EnsureValid(fallbackChapterId);
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
        loaded.EnsureValid(fallbackChapterId);
        return loaded;
    }

    public bool Save(SaveData data)
    {
        if (repository == null || data == null)
        {
            Debug.LogError("No se puede guardar: repositorio o data nulos.");
            return false;
        }

        data.EnsureValid(fallbackChapterId);
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
