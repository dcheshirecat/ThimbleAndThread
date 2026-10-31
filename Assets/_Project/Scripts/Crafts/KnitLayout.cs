using System.Collections.Generic;
using UnityEngine;

/// What a knitted item is shaped like (owner, 2026-10-31: a sweater is knitted like a real sweater, in
/// pieces, with the pattern worked into it, not as a square).
public enum Garment { Square, Scarf, Sweater }

/// Where a knitted piece's stitches sit in the step area, in the order they're knitted: rows from the
/// bottom up, each row left to right, one sub-piece after another (e.g. two sleeves).
/// - Square (old items): the pattern's grid; its filled squares are purls, which make the texture.
/// - Scarf: a long strip with ribbed ends; the pattern's purls repeat along it, and stripes of four rows in
///   the pattern's (first) colour alternate with the main yarn.
/// - Sweater, by piece ("back", "front", "sleeves"): ribbed hems and cuffs, a shaped neckline, tapered
///   sleeves; the front carries the pattern as colourwork (each filled square in its palette colour).
public class KnitLayout
{
    public struct KnitStitch
    {
        public Vector2Int cell;    // in the layout's grid
        public bool purl;
        public int color;          // palette index for colourwork, or -1 for the main yarn
        public int part;           // which sub-piece (0, or 1 for the second sleeve)
        public int row;            // row within its sub-piece
    }

    public float cell;             // size of one stitch, in 0..1 of the area
    public Vector2 origin;         // bottom-left corner of the grid
    public int gridWidth, gridHeight;
    public readonly List<KnitStitch> stitches = new List<KnitStitch>();
    Pattern pattern;

    public int Stitches => stitches.Count;
    public Vector2 Size => new Vector2(gridWidth, gridHeight) * cell;
    public Vector2 Center => origin + Size / 2f;

    public Vector2 CellCenter(int i) => CellCenter(stitches[i].cell);
    public Vector2 CellCenter(Vector2Int s) => origin + new Vector2((s.x + 0.5f) * cell, (s.y + 0.5f) * cell);
    public bool Purl(int i) => stitches[i].purl;

    /// The stitch's yarn colour: the main yarn, or the pattern's colour for colourwork and stripes.
    public Color ColorOf(int i, Color yarn) => PaletteColor(stitches[i].color, yarn);

    /// A yarn by its index: -1 the main yarn, else that pattern colour.
    public Color PaletteColor(int index, Color yarn) =>
        index >= 0 && pattern != null && index < pattern.palette.Count ? pattern.palette[index] : yarn;

    /// The first row of every sub-piece: the stitches cast on.
    public List<int> CastOnStitches()
    {
        var list = new List<int>();
        for (int i = 0; i < stitches.Count; i++) if (stitches[i].row == 0) list.Add(i);
        return list;
    }

    /// The top edge of the first sub-piece, left to right (for casting off).
    public List<Vector2> TopEdge()
    {
        int top = -1;
        foreach (var s in stitches) if (s.part == 0) top = Mathf.Max(top, s.cell.y);
        float minX = float.MaxValue, maxX = float.MinValue;
        foreach (var s in stitches)
            if (s.part == 0 && s.cell.y == top) { minX = Mathf.Min(minX, s.cell.x); maxX = Mathf.Max(maxX, s.cell.x + 1); }
        float y = origin.y + (top + 1) * cell;
        return new List<Vector2> { new Vector2(origin.x + minX * cell, y), new Vector2(origin.x + maxX * cell, y) };
    }

    /// The layout for one step of an order: the item's garment, and the step's piece ("" = the whole thing).
    public static KnitLayout For(Order order, string piece)
    {
        var garment = order != null && order.item != null && !order.legacyKnit ? order.item.garment : Garment.Square;
        return For(order?.target, garment, piece);
    }

