using System.Collections.Generic;
using UnityEngine;

/// A step where the player drags along a dashed line: hemming, cutting along a pattern line,
/// sewing a seam, closing an opening. Subclasses give the line and draw the piece; this class
/// records the strokes (saved in the order) and scores how much of the line was followed and how closely.
public abstract class TracingStep : PieceStep
{
    /// The dashed line to follow, in 0..1 of the area.
    protected abstract List<Vector2> Guide();
    protected virtual bool ClosedGuide => false;
    protected virtual Color ThreadColor => new Color(0.30f, 0.25f, 0.30f);
    protected virtual Color GuideColor => new Color(1f, 1f, 1f, 0.85f);

    /// Draws what's being worked on, under the line. Default: a square sheet of the fabric.
    protected virtual void DrawPiece() => Square(Vector2.one * 0.5f, 0.9f, area.fabricColor, 0);

    const float Tolerance = 0.035f;    // how far from the line still counts, as a share of the area
    const int Samples = 160;           // checkpoints along the line

    readonly List<Vector2> points = new List<Vector2>();
    readonly List<int> strokeStarts = new List<int>();
    readonly List<LineRenderer> strokeLines = new List<LineRenderer>();
    List<Vector2> samples;
    bool[] covered;

    /// How much of the line has been followed (0..1).
    public float Coverage { get; private set; }

    protected override void Build()
    {
        DrawPiece();
        samples = PieceShape.Resample(Guide(), ClosedGuide, Samples);
        covered = new bool[samples.Count];
        Dashes(samples, GuideColor, 0.008f, 1);
        points.AddRange(State.points);
        strokeStarts.AddRange(State.strokeStarts);
        Redraw();
    }

    public override float SaveWork()
    {
        State.points = new List<Vector2>(points);
        State.strokeStarts = new List<int>(strokeStarts);
        State.progress = Coverage;
        return Score();
    }

    /// Followed share of the line, lowered by stitches that wander off it.
    public float Score()
    {
        if (points.Count == 0) return 0f;
        int onLine = 0;
        foreach (var p in points) if (PieceShape.DistanceToLine(samples, p) <= Tolerance * 1.5f) onLine++;
        return Coverage * onLine / points.Count;
    }

    /// Removes the last stroke.
    public override void Undo()
    {
        if (strokeStarts.Count == 0) return;
        int from = strokeStarts[strokeStarts.Count - 1];
        strokeStarts.RemoveAt(strokeStarts.Count - 1);
        points.RemoveRange(from, points.Count - from);
        Redraw();
    }

    protected override void OnPress(Vector2 p)
    {
        strokeStarts.Add(points.Count);
        strokeLines.Add(Line(new Vector2[0], ThreadColor, 0.012f, 2));
        AddPoint(p);
    }

    protected override void OnDrag(Vector2 p)
    {
        if (points.Count > 0 && (points[points.Count - 1] - p).sqrMagnitude < 0.005f * 0.005f) return;
        AddPoint(p);
    }

    void AddPoint(Vector2 p)
    {
        points.Add(p);
        Cover(p);
        Extend(strokeLines[strokeLines.Count - 1], p);
    }

    void Cover(Vector2 p)
    {
        for (int i = 0; i < samples.Count; i++)
            if (!covered[i] && (samples[i] - p).sqrMagnitude <= Tolerance * Tolerance) covered[i] = true;
        int c = 0;
        foreach (bool b in covered) if (b) c++;
        Coverage = covered.Length > 0 ? (float)c / covered.Length : 0f;
    }

    void Redraw()
    {
        foreach (var l in strokeLines) if (l) Destroy(l.gameObject);
        strokeLines.Clear();
        System.Array.Clear(covered, 0, covered.Length);
        Coverage = 0f;
        for (int s = 0; s < strokeStarts.Count; s++)
        {
            int from = strokeStarts[s], to = s + 1 < strokeStarts.Count ? strokeStarts[s + 1] : points.Count;
            var line = Line(new Vector2[0], ThreadColor, 0.012f, 2);
            strokeLines.Add(line);
            for (int i = from; i < to; i++) { Extend(line, points[i]); Cover(points[i]); }
        }
    }
}
