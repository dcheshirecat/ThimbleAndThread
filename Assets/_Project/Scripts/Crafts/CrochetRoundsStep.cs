using System.Collections.Generic;
using UnityEngine;

/// Crochet recipe step: work the rounds of a written amigurumi pattern ("Round 3: (sc, inc) x6 (18)"),
/// spiralling out from the magic ring (see CrochetWork for each piece's pattern). The highlighted spot is
/// the next stitch to work into:
///   single crochet: tap it to hook a loop, then flick to pull through;
///   increase: after a single crochet, hook into the SAME stitch again (tap it) and flick;
///   decrease: tap the next stitch, then the one after it (two loops on the hook), then flick.
/// A stripe round needs its colour: flick sideways (with nothing on the hook) to change yarn.
/// Moves that don't follow the pattern are mistakes, and a round with the wrong count makes the next
/// one go wrong too. Each move is saved as a point: x its neatness, y its code (CrochetWork.Code).
public class CrochetRoundsStep : PieceStep, IMistakes
{
    public const int Rounds = 3;        // the first plushies' rounds (CrochetWork's default piece)
    const float Reach = 0.05f;          // how close a tap must be to hook
    const float MinFlick = 0.08f;       // how far a flick must travel

    readonly List<GameObject> drawn = new List<GameObject>();
    CrochetWork work;
    SpriteRenderer current, second, loop;
    GameObject ball;
    int hooked;                         // loops on the hook: 0, 1, or 2 (a decrease)
    bool again;                         // hooked back into the last stitch (an increase)
    bool pressing;
    int held = -1;                      // the yarn in hand: -1 the main yarn, else a pattern colour
    float hookNeatness;
    Vector2 pressAt;

    int Done => State.points.Count;
    Color MainYarn => Color.Lerp(area.fabricColor, Color.black, 0.15f);
    Color YarnColor(int i) => i >= 0 && order.target != null && i < order.target.palette.Count ? order.target.palette[i] : MainYarn;

    /// The written pattern line and where you are in it, shown under the work.
    public override string LiveHint
    {
        get
        {
            if (work == null) return null;
            if (work.Finished) return "All rounds done. Fasten off.";
            var r = work.Upcoming;
            int ops = work.UpcomingDone, want = r.ops.Count;
            string next = ops < want ? r.ops[ops] switch { CrochetOp.Inc => "inc", CrochetOp.Dec => "dec", _ => "sc" } : "...";
            string yarn = held != r.color ? " Change yarn first (flick sideways)." : "";
            return $"{r.text}. Stitch {ops + 1} of {want}: next is {next}.{yarn}";
        }
    }

    /// The spots for every stitch of the first plushies' three rounds (kept for older callers).
    public static List<Vector2> Slots()
    {
        var w = CrochetWork.For("");
        var pts = new List<Vector2>();
        for (int i = 0; i < w.TotalOps; i++) pts.Add(new Vector2(1, 0));
        w.Follow(pts);
        var list = new List<Vector2>();
        foreach (var r in w.stitches) list.AddRange(r);
        return list;
    }

    protected override void Build()
    {
        work = CrochetWork.For(Recipe.piece);
        for (int p = 0; p < work.centres.Count; p++) Dot(work.centres[p], 0.07f * work.scale, MainYarn, 1);   // the magic ring(s)
        if (Recipe.piece == "head_close")   // the stuffed head, with its eyes, waiting to be closed
        {
            Piece(work.Outline(), MainYarn, 0);
            foreach (var e in CrochetEyesStep.EyeSpots()) Dot(e, AttachEyesStep.EyeSize * 1.2f, AttachEyesStep.EyeColor, 2, shine: true);
        }
        current = Square(Vector2.zero, 0.06f, new Color(1, 1, 1, 0.5f), 6);
        second = Square(Vector2.zero, 0.05f, new Color(1, 0.9f, 0.5f, 0.6f), 6);
        loop = Square(Vector2.zero, 0.03f, Color.white, 8);
        held = -1;
        Redraw();
        if (Done > 0) held = LastYarn();
    }

    int LastYarn()
    {
        for (int r = work.yarnsMade.Count - 1; r >= 0; r--)
            if (work.yarnsMade[r].Count > 0) return work.yarnsMade[r][work.yarnsMade[r].Count - 1];
        return -1;
    }

    void Redraw()
    {
        foreach (var g in drawn) if (g) Destroy(g);
        drawn.Clear();
        work.Follow(State.points);
        for (int r = 0; r < work.stitches.Count; r++)
        {
            var c = work.Centre(r);
            for (int k = 0; k < work.stitches[r].Count; k++)
            {
                var at = work.stitches[r][k];
                var out_ = at - c;
                float angle = Mathf.Atan2(out_.y, out_.x) * Mathf.Rad2Deg - 90f;
                int yarn = k < work.yarnsMade[r].Count ? work.yarnsMade[r][Mathf.Min(k, work.yarnsMade[r].Count - 1)] : -1;
                var col = YarnColor(yarn);
                if (Recipe.piece == "head_close") col = Color.Lerp(col, Color.black, 0.15f);   // curving away, round the back
                float room = Mathf.PI * 2f * work.Radius(r) / Mathf.Max(6, work.stitches[r].Count) * 1.1f;
                drawn.Add(Stitch(at, Mathf.Min(0.05f * work.scale + 0.012f, room), false, col, 3 + r % 2, angle));
            }
        }
        ShowBall();
        Show();
    }

