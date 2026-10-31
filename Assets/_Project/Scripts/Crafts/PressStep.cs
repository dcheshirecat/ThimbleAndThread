using System.Collections.Generic;
using UnityEngine;

/// Finishing step: press the piece with the iron (or block a knit with steam). Drag the iron all over it until
/// the creases are gone. Like real ironing (owner, 2026-10-31: deeper mini-games):
///  - The piece's care label says what heat it takes: acrylic, felt and fleece a cool iron (they melt), wool a
///    medium steam, cotton and linen a hot iron. Tap the dial under the work to set the heat.
///  - Too cool: the creases need several passes (far too cool, a cool iron on cotton, never gets them out). Too hot for the fabric: it scorches as you go. Holding the hot
///    iron still on one spot scorches anything. Scorches are mistakes; Undo dabs the last one off.
/// Saved points: (x, y) where the iron went; a scorch is (x - 2, y); the heat setting is a point (-10 - heat, -10).
public class PressStep : PieceStep, IMistakes
{
    const int Grid = 8;
    const float IronSize = 0.16f;
    const float DwellScorch = 1.2f;     // seconds holding still before it scorches
    const int TooHotEvery = 12;         // moves at too high a heat between scorches
    static readonly string[] HeatNames = { "Cool", "Warm", "Hot" };

    readonly List<Vector2> spots = new List<Vector2>();
    int[] passes;
    readonly List<(LineRenderer line, Vector2 a, Vector2 b)> creases = new List<(LineRenderer, Vector2, Vector2)>();
    readonly List<GameObject> scorchMarks = new List<GameObject>();
    readonly List<SpriteRenderer> dial = new List<SpriteRenderer>();
    SpriteRenderer iron;
    int heat = 1, right;                // the iron's setting; the fabric's right setting (0 cool, 1 warm, 2 hot)
    int movesAtHeat;
    float stillSince = -1f;
    Vector2 lastAt;
    string warning;

    /// The heat a fabric or yarn takes, from what it's made of.
    public static int RightHeat(SupplyDef s)
    {
        string id = s != null ? s.supplyId : "";
        if (id.Contains("acrylic") || id.Contains("felt") || id.Contains("fleece") || id.Contains("minky")) return 0;
        if (id.Contains("wool") || id.Contains("merino") || id.Contains("alpaca") || id.Contains("mohair")) return 1;
        return 2;   // cotton, linen, canvas, Aida
    }

    static string CareLabel(int heat) => heat switch
    {
        0 => "Care: COOL iron only (it melts)",
        1 => "Care: WARM iron, with steam",
        _ => "Care: HOT iron",
    };

