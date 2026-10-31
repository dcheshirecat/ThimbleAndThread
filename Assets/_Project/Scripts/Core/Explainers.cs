using System;
using UnityEngine;

/// Short, well-timed explanations (owner, 2026-10-31): the first time a crafting step is opened, and when
/// anything that wasn't there at the start unlocks (a shop, a craft, the Pattern Studio). Each one shows
/// once per save: it's remembered as "explained:<key>" in SaveData.flags when the player has read it.
/// UI/ExplainerUI.cs shows them, a page at a time.
public static class Explainers
{
    /// key, pages (the first line of a page is its title), anywhere (false: waits for a calm moment).
    public static event Action<string, string[], bool> OnExplain;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => OnExplain = null;

    public static string Flag(string key) => $"explained:{key}";

    public static bool Seen(SaveData save, string key) => save == null || save.flags.Contains(Flag(key));

    /// Explains something once. anywhere: show it right away (a crafting step), else wait for a calm moment.
    public static void Explain(SaveData save, string key, bool anywhere, params string[] pages)
    {
        if (Seen(save, key) || pages == null || pages.Length == 0) return;
        OnExplain?.Invoke(key, pages, anywhere);
    }

    /// The player has read it: don't show it again.
    public static void MarkSeen(SaveData save, string key)
    {
        if (save != null && !save.flags.Contains(Flag(key))) save.flags.Add(Flag(key));
    }
}
