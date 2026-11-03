using System.Collections.Generic;
using UnityEngine;

/// Darning a hole (owner, 2026-11-02 evening: repairs). As it's really done: first lay long threads up and down across
/// the hole, from sound knitting above to sound knitting below; then weave across them row by row, over one and under the
/// next, each row going back the other way. A long thread off its mark leaves the darn lumpy; a row woven the same way as
/// the one before slips over instead of locking in. Points: (100 + column, accuracy 0..1) for the long threads, then
/// (200 + row, 1 = the right way, 0 = the same way as the last) for the rows across.
public class DarnStep : PieceStep, IMistakes
{
    const int Columns = 7, Rows = 7;
    const float HoleR = 0.2f, Gap = 0.05f;
    static readonly Vector2 Centre = new Vector2(0.5f, 0.5f);

    readonly List<GameObject> drawn = new List<GameObject>();
    Vector2 pressAt;
    float drift;   // how far the current drag wandered from its column
    int dragSamples;

    float ColX(int i) => Centre.x + (i - (Columns - 1) / 2f) * Gap;
    float RowY(int j) => Centre.y + (j - (Rows - 1) / 2f) * Gap;
    int WarpsDone => State.points.FindAll(p => p.x >= 100 && p.x < 200).Count;
    int RowsDone => State.points.FindAll(p => p.x >= 200).Count;

    Color Knit => order.fabricId != null && area.content.Supply(order.fabricId) is SupplyDef y ? y.color : new Color(0.35f, 0.45f, 0.65f);
    Color Mend => Color.Lerp(Knit, Color.white, 0.18f);

    protected override void Build()
    {
        // The jumper's knitting, with a ragged hole in it
        Square(Centre, 0.86f, Knit * 0.85f, -2);
        for (int r = 0; r < 14; r++)   // a hint of rows of knit stitches
            for (int c = 0; c < 14; c++)
                Dot(new Vector2(0.1f + c * 0.06f, 0.1f + r * 0.06f), 0.012f, Knit, -1);
        Dot(Centre, HoleR, new Color(0.12f, 0.10f, 0.12f), 0);
        for (int k = 0; k < 12; k++)   // frayed edges
        {
            float a = k * Mathf.PI * 2 / 12;
            Dot(Centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * HoleR, 0.025f, Knit, 0);
        }
        Redraw();
    }

    void Redraw()
    {
        foreach (var g in drawn) if (g != null) Destroy(g);
        drawn.Clear();
        float top = Centre.y + HoleR + 0.06f, bottom = Centre.y - HoleR - 0.06f;
        foreach (var p in State.points)
        {
            if (p.x >= 200) continue;
            int i = (int)p.x - 100;
            float wobble = (1 - p.y) * 0.02f * (i % 2 == 0 ? 1 : -1);
            drawn.Add(Line(new List<Vector2> { new Vector2(ColX(i) + wobble, top), new Vector2(ColX(i) - wobble, bottom) }, Mend, 0.016f, 2).gameObject);
        }
        for (int j = 0; j < RowsDone; j++)
        {
            var p = State.points.Find(x => (int)x.x == 200 + j);
            bool locked = p.y > 0.5f;
            float y = RowY(j);
            drawn.Add(Line(new List<Vector2> { new Vector2(ColX(0) - 0.04f, y), new Vector2(ColX(Columns - 1) + 0.04f, y) }, Color.Lerp(Mend, Color.white, 0.1f), 0.016f, 3).gameObject);
            if (!locked) continue;   // slipped over the top: no long threads show through
            for (int i = 0; i < Columns; i++)
                if ((i + j) % 2 == 0)   // over one, under the next: where the long thread is on top
                    drawn.Add(Line(new List<Vector2> { new Vector2(ColX(i), y - 0.02f), new Vector2(ColX(i), y + 0.02f) }, Color.Lerp(Mend, Color.black, 0.25f), 0.018f, 4).gameObject);
        }
        if (WarpsDone < Columns)   // the next long thread's mark
        {
            drawn.Add(Line(new List<Vector2> { new Vector2(ColX(WarpsDone), top), new Vector2(ColX(WarpsDone), bottom) }, new Color(1f, 0.9f, 0.4f, 0.55f), 0.006f, 1).gameObject);
            Tool(CraftTool.Kind.Needle, 0.12f).MoveTo(World(new Vector2(ColX(WarpsDone), top + 0.03f)));
        }
        else if (RowsDone < Rows)
        {
            bool fromLeft = RowsDone % 2 == 0;
            drawn.Add(Dot(new Vector2(fromLeft ? ColX(0) - 0.07f : ColX(Columns - 1) + 0.07f, RowY(RowsDone)), 0.018f, new Color(1f, 0.9f, 0.4f), 5));
            Tool(CraftTool.Kind.Needle, 0.12f).MoveTo(World(new Vector2(fromLeft ? ColX(0) - 0.07f : ColX(Columns - 1) + 0.07f, RowY(RowsDone))));
        }
        else Tool(CraftTool.Kind.Needle).Hide();
    }

