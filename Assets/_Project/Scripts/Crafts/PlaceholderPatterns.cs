using UnityEngine;

/// Small stand-in target patterns for items that don't have a requiredPattern yet.
/// Rows are written top to bottom; 'X' is a stitch.
public static class PlaceholderPatterns
{
    public const int Size = 8;

    static readonly string[][] Shapes =
    {
        new[] { // heart
            "........",
            "........",
            "..X..X..",
            ".XXXXXX.",
            "..XXXX..",
            "...XX...",
            "........",
            "........" },
        new[] { // diamond
            "........",
            "...X....",
            "..X.X...",
            ".X...X..",
            "..X.X...",
            "...X....",
            "........",
            "........" },
        new[] { // flower
            "........",
            "...X....",
            "..XXX...",
            "...X....",
            "...X....",
            "..XX....",
            "...X....",
            "........" },
    };

    static Color CraftColor(CraftType craft) => craft switch
    {
        CraftType.CrossStitch => new Color(0.85f, 0.25f, 0.30f),
        CraftType.Knitting    => new Color(0.25f, 0.45f, 0.85f),
        CraftType.Crochet     => new Color(0.75f, 0.35f, 0.75f),
        CraftType.Embroidery  => new Color(0.95f, 0.65f, 0.15f),
        _                     => new Color(0.30f, 0.65f, 0.40f),
    };

    /// The item's own pattern if it has one, otherwise a stand-in picked from the item name.
    public static Pattern For(CraftItemDef item)
    {
        if (item.requiredPattern != null && item.requiredPattern.cells.Count > 0) return item.requiredPattern;

        int hash = 0;
        foreach (char ch in item.itemName) hash += ch;   // stable across runs, unlike string.GetHashCode
        var shape = Shapes[hash % Shapes.Length];
        var p = new Pattern { id = $"placeholder_{item.itemName}", displayName = item.itemName, craft = item.craft, width = Size, height = Size };
        p.palette.Add(CraftColor(item.craft));
        for (int row = 0; row < Size; row++)
            for (int x = 0; x < Size; x++)
                if (shape[row][x] == 'X')
                    p.cells.Add(new StitchCell { x = x, y = Size - 1 - row, colorIndex = 0 });
        return p;
    }
}
