using System.Collections.Generic;
using UnityEngine;

/// Crochet recipe step: work the rounds, spiralling out from the magic ring. Each stitch is two
/// moves: tap the highlighted spot to hook a loop, then flick to pull it through. Each round has
/// six more stitches than the last, as in amigurumi. Hooking close to the spot makes a neat stitch.
/// Each finished stitch is saved as a point whose x is its neatness (0..1).
public class CrochetRoundsStep : PieceStep
{
    public const int Rounds = 3;
    const float Reach = 0.06f;          // how close the tap must be to hook
    const float MinFlick = 0.08f;       // how far a flick must travel

    readonly List<Vector2> slots = new List<Vector2>();
    readonly List<GameObject> made = new List<GameObject>();
    SpriteRenderer current, loop;
    bool hooked, flicking;
    float hookNeatness;
    Vector2 pressAt;

    int Done => State.points.Count;
    Color Yarn => Color.Lerp(area.fabricColor, Color.black, 0.15f);

    /// The spots for every stitch, round by round.
    public static List<Vector2> Slots()
    {
        var list = new List<Vector2>();
        for (int r = 1; r <= Rounds; r++)
            for (int k = 0; k < 6 * r; k++)
            {
                float a = Mathf.PI * 2f * k / (6 * r) + r * 0.35f;
                list.Add(new Vector2(0.5f, 0.5f) + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (0.1f * r));
            }
        return list;
    }

    protected override void Build()
    {
        slots.AddRange(Slots());
        Square(new Vector2(0.5f, 0.5f), 0.07f, Yarn, 1);   // the magic ring
        foreach (var s in slots) Square(s, 0.02f, new Color(1, 1, 1, 0.3f), 1);
        current = Square(Vector2.zero, 0.08f, new Color(1, 1, 1, 0.45f), 2);
        loop = Square(Vector2.zero, 0.035f, Color.white, 4);
        for (int i = 0; i < Done; i++) MakeStitch(i);
        Show();
    }

    void MakeStitch(int i) => made.Add(Square(slots[i], 0.07f + 0.02f * State.points[i].x, Yarn, 3).gameObject);

    void Show()
    {
        current.enabled = Done < slots.Count;
        loop.enabled = hooked && current.enabled;
        if (!current.enabled) return;
        current.transform.position = World(slots[Done]);
        loop.transform.position = World(slots[Done]);
    }

    protected override void OnPress(Vector2 p)
    {
        if (Done >= slots.Count) return;
        if (hooked) { flicking = true; pressAt = p; return; }
        float d = Vector2.Distance(p, slots[Done]);
        if (d > Reach) return;
        hooked = true;
        hookNeatness = 1f - Mathf.InverseLerp(0.015f, Reach, d);
        Show();
    }

    protected override void OnRelease(Vector2 p)
    {
        if (!flicking) return;
        flicking = false;
        if (Vector2.Distance(p, pressAt) < MinFlick) return;
        hooked = false;
        State.points.Add(new Vector2(hookNeatness, 0));
        MakeStitch(Done - 1);
        Show();
    }

    public override void Undo()
    {
        if (hooked) { hooked = false; Show(); return; }
        if (Done == 0) return;
        State.points.RemoveAt(Done - 1);
        Destroy(made[made.Count - 1]);
        made.RemoveAt(made.Count - 1);
        Show();
    }

    public override float SaveWork()
    {
        State.progress = Done / (float)slots.Count;
        if (Done == 0) return 0f;
        float sum = 0f;
        foreach (var p in State.points) sum += p.x;
        return State.progress * (0.5f + 0.5f * sum / Done);   // a loose stitch still holds
    }
}
