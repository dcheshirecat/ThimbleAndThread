using System.Collections.Generic;
using UnityEngine;

/// Tablecloth step: mitre the corners before hemming, by folding (owner, 2026-11-03: not another line to trace). Each
/// corner of the cloth is folded in so the two hems will meet in a neat 45-degree seam. Grab a corner and drag it inward:
/// the flap folds over as real cloth does, its crease running halfway between the corner and your finger. Let go when the
/// crease sits on the dashed line. A crease well off the line is a mistake (Undo unfolds it). Each fold is saved as the
/// point the corner was laid at, plus 100 + 10 x the corner's number on x (older traced saves are ignored).
public class MitreCornersStep : PieceStep, IMistakes
{
    const float Lo = 0.08f, Hi = 0.92f, Depth = 0.19f;   // the cloth's edges, and how far in the right crease sits
    static readonly Color Table = new Color(0.30f, 0.26f, 0.32f);

    int held = -1;                 // the corner being folded
    Vector2 hand;
    readonly List<GameObject> drawn = new List<GameObject>();

    static Vector2 CornerAt(int i) => new Vector2(i % 2 == 0 ? Lo : Hi, i < 2 ? Lo : Hi);
    static Vector2 U(int i) => new Vector2(i % 2 == 0 ? 1 : -1, 0);   // along the bottom/top edge, inward
    static Vector2 V(int i) => new Vector2(0, i < 2 ? 1 : -1);         // along the side edge, inward
    static Vector2 Right(int i) => CornerAt(i) + (U(i) + V(i)) * Depth;   // where the corner lands for a true mitre

    /// The folds made so far: corner -> where it was laid.
    Dictionary<int, Vector2> Folds()
    {
        var folds = new Dictionary<int, Vector2>();
        foreach (var p in State.points)
        {
            if (p.x < 100) continue;   // traced points from before folding
            int c = Mathf.FloorToInt((p.x - 100) / 10f);
            folds[c] = new Vector2(p.x - 100 - c * 10, p.y);
        }
        return folds;
    }

    /// Where the crease meets the two edges when corner i is laid at p (null if it can't fold that way).
    static bool Crease(int i, Vector2 p, out Vector2 a, out Vector2 b)
    {
        var c = CornerAt(i);
        var d = p - c;
        float du = Vector2.Dot(U(i), d), dv = Vector2.Dot(V(i), d);
        a = b = c;
        if (du <= 0.005f || dv <= 0.005f) return false;
        float t = Mathf.Min(d.sqrMagnitude / (2 * du), Hi - Lo), s = Mathf.Min(d.sqrMagnitude / (2 * dv), Hi - Lo);
        a = c + U(i) * t;
        b = c + V(i) * s;
        return true;
    }

    static float Accuracy(int i, Vector2 p) => Mathf.Clamp01(1f - Vector2.Distance(p, Right(i)) / 0.08f);

    protected override void Build() => Redraw();

