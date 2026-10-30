using System.Collections.Generic;
using UnityEngine;

/// Knitting recipe step: knit the piece, one swipe per stitch. Swipe down for a knit stitch, up for
/// a purl; the faint marks show which is due (purls make the pattern's texture). A steady rhythm
/// makes even stitches; a wrong swipe is a mistake and makes no stitch.
/// Each finished stitch is saved as a point whose x is its evenness (0..1).
public class KnitRowsStep : PieceStep
{
    const float MinSwipe = 0.04f;       // shorter drags aren't swipes

    KnitLayout knit;
    readonly List<GameObject> stitches = new List<GameObject>();
    readonly List<GameObject> guide = new List<GameObject>();
    SpriteRenderer current;
    Vector2 pressAt;
    float lastStitchTime = -1f, rhythm = -1f;   // seconds between stitches, a running average

    Color Yarn => Color.Lerp(area.fabricColor, Color.black, 0.25f);
    int Done => State.points.Count;

    protected override void Build()
    {
        knit = KnitLayout.For(order.target);
        Rect(knit.Center, knit.Size, new Color(1, 1, 1, 0.08f), 0);
        current = Square(Vector2.zero, knit.cell, new Color(1, 1, 1, 0.35f), 1);
        for (int i = 0; i < Done; i++) DrawStitch(i);
        ShowNext();
    }

    void DrawStitch(int i)
    {
        var s = knit.At(i);
        float evenness = State.points[i].x;
        // Uneven stitches sit a little off; the offset is fixed per stitch so it redraws the same.
        var wobble = new Vector2(Mathf.Sin(i * 12.9898f), Mathf.Cos(i * 78.233f)) * (1 - evenness) * knit.cell * 0.25f;
        var stitch = new GameObject($"Stitch_{i}");
        stitch.transform.SetParent(transform, false);
        Rect(knit.CellCenter(s), Vector2.one * knit.cell * 0.96f, area.fabricColor, 1).transform.SetParent(stitch.transform, true);
        Stitch(knit.CellCenter(s) + wobble, knit.cell, knit.Purl(s), Yarn, 2).transform.SetParent(stitch.transform, true);
        stitches.Add(stitch);
    }

    /// Highlights the next stitch and shows faint marks for the rest of its row.
    void ShowNext()
    {
        foreach (var g in guide) Destroy(g);
        guide.Clear();
        current.enabled = Done < knit.Stitches;
        if (!current.enabled) return;
        var next = knit.At(Done);
        current.transform.position = World(knit.CellCenter(next));
        for (int x = next.x; x < knit.width; x++)
        {
            var s = new Vector2Int(x, next.y);
            guide.Add(Stitch(knit.CellCenter(s), knit.cell, knit.Purl(s), new Color(1, 1, 1, 0.5f), 2));
        }
    }

    protected override void OnPress(Vector2 p) => pressAt = p;

    protected override void OnRelease(Vector2 p)
    {
        var d = p - pressAt;
        if (Done >= knit.Stitches || d.magnitude < MinSwipe || Mathf.Abs(d.y) < Mathf.Abs(d.x)) return;
        bool purl = d.y > 0;
        if (purl != knit.Purl(knit.At(Done))) { State.mistakes++; return; }

        float now = Time.realtimeSinceStartup, evenness = 1f;
        if (lastStitchTime >= 0)
        {
            float gap = now - lastStitchTime;
            if (rhythm > 0.08f) evenness = 1f - 0.6f * Mathf.Clamp01(Mathf.Abs(gap - rhythm) / rhythm);
            rhythm = rhythm < 0 ? gap : Mathf.Lerp(rhythm, gap, 0.3f);
        }
        lastStitchTime = now;
        State.points.Add(new Vector2(evenness, 0));
        DrawStitch(Done - 1);
        ShowNext();
    }

    public override void Undo()
    {
        if (Done == 0) return;
        State.points.RemoveAt(Done - 1);
        Destroy(stitches[stitches.Count - 1]);
        stitches.RemoveAt(stitches.Count - 1);
        ShowNext();
    }

    /// Average evenness of the stitches, lowered by mistakes.
    public override float SaveWork()
    {
        State.progress = Done / (float)knit.Stitches;
        if (Done == 0) return 0f;
        float sum = 0f;
        foreach (var p in State.points) sum += p.x;
        float mistakes = Mathf.Clamp01(State.mistakes / (float)knit.Stitches);
        return State.progress * (sum / Done) * (1f - 0.5f * mistakes);
    }
}