    protected override void Build()
    {
        right = RightHeat(area.content != null ? area.content.Supply(order.fabricId) : null);
        Square(new Vector2(0.5f, 0.52f), 0.76f, area.fabricColor, 0);
        Label(new Vector2(0.5f, 0.95f), CareLabel(right), 0.034f, Color.white, 5);
        for (int x = 0; x < Grid; x++)
            for (int y = 0; y < Grid; y++) spots.Add(new Vector2(0.12f + (x + 0.5f) * 0.76f / Grid, 0.14f + (y + 0.5f) * 0.76f / Grid));
        passes = new int[spots.Count];
        var rng = new System.Random(order.id != null ? order.id.GetHashCode() : 1);   // the same creases every time
        for (int i = 0; i < 6; i++)
        {
            var a = new Vector2(0.14f + (float)rng.NextDouble() * 0.72f, 0.16f + (float)rng.NextDouble() * 0.72f);
            var b = a + new Vector2((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f) * 0.4f;
            b = Vector2.Min(Vector2.Max(b, new Vector2(0.14f, 0.16f)), new Vector2(0.86f, 0.88f));
            creases.Add((Line(new[] { a, b }, Color.Lerp(area.fabricColor, Color.black, 0.3f), 0.008f, 1), a, b));
        }
        // The heat dial: three settings under the work
        for (int h = 0; h < 3; h++)
        {
            dial.Add(Square(DialSpot(h), 0.07f, Color.white, 4));
            Label(DialSpot(h) + Vector2.down * 0.065f, HeatNames[h], 0.03f, Color.white, 4);
        }
        iron = Square(new Vector2(0.5f, -1f), IronSize, new Color(0.75f, 0.78f, 0.82f), 3);
        // Replay the saved work: heat changes, passes and scorches
        if (State.points.Count > 0 && !State.points.Exists(q => q.y <= -9f)) heat = right;   // pressed before the dial existed
        foreach (var p in State.points)
        {
            if (p.y <= -9f) heat = Mathf.Clamp(Mathf.RoundToInt(-p.x - 10f), 0, 2);
            else if (p.x < -0.5f) scorchMarks.Add(Scorch(new Vector2(p.x + 2f, p.y)));
            else PressAt(p);
        }
        ShowDial();
        Refresh();
    }

    static Vector2 DialSpot(int h) => new Vector2(0.62f + h * 0.13f, 0.06f);

    GameObject Scorch(Vector2 at) => Dot(at, 0.045f, new Color(0.45f, 0.30f, 0.15f, 0.85f), 2);

    void ShowDial()
    {
        var colours = new[] { new Color(0.55f, 0.75f, 0.95f), new Color(0.98f, 0.75f, 0.35f), new Color(0.95f, 0.35f, 0.25f) };
        for (int h = 0; h < 3; h++) dial[h].color = h == heat ? colours[h] : new Color(1, 1, 1, 0.3f);
    }

    public override string LiveHint =>
        warning ?? $"Iron: {HeatNames[heat].ToLower()}. " + (heat < right ? "The creases are stubborn at this heat." : "Keep it moving.");

    protected override void OnPress(Vector2 p)
    {
        for (int h = 0; h < 3; h++)
            if (Vector2.Distance(p, DialSpot(h)) < 0.06f)   // the dial
            {
                if (h != heat) { heat = h; movesAtHeat = 0; State.points.Add(new Vector2(-10f - h, -10f)); ShowDial(); warning = null; }
                return;
            }
        stillSince = Time.realtimeSinceStartup;
        lastAt = p;
        Iron(p);
    }

    protected override void OnDrag(Vector2 p)
    {
        if (stillSince < 0) return;
        if ((p - lastAt).sqrMagnitude > 0.015f * 0.015f) { stillSince = Time.realtimeSinceStartup; lastAt = p; }
        else if (heat >= 1 && Time.realtimeSinceStartup - stillSince > DwellScorch)   // left sitting on the fabric
        {
            AddScorch(p, "You held the iron still too long: a scorch mark. Undo dabs it off.");
            stillSince = Time.realtimeSinceStartup;
        }
        Iron(p);
    }

    protected override void OnRelease(Vector2 p)
    {
        stillSince = -1f;
        iron.transform.position = World(new Vector2(0.5f, -1f));   // put it down out of the way
    }

    void Iron(Vector2 p)
    {
        iron.transform.position = World(p);
        if (p.y < 0.12f) return;   // off the piece
        var last = LastPress();
        if (last.HasValue && (last.Value - p).sqrMagnitude < 0.02f * 0.02f) return;
        State.points.Add(p);
        PressAt(p);
        if (heat > right && ++movesAtHeat % TooHotEvery == 1)   // too hot for this fabric: it scorches as you go
            AddScorch(p, $"Too hot for this fabric! Check the care label and turn the iron down.");
        Refresh();
    }

    Vector2? LastPress()
    {
        for (int i = State.points.Count - 1; i >= 0; i--)
            if (State.points[i].x >= -0.5f && State.points[i].y > -9f) return State.points[i];
        return null;
    }

    void AddScorch(Vector2 at, string why)
    {
        State.points.Add(new Vector2(at.x - 2f, at.y));
        scorchMarks.Add(Scorch(at));
        warning = why;
        AudioManager.Play(Sfx.Click);
    }

    void PressAt(Vector2 p)
    {
        for (int i = 0; i < spots.Count; i++)
            if (Mathf.Abs(spots[i].x - p.x) <= IronSize / 2 + 0.03f && Mathf.Abs(spots[i].y - p.y) <= IronSize / 2 + 0.03f)
                passes[i] = Mathf.Min(passes[i] + (heat < right ? 1 : 3), heat <= right - 2 ? 2 : 3);   // far too cool never gets them out
    }

    bool Pressed(int i) => passes[i] >= 3;

    /// Each crease fades with the share of the fabric around it that's been pressed.
    void Refresh()
    {
        foreach (var (line, a, b) in creases)
        {
            int near = 0, done = 0;
            for (int i = 0; i < spots.Count; i++)
                if (PieceShape.DistanceToSegment(spots[i], a, b) < 0.1f) { near++; if (Pressed(i)) done++; }
            float left = near > 0 ? 1f - done / (float)near : 0f;
            var c = Color.Lerp(area.fabricColor, Color.black, 0.3f);
            c.a = left;
            line.startColor = line.endColor = c;
        }
    }

    /// Dabs off the last scorch (pressing itself can't be undone).
    public override void Undo()
    {
        for (int i = State.points.Count - 1; i >= 0; i--)
        {
            var p = State.points[i];
            if (p.x < -0.5f && p.y > -9f)
            {
                State.points.RemoveAt(i);
                if (scorchMarks.Count > 0) { Destroy(scorchMarks[scorchMarks.Count - 1]); scorchMarks.RemoveAt(scorchMarks.Count - 1); }
                warning = null;
                return;
            }
        }
    }

    public string MistakesLeft => scorchMarks.Count == 0 ? null : scorchMarks.Count == 1 ? "a scorch mark" : $"{scorchMarks.Count} scorch marks";

    public override float SaveWork()
    {
        int done = 0;
        for (int i = 0; i < passes.Length; i++) if (Pressed(i)) done++;
        State.progress = done / (float)passes.Length;
        State.mistakes = scorchMarks.Count;
        return State.progress * Mathf.Clamp01(1f - 0.15f * scorchMarks.Count);
    }
}
