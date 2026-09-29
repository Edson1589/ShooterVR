using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const int CurrentVersion = 1;
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static bool TryLoad(out SaveData data)
    {
        return TryRead(SavePath, out data) || TryRead(SavePath + ".bak", out data);
    }

    public static bool StartNewGame(string scene)
    {
        return Save(new SaveData { version = CurrentVersion, nextScene = scene });
    }

    public static bool CompleteLevel(string nextScene, bool isFinalLevel, SaveData.LevelResult result)
    {
        if (!TryLoad(out SaveData data)) data = new SaveData { version = CurrentVersion };
        data.nextScene = isFinalLevel ? "" : nextScene;
        data.gameCompleted = isFinalLevel;
        if (result != null)
        {
            data.results.RemoveAll(previous => previous.scene == result.scene);
            data.results.Add(result);
        }
        return Save(data);
    }

    private static bool IsValid(SaveData data)
    {
        return data != null && data.version == CurrentVersion && data.results != null && data.results.TrueForAll(result => result != null) && (data.gameCompleted ? string.IsNullOrEmpty(data.nextScene) : !string.IsNullOrWhiteSpace(data.nextScene) && Application.CanStreamedLevelBeLoaded(data.nextScene));
    }

    private static bool TryRead(string path, out SaveData data)
    {
        data = null;
        try
        {
            if (!File.Exists(path)) return false;
            var saved = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            if (IsValid(saved))
            {
                data = saved;
                return true;
            }
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
        {
            return false;
        }
        return false;
    }

    private static bool Save(SaveData data)
    {
        if (!IsValid(data)) return false;

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string temporaryPath = SavePath + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
            if (File.Exists(SavePath)) File.Replace(temporaryPath, SavePath, SavePath + ".bak");
            else File.Move(temporaryPath, SavePath);
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is NotSupportedException)
        {
            return false;
        }
    }
}
