using System.Collections.Generic;
using UnityEngine;

/// Recipe step: stitch (or, for a pattern-only order, fill in) the order's pattern on the cross stitch board.
/// The stitches are saved in Order.work. Needlepoint uses it too (owner, 2026-10-31: new crafts): TentStitch works the
/// design in tent stitch on canvas on stretcher bars; BackgroundFill then fills every other square in the background
/// colour (Order.backgroundWork), with the design already stitched.
public class CrossStitchStep : MonoBehaviour, ICraftStep, IMistakes, IStitchPicker
{
    Order order;
    StepKind kind;
    int stepIndex;

    public CrossStitchCanvas Board { get; private set; }

    bool Tent => kind == StepKind.TentStitch || kind == StepKind.BackgroundFill;
    bool Background => kind == StepKind.BackgroundFill;
    Pattern target;   // what this step stitches: the design, or the background

    /// Needlepoint: the background colour (always-stocked white floss).
    public const string BackgroundThread = "floss_white";
    public static readonly Color BackgroundColor = new Color(0.98f, 0.98f, 0.96f);

    /// The background: every square the design leaves empty, in the background colour (after the design's colours).
    public static Pattern BackgroundOf(Pattern design)
    {
        var bg = new Pattern { width = design.width, height = design.height, craft = design.craft };
        bg.palette.AddRange(design.palette);
        bg.threads.AddRange(design.threads);
        int c = bg.palette.Count;
        bg.palette.Add(BackgroundColor);
        bg.threads.Add(BackgroundThread);
        var used = new HashSet<Vector2Int>();
        foreach (var cell in design.cells) used.Add(new Vector2Int(cell.x, cell.y));
        for (int x = 0; x < design.width; x++)
            for (int y = 0; y < design.height; y++)
                if (!used.Contains(new Vector2Int(x, y))) bg.cells.Add(new StitchCell { x = x, y = y, colorIndex = c });
        return bg;
    }

    /// Only the background's own stitches (the design's squares stay as they were).
    List<StitchMark> BackgroundMarks()
    {
        var mine = new List<StitchMark>();
        foreach (var m in Board.Marks())
            if (!order.target.cells.Exists(c => c.x == m.x && c.y == m.y)) mine.Add(m);
        return mine;
    }

    public void Begin(Order forOrder, int stepIndex, StepArea area)
    {
        order = forOrder;
        this.stepIndex = Mathf.Clamp(stepIndex, 0, order.Steps.Length - 1);
        kind = order.Steps[this.stepIndex].kind;
        target = Background ? BackgroundOf(order.target) : order.target;
        // Stitched in a hoop, or in the Q-Snap frame once you have one (owner, 2026-10-31: the hoop shows while stitching).
        var holder = order.patternOnly ? CrossStitchCanvas.Holder.None : Tent ? CrossStitchCanvas.Holder.Frame
                   : area.shop != null && area.shop.Owns("qsnap_frame") ? CrossStitchCanvas.Holder.QSnap : CrossStitchCanvas.Holder.Hoop;
        Board = MakeBoard(transform, area, target.width, target.height, holder);
        Board.holder = holder;
        Board.mode = order.patternOnly ? CrossStitchCanvas.Mode.PatternFill : CrossStitchCanvas.Mode.Stitch;
        Board.target = target;
        Board.tent = Tent;
        var restore = new List<StitchMark>(order.work);
        if (Background) restore.AddRange(order.backgroundWork);
        Board.restore = restore;
        Board.restoreCarried = Tent ? null : new List<Vector2Int>(order.carried);
        Board.fabricColor = area.fabricColor;
        Board.assist = area.assist;
    }

    public float SaveWork()
    {
        if (Background)
        {
            if (Board != null && Board.Ready) order.backgroundWork = BackgroundMarks();
            order.StepState(stepIndex).progress = Done(order.backgroundWork);
            return CrossStitchCanvas.TentScore(order.backgroundWork, target);
        }
        if (Board != null && Board.Ready) { order.work = Board.Marks(); if (!Tent) order.carried = Board.Carried(); }   // before Ready the board is still empty
        if (Tent) order.StepState(stepIndex).progress = Done(order.work);
        if (Tent) return CrossStitchCanvas.TentScore(order.work, order.target);
        float carries = target.cells.Count == 0 ? 0f : order.carried.Count / (float)target.cells.Count;
        return CrossStitchCanvas.Score(order.work, order.target) * Mathf.Clamp01(1f - 0.5f * carries);
    }

    public void Undo()
    {
        if (Board != null) Board.Undo();
    }

    public string FinishBlocker => null;