    void ShowBall()
    {
        if (ball) Destroy(ball);
        ball = Dot(new Vector2(0.12f, 0.04f), 0.07f, YarnColor(held), 5, shine: true);
    }

    void Show()
    {
        bool working = !work.Finished;
        current.enabled = working;
        second.enabled = working && hooked == 2;
        loop.enabled = working && hooked > 0;
        if (!working) return;
        var next = again ? work.BasePoint(work.round, work.cursor - 1) : work.BasePoint(work.NextRound, work.NextCursor);
        current.transform.position = World(next);
        second.transform.position = World(work.BasePoint(work.NextRound, work.NextCursor + 1));
        loop.transform.position = World(next);
        if (hooked == 0) Tool(CraftTool.Kind.Hook).MoveTo(World(next));
    }

    bool Near(Vector2 p, Vector2 spot, out float d)
    {
        d = Vector2.Distance(p, spot);
        return d <= Reach * (1f + Assist) * Mathf.Max(0.8f, work.scale);   // a comfier hook catches the loop more easily
    }

    protected override void OnPress(Vector2 p) { pressAt = p; pressing = true; }

    protected override void OnRelease(Vector2 p)
    {
        if (!pressing || work.Finished) return;
        pressing = false;
        var d = p - pressAt;
        if (d.magnitude >= MinFlick)
        {
            if (hooked == 0)   // a sideways flick with nothing on the hook: change yarn
            {
                if (Mathf.Abs(d.x) > Mathf.Abs(d.y) && held != work.Upcoming.color) { held = work.Upcoming.color; ShowBall(); AudioManager.Play(Sfx.Click); }
                return;
            }
            Pull(d);
            return;
        }
        Tap(pressAt);
    }

    void Tap(Vector2 p)
    {
        float reach = Reach * (1f + Assist);
        if (hooked == 0)
        {
            bool next = Near(p, work.BasePoint(work.NextRound, work.NextCursor), out float dNext);
            float dSame = float.MaxValue;
            bool same = work.lastInRound && work.lastOp == CrochetOp.Sc && work.cursor > 0 && Near(p, work.BasePoint(work.round, work.cursor - 1), out dSame);
            if (same && (!next || dSame < dNext)) { Hook(dSame, reach); again = true; }   // into the same stitch again: an increase
            else if (next) { Hook(dNext, reach); again = false; }
        }
        else if (hooked == 1 && !again && Near(p, work.BasePoint(work.NextRound, work.NextCursor + 1), out _))
        {
            hooked = 2;   // a second loop: a decrease
            Tool(CraftTool.Kind.Hook).MoveTo(World(work.BasePoint(work.NextRound, work.NextCursor + 1)));
            Tool(CraftTool.Kind.Hook).Poke();
        }
        Show();
    }

    void Hook(float dist, float reach)
    {
        hooked = 1;
        Tool(CraftTool.Kind.Hook).Poke();
        hookNeatness = 1f - Mathf.InverseLerp(0.012f * (1f + 2f * Assist), reach, dist);
    }

    void Pull(Vector2 flick)
    {
        Tool(CraftTool.Kind.Hook).Pull(flick);
        if (again)   // the last single crochet becomes an increase
        {
            var last = State.points[Done - 1];
            State.points[Done - 1] = new Vector2((last.x + hookNeatness) * 0.5f, CrochetWork.Code(CrochetOp.Inc, held));
        }
        else State.points.Add(new Vector2(hookNeatness, CrochetWork.Code(hooked == 2 ? CrochetOp.Dec : CrochetOp.Sc, held)));
        hooked = 0; again = false;
        AudioManager.Play(Sfx.Stitch);
        Redraw();
    }

    public override void Undo()
    {
        if (hooked > 0) { hooked = 0; again = false; Show(); return; }
        if (Done == 0) return;
        State.points.RemoveAt(Done - 1);
        Redraw();
    }

    public string MistakesLeft
    {
        get
        {
            if (work == null) return null;
            int n = work.Mistakes(out string where);
            return n == 0 ? null : $"{n} stitch{(n == 1 ? "" : "es")} not as the pattern says ({where})";
        }
    }

    public override float SaveWork()
    {
        work.Follow(State.points);
        int total = work.TotalOps, made = 0;
        foreach (var r in work.opsMade) made += r.Count;
        State.progress = work.Finished ? 1f : Mathf.Clamp01(made / (float)total);
        State.mistakes = work.Mistakes(out _);
        if (Done == 0) return 0f;
        float sum = 0f;
        foreach (var p in State.points) sum += p.x;
        float neat = 0.5f + 0.5f * sum / Done;   // a loose stitch still holds
        return State.progress * neat * Mathf.Clamp01(1f - State.mistakes / (float)Mathf.Max(1, total));
    }
}
