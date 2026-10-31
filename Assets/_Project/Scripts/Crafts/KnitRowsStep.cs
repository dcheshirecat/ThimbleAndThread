using System.Collections.Generic;
using UnityEngine;

/// Knitting recipe step: knit the piece, one swipe per stitch. Swipe down for a knit stitch, up for
/// a purl; the faint marks show which is due (purls make the pattern's texture). A steady rhythm
/// makes even stitches. A wrong swipe makes the wrong stitch (a purl where a knit was due): it shows in
/// the fabric, and the only fix is to undo back to it, as knitters "tink" back.
/// Garments are knitted piece by piece (KnitLayout): ribbed edges alternate knit and purl; stripes and
/// colourwork are knitted in other yarns. The player holds one yarn at a time (the ball by the work): when
/// the marker changes colour, swipe sideways to pick up that yarn. Knitting on with the wrong one puts the
/// stitch in the wrong colour, which also has to be unpicked.
/// A stitch rushed far faster than your rhythm can drop (owner, 2026-10-31: deeper mini-games): it leaves a ladder in the
/// fabric. Tap the ladder to pick it back up with a hook; a dropped stitch left in is a mistake.
/// Each finished stitch is saved as a point: x is its evenness (0..1); y codes the stitch, see Code() (+Dropped
/// while it's dropped; saves from before dropped stitches never have it).
public class KnitRowsStep : PieceStep, IMistakes
{
    const float MinSwipe = 0.04f;       // shorter drags aren't swipes
    const float RushedBelow = 0.35f;    // a stitch made this much faster than your rhythm drops off the needle
    const int Dropped = 2000;           // added to a stitch's code while it's dropped

    readonly List<GameObject> stitches = new List<GameObject>();
    readonly List<GameObject> guide = new List<GameObject>();
    SpriteRenderer current;
    GameObject ball;
    Vector2 pressAt;
    float lastStitchTime = -1f, rhythm = -1f;   // seconds between stitches, a running average
    int held;                                    // the yarn in hand: -1 the main yarn, else a pattern colour

    int Done => State.points.Count;

    // A stitch's code (point.y): wrong type (0/1) + 2 * (yarn held + 2). Saves from before yarns were held
    // stored only 0 or 1: those stitches were in the right yarn.
    static float Code(bool wrongType, int heldYarn) => (wrongType ? 1 : 0) + 2 * (heldYarn + 2);
    int CodeOf(int i) => (int)State.points[i].y % Dropped;
    bool IsDropped(int i) => (int)State.points[i].y >= Dropped;
    bool WrongType(int i) => CodeOf(i) % 2 == 1;
    int HeldFor(int i)
    {
        int code = CodeOf(i);
        return code < 2 ? Knit.stitches[i].color : code / 2 - 2;
    }
    bool WrongYarn(int i) => HeldFor(i) != Knit.stitches[i].color;

    protected override void Build()
    {
        for (int i = 0; i < Knit.Stitches; i++) Square(Knit.CellCenter(i), Knit.cell, new Color(1, 1, 1, 0.08f), 0);   // the piece's shape
        current = Square(Vector2.zero, Knit.cell, new Color(1, 1, 1, 0.35f), 1);
        for (int i = 0; i < Done; i++) DrawStitch(i);
        held = Done > 0 ? HeldFor(Done - 1) : Knit.Stitches > 0 ? Knit.stitches[0].color : -1;
        Label(new Vector2(0.12f, -0.02f), "yarn in hand", 0.03f, new Color(1, 1, 1, 0.7f), 5);
        ShowBall();
        ShowNext();
    }

    /// The ball of yarn the player is holding, by the work.
    void ShowBall()
    {
        if (ball != null) Destroy(ball);
        ball = Dot(new Vector2(0.12f, 0.04f), 0.07f, Knit.PaletteColor(held, Yarn), 5, shine: true);
    }

