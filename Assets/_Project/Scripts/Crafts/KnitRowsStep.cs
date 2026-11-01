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
/// Cables (KnitLayout.CableScarf): where the chart crosses a cable, swipe diagonally down to work the cross: down-left
/// slips two stitches onto the cable needle held in front (C4F), down-right holds them behind (C4B). One swipe knits all
/// four stitches of the cross. A cross the wrong way twists against its pair; a plain knit there leaves it uncrossed.
/// Each finished stitch is saved as a point: x is its evenness (0..1); y codes the stitch, see Code() (+Dropped
/// while it's dropped; saves from before dropped stitches never have it; +Crossed on the first stitch of a worked cross).
public class KnitRowsStep : PieceStep, IMistakes, IStitchPicker
{
    const float MinSwipe = 0.04f;       // shorter drags aren't swipes
    const float RushedBelow = 0.35f;    // a stitch made this much faster than your rhythm drops off the needle
    const int Dropped = 2000;           // added to a stitch's code while it's dropped
    const int Crossed = 1000;           // added to the first stitch of a cable cross that was worked as a cross

    readonly List<GameObject> stitches = new List<GameObject>();
    readonly List<GameObject> guide = new List<GameObject>();
    SpriteRenderer current;
    GameObject ball;
    Vector2 pressAt;
    float lastStitchTime = -1f, rhythm = -1f;   // seconds between stitches, a running average
    int held;                                    // the yarn in hand: -1 the main yarn, else a pattern colour

    int Done => State.points.Count;

    // ---- Frogging (owner, 2026-11-01): rip the knitting back to a chosen stitch ----
    public bool CanPick => Done > 0;
    public bool Picking { get => frogging; set => frogging = value && Done > 0; }
    bool frogging;

    /// Rips back: every stitch knitted after the one tapped comes undone (a cable cross all together).
    void FrogTo(Vector2 p)
    {
        int at = -1;
        for (int i = 0; i < Done; i++) if (Vector2.Distance(p, Knit.CellCenter(i)) <= Knit.cell * 0.7f) { at = i; break; }
        if (at < 0) return;
        if (InWorkedCross(at)) at = CrossStart(at);
        while (Done > at)
        {
            State.points.RemoveAt(Done - 1);
            Destroy(stitches[stitches.Count - 1]);
            stitches.RemoveAt(stitches.Count - 1);
        }
        held = Done > 0 ? HeldFor(Done - 1) : Knit.Stitches > 0 ? Knit.stitches[0].color : -1;
        ShowBall();
        frogging = false;
        AudioManager.Play(Sfx.Click);
        ShowNext();
    }

    // A stitch's code (point.y): wrong type (0/1) + 2 * (yarn held + 2). Saves from before yarns were held
    // stored only 0 or 1: those stitches were in the right yarn.
    static float Code(bool wrongType, int heldYarn) => (wrongType ? 1 : 0) + 2 * (heldYarn + 2);
    int CodeOf(int i) => (int)State.points[i].y % Crossed;
    bool IsCrossed(int i) => (int)State.points[i].y % Dropped >= Crossed;
    int CableOf(int i) => Knit.stitches[i].cable;
    /// The first stitch of the cable cross a stitch belongs to (or -1).
    int CrossStart(int i)
    {
        if (CableOf(i) == 1 || CableOf(i) == 2) return i;
        if (CableOf(i) != 3) return -1;
        for (int j = i - 1; j >= 0 && j >= i - 3; j--) if (CableOf(j) == 1 || CableOf(j) == 2) return j;
        return -1;
    }
    bool InWorkedCross(int i) { int s = CrossStart(i); return s >= 0 && s < Done && IsCrossed(s); }
    int Uncrossed()
    {
        int n = 0;
        for (int i = 0; i < Done; i++) if ((CableOf(i) == 1 || CableOf(i) == 2) && !IsCrossed(i)) n++;
        return n;
    }
    bool IsDropped(int i) => (int)State.points[i].y >= Dropped;
    bool WrongType(int i) => CodeOf(i) % 2 == 1;
    int HeldFor(int i)
    {
        int code = CodeOf(i);
        return code < 2 ? Knit.stitches[i].color : code / 2 - 2;
    }
    bool WrongYarn(int i) => HeldFor(i) != Knit.stitches[i].color;

    /// The yarn a saved stitch was knitted in (-1 = the main yarn); planned = the yarn the chart wanted there.
    public static int HeldYarn(Vector2 point, int planned)
    {
        int code = (int)point.y % Crossed;
        return code < 2 ? planned : code / 2 - 2;
    }

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

