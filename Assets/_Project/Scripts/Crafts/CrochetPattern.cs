using System.Collections.Generic;
using UnityEngine;

/// A crochet stitch move: a single crochet into one stitch, an increase (two into one), or a decrease
/// (one stitch over two).
public enum CrochetOp { Sc, Inc, Dec }

/// One round of a written amigurumi pattern, e.g. "Round 3: (sc, inc) x6 (18)".
public class CrochetRound
{
    public string text;                  // as written in the pattern
    public List<CrochetOp> ops = new List<CrochetOp>();
    public bool intoRing;                // the first round goes into the magic ring
    public int color = -1;               // -1 the main yarn, else a pattern colour (a stripe)
    public int part;                     // which separate part it belongs to (two ears are two parts)

    public int Stitches
    {
        get { int n = 0; foreach (var o in ops) n += o == CrochetOp.Inc ? 2 : 1; return n; }
    }
}

/// The written pattern for each crocheted piece (RecipeStep.piece), and how the work made so far lies on
/// the screen: rounds spiral out from the ring; flat rounds step out a little, decreases pull back in.
public class CrochetWork
{
    public const int RingStitches = 6;
    const float RingRadius = 0.05f, Step = 0.06f;

    public readonly List<CrochetRound> rounds = new List<CrochetRound>();
    public readonly List<Vector2> centres = new List<Vector2>();   // per part
    public float scale = 1f;
    int startBases;                                    // a piece that carries on from another (closing the head)
    float startRadius;

    // What's been made (filled in by Follow)
    public int round;                                  // the round being worked (rounds.Count when finished)
    public int cursor;                                 // the next base stitch in that round
    public CrochetOp lastOp;                           // the last move made in this round
    public bool lastInRound;                           // whether there is one
    public readonly List<int> made = new List<int>();  // stitches made per round so far
    public readonly List<List<Vector2>> stitches = new List<List<Vector2>>();   // where each stitch sits
    public readonly List<List<CrochetOp>> opsMade = new List<List<CrochetOp>>();
    public readonly List<List<int>> yarnsMade = new List<List<int>>();
    readonly List<List<float>> angles = new List<List<float>>();
    readonly List<float> radii = new List<float>();

    public bool full;                                  // every base of the round is used; it closes when the next stitch starts
                                                       // (so the last stitch can still become an increase)
    public bool Finished => round >= rounds.Count || (full && round == rounds.Count - 1);
    public CrochetRound Current => Finished ? null : rounds[round];
    /// The round the next stitch goes in, and its first free base.
    public int NextRound => full ? round + 1 : round;
    public int NextCursor => full ? 0 : cursor;
    public CrochetRound Upcoming => Finished ? null : rounds[NextRound];
    public int UpcomingDone => full ? 0 : opsMade[round].Count;
    public int TotalOps { get { int n = 0; foreach (var r in rounds) n += r.ops.Count; return n; } }

    public static CrochetWork For(string piece)
    {
        var w = new CrochetWork();
        var c = new Vector2(0.5f, 0.5f);
        switch (piece)
        {
            case "head":
                w.centres.Add(c);
                w.Ring(); w.R("inc x6", 6, CrochetOp.Inc); w.R("(sc, inc) x6", 6, CrochetOp.Sc, CrochetOp.Inc);
                w.R("(2 sc, inc) x6", 6, CrochetOp.Sc, CrochetOp.Sc, CrochetOp.Inc); w.R("sc around", 24, CrochetOp.Sc); w.R("sc around", 24, CrochetOp.Sc);
                break;
            case "head_close":   // carries on from the head's last round, after the eyes and stuffing
                w.centres.Add(c);
                w.startBases = 24; w.startRadius = HeadRadius;
                w.R("(2 sc, dec) x6", 6, CrochetOp.Sc, CrochetOp.Sc, CrochetOp.Dec); w.R("(sc, dec) x6", 6, CrochetOp.Sc, CrochetOp.Dec); w.R("dec x6", 6, CrochetOp.Dec);
                break;
            case "body":         // with a stripe in the pattern's first colour
                w.centres.Add(c);
                w.Ring(); w.R("inc x6", 6, CrochetOp.Inc); w.R("(sc, inc) x6", 6, CrochetOp.Sc, CrochetOp.Inc);
                w.R("sc around, in the stripe colour", 18, CrochetOp.Sc); w.rounds[w.rounds.Count - 1].color = 0;
                w.R("sc around, back in the main colour", 18, CrochetOp.Sc);
                break;
            case "ears":         // two little ears, one after the other
                w.scale = 0.7f;
                w.centres.Add(new Vector2(0.3f, 0.5f)); w.centres.Add(new Vector2(0.7f, 0.5f));
                for (int part = 0; part < 2; part++)
                {
                    w.Ring(part); w.R("(sc, inc) x3", 3, CrochetOp.Sc, CrochetOp.Inc); w.R("sc around", 9, CrochetOp.Sc);
                }
                break;
            default:             // the first plushies: three rounds, 6, 12, 18
                w.centres.Add(c);
                w.Ring(); w.R("inc x6", 6, CrochetOp.Inc); w.R("(sc, inc) x6", 6, CrochetOp.Sc, CrochetOp.Inc);
                break;
        }
        for (int i = 0; i < w.rounds.Count; i++)   // number the rounds within each part, as a pattern does
        {
            int n = 1;
            for (int j = 0; j < i; j++) if (w.rounds[j].part == w.rounds[i].part) n++;
            if (piece == "head_close") n += 6;
            w.rounds[i].text = $"Round {n}: {w.rounds[i].text} ({w.rounds[i].Stitches})";
        }
        return w;
    }