    void DrawStitch(int i)
    {
        float evenness = State.points[i].x;
        // Uneven stitches sit a little off; the offset is fixed per stitch so it redraws the same.
        var wobble = new Vector2(Mathf.Sin(i * 12.9898f), Mathf.Cos(i * 78.233f)) * (1 - evenness) * Knit.cell * 0.25f;
        var stitch = new GameObject($"Stitch_{i}");
        stitch.transform.SetParent(transform, false);
        Rect(Knit.CellCenter(i), Vector2.one * Knit.cell * 1.02f, area.fabricColor, 1).transform.SetParent(stitch.transform, true);
        if (IsDropped(i))   // a ladder: the strands of yarn left across the gap
            for (int r = 0; r < 3; r++)
            {
                var at = Knit.CellCenter(i) + new Vector2(0, (r - 1) * Knit.cell * 0.28f);
                Line(new[] { at - new Vector2(Knit.cell * 0.42f, 0), at + new Vector2(Knit.cell * 0.42f, 0) }, Knit.PaletteColor(HeldFor(i), Yarn), Knit.cell * 0.07f, 2)
                    .transform.SetParent(stitch.transform, true);
            }
        else Stitch(Knit.CellCenter(i) + wobble, Knit.cell, Knit.Purl(i) != WrongType(i), Knit.PaletteColor(HeldFor(i), Yarn), 2).transform.SetParent(stitch.transform, true);
        if (i < stitches.Count) { Destroy(stitches[i]); stitches[i] = stitch; }
        else stitches.Add(stitch);
    }

    /// Highlights the next stitch and shows faint marks for the rest of its row.
    void ShowNext()
    {
        foreach (var g in guide) Destroy(g);
        guide.Clear();
        current.enabled = Done < Knit.Stitches;
        if (!current.enabled) { Tool(CraftTool.Kind.KnittingNeedles, Knit.cell * 1.6f).Hide(); return; }
        current.transform.position = World(Knit.CellCenter(Done));
        Tool(CraftTool.Kind.KnittingNeedles, Knit.cell * 1.6f).MoveTo(World(Knit.CellCenter(Done)));
        // The marker takes the colour of the yarn the next stitch needs.
        var needs = Knit.ColorOf(Done, Color.white);
        current.color = new Color(needs.r, needs.g, needs.b, 0.55f);
        var row = Knit.stitches[Done];
        for (int i = Done; i < Knit.Stitches && Knit.stitches[i].row == row.row && Knit.stitches[i].part == row.part; i++)
        {
            var c = Knit.ColorOf(i, Color.white);
            guide.Add(Stitch(Knit.CellCenter(i), Knit.cell, Knit.Purl(i), new Color(c.r, c.g, c.b, 0.5f), 2));
        }
    }

    /// The yarn a pattern colour is knitted in (from the cupboard, matched to the main yarn), by name.
    string YarnName(int colour)
    {
        var main = area.content.Supply(order.fabricId);
        if (colour < 0 || order.target == null || colour >= order.target.palette.Count) return main != null ? main.displayName : "main yarn";
        var y = Materials.ColourworkYarn(order.target.palette[colour], main, area.content, area.shop);
        return y != null ? y.displayName : "contrast yarn";
    }

    /// Under the hint: which yarn is in hand, and when the next stitch needs another one.
    public override string LiveHint
    {
        get
        {
            int dropped = DroppedCount();
            if (dropped > 0) return dropped == 1 ? "A stitch dropped! Tap the ladder to pick it up with your hook." : $"{dropped} stitches dropped! Tap each ladder to pick it up.";
            if (Done >= Knit.Stitches) return "All knitted. Tap Finish.";
            int needed = Knit.stitches[Done].color;
            return held != needed
                ? $"The next stitch is in {YarnName(needed)}: swipe sideways to pick it up (you're holding {YarnName(held)})."
                : $"Knitting in {YarnName(held)}.";
        }
    }

    protected override void OnPress(Vector2 p) => pressAt = p;

