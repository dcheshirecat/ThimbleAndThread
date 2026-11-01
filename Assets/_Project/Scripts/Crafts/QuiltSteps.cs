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

/// Lay out the patches on the floor (owner: deeper mini-games). The cut patches wait in piles by fabric along the bottom:
/// tap a pile to pick it up, then tap or drag over the squares where that fabric goes, following the chart. A patch in
/// the wrong fabric spoils the pattern (lay the right one over it; Undo takes the last one back).
/// Saved points: (square index, fabric) per patch laid, fabric -1 = the cream background.
public class LayoutPatchesStep : PieceStep, IMistakes
{
    const float TrayY = 0.035f;
    int held = -1;
    readonly Dictionary<int, GameObject> laid = new Dictionary<int, GameObject>();
    readonly List<SpriteRenderer> piles = new List<SpriteRenderer>();
    List<int> fabrics;   // -1 (cream) then each palette colour the chart uses

    Pattern P => order.target;
    int Width => Mathf.Max(1, P.width);
    int Count => Width * Mathf.Max(1, P.height);

    int Wanted(int i)
    {
        var cell = P.cells.Find(c => c.x == i % Width && c.y == i / Width);
        return cell != null && cell.colorIndex < P.palette.Count ? cell.colorIndex : -1;
    }

    Color FabricColor(int f) => f < 0 || f >= P.palette.Count ? Quilt.Cream : new Color(P.palette[f].r, P.palette[f].g, P.palette[f].b, 1f);

    protected override void Build()
    {
        Square(new Vector2(0.5f, 0.5f), 0.86f, new Color(0.62f, 0.50f, 0.40f), -1);   // the floorboards
        for (int i = 0; i < Count; i++)   // the chart, faintly chalked where each patch goes
        {
            var at = Quilt.PatchCenter(P, i % Width, i / Width, out float size);
            var c = FabricColor(Wanted(i)); c.a = 0.28f;
            Rect(at, Vector2.one * size * 0.9f, c, 0);
        }
        fabrics = new List<int> { -1 };
        foreach (var c in P.cells) if (!fabrics.Contains(c.colorIndex) && c.colorIndex < P.palette.Count) fabrics.Add(c.colorIndex);
        for (int k = 0; k < fabrics.Count; k++)
        {
            piles.Add(Square(PileAt(k), 0.07f, FabricColor(fabrics[k]), 3));
            Square(PileAt(k) + new Vector2(0.006f, -0.006f), 0.07f, new Color(0, 0, 0, 0.3f), 2);
        }
        foreach (var p in State.points) Lay((int)p.x, (int)p.y);
        ShowHeld();
    }

    Vector2 PileAt(int k) => new Vector2(0.5f + (k - (fabrics.Count - 1) / 2f) * 0.12f, TrayY);

    void ShowHeld()
    {
        for (int k = 0; k < piles.Count; k++) piles[k].transform.localScale = Vector3.one * Side * (fabrics[k] == held ? 0.09f : 0.07f);
    }

    void Lay(int i, int fabric)
    {
        if (laid.TryGetValue(i, out var old)) Destroy(old);
        var at = Quilt.PatchCenter(P, i % Width, i / Width, out float size);
        laid[i] = Rect(at, Vector2.one * size * 0.95f, FabricColor(fabric), 1).gameObject;
    }

    /// The patch laid on each square now (the last one laid there).
    Dictionary<int, int> Current()
    {
        var now = new Dictionary<int, int>();
        foreach (var p in State.points) now[(int)p.x] = (int)p.y;
        return now;
    }

    public override string LiveHint
    {
        get
        {
            int left = Count - Current().Count;
            if (held == -2 || left > 0 && fabrics != null && held == -1 && State.points.Count == 0) return "Tap a pile of patches to pick it up.";
            return left > 0 ? $"{left} squares still to fill." : "All laid out. Tap Finish.";
        }
    }

    protected override void OnPress(Vector2 p)
    {
        for (int k = 0; k < fabrics.Count; k++)
            if (Vector2.Distance(p, PileAt(k)) < 0.05f) { held = fabrics[k]; ShowHeld(); AudioManager.Play(Sfx.Click); return; }
        LayAt(p);
    }

    protected override void OnDrag(Vector2 p) => LayAt(p);

    void LayAt(Vector2 p)
    {
        int n = Mathf.Max(Width, P.height);
        float size = (Quilt.Hi - Quilt.Lo) / n;
        int x = Mathf.FloorToInt((p.x - Quilt.Lo) / size), y = Mathf.FloorToInt((p.y - Quilt.Lo) / size);
        if (x < 0 || y < 0 || x >= Width || y >= P.height) return;
        int i = y * Width + x;
        var now = Current();
        if (now.TryGetValue(i, out int there) && there == held) return;
        State.points.Add(new Vector2(i, held));
        Lay(i, held);
    }

    public override void Undo()
    {
        if (State.points.Count == 0) return;
        int i = (int)State.points[State.points.Count - 1].x;
        State.points.RemoveAt(State.points.Count - 1);
        var now = Current();
        if (now.TryGetValue(i, out int f)) Lay(i, f);
        else if (laid.TryGetValue(i, out var old)) { Destroy(old); laid.Remove(i); }
    }

    public string MistakesLeft
    {
        get
        {
            int wrong = 0;
            foreach (var kv in Current()) if (kv.Value != Wanted(kv.Key)) wrong++;
            return wrong == 0 ? null : wrong == 1 ? "1 patch in the wrong fabric" : $"{wrong} patches in the wrong fabric";
        }
    }

    public override float SaveWork()
    {
        var now = Current();
        int right = 0;
        foreach (var kv in now) if (kv.Value == Wanted(kv.Key)) right++;
        State.progress = now.Count / (float)Count;
        return right / (float)Count;
    }
}

/// Press the quilt top's seams before layering it: iron along every row seam (PressSeamsStep: the heat matters).
public class QuiltPressStep : PressSeamsStep
{
    protected override void DrawPiece() => DrawPatchwork(order.target);

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
