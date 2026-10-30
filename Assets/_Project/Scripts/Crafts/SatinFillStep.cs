using System.Collections.Generic;
using UnityEngine;

/// Embroidery recipe step: fill the shape with satin stitch, dragging back and forth across it.
/// Cover it all; stitches that stray outside the outline lower the quality.
public class SatinFillStep : PieceStep
{
    const float Reach = 0.035f;         // how much of the fabric one pass of thread covers, either side
    const int Grid = 14;                // checkpoints across the shape

    readonly List<Vector2> spots = new List<Vector2>();
    readonly List<LineRenderer> strokes = new List<LineRenderer>();
    bool[] covered;

    protected override float ShapePadding => 0.25f;
    List<Vector2> Points => State.points;
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
        return State.progress * inside / Points.Count;
    }
}
