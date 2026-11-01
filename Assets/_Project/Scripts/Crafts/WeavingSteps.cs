using System.Collections.Generic;
using UnityEngine;

/// Loom weaving (owner, 2026-10-31: new crafts). A wall hanging on a frame loom: wind the warp, weave the rows, then tie
/// it onto a dowel to hang. Each row of the pattern is woven in the colour that fills most of that row.
public static class Loom
{
    public const float Left = 0.15f, Right = 0.85f, Bottom = 0.16f, Top = 0.84f;

    public static int Warps(Pattern p) => Mathf.Clamp(p != null ? p.width : 8, 6, 16);
    public static int Rows(Pattern p) => Mathf.Clamp(p != null ? p.height : 8, 4, 16);
    public static float WarpX(Pattern p, int i) => Mathf.Lerp(Left, Right, (i + 0.5f) / Warps(p));
    public static float RowY(Pattern p, int r) => Mathf.Lerp(Bottom, Top, (r + 0.5f) / Rows(p));

    /// The weft colour for a row: the colour most of that row of the chart is in (cream if it's empty).
    public static Color RowColor(Pattern p, int r)
    {
        if (p == null) return Quilt.Cream;
        int y = r;   // chart rows count up from the bottom, as the weaving grows
        var counts = new Dictionary<int, int>();
        foreach (var c in p.cells) if (c.y == y) { counts.TryGetValue(c.colorIndex, out int n); counts[c.colorIndex] = n + 1; }
        int best = -1, most = 0;
        foreach (var kv in counts) if (kv.Value > most) { best = kv.Key; most = kv.Value; }
        if (best < 0 || best >= p.palette.Count) return Quilt.Cream;
        var col = p.palette[best];
        col.a = 1f;
        return col;
    }

    public static readonly Color Wood = new Color(0.72f, 0.55f, 0.36f), Warp = new Color(0.95f, 0.93f, 0.88f);
}

/// Warp the loom: one long thread wound up and down between the pegs at the top and bottom of the frame.
public class WarpLoomStep : TracingStep
{
    protected override float StitchLength => 0f;   // a strand of thread
    protected override Color ThreadColor => Loom.Warp;
    protected override Color GuideColor => new Color(0.4f, 0.3f, 0.2f, 0.85f);
    protected override string StrokeName => "pass of thread";

    protected override void DrawPiece()
    {
        Square(new Vector2(0.5f, 0.5f), 0.86f, new Color(0.3f, 0.26f, 0.32f), -2);   // behind the loom
        foreach (var (a, b) in new[] { (new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.1f)), (new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.9f)),
                                       (new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.9f)), (new Vector2(0.9f, 0.1f), new Vector2(0.9f, 0.9f)) })
            Line(new List<Vector2> { a, b }, Loom.Wood, 0.05f, 0);
        var p = order.target;
        for (int i = 0; i < Loom.Warps(p); i++)   // the pegs
        {
            Dot(new Vector2(Loom.WarpX(p, i), 0.1f), 0.022f, new Color(0.55f, 0.40f, 0.25f), 1);
            Dot(new Vector2(Loom.WarpX(p, i), 0.9f), 0.022f, new Color(0.55f, 0.40f, 0.25f), 1);
        }
    }

    protected override List<Vector2> Guide()
    {
        var p = order.target;
        var zig = new List<Vector2>();
        for (int i = 0; i < Loom.Warps(p); i++)
        {
            float x = Loom.WarpX(p, i);
            bool up = i % 2 == 0;
            zig.Add(new Vector2(x, up ? 0.1f : 0.9f));
            zig.Add(new Vector2(x, up ? 0.9f : 0.1f));
        }
        return zig;
    }
}

/// Weave the rows. For each row: tap the shed stick (below the loom) to lift the other half of the warps from the last
/// row, swipe the shuttle across, then swipe down to beat the row into place with the comb. The same shed twice makes the
/// weft float loose over the warps; a row left unbeaten leaves gaps. The weft is laid in a gentle hill ("bubbled") before
/// it's beaten: swiped straight across it's pulled tight and the edges draw in. Each row is saved as a point: x = shed
/// (0/1), y = 1 once it's beaten, + 2 if it was pulled tight (older saves: never).
public class WeaveRowsStep : PieceStep, IMistakes
{
    const float MinSwipe = 0.25f;
    int shed;
    Vector2 pressAt;
    readonly List<GameObject> drawn = new List<GameObject>();
    TextMesh stickLabel;

    int Rows => Loom.Rows(order.target);
    int Done => State.points.Count;
    const float Bubble = 0.04f;   // how high the weft's hill must rise above a straight line
    float hill;                   // the highest the current swipe has risen

    bool Beaten(int r) => (int)State.points[r].y % 2 == 1;
    bool Tight(int r) => (int)State.points[r].y >= 2;

