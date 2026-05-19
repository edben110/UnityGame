using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Persiste ansiedad y pertenencia al grupo en JSON (para fin de juego y continuidad).
/// </summary>
public static class CharacterGroupStateRepository
{
    private const string FileName = "character_group_state.json";

    private static CharacterGroupStateFile cache;
    private static readonly Dictionary<string, CharacterGroupStateEntry> lookup =
        new Dictionary<string, CharacterGroupStateEntry>(StringComparer.OrdinalIgnoreCase);

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void Load()
    {
        lookup.Clear();
        cache = new CharacterGroupStateFile();

        if (!File.Exists(FilePath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            CharacterGroupStateFile loaded = JsonUtility.FromJson<CharacterGroupStateFile>(json);
            if (loaded?.characters == null)
            {
                return;
            }

            cache = loaded;
            for (int i = 0; i < cache.characters.Count; i++)
            {
                CharacterGroupStateEntry entry = cache.characters[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.id))
                {
                    continue;
                }

                lookup[entry.id] = entry;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CharacterGroupStateRepository] Error al cargar: {ex.Message}");
        }
    }

    public static void Save()
    {
        if (cache == null)
        {
            cache = new CharacterGroupStateFile();
        }

        cache.characters.Clear();
        foreach (CharacterGroupStateEntry entry in lookup.Values)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.id))
            {
                continue;
            }

            cache.characters.Add(entry);
        }

        try
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CharacterGroupStateRepository] Error al guardar: {ex.Message}");
        }
    }

    public static void Reset()
    {
        lookup.Clear();
        cache = new CharacterGroupStateFile();

        if (File.Exists(FilePath))
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CharacterGroupStateRepository] No se pudo borrar archivo: {ex.Message}");
            }
        }
    }

    public static CharacterGroupStateEntry GetOrCreate(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        if (!lookup.TryGetValue(characterId, out CharacterGroupStateEntry entry) || entry == null)
        {
            entry = new CharacterGroupStateEntry
            {
                id = characterId,
                anxiety = 0f,
                isInGroup = true,
                cinematicPlayed = false
            };
            lookup[characterId] = entry;
        }

        return entry;
    }

    public static bool TryGet(string characterId, out CharacterGroupStateEntry entry)
    {
        entry = null;
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        return lookup.TryGetValue(characterId, out entry) && entry != null;
    }

    public static void SetAnxiety(string characterId, float anxiety)
    {
        CharacterGroupStateEntry entry = GetOrCreate(characterId);
        if (entry == null)
        {
            return;
        }

        entry.anxiety = Mathf.Clamp(anxiety, 0f, 100f);
        Save();
    }

    public static void SetInGroup(string characterId, bool inGroup)
    {
        CharacterGroupStateEntry entry = GetOrCreate(characterId);
        if (entry == null)
        {
            return;
        }

        entry.isInGroup = inGroup;
        Save();
    }

    public static void SetCinematicPlayed(string characterId, bool played)
    {
        CharacterGroupStateEntry entry = GetOrCreate(characterId);
        if (entry == null)
        {
            return;
        }

        entry.cinematicPlayed = played;
        Save();
    }

    public static bool IsInGroup(string characterId)
    {
        return TryGet(characterId, out CharacterGroupStateEntry entry) && entry.isInGroup;
    }

    public static IReadOnlyList<CharacterGroupStateEntry> GetAllEntries()
    {
        if (cache == null)
        {
            Load();
        }

        return cache?.characters ?? new List<CharacterGroupStateEntry>();
    }

    public static int CountSeparated()
    {
        int count = 0;
        foreach (CharacterGroupStateEntry entry in lookup.Values)
        {
            if (entry != null && !entry.isInGroup)
            {
                count++;
            }
        }

        return count;
    }
}
