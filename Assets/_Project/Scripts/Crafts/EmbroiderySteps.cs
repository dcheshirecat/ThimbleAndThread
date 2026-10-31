using System.Collections.Generic;
using UnityEngine;

// The embroidered hoop, done the way an embroiderer does it (owner, 2026-10-31): trace the design onto the
// fabric, hoop it, pull it drum-tight, outline, satin fill at an even slant, stem-stitch the waves, French
// knots, and gather the back.

/// Embroidery step: transfer the design. The printed pattern is taped under the fabric against a bright
/// window; trace its lines with a water-soluble pen (it washes out later).
public class TransferDesignStep : TracingStep
{
    protected override float ShapePadding => 0.25f;
    protected override bool ClosedGuide => true;
    protected override float StitchLength => 0f;   // a pen line
    protected override Color ThreadColor => new Color(0.20f, 0.35f, 0.85f);
    protected override Color GuideColor => new Color(0.25f, 0.25f, 0.3f, 0.45f);   // the paper's lines, showing through
    protected override string StrokeName => "pen line";

    protected override void DrawPiece()
    {
        Square(Vector2.one * 0.5f, 0.98f, new Color(1f, 1f, 0.94f), -1);         // the window light
        Square(Vector2.one * 0.5f, 0.94f, Color.Lerp(area.fabricColor, Color.white, 0.25f), 0);
        foreach (var e in Shape.Eyes) Dot(e, 0.02f, new Color(0.25f, 0.25f, 0.3f, 0.4f), 1);   // knot marks
    }

    protected override List<Vector2> Guide() => Shape.Outline;
}

/// Embroidery step: pull the fabric drum-tight. Ripples show where it's slack; tug the fabric outwards
/// at each one (drag away from the middle), working round the hoop, then it's tight enough to stitch on.
public class TightenHoopStep : PlacementStep
{
    const int Tugs = 8;
    const float TugAt = 0.485f, MinPull = 0.025f;
    List<Vector2> marks;
    readonly List<GameObject> ripples = new List<GameObject>();
    Vector2 pressAt;
    bool pressing;

    protected override float ShapePadding => 0.25f;
    protected override Color MarkColor => new Color(0, 0, 0, 0.12f);

    protected override List<Vector2> Marks
    {
        get
        {
            if (marks != null) return marks;
            marks = new List<Vector2>();
            for (int i = 0; i < Tugs; i++)
            {
                float a = (i + 0.5f) * Mathf.PI * 2f / Tugs;
                marks.Add(new Vector2(0.5f, 0.5f) + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * TugAt);
            }
            return marks;
        }
    }

    protected override void DrawPiece()
    {
        Square(Vector2.one * 0.5f, 0.99f, Color.Lerp(area.fabricColor, Color.black, 0.06f), -1);   // the spare fabric round the hoop
        DrawHoop(outlined: false, filled: false);
        foreach (var m in Marks)
        {
            var go = new GameObject("Ripple");
            go.transform.SetParent(transform, false);
            var dir = (m - new Vector2(0.5f, 0.5f)).normalized;
            var side = new Vector2(-dir.y, dir.x);
            for (int r = 0; r < 3; r++)   // three slack folds running in from the edge
            {
                var start = m - dir * (0.05f + r * 0.035f);
                var wave = new List<Vector2>();
                for (int k = 0; k <= 8; k++) wave.Add(start + side * ((k - 4) * 0.012f) + dir * (Mathf.Sin(k * 1.4f) * 0.008f));
                Line(wave, new Color(0, 0, 0, 0.16f), 0.006f, 2).transform.SetParent(go.transform, true);
            }
            ripples.Add(go);
        }
    }

    protected override GameObject DrawPlaced(Vector2 p) => Dot(p, 0.018f, new Color(0.72f, 0.55f, 0.35f), 6);

    protected override void Changed()
    {
        for (int i = 0; i < Marks.Count && i < ripples.Count; i++)
            ripples[i].SetActive(!Placed.Exists(p => Vector2.Distance(p, Marks[i]) < 0.08f));
    }

    protected override void OnPress(Vector2 p) { pressAt = p; pressing = true; }

    protected override void OnRelease(Vector2 p)
    {
        if (!pressing) return;
        pressing = false;
        var centre = new Vector2(0.5f, 0.5f);
        if (Vector2.Distance(p, centre) - Vector2.Distance(pressAt, centre) < MinPull) return;   // not pulled outwards
        Place(pressAt);
    }
}

/// Embroidery step: stem stitch along the waves. Each small stitch comes up halfway along the last one,
/// so the line looks like a twisted rope. Follow the line steadily.
public class StemStitchStep : TracingStep
{
    protected override float ShapePadding => 0.25f;
    protected override float StitchLength => 0.018f;
    protected override bool Twisted => true;
    protected override Color ThreadColor => Floss(0);
    protected override Color GuideColor => new Color(0.20f, 0.35f, 0.85f, 1f);

    protected override void DrawPiece()
    {
        DrawHoop(outlined: true, filled: false);
        foreach (var g in Guides()) Line(g, new Color(0.55f, 0.6f, 0.75f, 0.6f), 0.004f, 1);   // the pen line
    }

    protected override List<Vector2> Guide() => Guides()[0];

    /// Two gentle waves under the design (or across the bottom of the hoop when the design fills it).
    protected override List<List<Vector2>> Guides()
    {
        PieceShape.Bounds(Shape.Outline, out var min, out _);
        float y = Mathf.Clamp(min.y - 0.07f, 0.16f, 0.5f);
        var list = new List<List<Vector2>>();
        for (int w = 0; w < 2; w++)
        {
            float wy = y - w * 0.06f;
            float half = Mathf.Sqrt(Mathf.Max(0.01f, HoopRadius * HoopRadius - (wy - 0.5f) * (wy - 0.5f))) * 0.75f;
            var wave = new List<Vector2>();
            for (int k = 0; k <= 24; k++)
            {
                float t = k / 24f;
                wave.Add(new Vector2(Mathf.Lerp(0.5f - half, 0.5f + half, t), wy + Mathf.Sin(t * Mathf.PI * 3f + w) * 0.018f));
            }
            list.Add(wave);
        }
        return list;
    }
}

/// Embroidery step: finish the back. Turn the hoop over and sew a running stitch round the spare fabric,
/// just outside the ring; pulling it gathers the fabric neatly behind, ready to hang.
public class FinishBackStep : TracingStep
{
    const float Gather = 0.475f;
    protected override bool ClosedGuide => true;
    protected override float StitchLength => 0.03f;

    protected override void DrawPiece()
    {
        Piece(Circle(new Vector2(0.5f, 0.5f), 0.495f, 48), Color.Lerp(area.fabricColor, Color.black, 0.08f), 0);   // the spare fabric, from behind
        Piece(Circle(new Vector2(0.5f, 0.5f), HoopRadius, 48), Color.Lerp(area.fabricColor, Color.black, 0.15f), 1);
        var back = new List<Vector2>();
        foreach (var p in Shape.Outline) back.Add(new Vector2(1f - p.x, p.y));   // the stitching, mirrored on the back
        Line(back.Closed(), Color.Lerp(Floss(0), Color.white, 0.3f), 0.008f, 2);
        Line(Circle(new Vector2(0.5f, 0.5f), HoopRadius, 48).Closed(), new Color(0.72f, 0.55f, 0.35f), 0.035f, 5);
    }

    protected override float ShapePadding => 0.25f;
    protected override List<Vector2> Guide() => Circle(new Vector2(0.5f, 0.5f), Gather, 64);
}
