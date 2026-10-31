using System.Collections.Generic;
using UnityEngine;

/// Knitting recipe step: cast on, putting the first row of loops on the needle. Tap once per stitch.
/// For sleeves, both cuffs are cast on.
public class CastOnStep : PieceStep
{
    List<int> first;
    readonly List<GameObject> loops = new List<GameObject>();

    int Count => State.points.Count;

    protected override void Build()
    {
        first = Knit.CastOnStitches();
        float y = Loop(0).y;
        Line(new[] { new Vector2(Knit.origin.x - 0.06f, y), new Vector2(Knit.origin.x + Knit.Size.x + 0.06f, y) }, new Color(0.75f, 0.65f, 0.5f), 0.015f, 1);   // the needle
        for (int i = 0; i < first.Count; i++) Square(Loop(i), Knit.cell * 0.25f, new Color(1, 1, 1, 0.25f), 1);
        for (int i = 0; i < Count; i++) AddLoop(i);
    }

    Vector2 Loop(int i) => Knit.CellCenter(first[i]) - new Vector2(0, Knit.cell * 0.7f);

    void AddLoop(int i) => loops.Add(Stitch(Loop(i), Knit.cell * 0.8f, true, Color.Lerp(area.fabricColor, Color.black, 0.1f), 2, 90f));

    protected override void OnPress(Vector2 p)
    {
        if (Count >= first.Count) return;
        State.points.Add(Loop(Count));
        AddLoop(Count - 1);
        var needles = Tool(CraftTool.Kind.KnittingNeedles, Knit.cell * 1.6f);
        needles.MoveTo(World(Loop(Count - 1)));
        needles.Poke();
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
        State.progress = Count / (float)first.Count;
        return State.progress;
    }
}
