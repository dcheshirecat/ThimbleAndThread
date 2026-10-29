using UnityEngine;

// A ready-made pattern, e.g. one of the great-aunt's book patterns.
// Create via Assets > Create > Craft > Pattern. Draw the design as text in "grid":
// one line per row, top row first; '.' is empty and '1'..'9' pick a colour from "palette".
//
//   ..1..1..
//   .111111.
//   ..1111..
//
// Patterns the player designs in the Pattern Studio are stored in their save instead.
[CreateAssetMenu(menuName = "Craft/Pattern")]
public class PatternDef : ScriptableObject
{
    public string patternId;            // stable id used in save files, e.g. "seaside_heart"
    public string displayName;
    public CraftItemDef item;           // what this pattern makes
    public int price = 10;              // starting shelf price; the player can change it
    public bool startsOnShelf = true;   // part of the great-aunt's book at the start of the game
    public Color[] palette = { new Color(0.85f, 0.25f, 0.30f) };
    [TextArea(8, 20)] public string grid;

    /// The grid as pattern data. Falls back to a placeholder shape if the grid is empty.
    public Pattern ToPattern()
    {
        var rows = (grid ?? "").Replace("\r", "").Trim().Split('\n');
        if (rows.Length == 0 || rows[0].Length == 0) return PlaceholderPatterns.For(item);

        int height = rows.Length, width = 0;
        foreach (var r in rows) width = Mathf.Max(width, r.Trim().Length);
        var p = new Pattern { id = patternId, displayName = displayName, craft = item.craft, width = width, height = height };
        p.palette.AddRange(palette);
        for (int row = 0; row < height; row++)
        {
            var line = rows[row].Trim();
            for (int x = 0; x < line.Length; x++)
            {
                int index = line[x] - '1';
                if (index >= 0 && index < palette.Length)
                    p.cells.Add(new StitchCell { x = x, y = height - 1 - row, colorIndex = index });
            }
        }
        return p;
    }
}
