using System.Collections.Generic;
using UnityEngine;

/// Knitting recipe step: cast on, putting the first row of loops on the needle. Tap once per stitch.
public class CastOnStep : PieceStep
{
    KnitLayout knit;
    readonly List<GameObject> loops = new List<GameObject>();

    int Count => State.points.Count;

    protected override void Build()
    {
        knit = KnitLayout.For(order.target);
        float y = knit.origin.y - knit.cell * 0.2f;
        Line(new[] { new Vector2(knit.origin.x - 0.06f, y), new Vector2(knit.origin.x + knit.Size.x + 0.06f, y) }, new Color(0.75f, 0.65f, 0.5f), 0.015f, 1);   // the needle
        for (int x = 0; x < knit.width; x++) Square(Loop(x), knit.cell * 0.25f, new Color(1, 1, 1, 0.25f), 1);
        for (int i = 0; i < Count; i++) AddLoop(i);
    }

    Vector2 Loop(int x) => new Vector2(knit.origin.x + (x + 0.5f) * knit.cell, knit.origin.y - knit.cell * 0.2f);

    void AddLoop(int x) => loops.Add(Stitch(Loop(x), knit.cell * 0.8f, true, Color.Lerp(area.fabricColor, Color.black, 0.1f), 2, 90f));

    protected override void OnPress(Vector2 p)
    {
        if (Count >= knit.width) return;
        State.points.Add(Loop(Count));
        AddLoop(Count - 1);
    }

    public override void Undo()
    {
        if (Count == 0) return;
        State.points.RemoveAt(Count - 1);
        Destroy(loops[loops.Count - 1]);
        loops.RemoveAt(loops.Count - 1);
    }

    public override float SaveWork()
    {
        State.progress = Count / (float)knit.width;
        return State.progress;
    }
}
