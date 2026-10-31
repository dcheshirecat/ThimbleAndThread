using System;
using System.Collections.Generic;
using UnityEngine;

/// Who the player is (owner, 2026-10-30): a name, pronouns everyone uses, and how their shopkeeper looks.
/// Chosen in the customization scene at the start of a game (UI/CustomizeUI.cs); clothes, hair and
/// accessories bought at the boutique are added to "owned" and worn per slot.
[Serializable]
public class PlayerProfile
{
    public bool created;                 // the customization scene has been finished
    public string name = "";
    public int pronouns = 2;             // index into PlayerText.Sets: 0 she/her, 1 he/him, 2 they/them
    public int skin = 2, hairStyle, hairColor, top, topColor;   // indices into the Appearance options
    public string wornHair = "", wornTop = "", wornAccessory = "";   // boutique items (CosmeticDef ids) worn, "" = none
    public List<string> owned = new List<string>();                 // boutique items bought
}

/// The player's pronouns in dialogue and story text. Writers use tokens (see Docs/ADDING_CONTENT.md):
/// {name}, {they} {them} {their} {theirs} {themself} (and capitalised {They} {Them} {Their}),
/// {s} for a verb ending ("{they} like{s}" = she likes / they like), {are} / {were} / {have} / {do},
/// {is} ("{they} {is}" = she is / they are), and {niece} (niece / nephew / nibling).
public static class PlayerText
{
    public class Pronouns
    {
        public string label, they, them, their, theirs, themself, s, are, were, have, does, niece;
    }

    public static readonly Pronouns[] Sets =
    {
        new Pronouns { label = "she/her", they = "she", them = "her", their = "her", theirs = "hers", themself = "herself", s = "s", are = "is", were = "was", have = "has", does = "does", niece = "niece" },
        new Pronouns { label = "he/him", they = "he", them = "him", their = "his", theirs = "his", themself = "himself", s = "s", are = "is", were = "was", have = "has", does = "does", niece = "nephew" },
        new Pronouns { label = "they/them", they = "they", them = "them", their = "their", theirs = "theirs", themself = "themself", s = "", are = "are", were = "were", have = "have", does = "do", niece = "nibling" },
    };

    static PlayerProfile Player => GameManager.Instance != null && GameManager.Instance.Save != null ? GameManager.Instance.Save.player : null;

    /// Replaces the player tokens in a line of text. Text without tokens comes back unchanged.
    public static string Fill(string text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;
        var p = Player;
        var set = Sets[Mathf.Clamp(p != null ? p.pronouns : 2, 0, Sets.Length - 1)];
        string name = p != null && !string.IsNullOrWhiteSpace(p.name) ? p.name.Trim() : "friend";
        return text
            .Replace("{name}", name)
            .Replace("{they}", set.they).Replace("{They}", Cap(set.they))
            .Replace("{them}", set.them).Replace("{Them}", Cap(set.them))
            .Replace("{their}", set.their).Replace("{Their}", Cap(set.their))
            .Replace("{theirs}", set.theirs).Replace("{themself}", set.themself)
            .Replace("{s}", set.s).Replace("{are}", set.are).Replace("{is}", set.are)
            .Replace("{were}", set.were).Replace("{was}", set.were).Replace("{have}", set.have).Replace("{do}", set.does)
            .Replace("{niece}", set.niece);
    }

    static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
}

/// The look options for the shopkeeper (placeholder colours; the owner's art will replace the shapes).
public static class Appearance
{
    public static readonly Color[] Skin =
    {
        new Color(0.99f, 0.88f, 0.78f), new Color(0.96f, 0.80f, 0.66f), new Color(0.87f, 0.68f, 0.52f),
        new Color(0.72f, 0.52f, 0.36f), new Color(0.55f, 0.38f, 0.25f), new Color(0.38f, 0.26f, 0.18f),
    };

    public static readonly Color[] Hair =
    {
        new Color(0.12f, 0.10f, 0.10f), new Color(0.40f, 0.26f, 0.18f), new Color(0.70f, 0.45f, 0.25f),
        new Color(0.92f, 0.80f, 0.50f), new Color(0.75f, 0.30f, 0.20f), new Color(0.80f, 0.80f, 0.82f),
        new Color(0.85f, 0.55f, 0.75f), new Color(0.40f, 0.60f, 0.85f),
    };

    public static readonly Color[] Cloth =
    {
        new Color(0.45f, 0.55f, 0.75f), new Color(0.85f, 0.45f, 0.45f), new Color(0.45f, 0.70f, 0.50f),
        new Color(0.95f, 0.85f, 0.55f), new Color(0.60f, 0.45f, 0.70f), new Color(0.30f, 0.30f, 0.35f),
        new Color(0.95f, 0.95f, 0.92f), new Color(0.80f, 0.60f, 0.40f),
    };

    public static readonly string[] HairStyles = { "Short", "Bob", "Long", "Bun", "Curly" };
    public static readonly string[] Tops = { "Jumper", "Apron", "Cardigan" };
    /// Hair styles and tops free from the start (the boutique sells the rest).
    public const int FreeHairStyles = 3, FreeTops = 2;
}
