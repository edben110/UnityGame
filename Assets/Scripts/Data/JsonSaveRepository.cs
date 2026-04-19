using System;
using System.IO;
using UnityEngine;

public class JsonSaveRepository : ISaveRepository
{
    private readonly string filePath;

    public JsonSaveRepository(string fileName)
    {
        string safeFileName = string.IsNullOrWhiteSpace(fileName) ? "savegame.json" : fileName;
        filePath = Path.Combine(Application.persistentDataPath, safeFileName);
    }

    public bool Exists()
    {
        return File.Exists(filePath);
    }

    public bool TryLoad(out SaveData saveData)
    {
        saveData = null;

        try
        {
            if (!Exists())
            {
                Debug.LogWarning("No existe archivo de guardado.");
                return false;
            }

            string rawJson = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                Debug.LogError("Archivo de guardado vacio o invalido.");
                return false;
            }

            saveData = JsonUtility.FromJson<SaveData>(rawJson);
            if (saveData == null)
            {
                Debug.LogError("No se pudo parsear el JSON de guardado.");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error cargando guardado JSON: {ex.Message}");
            return false;
        }
    }

    public bool TrySave(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("No se puede guardar SaveData nulo.");
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error guardando JSON: {ex.Message}");
            return false;
        }
    }

    public bool Delete()
    {
        try
        {
            if (Exists())
            {
                File.Delete(filePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error eliminando guardado: {ex.Message}");
            return false;
        }
    }
}