    readonly List<GameObject> yarnBalls = new List<GameObject>();

    /// The ball of yarn the player is holding, by the work, and every yarn this piece is knitted in beside it (owner,
    /// 2026-11-01: it wasn't clear how many yarns there were): the one in hand is ringed.
    void ShowBall()
    {
        if (ball != null) Destroy(ball);
        ball = Dot(new Vector2(0.12f, 0.04f), 0.07f, Knit.PaletteColor(held, Yarn), 5, shine: true);
        foreach (var g in yarnBalls) Destroy(g);
        yarnBalls.Clear();
        var yarns = new List<int>();
        foreach (var st in Knit.stitches) if (!yarns.Contains(st.color)) yarns.Add(st.color);
        if (yarns.Count < 2) return;
        yarns.Sort();
        for (int k = 0; k < yarns.Count; k++)
        {
            var at = new Vector2(0.3f + k * 0.08f, 0.04f);
            bool inHand = yarns[k] == held;   // the one in hand sits up, bigger
            yarnBalls.Add(Dot(at + (inHand ? new Vector2(0, 0.012f) : Vector2.zero), inHand ? 0.058f : 0.036f, Knit.PaletteColor(yarns[k], Yarn), 5, shine: true));
        }
        yarnBalls.Add(Label(new Vector2(0.3f + (yarns.Count - 1) * 0.04f, -0.055f), $"{yarns.Count} yarns in this piece", 0.028f, new Color(1, 1, 1, 0.7f), 5).gameObject);
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
        else if (InWorkedCross(i))
        {
            if (CrossStart(i) == i) DrawCross(i, stitch.transform);
        }
        else Stitch(Knit.CellCenter(i) + wobble, Knit.cell, Knit.Purl(i) != WrongType(i), Knit.PaletteColor(HeldFor(i), Yarn), 2).transform.SetParent(stitch.transform, true);
        if (i < stitches.Count) { Destroy(stitches[i]); stitches[i] = stitch; }
        else stitches.Add(stitch);
    }