    public override string LiveHint =>
        WarpsDone < Columns ? $"Long thread {WarpsDone + 1} of {Columns}: drag straight down the glowing line, from the knitting above the hole to the knitting below."
        : RowsDone < Rows ? $"Row {RowsDone + 1} of {Rows}: weave across, over one and under the next, starting from the {(RowsDone % 2 == 0 ? "left" : "right")} (the glowing dot)."
        : "The hole's darned. Tap Finish.";

    protected override void OnPress(Vector2 p) { pressAt = p; drift = 0; dragSamples = 0; }

    protected override void OnDrag(Vector2 p)
    {
        if (WarpsDone >= Columns) return;
        drift += Mathf.Abs(p.x - ColX(WarpsDone));
        dragSamples++;
    }

    protected override void OnRelease(Vector2 p)
    {
        var d = p - pressAt;
        if (WarpsDone < Columns)
        {
            float span = 2 * HoleR + 0.08f;
            if (d.y > -span * 0.75f || Mathf.Abs(d.y) < Mathf.Abs(d.x)) return;   // not down across the whole hole
            float off = dragSamples > 0 ? drift / dragSamples : Mathf.Abs(pressAt.x - ColX(WarpsDone));
            State.points.Add(new Vector2(100 + WarpsDone, Mathf.Clamp01(1f - off / 0.03f)));
            AudioManager.Play(Sfx.Stitch);
            Redraw();
            return;
        }
        if (RowsDone >= Rows || Mathf.Abs(d.x) < (ColX(Columns - 1) - ColX(0)) * 0.8f || Mathf.Abs(d.x) < Mathf.Abs(d.y)) return;
        bool shouldGoRight = RowsDone % 2 == 0;
        State.points.Add(new Vector2(200 + RowsDone, (d.x > 0) == shouldGoRight ? 1 : 0));
        AudioManager.Play(Sfx.Stitch);
        Redraw();
    }

    public override void Undo()
    {
        if (State.points.Count == 0) return;
        State.points.RemoveAt(State.points.Count - 1);
        Redraw();
    }

    public string MistakesLeft
    {
        get
        {
            int wobbly = State.points.FindAll(p => p.x >= 100 && p.x < 200 && p.y < 0.6f).Count;
            int slipped = State.points.FindAll(p => p.x >= 200 && p.y < 0.5f).Count;
            var parts = new List<string>();
            if (wobbly > 0) parts.Add(wobbly == 1 ? "1 long thread off its line (the darn will be lumpy)" : $"{wobbly} long threads off their lines (the darn will be lumpy)");
            if (slipped > 0) parts.Add(slipped == 1 ? "1 row woven the same way as the one before (it slips over instead of locking in)" : $"{slipped} rows woven the same way as the one before (they slip over instead of locking in)");
            return parts.Count == 0 ? null : string.Join(" and ", parts);
        }
    }

    public override float SaveWork()
    {
        State.progress = (WarpsDone + RowsDone) / (float)(Columns + Rows);
        if (State.points.Count == 0) return 0f;
        float warps = 0, rows = 0;
        foreach (var p in State.points)
        {
            if (p.x >= 200) rows += p.y > 0.5f ? 1f : 0.3f;
            else warps += 0.4f + 0.6f * p.y;
        }
        return State.progress * (warps + rows) / State.points.Count;
    }
}