    void Redraw()
    {
        foreach (var g in drawn) if (g != null) Destroy(g);
        drawn.Clear();
        drawn.Add(Square(Vector2.one * 0.5f, Hi - Lo, area.fabricColor, 0).gameObject);
        if (order.target != null)   // the stitched motif, in the middle
            foreach (var c in order.target.cells)
            {
                var at = PieceShape.SquareCenter(order.target, 0.3f, c.x, c.y, out float size);
                drawn.Add(Square(at, size * 0.92f, c.colorIndex < order.target.palette.Count ? order.target.palette[c.colorIndex] : Color.gray, 1).gameObject);
            }
        var folds = Folds();
        var under = Color.Lerp(area.fabricColor, Color.black, 0.18f);   // the back of the cloth, folded over
        for (int i = 0; i < 4; i++)
        {
            bool folding = i == held;
            if (!folds.TryGetValue(i, out var p) && !folding)
            {
                // Not folded yet: the crease to aim for, and a dot on the corner to grab
                Dashes(new List<Vector2> { CornerAt(i) + U(i) * Depth, CornerAt(i) + V(i) * Depth }, new Color(0.2f, 0.15f, 0.1f, 0.7f), 0.006f, 2);
                drawn.Add(Dot(CornerAt(i), 0.025f, new Color(1f, 0.9f, 0.4f), 3));
                continue;
            }
            if (folding) p = hand;
            if (!Crease(i, p, out var a, out var b)) continue;
            drawn.Add(Piece(new List<Vector2> { CornerAt(i), a, b }, Table, 2).gameObject);   // the corner's gone from here...
            drawn.Add(Piece(new List<Vector2> { a, p, b }, under, 3).gameObject);              // ...and lies over here
            drawn.Add(Line(new List<Vector2> { a, b }, Color.Lerp(under, Color.black, 0.35f), 0.006f, 4).gameObject);   // the crease
            if (folding)   // where it should be, while you're folding
                drawn.Add(Line(new List<Vector2> { CornerAt(i) + U(i) * Depth, CornerAt(i) + V(i) * Depth },
                               Accuracy(i, p) > 0.7f ? new Color(0.3f, 0.8f, 0.4f, 0.8f) : new Color(0.9f, 0.5f, 0.3f, 0.8f), 0.005f, 5).gameObject);
        }
    }

    void Dashes(List<Vector2> pts, Color color, float width, int sorting)
    {
        var line = Line(pts, color, width, sorting);
        MakeDashed(line, 0.025f);
        drawn.Add(line.gameObject);
    }

    public override string LiveHint
    {
        get
        {
            int done = Folds().Count;
            if (held >= 0) return Accuracy(held, hand) > 0.7f ? "That's on the crease: let go." : "Keep going: bring the crease onto the dashed line.";
            return done >= 4 ? "All four corners mitred. Tap Finish." : $"Corner {done + 1} of 4: grab a gold corner and fold it in until the crease sits on the dashed line.";
        }
    }

    protected override void OnPress(Vector2 p)
    {
        var folds = Folds();
        held = -1;
        float best = 0.1f;
        for (int i = 0; i < 4; i++)
            if (!folds.ContainsKey(i) && Vector2.Distance(p, CornerAt(i)) < best) { best = Vector2.Distance(p, CornerAt(i)); held = i; }
        hand = p;
        if (held >= 0) Redraw();
    }

    protected override void OnDrag(Vector2 p)
    {
        if (held < 0) return;
        hand = p;
        Redraw();
    }

    protected override void OnRelease(Vector2 p)
    {
        if (held < 0) return;
        int i = held;
        held = -1;
        if (Crease(i, p, out _, out _) && Vector2.Distance(p, CornerAt(i)) > 0.05f)
        {
            State.points.Add(new Vector2(100 + i * 10 + p.x, p.y));
            AudioManager.Play(Sfx.Stitch);
        }
        Redraw();
    }

    public override void Undo()
    {
        for (int k = State.points.Count - 1; k >= 0; k--)
            if (State.points[k].x >= 100) { State.points.RemoveAt(k); break; }
        Redraw();
    }

    public string MistakesLeft
    {
        get
        {
            int off = 0;
            foreach (var kv in Folds()) if (Accuracy(kv.Key, kv.Value) < 0.5f) off++;
            return off == 0 ? null : off == 1 ? "1 corner folded off its crease (the mitre won't meet neatly)" : $"{off} corners folded off their creases (the mitres won't meet neatly)";
        }
    }

    public override float SaveWork()
    {
        var folds = Folds();
        State.progress = folds.Count / 4f;
        if (folds.Count == 0) return 0f;
        float sum = 0;
        foreach (var kv in folds) sum += 0.3f + 0.7f * Accuracy(kv.Key, kv.Value);
        return State.progress * sum / folds.Count;
    }
}