    protected override void Build()
    {
        var p = order.target;
        Square(new Vector2(0.5f, 0.5f), 0.86f, new Color(0.3f, 0.26f, 0.32f), -2);
        foreach (var (a, b) in new[] { (new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.1f)), (new Vector2(0.1f, 0.9f), new Vector2(0.9f, 0.9f)),
                                       (new Vector2(0.1f, 0.1f), new Vector2(0.1f, 0.9f)), (new Vector2(0.9f, 0.1f), new Vector2(0.9f, 0.9f)) })
            Line(new List<Vector2> { a, b }, Loom.Wood, 0.05f, 0);
        for (int i = 0; i < Loom.Warps(p); i++)
            Line(new List<Vector2> { new Vector2(Loom.WarpX(p, i), 0.1f), new Vector2(Loom.WarpX(p, i), 0.9f) }, Loom.Warp, 0.008f, 1);
        Line(new List<Vector2> { new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.04f) }, Loom.Wood, 0.03f, 2);   // the shed stick
        stickLabel = Label(new Vector2(0.5f, -0.01f), "", 0.03f, Color.white, 3);
        shed = Done > 0 ? 1 - (int)State.points[Done - 1].x : 0;
        Redraw();
    }

    void Redraw()
    {
        foreach (var g in drawn) Destroy(g);
        drawn.Clear();
        var p = order.target;
        int warps = Loom.Warps(p);
        float gapY = (Loom.Top - Loom.Bottom) / Rows;
        for (int r = 0; r < Done; r++)
        {
            bool beaten = Beaten(r);
            int s = (int)State.points[r].x;
            bool floats = r > 0 && (int)State.points[r - 1].x == s;
            float y = Loom.RowY(p, r) + (beaten ? 0 : gapY * 0.35f);
            var col = Loom.RowColor(p, r);
            float pull = Tight(r) ? 0.035f : 0f;   // pulled tight, the edges draw in
            drawn.Add(Line(new List<Vector2> { new Vector2(Loom.Left - 0.02f + pull, y), new Vector2(Loom.Right + 0.02f - pull, y) }, col, gapY * (beaten ? 0.9f : 0.6f), 2).gameObject);
            if (floats) continue;   // a float: the weft lies loose on top, no warps show through
            for (int i = 0; i < warps; i++)   // the warps that pass over the weft in this row
                if ((i + s) % 2 == 0)
                    drawn.Add(Line(new List<Vector2> { new Vector2(Loom.WarpX(p, i), y - gapY * 0.4f), new Vector2(Loom.WarpX(p, i), y + gapY * 0.4f) },
                                   Loom.Warp, 0.012f, 3).gameObject);
        }
        stickLabel.text = $"shed {(shed == 0 ? "A" : "B")}: tap to turn";
        for (int i = 0; i < warps; i++)   // the warps lifted by the shed stick, ready for the next row
            if ((i + shed) % 2 == 0 && Done < Rows)
                drawn.Add(Dot(new Vector2(Loom.WarpX(p, i), 0.07f), 0.018f, new Color(1f, 0.9f, 0.5f), 3));
        if (Done < Rows) Tool(CraftTool.Kind.Needle, 0.12f).MoveTo(World(new Vector2(Loom.Left - 0.03f, Loom.RowY(p, Done))));
        else Tool(CraftTool.Kind.Needle).Hide();
    }

    public override string LiveHint
    {
        get
        {
            if (Done > 0 && !Beaten(Done - 1)) return Tight(Done - 1) ? "That weft went straight across: pulled tight, the edges will draw in. Undo and swipe in a gentle hill, or swipe down to beat it as it is."
                                                                    : "Swipe down to beat the row into place with the comb.";
            if (Done >= Rows) return "All woven. Tap Finish.";
            int last = Done > 0 ? (int)State.points[Done - 1].x : -1;
            return last == shed ? $"Row {Done + 1} of {Rows}: the shed's the same as the last row. Tap the shed stick to turn it."
                                : $"Row {Done + 1} of {Rows}: swipe across the loom in a gentle hill to pass the shuttle through.";
        }
    }

    protected override void OnPress(Vector2 p) { pressAt = p; hill = 0f; }

    protected override void OnDrag(Vector2 p)
    {
        float straight = pressAt.y;   // swipes start and end at about the same height
        hill = Mathf.Max(hill, p.y - straight);
    }

    protected override void OnRelease(Vector2 p)
    {
        var d = p - pressAt;
        if (d.magnitude < 0.04f)
        {
            if (p.y < 0.1f && p.y > -0.05f) { shed = 1 - shed; AudioManager.Play(Sfx.Click); Redraw(); }   // turn the shed stick
            return;
        }
        if (Mathf.Abs(d.x) >= MinSwipe && Mathf.Abs(d.x) > Mathf.Abs(d.y))   // the shuttle across
        {
            if (Done >= Rows) return;
            float rise = hill - Mathf.Max(0f, p.y - pressAt.y);
            State.points.Add(new Vector2(shed, rise >= Bubble ? 0 : 2));
            shed = 1 - shed;   // the next row naturally goes the other way, unless you turn it back
            Redraw();
            return;
        }
        if (d.y < -0.08f && Mathf.Abs(d.y) > Mathf.Abs(d.x) && Done > 0 && !Beaten(Done - 1))   // beat it down
        {
            var last = State.points[Done - 1];
            State.points[Done - 1] = new Vector2(last.x, last.y + 1);
            Redraw();
        }
    }

    public override void Undo()
    {
        if (Done == 0) return;
        State.points.RemoveAt(Done - 1);
        shed = Done > 0 ? 1 - (int)State.points[Done - 1].x : 0;
        Redraw();
    }

    public string MistakesLeft
    {
        get
        {
            int floats = 0, loose = 0, tight = 0;
            for (int r = 0; r < Done; r++)
            {
                if (r > 0 && (int)State.points[r].x == (int)State.points[r - 1].x) floats++;
                if (!Beaten(r)) loose++;
                if (Tight(r)) tight++;
            }
            var parts = new List<string>();
            if (floats > 0) parts.Add(floats == 1 ? "1 row woven in the same shed as the one before (it floats loose)" : $"{floats} rows woven in the same shed as the one before (they float loose)");
            if (loose > 0) parts.Add(loose == 1 ? "1 row not beaten down" : $"{loose} rows not beaten down");
            if (tight > 0) parts.Add(tight == 1 ? "1 row pulled tight (the edges draw in: lay the weft in a hill)" : $"{tight} rows pulled tight (the edges draw in: lay the weft in a hill)");
            return parts.Count == 0 ? null : string.Join(" and ", parts);
        }
    }

    public override float SaveWork()
    {
        State.progress = Done / (float)Rows;
        if (Done == 0) return 0f;
        float good = 0;
        for (int r = 0; r < Done; r++)
        {
            bool floats = r > 0 && (int)State.points[r].x == (int)State.points[r - 1].x;
            good += (floats ? 0.3f : 1f) * (Beaten(r) ? 1f : 0.5f) * (Tight(r) ? 0.6f : 1f);
        }
        return State.progress * good / Done;
    }
}

