using System.Collections.Generic;
using UnityEngine;

/// The tablecloth's hem, done the real way (owner, 2026-11-03: not another line to trace). After the corners are mitred,
/// each edge gets a double-fold hem: fold the raw edge under a little, fold it over again to the hem's width, then slip
/// stitch the folded edge down with small, evenly spaced stitches.
/// - Folding: grab an edge and drag it inward; the strip folds over with its crease halfway between the edge and your
///   finger. Let go when the crease sits on the dashed line. The cloth gets smaller as it folds, as real cloth does.
/// - Stitching: once an edge is folded twice, tap along its folded edge to make each stitch (Stitches per edge),
///   spacing them evenly. A crease well off its line, a stitch off the fold, or stitches bunched up are mistakes.
/// Points: a fold is (100 + 10 x edge + fold, finger depth); a stitch is (1000 x (edge + 1) + along, depth tapped).
/// Older traced hem points (below 100) are ignored, so a hem begun the old way starts again.
public class HemFoldStep : PieceStep, IMistakes
{
    public const int Stitches = 6;
    const float Lo = 0.08f, Hi = 0.92f, S = Hi - Lo;
    const float TurnUnder = 0.04f, HemWidth = 0.06f;   // where the first crease goes, and how far in the second goes from it
    const float Grab = 0.06f, StitchReach = 0.04f;
    static readonly Color Table = new Color(0.30f, 0.26f, 0.32f);

    /// The tablecloth's hem plays as folding; other items' Hem steps (cushion backing, quilt binding...) stay as they are.
    public static bool Applies(Order o) => o != null && o.item != null && o.item.itemId == "tablecloth" && o.CurrentStep.kind == StepKind.Hem;
    public const string Hint = "Fold each edge under twice, then stitch it down. Grab an edge and drag it inward until the crease sits on the dashed line; " +
                               "fold it again to the next line; then tap along the folded edge to make six small stitches, evenly spaced.";

    int held = -1;            // the edge being folded
    float hand;               // how far in the finger is, for that edge
    readonly List<GameObject> drawn = new List<GameObject>();

    // ---- Geometry: each edge has a depth (inward from the cloth's edge) and an "along" (0..1 across the cloth) ----

    static float DepthOf(int e, Vector2 p) => e == 0 ? p.y - Lo : e == 1 ? Hi - p.x : e == 2 ? Hi - p.y : p.x - Lo;
    static float AlongOf(int e, Vector2 p) => (e == 0 || e == 2 ? p.x - Lo : p.y - Lo) / S;
    static Vector2 At(int e, float depth, float along)
    {
        float a = Lo + along * S;
        return e == 0 ? new Vector2(a, Lo + depth) : e == 1 ? new Vector2(Hi - depth, a) : e == 2 ? new Vector2(a, Hi - depth) : new Vector2(Lo + depth, a);
    }

    /// A band along an edge between two depths, cut at 45 degrees at the corners (they're mitred).
    static List<Vector2> Band(int e, float d0, float d1)
    {
        var band = new List<Vector2> { At(e, d0, d0 / S), At(e, d0, 1 - d0 / S), At(e, d1, 1 - d1 / S), At(e, d1, d1 / S) };
        float area = 0;
        for (int i = 0; i < band.Count; i++) { var p = band[i]; var q = band[(i + 1) % band.Count]; area += p.x * q.y - q.x * p.y; }
        if (area < 0) band.Reverse();   // the same way round for every edge, or the mesh faces away and doesn't show
        return band;
    }

    struct Edge
    {
        public List<float> folds;                 // finger depths of the folds made
        public List<Vector2> stitches;            // (along, depth tapped)
        public float Crease1 => folds.Count > 0 ? folds[0] / 2f : 0f;
        public float Crease2 => folds.Count > 1 ? (Crease1 + folds[1]) / 2f : Crease1;
        public float Outer => folds.Count == 0 ? 0f : folds.Count == 1 ? Crease1 : Crease2;   // the cloth's edge now
        public float Inner => folds.Count == 0 ? 0f : folds.Count == 1 ? folds[0] : folds[1]; // the folded edge, to stitch
        public float Target => folds.Count == 0 ? TurnUnder : Crease1 + HemWidth;            // where the next crease should go
    }

    Edge[] Edges()
    {
        var edges = new Edge[4];
        for (int e = 0; e < 4; e++) edges[e] = new Edge { folds = new List<float>(), stitches = new List<Vector2>() };
        foreach (var p in State.points)
        {
            if (p.x >= 1000) { int e = Mathf.FloorToInt(p.x / 1000f) - 1; if (e >= 0 && e < 4) edges[e].stitches.Add(new Vector2(p.x - 1000 * (e + 1), p.y)); }
            else if (p.x >= 100) { int e = Mathf.FloorToInt((p.x - 100) / 10f); if (e >= 0 && e < 4 && edges[e].folds.Count < 2) edges[e].folds.Add(p.y); }
        }
        return edges;
    }