    /// A worked cable cross: two pairs of stitches swap places, one pair passing in front of the other. Front crosses (C4F)
    /// lean left (the front strand runs from bottom right to top left); back crosses lean right. Crossed the wrong way,
    /// it's drawn leaning the way it was actually worked.
    void DrawCross(int start, Transform parent)
    {
        bool front = (CableOf(start) == 1) != WrongType(start);
        var a = Knit.CellCenter(start) + new Vector2(-Knit.cell * 0.5f, 0);
        var b = Knit.CellCenter(start + 3) + new Vector2(Knit.cell * 0.5f, 0);
        float h = Knit.cell * 0.55f, inset = Knit.cell * 0.9f;
        var bl = new Vector2(a.x + inset, a.y - h); var tl = new Vector2(a.x + inset, a.y + h);
        var br = new Vector2(b.x - inset, b.y - h); var tr = new Vector2(b.x - inset, b.y + h);
        var yarn = Knit.PaletteColor(HeldFor(start), Yarn);
        var shade = Color.Lerp(yarn, Color.black, 0.3f);
        // The strand behind, then the one in front with a shadow and a sheen
        var (backFrom, backTo, frontFrom, frontTo) = front ? (bl, tr, br, tl) : (br, tl, bl, tr);
        Line(new[] { backFrom, backTo }, shade, Knit.cell * 1.2f, 2).transform.SetParent(parent, true);
        Line(new[] { frontFrom + new Vector2(0.004f, -0.004f), frontTo + new Vector2(0.004f, -0.004f) }, new Color(0, 0, 0, 0.35f), Knit.cell * 1.3f, 3).transform.SetParent(parent, true);
        Line(new[] { frontFrom, frontTo }, yarn, Knit.cell * 1.2f, 4).transform.SetParent(parent, true);
        Line(new[] { Vector2.Lerp(frontFrom, frontTo, 0.15f), Vector2.Lerp(frontFrom, frontTo, 0.85f) }, Color.Lerp(yarn, Color.white, 0.35f), Knit.cell * 0.25f, 5).transform.SetParent(parent, true);
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
            if (CableOf(i) == 1 || CableOf(i) == 2)   // the chart's cable symbol: which way this cross leans
            {
                var mid = (Knit.CellCenter(i) + Knit.CellCenter(i + 3)) / 2f;
                guide.Add(Label(mid, CableOf(i) == 1 ? "<< C4F" : "C4B >>", Knit.cell * 0.55f, new Color(1f, 0.95f, 0.6f), 6).gameObject);
            }
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
            if (frogging) return "Frogging: tap the stitch to rip back to. Everything knitted after it comes undone.";
            int dropped = DroppedCount();
            if (dropped > 0) return dropped == 1 ? "A stitch dropped! Tap the ladder to pick it up with your hook." : $"{dropped} stitches dropped! Tap each ladder to pick it up.";
            if (Done >= Knit.Stitches) return "All knitted. Tap Finish.";
            if (CableOf(Done) == 1) return "Cable cross (C4F): swipe down and to the LEFT to hold two stitches in front on the cable needle and knit across.";
            if (CableOf(Done) == 2) return "Cable cross (C4B): swipe down and to the RIGHT to hold two stitches behind on the cable needle and knit across.";
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
        if (frogging) { if (d.magnitude < MinSwipe) FrogTo(pressAt); return; }
        if (d.magnitude < MinSwipe) { PickUp(pressAt); return; }   // a tap: on a ladder, pick the stitch back up
        if (Done >= Knit.Stitches) return;
        bool diagonal = d.y < 0 && Mathf.Abs(d.x) >= 0.5f * Mathf.Abs(d.y) && Mathf.Abs(d.y) >= 0.5f * Mathf.Abs(d.x);
        if (diagonal && (CableOf(Done) == 1 || CableOf(Done) == 2) && Done + 3 < Knit.Stitches) { CrossCable(d.x < 0); return; }
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

    /// Works a cable cross: all four stitches at once. toFront = the swipe went down-left (stitches held in front).
    void CrossCable(bool toFront)
    {
        bool wrongWay = toFront != (CableOf(Done) == 1);
        lastStitchTime = Time.realtimeSinceStartup;   // a cross takes its time: it doesn't upset the rhythm
        int start = Done;
        for (int k = 0; k < 4; k++)
            State.points.Add(new Vector2(1f, Code(k == 0 && wrongWay, held) + (k == 0 ? Crossed : 0)));
        for (int k = 0; k < 4; k++) DrawStitch(start + k);
        AudioManager.Play(wrongWay ? Sfx.Click : Sfx.Stitch);
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
        int start = InWorkedCross(Done - 1) ? CrossStart(Done - 1) : Done - 1;   // a cross comes undone all four stitches together
        while (Done > start)
        {
            State.points.RemoveAt(Done - 1);
            Destroy(stitches[stitches.Count - 1]);
            stitches.RemoveAt(stitches.Count - 1);
        }
        ShowNext();
    }

    public string MistakesLeft
    {
        get
        {
            int type = 0, yarn = 0, dropped = DroppedCount(), uncrossed = Uncrossed(), wrongWay = 0;
            for (int i = 0; i < Done; i++) if (IsCrossed(i) && WrongType(i)) wrongWay++;
            for (int i = 0; i < Done; i++) { if (WrongType(i)) type++; if (WrongYarn(i)) yarn++; }
            var parts = new List<string>();
            if (dropped > 0) parts.Add(dropped == 1 ? "1 dropped stitch (tap its ladder to pick it up)" : $"{dropped} dropped stitches (tap each ladder to pick it up)");
            type -= wrongWay;
            if (type > 0) parts.Add(type == 1 ? "1 wrong stitch" : $"{type} wrong stitches");
            if (uncrossed > 0) parts.Add(uncrossed == 1 ? "1 cable not crossed (knitted plain where the chart crosses it)" : $"{uncrossed} cables not crossed (knitted plain where the chart crosses them)");
            if (wrongWay > 0) parts.Add(wrongWay == 1 ? "1 cable crossed the wrong way" : $"{wrongWay} cables crossed the wrong way");
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
        for (int i = 0; i < Done; i++)
        {
            bool spoilt = WrongType(i) || WrongYarn(i) || IsDropped(i);
            int s = CrossStart(i);
            if (s >= 0 && s < Done && (!IsCrossed(s) || WrongType(s))) spoilt = true;   // a cable not crossed, or crossed the wrong way, spoils all four
            sum += spoilt ? State.points[i].x * 0.2f : State.points[i].x;   // a wrong or dropped stitch spoils it
        }
        float mistakes = Mathf.Clamp01(State.mistakes / (float)Knit.Stitches);
        return State.progress * (sum / Done) * (1f - 0.5f * mistakes);
    }
}