/// Hang it: tie the warp loops onto a wooden dowel, one knot at each mark along the top.
public class HangWeavingStep : PlacementStep
{
    protected override List<Vector2> Marks
    {
        get
        {
            var p = order.target;
            var marks = new List<Vector2>();
            for (int i = 0; i + 1 < Loom.Warps(p); i += 2) marks.Add(new Vector2((Loom.WarpX(p, i) + Loom.WarpX(p, i + 1)) / 2f, 0.86f));
            return marks;
        }
    }

    protected override void DrawPiece()
    {
        var p = order.target;
        int rows = Loom.Rows(p);
        float gapY = (Loom.Top - Loom.Bottom) / rows;
        for (int r = 0; r < rows; r++)   // the finished weaving, off the loom
            Line(new List<Vector2> { new Vector2(Loom.Left, Loom.RowY(p, r)), new Vector2(Loom.Right, Loom.RowY(p, r)) }, Loom.RowColor(p, r), gapY * 0.95f, 0);
        Line(new List<Vector2> { new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.88f) }, Loom.Wood, 0.035f, 1);   // the dowel
        for (int i = 0; i < Loom.Warps(p); i++)   // the warp loops left at the top
            Line(new List<Vector2> { new Vector2(Loom.WarpX(p, i), Loom.Top), new Vector2(Loom.WarpX(p, i), 0.87f) }, Loom.Warp, 0.008f, 2);
    }

    protected override GameObject DrawPlaced(Vector2 p) => Dot(p, 0.03f, Loom.Warp, 3);   // a knot round the dowel
}

/// Off the loom: cut the warps at the bottom and tie them off in pairs with overhand knots, so the rows can't slip out.
public class TieOffWarpsStep : PlacementStep
{
    protected override List<Vector2> Marks
    {
        get
        {
            var p = order.target;
            var marks = new List<Vector2>();
            for (int i = 0; i + 1 < Loom.Warps(p); i += 2) marks.Add(new Vector2((Loom.WarpX(p, i) + Loom.WarpX(p, i + 1)) / 2f, Loom.Bottom - 0.05f));
            return marks;
        }
    }

    protected override void DrawPiece()
    {
        var p = order.target;
        int rows = Loom.Rows(p);
        float gapY = (Loom.Top - Loom.Bottom) / rows;
        for (int r = 0; r < rows; r++)   // the weaving, cut off the loom
            Line(new List<Vector2> { new Vector2(Loom.Left, Loom.RowY(p, r)), new Vector2(Loom.Right, Loom.RowY(p, r)) }, Loom.RowColor(p, r), gapY * 0.95f, 0);
        for (int i = 0; i < Loom.Warps(p); i++)   // the cut warp ends hanging below
            Line(new List<Vector2> { new Vector2(Loom.WarpX(p, i), Loom.Bottom), new Vector2(Loom.WarpX(p, i), Loom.Bottom - 0.1f) }, Loom.Warp, 0.008f, 1);
    }

    protected override GameObject DrawPlaced(Vector2 p) => Dot(p, 0.03f, Loom.Warp, 3);   // an overhand knot round a pair
}