    /// The head's outer radius on screen, where the closing rounds start.
    public const float HeadRadius = RingRadius + Step * 3f + Step * 0.4f * 2f;

    void Ring(int part = 0)
    {
        var r = new CrochetRound { text = "6 sc into the magic ring", intoRing = true, part = part };
        for (int i = 0; i < RingStitches; i++) r.ops.Add(CrochetOp.Sc);
        rounds.Add(r);
    }

    void R(string text, int times, params CrochetOp[] unit)
    {
        var r = new CrochetRound { text = text, part = rounds.Count > 0 ? rounds[rounds.Count - 1].part : 0 };
        for (int t = 0; t < times; t++) r.ops.AddRange(unit);
        rounds.Add(r);
    }

    /// Bases for a round: the ring's 6, or the stitches of the round before.
    int BasesFor(int r)
    {
        if (rounds[r].intoRing) return RingStitches;
        if (r == 0) return startBases;
        return made[r - 1];
    }

    float BaseAngle(int r, int b, int bases)
    {
        if (rounds[r].intoRing || r == 0) return Mathf.PI * 2f * (b + 0.5f) / Mathf.Max(1, bases) + 0.3f;
        var prev = angles[r - 1];
        return prev.Count > 0 ? prev[Mathf.Clamp(b, 0, prev.Count - 1)] : 0f;
    }

    public float Radius(int r) => r < radii.Count ? radii[r] : radii.Count > 0 ? radii[radii.Count - 1] : RingRadius * scale;

    public Vector2 Centre(int r) => centres[Mathf.Clamp(rounds[Mathf.Clamp(r, 0, rounds.Count - 1)].part, 0, centres.Count - 1)];

    /// Where base b of round r is (the stitch it goes into).
    public Vector2 BasePoint(int r, int b)
    {
        if (r >= rounds.Count) return Vector2.zero;
        int bases = BasesFor(r);
        float radius = rounds[r].intoRing ? RingRadius * scale * 0.8f : r == 0 ? startRadius : Radius(r - 1);
        float a = BaseAngle(r, b, bases);
        return Centre(r) + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
    }

    /// Replays the moves made (each point: x neatness, y code) to find where the work is.
    public void Follow(List<Vector2> points)
    {
        round = 0; cursor = 0; lastInRound = false; full = false;
        made.Clear(); stitches.Clear(); opsMade.Clear(); yarnsMade.Clear(); angles.Clear(); radii.Clear();
        StartRound(0);
        int opInRound = 0;
        foreach (var p in points)
        {
            if (Finished) break;
            if (full)
            {
                Place(round);
                round++; cursor = 0; opInRound = 0; lastInRound = false; full = false;
                StartRound(round);
            }
            Decode(p.y, Current, opInRound, out var op, out int yarn);
            int bases = BasesFor(round);
            float a = BaseAngle(round, cursor, bases);
            float spacing = Mathf.PI * 2f / Mathf.Max(6, rounds[round].Stitches);
            var list = angles[round];
            switch (op)
            {
                case CrochetOp.Sc: list.Add(a); cursor += 1; break;
                case CrochetOp.Inc: list.Add(a - spacing * 0.25f); list.Add(a + spacing * 0.25f); cursor += 1; break;
                case CrochetOp.Dec:
                    float a2 = BaseAngle(round, Mathf.Min(cursor + 1, bases - 1), bases);
                    list.Add(a + Mathf.DeltaAngle(a * Mathf.Rad2Deg, a2 * Mathf.Rad2Deg) * Mathf.Deg2Rad * 0.5f);
                    cursor += Mathf.Min(2, bases - cursor);
                    break;
            }
            opsMade[round].Add(op);
            yarnsMade[round].Add(yarn);
            lastOp = op; lastInRound = true;
            opInRound++;
            made[round] = list.Count;
            if (cursor >= bases) { full = true; Even(round); }
        }
        if (round < rounds.Count) Place(round);
    }

