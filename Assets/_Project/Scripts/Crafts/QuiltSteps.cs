using System.Collections.Generic;
using UnityEngine;

/// Quilting (owner, 2026-10-31: new crafts). A patchwork quilt: the pattern's squares are the patches (empty squares
/// are cream ones). Cut the patches with the rotary cutter, hand-piece them into rows, pin the quilt sandwich (top,
/// wadding, backing), quilt diagonal lines through it, then bind the edges (HemStep).
public static class Quilt
{
    public const float Lo = 0.1f, Hi = 0.9f;
    public static readonly Color Cream = new Color(0.96f, 0.92f, 0.82f);

    /// The colour of each patch, row by row from the bottom.
    public static Color PatchColor(Pattern p, int x, int y)
    {
        var cell = p != null ? p.cells.Find(c => c.x == x && c.y == y) : null;
        if (cell == null || cell.colorIndex >= p.palette.Count) return Cream;
        var c = p.palette[cell.colorIndex];
        c.a = 1f;
        return c;
    }

    public static Vector2 PatchCenter(Pattern p, int x, int y, out float size)
    {
        int n = Mathf.Max(1, Mathf.Max(p.width, p.height));
        size = (Hi - Lo) / n;
        return new Vector2(Lo + (x + 0.5f) * size, Lo + (y + 0.5f) * size);
    }
}

/// Cut the patches: roll the cutter along the ruler lines across the stack of fabric.
public class CutPatchesStep : TracingStep
{
    protected override float StitchLength => 0f;   // a cut
    protected override Color ThreadColor => new Color(0.30f, 0.30f, 0.35f);
    protected override string StrokeName => "cut";

    protected override void DrawPiece()
    {
        Square(new Vector2(0.5f, 0.5f), 0.84f, Color.Lerp(area.fabricColor, Color.black, 0.12f), -1);   // the layers underneath
        Square(new Vector2(0.505f, 0.495f), 0.82f, area.fabricColor, 0);
        Square(new Vector2(0.5f, 0.5f), 0.8f, new Color(0.35f, 0.6f, 0.55f, 0.18f), 0);   // the cutting mat's green under it all
    }

    protected override List<Vector2> Guide() => Guides()[0];

    protected override List<List<Vector2>> Guides()
    {
        var lines = new List<List<Vector2>>();
        for (int i = 1; i <= 3; i++)
        {
            float t = Quilt.Lo + i * 0.2f;
            lines.Add(new List<Vector2> { new Vector2(t, Quilt.Lo), new Vector2(t, Quilt.Hi) });   // strips
            lines.Add(new List<Vector2> { new Vector2(Quilt.Lo, t), new Vector2(Quilt.Hi, t) });   // then squares
        }
        return lines;
    }
}

/// The patches laid out as the quilt top.
public abstract class PatchworkTracing : TracingStep
{
    protected override void DrawPiece() => DrawPatchwork(order.target);
}

/// Hand-piece the patches into rows, then the rows together: running stitch along each seam between rows.
public class SewRowsStep : PatchworkTracing
{
    protected override float StitchLength => 0.02f;
    protected override Color ThreadColor => new Color(0.45f, 0.40f, 0.40f);

    protected override List<Vector2> Guide() => Guides()[0];

    protected override List<List<Vector2>> Guides()
    {
        var lines = new List<List<Vector2>>();
        var p = order.target;
        int n = Mathf.Max(p.width, p.height);
        for (int y = 1; y < p.height; y++)
        {
            float at = Quilt.Lo + y * (Quilt.Hi - Quilt.Lo) / n;
            lines.Add(new List<Vector2> { new Vector2(Quilt.Lo, at), new Vector2(Quilt.Lo + p.width * (Quilt.Hi - Quilt.Lo) / n, at) });
        }
        if (lines.Count == 0) lines.Add(new List<Vector2> { new Vector2(Quilt.Lo, 0.5f), new Vector2(Quilt.Hi, 0.5f) });
        return lines;
    }
}

/// Quilting: lines of running stitch through all three layers, on the diagonal.
public class QuiltItStep : PatchworkTracing
{
    protected override float StitchLength => 0.022f;
    protected override Color ThreadColor => new Color(0.95f, 0.93f, 0.88f);

    protected override List<Vector2> Guide() => Guides()[0];

    protected override List<List<Vector2>> Guides()
    {
        var lines = new List<List<Vector2>>();
        foreach (float d in new[] { 0f, 0.2f, 0.4f })
        {
            lines.Add(new List<Vector2> { new Vector2(Quilt.Lo, Quilt.Lo + d), new Vector2(Quilt.Hi - d, Quilt.Hi) });
            if (d > 0) lines.Add(new List<Vector2> { new Vector2(Quilt.Lo + d, Quilt.Lo), new Vector2(Quilt.Hi, Quilt.Hi - d) });
        }
        return lines;
    }
}

/// The quilt sandwich: backing, wadding and the pieced top, pinned together with safety pins at each mark.
public class QuiltSandwichStep : PlacementStep
{
    protected override List<Vector2> Marks
    {
        get
        {
            var marks = new List<Vector2>();
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) marks.Add(new Vector2(0.23f + i * 0.27f, 0.23f + j * 0.27f));
            return marks;
        }
    }

    protected override void DrawPiece()
    {
        Square(new Vector2(0.5f, 0.5f), 0.9f, Color.Lerp(area.fabricColor, Color.black, 0.15f), -2);   // the backing, a little bigger
        Square(new Vector2(0.5f, 0.5f), 0.86f, new Color(0.98f, 0.98f, 0.95f), -1);                     // the wadding
        DrawPatchwork(order.target);
    }

    protected override GameObject DrawPlaced(Vector2 p)   // a safety pin: a little bar with a round head
    {
        var pin = new GameObject("SafetyPin");
        pin.transform.SetParent(transform, false);
        Line(new List<Vector2> { p - new Vector2(0.03f, 0.01f), p + new Vector2(0.03f, 0.01f) }, new Color(0.80f, 0.82f, 0.86f), 0.008f, 3).transform.SetParent(pin.transform, true);
        Dot(p + new Vector2(0.035f, 0.012f), 0.016f, new Color(0.85f, 0.75f, 0.35f), 4).transform.SetParent(pin.transform, true);
        return pin;
    }
}