    static float FoldAccuracy(float crease, float target) => Mathf.Clamp01(1f - Mathf.Abs(crease - target) / 0.025f);
    static float StitchAccuracy(Edge e, Vector2 s) => Mathf.Clamp01(1f - Mathf.Abs(s.y - e.Inner) / 0.03f);

    /// How evenly an edge's stitches are spaced: 1 = perfectly even.
    static float Evenness(Edge e)
    {
        if (e.stitches.Count < 3) return 1f;
        var along = new List<float>();
        foreach (var s in e.stitches) along.Add(s.x);
        along.Sort();
        float mean = (along[along.Count - 1] - along[0]) / (along.Count - 1);
        if (mean <= 0.001f) return 0f;
        float var = 0;
        for (int i = 1; i < along.Count; i++) { float g = along[i] - along[i - 1] - mean; var += g * g; }
        float cv = Mathf.Sqrt(var / (along.Count - 1)) / mean;
        return Mathf.Clamp01(1f - cv * 1.5f);
    }

    // ---- Drawing ----

    protected override void Build() => Redraw();

    void Redraw()
    {
        foreach (var g in drawn) if (g != null) Destroy(g);
        drawn.Clear();
        var edges = Edges();
        float Out(int e) => e == held ? (edges[e].Outer + hand) / 2f : edges[e].Outer;
        drawn.Add(Square(Vector2.one * 0.5f, S, Table, 0).gameObject);   // the table shows where the cloth has folded in
        // The cloth, smaller by however far each edge has folded
        drawn.Add(Piece(new List<Vector2> { new Vector2(Lo + Out(3), Lo + Out(0)), new Vector2(Hi - Out(1), Lo + Out(0)),
                                            new Vector2(Hi - Out(1), Hi - Out(2)), new Vector2(Lo + Out(3), Hi - Out(2)) }, area.fabricColor, 1).gameObject);
        if (order.target != null)   // the stitched motif, in the middle
            foreach (var c in order.target.cells)
            {
                var at = PieceShape.SquareCenter(order.target, 0.3f, c.x, c.y, out float size);
                drawn.Add(Square(at, size * 0.92f, c.colorIndex < order.target.palette.Count ? order.target.palette[c.colorIndex] : Color.gray, 2).gameObject);
            }
        var under = Color.Lerp(area.fabricColor, Color.black, 0.12f);
        var crease = Color.Lerp(area.fabricColor, Color.black, 0.4f);
        var thread = Color.Lerp(area.fabricColor, Color.black, 0.55f);
        for (int e = 0; e < 4; e++)
        {
            var ed = edges[e];
            float outer = Out(e), inner = e == held ? hand : ed.Inner;
            if (inner > outer + 0.001f)   // the folded-over layers
            {
                drawn.Add(Piece(Band(e, outer, inner), under, 3).gameObject);
                drawn.Add(Line(new List<Vector2> { At(e, inner, inner / S), At(e, inner, 1 - inner / S) }, crease, 0.004f, 4).gameObject);   // the folded edge
            }
            if (ed.folds.Count < 2)   // where the next crease should go
            {
                float target = ed.Target;
                var line = Line(new List<Vector2> { At(e, target, target / S), At(e, target, 1 - target / S) },
                                e == held ? (FoldAccuracy(Out(e), target) > 0.6f ? new Color(0.3f, 0.8f, 0.4f, 0.9f) : new Color(0.9f, 0.5f, 0.3f, 0.9f))
                                          : new Color(0.2f, 0.15f, 0.1f, 0.7f), 0.005f, 5);
                MakeDashed(line, 0.02f);
                drawn.Add(line.gameObject);
                if (e != held) drawn.Add(Dot(At(e, outer, 0.5f), 0.022f, new Color(1f, 0.9f, 0.4f), 6));   // grab here
            }
            foreach (var s in ed.stitches)   // little slanted slip stitches catching the folded edge
                drawn.Add(Line(new List<Vector2> { At(e, s.y - 0.012f, s.x - 0.006f), At(e, s.y + 0.012f, s.x + 0.006f) }, thread, 0.006f, 6).gameObject);
        }
    }

    public override string LiveHint
    {
        get
        {
            var edges = Edges();
            if (held >= 0) return FoldAccuracy((edges[held].Outer + hand) / 2f, edges[held].Target) > 0.6f ? "That's on the line: let go." : "Keep going: bring the crease onto the dashed line.";
            int folds = 0, stitches = 0;
            foreach (var e in edges) { folds += e.folds.Count; stitches += Mathf.Min(Stitches, e.stitches.Count); }
            if (folds < 8) return $"Folds: {folds} of 8. Grab a gold edge and fold it in to the dashed line" + (folds >= 4 ? ", then fold it again." : ".");
            if (stitches < 4 * Stitches) return $"Stitches: {stitches} of {4 * Stitches}. Tap along each folded edge, evenly spaced.";
            return "The hem's done all the way round. Tap Finish.";
        }
    }