    /// A finished round's stitches settle evenly round the circle (the next round works into them).
    void Even(int r)
    {
        var list = angles[r];
        if (list.Count == 0) return;
        float a0 = list[0];
        for (int k = 0; k < list.Count; k++) list[k] = a0 + Mathf.PI * 2f * k / list.Count;
    }

    void StartRound(int r)
    {
        made.Add(0); angles.Add(new List<float>()); opsMade.Add(new List<CrochetOp>()); yarnsMade.Add(new List<int>()); stitches.Add(new List<Vector2>());
        float prev = rounds[r].intoRing ? RingRadius * scale - Step * scale : r == 0 ? startRadius : radii[r - 1];
        int before = rounds[r].intoRing ? 0 : r == 0 ? startBases : rounds[r - 1].Stitches;
        int now = rounds[r].Stitches;
        radii.Add(prev + Step * scale * (now > before ? 1f : now == before ? 0.4f : -1f));
    }

    void Place(int r)
    {
        var list = stitches[r];
        list.Clear();
        foreach (var a in angles[r]) list.Add(Centre(r) + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radii[r]);
    }

    /// A move's saved code: 1 + move + 4 * (yarn + 1). 0 = saved before moves were recorded (taken as the
    /// pattern's move in the main yarn).
    public static float Code(CrochetOp op, int yarn) => 1 + (int)op + 4 * (yarn + 1);

    static void Decode(float y, CrochetRound round, int index, out CrochetOp op, out int yarn)
    {
        int code = Mathf.RoundToInt(y);
        if (code <= 0) { op = index < round.ops.Count ? round.ops[index] : CrochetOp.Sc; yarn = round.color; return; }
        op = (CrochetOp)((code - 1) % 4);
        yarn = (code - 1) / 4 - 1;
    }

    /// Moves that don't follow the pattern, in finished rounds and the one being worked.
    public int Mistakes(out string where)
    {
        int n = 0; where = null;
        for (int r = 0; r < opsMade.Count && r < rounds.Count; r++)
        {
            var want = rounds[r].ops;
            var got = opsMade[r];
            bool roundDone = r < round;
            int wrong = 0;
            for (int i = 0; i < got.Count; i++)
            {
                if (i >= want.Count || got[i] != want[i]) wrong++;
                if (yarnsMade[r][i] != rounds[r].color) wrong++;
            }
            if (roundDone && got.Count < want.Count) wrong += want.Count - got.Count;
            if (wrong > 0 && where == null)
                where = roundDone && made[r] != rounds[r].Stitches ? $"round {r + 1} has {made[r]} stitches, the pattern says {rounds[r].Stitches}" : $"in round {r + 1}";
            n += wrong;
        }
        return n;
    }

    /// The ball's outline when finished (for stuffing and eyes).
    public List<Vector2> Outline(int part = 0)
    {
        float r = 0f;
        for (int i = 0; i < rounds.Count; i++) if (rounds[i].part == part) r = Mathf.Max(r, ExpectedRadius(i));
        var c = centres[Mathf.Clamp(part, 0, centres.Count - 1)];
        var list = new List<Vector2>();
        for (int i = 0; i < 40; i++) { float a = i * Mathf.PI * 2f / 40; list.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (r + 0.02f)); }
        return list;
    }

    float ExpectedRadius(int r)
    {
        float radius = 0f;
        for (int i = 0; i <= r; i++)
        {
            float prev = rounds[i].intoRing ? RingRadius * scale - Step * scale : i == 0 ? startRadius : radius;
            int before = rounds[i].intoRing ? 0 : i == 0 ? startBases : rounds[i - 1].Stitches;
            radius = prev + Step * scale * (rounds[i].Stitches > before ? 1f : rounds[i].Stitches == before ? 0.4f : -1f);
        }
        return radius;
    }
}
