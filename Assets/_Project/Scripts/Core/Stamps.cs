using System;
using UnityEngine;

/// Achievement themes: each is a differently coloured stamp in the Journal (owner, 2026-10-31: every
/// achievement is announced with a pop-up and logged in the Journal as themed stamps).
public enum StampTheme { Craft, Skill, Friendship, Romance, Story, Shop }

/// One achievement, stamped in the Journal.
[Serializable]
public class Stamp
{
    public string id;          // permanent, e.g. "skill:french_knots", "friend:mara:2"
    public string title;
    public string detail;
    public StampTheme theme;
    public int day;
}

public static class Stamps
{
    /// Raised when a stamp is earned (not for ones filled in quietly for older saves).
    public static event Action<Stamp> OnEarned;

    /// A short message to show in the same pop-up (e.g. the doorbell while on another screen).
    /// The bool is true when it may wait for a calm moment (a goal reached), false to show it now.
    public static event Action<string, bool> OnNotice;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { OnEarned = null; OnNotice = null; }

    public static void Notice(string text) => OnNotice?.Invoke(text, false);

    /// Like Notice, but held until the player isn't talking or crafting (goals reached, rewards).
    public static void Announce(string text) => OnNotice?.Invoke(text, true);

    /// Stamps it if it's new. quiet: no pop-up and no news (catching up an older save).
    public static bool Earn(SaveData save, string id, string title, string detail, StampTheme theme, bool quiet = false)
    {
        if (save == null || save.stamps.Exists(s => s.id == id)) return false;
        var stamp = new Stamp { id = id, title = title, detail = detail, theme = theme, day = save.day };
        save.stamps.Add(stamp);
        if (!quiet) OnEarned?.Invoke(stamp);
        return true;
    }

    public static Color ColorOf(StampTheme t) => t switch
    {
        StampTheme.Craft => new Color(0.78f, 0.38f, 0.33f),
        StampTheme.Skill => new Color(0.30f, 0.50f, 0.72f),
        StampTheme.Friendship => new Color(0.88f, 0.58f, 0.25f),
        StampTheme.Romance => new Color(0.82f, 0.33f, 0.52f),
        StampTheme.Story => new Color(0.45f, 0.38f, 0.68f),
        _ => new Color(0.35f, 0.62f, 0.42f),
    };

    /// The word inked round the stamp.
    public static string Word(StampTheme t) => t switch
    {
        StampTheme.Craft => "CRAFT",
        StampTheme.Skill => "SKILL",
        StampTheme.Friendship => "FRIEND",
        StampTheme.Romance => "♥",
        StampTheme.Story => "STORY",
        _ => "SHOP",
    };
}