    // ---- Touch ----

    protected override void OnPress(Vector2 p)
    {
        var edges = Edges();
        held = -1;
        float best = Grab;
        for (int e = 0; e < 4; e++)
        {
            if (edges[e].folds.Count >= 2) continue;
            float along = AlongOf(e, p), d = Mathf.Abs(DepthOf(e, p) - edges[e].Outer);
            if (along < 0.05f || along > 0.95f || d >= best) continue;
            best = d; held = e;
        }
        if (held >= 0) { hand = DepthOf(held, p); Redraw(); return; }
        // Not an edge to fold: a stitch on a folded edge, if it's near one
        int which = -1;
        best = StitchReach;
        for (int e = 0; e < 4; e++)
        {
            var ed = edges[e];
            if (ed.folds.Count < 2 || ed.stitches.Count >= Stitches) continue;
            float along = AlongOf(e, p), d = Mathf.Abs(DepthOf(e, p) - ed.Inner);
            if (along < ed.Inner / S || along > 1 - ed.Inner / S || d >= best) continue;
            best = d; which = e;
        }
        if (which < 0) return;
        State.points.Add(new Vector2(1000 * (which + 1) + Mathf.Clamp01(AlongOf(which, p)), DepthOf(which, p)));
        AudioManager.Play(Sfx.Stitch);
        Redraw();
    }

    protected override void OnDrag(Vector2 p)
    {
        if (held < 0) return;
        hand = DepthOf(held, p);
        Redraw();
    }

    protected override void OnRelease(Vector2 p)
    {
        if (held < 0) return;
        int e = held;
        held = -1;
        float depth = Mathf.Clamp(DepthOf(e, p), 0f, 0.3f);
        var ed = Edges()[e];
        if (depth > ed.Outer + 0.015f)   // it really folded
        {
            State.points.Add(new Vector2(100 + 10 * e + ed.folds.Count, depth));
            AudioManager.Play(Sfx.Stitch);
        }
        Redraw();
    }

    public override void Undo()
    {
        for (int k = State.points.Count - 1; k >= 0; k--)
            if (State.points[k].x >= 100) { State.points.RemoveAt(k); break; }
        Redraw();
    }

    // ---- Judging ----

    public string MistakesLeft
    {
        get
        {
            var edges = Edges();
            int offFolds = 0, offStitches = 0, uneven = 0;
            foreach (var e in edges)
            {
                if (e.folds.Count > 0 && FoldAccuracy(e.Crease1, TurnUnder) < 0.5f) offFolds++;
                if (e.folds.Count > 1 && FoldAccuracy(e.Crease2, e.Crease1 + HemWidth) < 0.5f) offFolds++;
                foreach (var s in e.stitches) if (StitchAccuracy(e, s) < 0.4f) offStitches++;
                if (e.stitches.Count >= Stitches && Evenness(e) < 0.5f) uneven++;
            }
            var list = new List<string>();
            if (offFolds > 0) list.Add(offFolds == 1 ? "1 fold off its line" : $"{offFolds} folds off their lines");
            if (offStitches > 0) list.Add(offStitches == 1 ? "1 stitch that missed the folded edge" : $"{offStitches} stitches that missed the folded edge");
            if (uneven > 0) list.Add(uneven == 1 ? "1 edge with bunched-up stitches" : $"{uneven} edges with bunched-up stitches");
            return list.Count == 0 ? null : string.Join(", ", list);
        }
    }

    public override float SaveWork()
    {
        var edges = Edges();
        int folds = 0, stitches = 0;
        float foldScore = 0, stitchScore = 0, even = 0;
        foreach (var e in edges)
        {
            folds += e.folds.Count;
            if (e.folds.Count > 0) foldScore += FoldAccuracy(e.Crease1, TurnUnder);
            if (e.folds.Count > 1) foldScore += FoldAccuracy(e.Crease2, e.Crease1 + HemWidth);
            int n = Mathf.Min(Stitches, e.stitches.Count);
            stitches += n;
            for (int i = 0; i < n; i++) stitchScore += StitchAccuracy(e, e.stitches[i]);
            even += Evenness(e);
        }
        State.progress = (folds + stitches) / (8f + 4 * Stitches);
        if (folds + stitches == 0) return 0f;
        float quality = 0.4f * (folds > 0 ? foldScore / folds : 1f) + 0.3f * (stitches > 0 ? stitchScore / stitches : 1f) + 0.3f * (even / 4f);
        return State.progress * (0.3f + 0.7f * quality);
    }
}
