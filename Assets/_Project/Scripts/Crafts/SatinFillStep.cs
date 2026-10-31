using System.Collections.Generic;
using UnityEngine;

/// Embroidery recipe step: fill the shape with satin stitch, dragging back and forth across it.
/// Cover it all; stitches that stray outside the outline lower the quality. Satin stitches lie side by
/// side at one even slant (the printed guide lines): stitches at another angle look messy and count as mistakes.
public class SatinFillStep : PieceStep, IMistakes
{
    const float SlantTolerance = 25f;   // degrees off the slant that still look even
    const float Reach = 0.035f;         // how much of the fabric one pass of thread covers, either side
    const int Grid = 14;                // checkpoints across the shape

    readonly List<Vector2> spots = new List<Vector2>();
    readonly List<LineRenderer> strokes = new List<LineRenderer>();
    bool[] covered;

    protected override float ShapePadding => 0.25f;
    List<Vector2> Points => State.points;
    float slant;

    /// How well each stroke keeps the slant (0..1, by length).
    float Even(int from, int to)
    {
        float good = 0, all = 0;
        for (int i = from + 1; i < to; i++)
        {
            var d = Points[i] - Points[i - 1];
            float len = d.magnitude;
            if (len < 0.004f) continue;
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            float off = Mathf.Abs(Mathf.DeltaAngle(angle, slant));
            off = Mathf.Min(off, 180f - off);                    // back and forth both count
            all += len;
            if (off <= SlantTolerance * (1f + Assist)) good += len;
        }
        return all > 0 ? good / all : 1f;
    }

    int Stroke(int s, out int to)
    {
        to = s + 1 < Starts.Count ? Starts[s + 1] : Points.Count;
        return Starts[s];
    }

    /// Strokes mostly at the wrong slant.
    public string MistakesLeft
    {
        get
        {
            int n = 0;
            for (int s = 0; s < Starts.Count; s++) { int from = Stroke(s, out int to); if (to - from > 2 && Even(from, to) < 0.6f) n++; }
            return n == 0 ? null : n == 1 ? "1 patch of satin stitch at the wrong slant" : $"{n} patches of satin stitch at the wrong slant";
        }
    }
    List<int> Starts => State.strokeStarts;

    protected override void Build()
    {
        DrawHoop(outlined: true, filled: false);
        PieceShape.Bounds(Shape.Outline, out var min, out var max);
        for (int x = 0; x < Grid; x++)
            for (int y = 0; y < Grid; y++)
            {
                var p = new Vector2(Mathf.Lerp(min.x, max.x, (x + 0.5f) / Grid), Mathf.Lerp(min.y, max.y, (y + 0.5f) / Grid));
                if (PieceShape.Inside(Shape.Outline, p)) spots.Add(p);
            }
        covered = new bool[spots.Count];
        // The slant: across the shape's narrower side, tilted a little, as satin stitch is usually worked.
        slant = (max.x - min.x) > (max.y - min.y) ? 70f : 20f;
        var dir = new Vector2(Mathf.Cos(slant * Mathf.Deg2Rad), Mathf.Sin(slant * Mathf.Deg2Rad));
        var across = new Vector2(-dir.y, dir.x);
        var mid = (min + max) * 0.5f;
        float reach = (max - min).magnitude * 0.5f;
        for (int g = -2; g <= 2; g++)   // a few printed guide lines
        {
            var c = mid + across * (g * reach * 0.35f);
            var guide = new List<Vector2>();
            for (int k = -10; k <= 10; k++) { var q = c + dir * (k * reach / 10f); if (PieceShape.Inside(Shape.Outline, q)) guide.Add(q); }
            if (guide.Count > 1) Dashes(guide, new Color(0.20f, 0.35f, 0.85f, 0.5f), 0.004f, 1);
        }
        Redraw();
    }

    protected override void OnPress(Vector2 p)
    {
        Starts.Add(Points.Count);
        strokes.Add(Line(new Vector2[0], Floss(0), Reach * 2f, 3));
        Add(p);
    }

    protected override void OnDrag(Vector2 p)
    {
        if (Points.Count > 0 && (Points[Points.Count - 1] - p).sqrMagnitude < 0.006f * 0.006f) return;
        Add(p);
    }

    void Add(Vector2 p)
    {
        if (Starts.Count > 0 && Points.Count > Starts[Starts.Count - 1]) Cover(Points[Points.Count - 1], p);
        else Cover(p, p);
        Points.Add(p);
        Extend(strokes[strokes.Count - 1], p);
    }

    void Cover(Vector2 a, Vector2 b)
    {
        for (int i = 0; i < spots.Count; i++)
            if (!covered[i] && PieceShape.DistanceToSegment(spots[i], a, b) <= Reach) covered[i] = true;
    }

    public override void Undo()
    {
        if (Starts.Count == 0) return;
        int from = Starts[Starts.Count - 1];
        Starts.RemoveAt(Starts.Count - 1);
        Points.RemoveRange(from, Points.Count - from);
        Redraw();
    }

    void Redraw()
    {
        foreach (var l in strokes) if (l) Destroy(l.gameObject);
        strokes.Clear();
        System.Array.Clear(covered, 0, covered.Length);
        for (int s = 0; s < Starts.Count; s++)
        {
            int from = Starts[s], to = s + 1 < Starts.Count ? Starts[s + 1] : Points.Count;
            var line = Line(new Vector2[0], Floss(0), Reach * 2f, 3);
            strokes.Add(line);
            for (int i = from; i < to; i++)
            {
                Extend(line, Points[i]);
                Cover(i > from ? Points[i - 1] : Points[i], Points[i]);
            }
        }
    }

    /// Share of the shape filled, lowered by thread that strays outside it.
    public override float SaveWork()
    {
        int c = 0;
        foreach (bool b in covered) if (b) c++;
        State.progress = covered.Length > 0 ? c / (float)covered.Length : 0f;
        if (Points.Count == 0) return 0f;
        int inside = 0;
        foreach (var p in Points)
            if (PieceShape.Inside(Shape.Outline, p) || PieceShape.DistanceToLine(Shape.Outline.Closed(), p) < Reach) inside++;
        float even = 0; int strokesCounted = 0;
        for (int s = 0; s < Starts.Count; s++) { int from = Stroke(s, out int to); if (to - from > 1) { even += Even(from, to); strokesCounted++; } }
        even = strokesCounted > 0 ? even / strokesCounted : 1f;
        return State.progress * inside / Points.Count * Mathf.Lerp(0.5f, 1f, even);
    }
}