    public static KnitLayout For(Pattern p, Garment garment, string piece)
    {
        var k = new KnitLayout { pattern = p };
        int pw = Mathf.Max(1, p != null ? p.width : 8), ph = Mathf.Max(1, p != null ? p.height : 8);
        switch (garment)
        {
            case Garment.Scarf: k.Scarf(pw, ph); break;
            case Garment.Sweater:
                int w = Mathf.Clamp(pw + 2, 8, 14), h = Mathf.Clamp(ph + 5, 10, 17);
                if (piece == "back") k.Body(w, h, false);
                else if (piece == "sleeves") k.Sleeves(w, h);
                else k.Body(w, h, true);   // the front (also what finishing steps show)
                break;
            default: k.Square(Mathf.Max(4, pw), Mathf.Max(4, ph)); break;
        }
        k.Fit();
        return k;
    }

    bool PatternAt(int x, int y) => pattern != null && pattern.cells.Exists(c => c.x == x && c.y == y);

    void Add(int x, int y, bool purl, int color, int part, int row)
    {
        stitches.Add(new KnitStitch { cell = new Vector2Int(x, y), purl = purl, color = color, part = part, row = row });
        gridWidth = Mathf.Max(gridWidth, x + 1);
        gridHeight = Mathf.Max(gridHeight, y + 1);
    }

    void Square(int w, int h)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++) Add(x, y, PatternAt(x, y), -1, 0, y);
    }

    /// k1 p1 ribbing: alternate knit and purl stitches, so the edge pulls in and stays put.
    static bool Rib(int x) => x % 2 == 1;

    void Scarf(int pw, int ph)
    {
        int w = Mathf.Clamp(pw, 6, 9);
        int h = Mathf.Clamp(Mathf.Max(ph * 2, w * 2), 12, 22);
        int offX = Mathf.Max(0, (pw - w) / 2);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool rib = y < 2 || y >= h - 2;
                bool stripe = !rib && pattern != null && pattern.palette.Count > 0 && ((y - 2) / 4) % 2 == 1;
                Add(x, y, rib ? Rib(x) : PatternAt((x + offX) % pw, (y - 2) % ph), stripe ? 0 : -1, 0, y);
            }
    }

    /// The back or front of a sweater: ribbed hem, then the body, then the neckline taken out of the top
    /// rows (deeper on the front). The front has the pattern worked in as colourwork.
    void Body(int w, int h, bool front)
    {
        int pw = pattern != null ? pattern.width : 0, ph = pattern != null ? pattern.height : 0;
        int offX = (w - pw) / 2, offY = 3;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = (x + 0.5f) / w;
                if (front && y == h - 1 && t > 0.25f && t < 0.75f) continue;
                if (front && y == h - 2 && t > 0.35f && t < 0.65f) continue;
                if (!front && y == h - 1 && t > 0.3f && t < 0.7f) continue;
                int color = -1;
                if (front && pattern != null)
                {
                    var c = pattern.cells.Find(s => s.x == x - offX && s.y == y - offY);
                    if (c != null) color = c.colorIndex;
                }
                Add(x, y, y < 2 && Rib(x), color, 0, y);
            }
    }

    /// Two sleeves, side by side: ribbed cuffs, then increases at both edges every few rows, widening
    /// towards the shoulder (as knitters shape sleeves). One is knitted, then the other.
    void Sleeves(int bodyWidth, int bodyHeight)
    {
        int cuff = Mathf.Max(4, Mathf.RoundToInt(bodyWidth * 0.45f)), h = bodyHeight - 3;
        int increases = Mathf.Max(1, Mathf.RoundToInt((bodyWidth * 0.75f - cuff) / 2f));
        int box = cuff + 2 * increases;
        for (int part = 0; part < 2; part++)
        {
            int left = part * (box + 1);
            for (int y = 0; y < h; y++)
            {
                int done = y < 2 ? 0 : Mathf.Min(increases, (y - 1) * (increases + 1) / Mathf.Max(1, h - 2));   // increases so far
                int width = cuff + 2 * done, start = left + increases - done;
                for (int x = 0; x < width; x++) Add(start + x, y, y < 2 && Rib(x), -1, part, y);
            }
        }
    }

    void Fit()
    {
        cell = 0.8f / Mathf.Max(gridWidth, gridHeight);
        origin = new Vector2(0.5f - gridWidth * cell / 2f, 0.5f - gridHeight * cell / 2f);
    }
}
