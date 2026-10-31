using System.Collections.Generic;
using UnityEngine;

/// Sewing step: pin the paper pattern to the fabric before cutting. The paper shows the cutting line and,
/// inside it, the printed seam line; the pins go in the seam allowance between them. Tap each mark.
public class PinPatternStep : PlacementStep
{
    static readonly Color Paper = new Color(0.97f, 0.95f, 0.86f, 0.92f);
    List<Vector2> marks;

    protected override List<Vector2> Marks
    {
        get
        {
            if (marks != null) return marks;
            marks = PieceShape.Resample(Shape.Outline, true, 9);
            marks.RemoveAt(marks.Count - 1);
            var c = Centre(Shape.Outline);
            for (int i = 0; i < marks.Count; i++) marks[i] = Vector2.MoveTowards(marks[i], c, 0.035f);   // in the allowance
            return marks;
        }
    }

    protected override void DrawPiece()
    {
        Square(Vector2.one * 0.5f, 0.94f, area.fabricColor, -2);   // the fabric, folded double
        Piece(Shape.Outline, Paper, 0);                             // the paper pattern piece
        Dashes(PieceShape.Resample(Shape.Outline, true, 120), new Color(0.15f, 0.15f, 0.2f, 0.8f), 0.005f, 1);   // cutting line
        Line(Shape.Seam, new Color(0.3f, 0.3f, 0.5f, 0.4f), 0.003f, 1);                                        // printed seam line
    }

    protected override GameObject DrawPlaced(Vector2 at)
    {
        var pin = new GameObject("Pin");
        pin.transform.SetParent(transform, false);
        Line(new[] { at, at + new Vector2(0.03f, 0.06f) }, new Color(0.75f, 0.75f, 0.8f), 0.007f, 3).transform.SetParent(pin.transform, true);
        Dot(at + new Vector2(0.03f, 0.06f), 0.032f, new Color(0.85f, 0.2f, 0.3f), 4, shine: true).transform.SetParent(pin.transform, true);
        return pin;
    }

    public static Vector2 Centre(List<Vector2> pts)
    {
        var sum = Vector2.zero;
        foreach (var p in pts) sum += p;
        return pts.Count > 0 ? sum / pts.Count : Vector2.one * 0.5f;
    }
}

/// Sewing step: clip the curves. With the piece still inside out, snip little notches into the seam
/// allowance on every curve, so it turns out smooth instead of puckered. Snip at each mark: between the
/// edge and the stitching. A snip off the mark (or into the stitching) is a mistake.
public class ClipCurvesStep : PlacementStep
{
    List<Vector2> marks;

    protected override List<Vector2> Marks
    {
        get
        {
            if (marks != null) return marks;
            marks = new List<Vector2>();
            var ring = PieceShape.Resample(Shape.Outline, true, 72);
            ring.RemoveAt(ring.Count - 1);
            int n = ring.Count;
            var turn = new float[n];
            for (int i = 0; i < n; i++)
            {
                var a = ring[(i - 2 + n) % n]; var b = ring[i]; var c = ring[(i + 2) % n];
                turn[i] = Vector2.Angle(b - a, c - b);
            }
            var centre = PinPatternStep.Centre(ring);
            var picked = new List<int>();
            for (int k = 0; k < 10; k++)   // the sharpest bends, not too close together
            {
                int best = -1;
                for (int i = 0; i < n; i++)
                    if (turn[i] > 12f && !picked.Exists(j => Vector2.Distance(ring[j], ring[i]) < 0.09f) && (best < 0 || turn[i] > turn[best])) best = i;
                if (best < 0) break;
                picked.Add(best);
            }
            foreach (int i in picked)
                if (Shape.Opening == null || PieceShape.DistanceToLine(Shape.Opening, ring[i]) > 0.05f)   // not across the opening
                    marks.Add(Vector2.MoveTowards(ring[i], centre, 0.018f));
            return marks;
        }
    }

    protected override Color MarkColor => new Color(1, 1, 1, 0.6f);

    protected override void DrawPiece()
    {
        Piece(Shape.Outline, Color.Lerp(area.fabricColor, Color.black, 0.12f), 0);   // inside out: the wrong side
        Line(Shape.Seam, Color.Lerp(area.fabricColor, Color.black, 0.45f), 0.006f, 1);   // the machine seam
    }

    /// A small V notch, pointing in towards the seam.
    protected override GameObject DrawPlaced(Vector2 p)
    {
        var centre = PinPatternStep.Centre(Shape.Outline);
        var inward = (centre - p).normalized;
        var side = new Vector2(-inward.y, inward.x);
        var tip = p + inward * 0.012f;
        var notch = Line(new[] { p - inward * 0.012f + side * 0.012f, tip, p - inward * 0.012f - side * 0.012f }, new Color(0.25f, 0.2f, 0.25f), 0.005f, 3);
        return notch.gameObject;
    }
}

/// Tote step: box the corners. Each bottom corner is flattened into a triangle and sewn straight across,
/// so the bag has a flat bottom and stands up. Sew along both dashed lines.
public class BoxCornersStep : TracingStep
{
    const float Depth = 0.09f;
    protected override float StitchLength => 0.022f;
    protected override bool HandNeedle => false;   // on the machine

    protected override void DrawPiece()
    {
        DrawBag(sidesSewn: true, hemmed: false);
        foreach (var g in Guides()) Piece(new List<Vector2> { g[0], g[1], Corner(g) }, Color.Lerp(area.fabricColor, Color.black, 0.1f), 1);
    }

    static Vector2 Corner(List<Vector2> g) => new Vector2(g[0].x, g[1].y);

    protected override List<Vector2> Guide() => Guides()[0];

    protected override List<List<Vector2>> Guides()
    {
        PieceShape.Bounds(Shape.Outline, out var min, out var max);
        return new List<List<Vector2>>
        {
            new List<Vector2> { new Vector2(min.x + 0.01f, min.y + Depth), new Vector2(min.x + Depth, min.y + 0.01f) },
            new List<Vector2> { new Vector2(max.x - 0.01f, min.y + Depth), new Vector2(max.x - Depth, min.y + 0.01f) },
        };
    }
}
