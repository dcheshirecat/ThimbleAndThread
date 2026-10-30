using UnityEngine;

/// Where a knitted piece's stitches sit in the step area: one stitch per pattern square, rows from
/// the bottom up. Squares filled in on the pattern are purl stitches, the rest knit, which is what
/// gives the piece its texture.
public struct KnitLayout
{
    public int width, height;      // stitches per row, rows
    public float cell;             // size of one stitch, in 0..1 of the area
    public Vector2 origin;         // bottom-left corner of the piece
    Pattern pattern;

    public static KnitLayout For(Pattern p)
    {
        var k = new KnitLayout { pattern = p, width = Mathf.Max(4, p != null ? p.width : 8), height = Mathf.Max(4, p != null ? p.height : 8) };
        k.cell = 0.8f / Mathf.Max(k.width, k.height);
        k.origin = new Vector2(0.5f - k.width * k.cell / 2f, 0.5f - k.height * k.cell / 2f);
        return k;
    }

    public int Stitches => width * height;
    public Vector2 Size => new Vector2(width, height) * cell;
    public Vector2 Center => origin + Size / 2f;

    /// Stitch number i: rows from the bottom, each row left to right.
    public Vector2Int At(int i) => new Vector2Int(i % width, i / width);
    public Vector2 CellCenter(Vector2Int s) => origin + new Vector2((s.x + 0.5f) * cell, (s.y + 0.5f) * cell);
    public bool Purl(Vector2Int s) => pattern != null && pattern.cells.Exists(c => c.x == s.x && c.y == s.y);
}
