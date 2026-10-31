using System.Collections.Generic;
using UnityEngine;

/// Base for steps where the player taps where things go (eyes, handle ends, buttons...).
/// Faint marks show the right spots; each tap places one thing, up to one per mark, and each mark
/// scores by how close the nearest placed thing is to it.
public abstract class PlacementStep : PieceStep, IMistakes
{
    const float Perfect = 0.02f, Miss = 0.12f;   // distance for full credit, and for none

    readonly List<GameObject> placed = new List<GameObject>();

    /// Where things should go, in 0..1 of the area.
    protected abstract List<Vector2> Marks { get; }
    /// Draws the piece being worked on.
    protected abstract void DrawPiece();
    /// Draws one placed thing.
    protected abstract GameObject DrawPlaced(Vector2 p);
    /// Called after anything is placed or undone, e.g. to redraw a handle between its two ends.
    protected virtual void Changed() { }
    /// Whether a tap here may place something.
    protected virtual bool CanPlace(Vector2 p) => true;

    protected List<Vector2> Placed => State.points;
    /// The faint marks' colour; light work needs dark marks and the other way round.
    protected virtual Color MarkColor => new Color(0, 0, 0, 0.18f);

    protected override void Build()
    {
        DrawPiece();
        foreach (var m in Marks) Dot(m, 0.06f, MarkColor, 4);
        foreach (var p in Placed) placed.Add(DrawPlaced(p));
        Changed();
    }

    protected override void OnPress(Vector2 p) => Place(p);

    /// Places one thing at p, if there's still something to place and p is allowed.
    protected void Place(Vector2 p)
    {
        if (Placed.Count >= Marks.Count || !CanPlace(p)) return;
        Placed.Add(p);
        placed.Add(DrawPlaced(p));
        Changed();
    }

    public override void Undo()
    {
        if (Placed.Count == 0) return;
        Placed.RemoveAt(Placed.Count - 1);
        Destroy(placed[placed.Count - 1]);
        placed.RemoveAt(placed.Count - 1);
        Changed();
    }

    public string MistakesLeft
    {
        get
        {
            int n = 0;
            foreach (var e in Placed)
            {
                float best = float.MaxValue;
                foreach (var m in Marks) best = Mathf.Min(best, Vector2.Distance(m, e));
                if (best > Miss * 0.5f) n++;
            }
            return n == 0 ? null : n == 1 ? "1 in the wrong place" : $"{n} in the wrong places";
        }
    }

    public override float SaveWork()
    {
        State.progress = Mathf.Clamp01(Placed.Count / (float)Marks.Count);
        float sum = 0f;
        foreach (var m in Marks)
        {
            float best = float.MaxValue;
            foreach (var e in Placed) best = Mathf.Min(best, Vector2.Distance(m, e));
            sum += 1f - Mathf.InverseLerp(Perfect * (1f + Assist), Miss, best);
        }
        return Marks.Count > 0 ? sum / Marks.Count : 1f;
    }
}