    protected override void OnRelease(Vector2 p)
    {
        var d = p - pressAt;
        if (d.magnitude < MinSwipe) { PickUp(pressAt); return; }   // a tap: on a ladder, pick the stitch back up
        if (Done >= Knit.Stitches) return;
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))   // sideways: pick up the yarn the next stitch needs
        {
            int needed = Knit.stitches[Done].color;
            if (held == needed) return;
            held = needed;
            ShowBall();
            AudioManager.Play(Sfx.Click);
            return;
        }
        bool purl = d.y > 0;
        bool wrongType = purl != Knit.Purl(Done);

        float now = Time.realtimeSinceStartup, evenness = 1f;
        bool drops = false;
        if (lastStitchTime >= 0)
        {
            float gap = now - lastStitchTime;
            drops = rhythm > 0.08f && gap < rhythm * RushedBelow * (1f - 0.5f * Assist);   // rushing: it slides off the needle
            if (rhythm > 0.08f) evenness = 1f - 0.6f * (1f - 0.7f * Assist) * Mathf.Clamp01(Mathf.Abs(gap - rhythm) / rhythm);   // smooth needles forgive
            rhythm = rhythm < 0 ? gap : Mathf.Lerp(rhythm, gap, 0.3f);
        }
        lastStitchTime = now;
        State.points.Add(new Vector2(evenness, Code(wrongType, held) + (drops ? Dropped : 0)));
        if (drops) AudioManager.Play(Sfx.Click);
        if (wrongType || held != Knit.stitches[Done - 1].color) AudioManager.Play(Sfx.Click);
        DrawStitch(Done - 1);
        Tool(CraftTool.Kind.KnittingNeedles).Poke();
        ShowNext();
    }

    /// Picks a dropped stitch back up with the hook (a tap on its ladder). It comes back a little uneven.
    void PickUp(Vector2 p)
    {
        for (int i = 0; i < Done; i++)
        {
            if (!IsDropped(i) || Vector2.Distance(p, Knit.CellCenter(i)) > Knit.cell * 0.8f) continue;
            var point = State.points[i];
            State.points[i] = new Vector2(Mathf.Max(point.x, 0.6f), point.y - Dropped);
            DrawStitch(i);
            Tool(CraftTool.Kind.Hook, Knit.cell * 1.4f).MoveTo(World(Knit.CellCenter(i)));
            Tool(CraftTool.Kind.Hook).Poke();
            AudioManager.Play(Sfx.Stitch);
            return;
        }
    }

    int DroppedCount()
    {
        int n = 0;
        for (int i = 0; i < Done; i++) if (IsDropped(i)) n++;
        return n;
    }

    public override void Undo()
    {
        if (Done == 0) return;
        State.points.RemoveAt(Done - 1);
        Destroy(stitches[stitches.Count - 1]);
        stitches.RemoveAt(stitches.Count - 1);
        ShowNext();
    }

    public string MistakesLeft
    {
        get
        {
            int type = 0, yarn = 0, dropped = DroppedCount();
            for (int i = 0; i < Done; i++) { if (WrongType(i)) type++; if (WrongYarn(i)) yarn++; }
            var parts = new List<string>();
            if (dropped > 0) parts.Add(dropped == 1 ? "1 dropped stitch (tap its ladder to pick it up)" : $"{dropped} dropped stitches (tap each ladder to pick it up)");
            if (type > 0) parts.Add(type == 1 ? "1 wrong stitch" : $"{type} wrong stitches");
            if (yarn > 0) parts.Add((yarn == 1 ? "1 stitch" : $"{yarn} stitches") + " knitted in the wrong colour (the pattern's coloured squares are knitted in their own yarn: swipe sideways when the marker changes colour)");
            return parts.Count == 0 ? null : string.Join(" and ", parts);
        }
    }

    /// Average evenness of the stitches, lowered by wrong ones (and by mistakes counted in older saves).
    public override float SaveWork()
    {
        State.progress = Done / (float)Knit.Stitches;
        if (Done == 0) return 0f;
        float sum = 0f;
        for (int i = 0; i < Done; i++) sum += WrongType(i) || WrongYarn(i) || IsDropped(i) ? State.points[i].x * 0.2f : State.points[i].x;   // a wrong or dropped stitch spoils it
        float mistakes = Mathf.Clamp01(State.mistakes / (float)Knit.Stitches);
        return State.progress * (sum / Done) * (1f - 0.5f * mistakes);
    }
}
