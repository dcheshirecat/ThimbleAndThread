using System.Collections.Generic;
using UnityEngine;

/// Recipe step: stuff the piece. Tap all over it to push stuffing in; fill it evenly, without
/// overstuffing one spot.
public class StuffStep : PieceStep
{
    const float Reach = 0.12f;          // how far one tap of stuffing spreads
    const int Overstuffed = 4;          // taps on one spot that make it lumpy

    readonly List<Vector2> spots = new List<Vector2>();   // checkpoints inside the piece
    readonly List<GameObject> puffs = new List<GameObject>();

    /// Where stuffing can go: the piece's shape (a crocheted part's ball in CrochetStuffStep).
    protected virtual List<Vector2> Region => Shape.Outline;

    protected virtual void DrawBase()
    {
        Piece(Shape.Outline, area.fabricColor, 0);
        Line(Shape.Seam, Color.Lerp(area.fabricColor, Color.black, 0.35f), 0.006f, 1);
    }

    protected override void Build()
    {
        DrawBase();
        PieceShape.Bounds(Region, out var min, out var max);
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
            {
                var p = new Vector2(Mathf.Lerp(min.x, max.x, (x + 0.5f) / 10f), Mathf.Lerp(min.y, max.y, (y + 0.5f) / 10f));
                if (PieceShape.Inside(Region, p)) spots.Add(p);
            }
        foreach (var p in State.points) Puff(p);
    }

    protected override void OnPress(Vector2 p)
    {
        if (!PieceShape.Inside(Region, p)) return;
        State.points.Add(p);
        Puff(p);
    }

    void Puff(Vector2 p) => puffs.Add(Dot(p, 0.1f, new Color(1, 1, 1, 0.45f), 2));

    public override void Undo()
    {
        if (State.points.Count == 0) return;
        State.points.RemoveAt(State.points.Count - 1);
        Destroy(puffs[puffs.Count - 1]);
        puffs.RemoveAt(puffs.Count - 1);
    }

    /// Share of the piece that's filled, lowered by lumpy (overstuffed) spots.
    public override float SaveWork()
    {
        if (spots.Count == 0) return 0f;
        int filled = 0, lumpy = 0;
        foreach (var s in spots)
        {
            int taps = State.points.FindAll(p => (p - s).sqrMagnitude <= Reach * Reach).Count;
            if (taps > 0) filled++;
            if (taps >= Overstuffed) lumpy++;
        }
        State.progress = filled / (float)spots.Count;
        return State.progress * (1f - 0.5f * lumpy / spots.Count);
    }
}
