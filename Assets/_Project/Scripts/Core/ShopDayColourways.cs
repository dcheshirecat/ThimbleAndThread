using System.Collections.Generic;
using UnityEngine;

/// Colourways (owner, 2026-11-02): a pattern made in other colours, and customers who ask for one ("Could you do it in
/// blues?"). The order then starts in the Pattern Studio designer with the pattern's colours to swap (the designer's
/// recolour mode); the closer the colours come to what they asked for, the happier they are at the pickup. The new
/// colourway goes in the pattern book too.
public partial class ShopDay
{
    public const float ColourWishChance = 0.3f;

    /// The colour families a customer can ask for: a key (saved in Order.colourWish), how they say it, and why.
    static readonly (string key, string words, string[] why)[] ColourFamilies =
    {
        ("blue", "blues", new[] { "Everything at home is blue. I can't help it.", "Something the colour of the sea, maybe?" }),
        ("green", "greens", new[] { "Green's my colour. Always has been.", "Something that looks like the garden in June." }),
        ("pink", "pinks", new[] { "Pink, please. Unapologetically pink.", "Soft pinks, if you can. Like the inside of a shell." }),
        ("purple", "purples", new[] { "Purple would be perfect. Heather and plum.", "My gran always wore purple. It'd remind me of her." }),
        ("autumn", "autumn colours", new[] { "Rusts and golds, like the trees on the hill in October.", "Warm colours. Oranges, browns. Cosy." }),
        ("neutral", "creams and greys", new[] { "Nothing too bright. Creams, greys, quiet colours.", "Something calm. Whites and greys, like a foggy morning." }),
    };

    /// The family a colour belongs to (by hue, with greys and browns picked out first).
    public static string FamilyOf(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        if (s < 0.18f || v < 0.15f) return "neutral";
        if (h >= 0.03f && h < 0.17f) return "autumn";          // orange, gold, brown, mustard
        if (h >= 0.17f && h < 0.45f) return "green";
        if (h >= 0.45f && h < 0.72f) return "blue";
        if (h >= 0.72f && h < 0.85f) return "purple";
        if (h >= 0.85f || (h < 0.03f && s < 0.6f && v > 0.75f)) return "pink";
        return "autumn";                                        // deep reds sit with the warm colours
    }

    public static string WishWords(string key)
    {
        foreach (var f in ColourFamilies) if (f.key == key) return f.words;
        return key;
    }

    /// The share of a pattern's stitches in a colour family (0..1).
    public static float ColourMatch(Pattern p, string key)
    {
        if (p == null || p.cells.Count == 0) return 0f;
        int n = 0;
        foreach (var cell in p.cells)
            if (cell.colorIndex >= 0 && cell.colorIndex < p.palette.Count && FamilyOf(p.palette[cell.colorIndex]) == key) n++;
        return n / (float)p.cells.Count;
    }

    /// Now and then a customer ordering a pattern asks for it in other colours. Only once you can design for that craft;
    /// never the colours it's in already. Returns what they say about it, or "".
    string MaybeColourWish(CustomerDef c, Order o)
    {
        if (o == null || o.item == null || o.target == null || o.target.cells.Count == 0) return "";
        if (!Progress.Knows(PatternDesignSkill) || !Progress.CanDesign(o.item)) return "";
        if (Random.value >= ColourWishChance) return "";
        var choices = new List<int>();
        for (int i = 0; i < ColourFamilies.Length; i++)
            if (ColourMatch(o.target, ColourFamilies[i].key) < 0.4f) choices.Add(i);
        if (choices.Count == 0) return "";
        var f = ColourFamilies[choices[Random.Range(0, choices.Count)]];
        o.colourWish = f.key;
        o.needsDesign = true;
        return $"\n\nCould you do it in {f.words}, though? {f.why[Random.Range(0, f.why.Length)]}\n(You'll recolour the pattern before you start.)";
    }

    /// At the pickup: how they take the colours you chose.
    string ColourReaction(Order o, ref int points)
    {
        if (string.IsNullOrEmpty(o.colourWish)) return null;
        float match = ColourMatch(o.target, o.colourWish);
        string words = WishWords(o.colourWish);
        if (match >= 0.6f) { points += 6; return Pick($"And in {words}, just like I asked. It's perfect.", $"Oh, the colours! That's exactly what I pictured."); }
        if (match >= 0.3f) return Pick($"Some {words} in there, I see. It's nice.", "The colours are... nearly what I had in mind. It's lovely anyway.");
        points = Mathf.Max(0, points - 3);
        return Pick($"Oh. I did say {words}... Never mind.", $"It's lovely work. I just thought it'd be in {words}.");
    }
}
