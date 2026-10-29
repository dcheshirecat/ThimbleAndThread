using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string PathFor(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public static bool Exists(int slot = 0) => File.Exists(PathFor(slot));

    public static SaveData NewGame() => new SaveData { coins = 50, day = 1 };

    public static void Save(SaveData data, int slot = 0)
    {
        File.WriteAllText(PathFor(slot), JsonUtility.ToJson(data, true));
    }

    public static SaveData Load(int slot = 0)
    {
        if (!Exists(slot)) return NewGame();
        try { return JsonUtility.FromJson<SaveData>(File.ReadAllText(PathFor(slot))) ?? NewGame(); }
        catch { Debug.LogWarning("Save file unreadable, starting new game."); return NewGame(); }
    }
}