    /// Shown under the hint while stitching: a long carry just made.
    public string LiveHint => Board != null && !Tent ? Board.Warning : null;

    public bool CanPick => order != null && !order.patternOnly;
    public bool Picking { get => Board != null && Board.picking; set { if (Board != null) Board.SetPicking(value); } }

    /// Stitches in the wrong colour or outside the pattern, and crosses that go against the rest of the piece.
    public string MistakesLeft
    {
        get
        {
            if (Board == null || !Board.Ready || order.patternOnly) return null;
            if (Tent) return TentMistakes();
            int wrong = 0, overHalf = 0, quarters = 0;
            var marks = Board.Marks();
            int top = CrossStitchCanvas.TopLeg(marks);
            foreach (var m in marks)
            {
                var cell = order.target.cells.Find(c => c.x == m.x && c.y == m.y);
                if (cell == null || cell.colorIndex != m.color) wrong++;
                else if (cell.halfStitch && !CrossStitchCanvas.RightHalf(m.mask, top)) overHalf++;   // the bottom leg, or a full X
                else if (cell.quarter > 0 && m.mask != CrossStitchCanvas.QuarterMask(cell.quarter)) quarters++;
            }
            int crossed = Board.WrongCrosses();
            var parts = new List<string>();
            if (wrong > 0) parts.Add(wrong == 1 ? "1 stitch in the wrong place or colour" : $"{wrong} stitches in the wrong place or colour");
            if (overHalf > 0) parts.Add(overHalf == 1 ? "1 half stitch done wrong (it's just the leg that lies on top of your crosses)" : $"{overHalf} half stitches done wrong (they're just the leg that lies on top of your crosses)");
            if (crossed > 0) parts.Add(crossed == 1 ? "1 cross lying the other way from the rest" : $"{crossed} crosses lying the other way from the rest");
            if (quarters > 0) parts.Add(quarters == 1 ? "1 three-quarter stitch done wrong (a long leg plus a quarter leg from the marked corner to the middle)" : $"{quarters} three-quarter stitches done wrong (a long leg plus a quarter leg from the marked corner to the middle)");
            int carried = Board.Carried().Count;
            if (carried > 0) parts.Add(carried == 1 ? "1 stitch reached by carrying the thread too far across the back (it shows through)" : $"{carried} stitches reached by carrying the thread too far across the back (they show through)");
            return parts.Count == 0 ? null : string.Join(" and ", parts);
        }
    }

    /// Share of this step's squares with a stitch in them.
    float Done(List<StitchMark> marks) => target.cells.Count == 0 ? 1f : Mathf.Clamp01(marks.FindAll(m => target.cells.Exists(c => c.x == m.x && c.y == m.y)).Count / (float)target.cells.Count);

    /// Needlepoint: stitches off the chart or in the wrong colour, slanting the wrong way, or crossed into an X.
    string TentMistakes()
    {
        int wrong = 0, slant = 0, crossed = 0;
        foreach (var m in Background ? BackgroundMarks() : Board.Marks())
        {
            var cell = target.cells.Find(c => c.x == m.x && c.y == m.y);
            if (cell == null || cell.colorIndex != m.color) wrong++;
            else if (m.mask == 2) slant++;
            else if (m.mask == 3) crossed++;
        }
        var parts = new List<string>();
        if (wrong > 0) parts.Add(wrong == 1 ? "1 stitch in the wrong place or colour" : $"{wrong} stitches in the wrong place or colour");
        if (slant > 0) parts.Add(slant == 1 ? "1 stitch slanting the wrong way (tent stitches all go /)" : $"{slant} stitches slanting the wrong way (tent stitches all go /)");
        if (crossed > 0) parts.Add(crossed == 1 ? "1 square crossed into an X (needlepoint is one stitch a square)" : $"{crossed} squares crossed into Xs (needlepoint is one stitch a square)");
        return parts.Count == 0 ? null : string.Join(" and ", parts);
    }

    /// A board fitted into the area, as big as it can be while its squares stay square.
    public static CrossStitchCanvas MakeBoard(Transform parent, StepArea area, int width, int height, CrossStitchCanvas.Holder holder = CrossStitchCanvas.Holder.None)
    {
        float cell = area.size / CrossStitchCanvas.SpanFor(holder, width, height);
        var go = new GameObject("CrossStitchBoard");
        go.transform.SetParent(parent, false);
        go.transform.position = (Vector3)(area.center - new Vector2(width * cell, height * cell) / 2f);
        var b = go.AddComponent<CrossStitchCanvas>();
        b.cam = area.cam;
        b.material = area.material;
        b.cellSprite = area.square;
        b.width = width;
        b.height = height;
        b.cellSize = cell;
        return b;
    }
}
