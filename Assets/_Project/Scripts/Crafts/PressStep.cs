using System.Collections.Generic;
using UnityEngine;

/// Finishing step: press the piece with the iron. Drag the iron all over it until the creases are gone.
/// Creases fade as the iron passes over them; the score is how much of the piece got pressed.
public class PressStep : PieceStep
{
    const int Grid = 8;
    const float IronSize = 0.16f;

    readonly List<Vector2> spots = new List<Vector2>();
    bool[] pressed;
    readonly List<(LineRenderer line, Vector2 a, Vector2 b)> creases = new List<(LineRenderer, Vector2, Vector2)>();
    SpriteRenderer iron;

    protected override void Build()
    {
        Square(new Vector2(0.5f, 0.5f), 0.8f, area.fabricColor, 0);
        for (int x = 0; x < Grid; x++)
            for (int y = 0; y < Grid; y++) spots.Add(new Vector2(0.1f + (x + 0.5f) * 0.8f / Grid, 0.1f + (y + 0.5f) * 0.8f / Grid));
        pressed = new bool[spots.Count];
        var rng = new System.Random(order.id != null ? order.id.GetHashCode() : 1);   // the same creases every time
        for (int i = 0; i < 6; i++)
        {
            var a = new Vector2(0.12f + (float)rng.NextDouble() * 0.76f, 0.12f + (float)rng.NextDouble() * 0.76f);
            var b = a + new Vector2((float)rng.NextDouble() - 0.5f, (float)rng.NextDouble() - 0.5f) * 0.4f;
            b = Vector2.Min(Vector2.Max(b, Vector2.one * 0.12f), Vector2.one * 0.88f);
            creases.Add((Line(new[] { a, b }, Color.Lerp(area.fabricColor, Color.black, 0.3f), 0.008f, 1), a, b));
        }
        iron = Square(new Vector2(0.5f, -1f), IronSize, new Color(0.75f, 0.78f, 0.82f), 3);
        foreach (var p in State.points) PressAt(p);
        Refresh();
    }

    protected override void OnPress(Vector2 p) => Iron(p);
    protected override void OnDrag(Vector2 p) => Iron(p);
    protected override void OnRelease(Vector2 p) => iron.transform.position = World(new Vector2(0.5f, -1f));   // put it down out of the way

    void Iron(Vector2 p)
    {
        iron.transform.position = World(p);
        if (State.points.Count > 0 && (State.points[State.points.Count - 1] - p).sqrMagnitude < 0.02f * 0.02f) return;
        State.points.Add(p);
        PressAt(p);
        Refresh();
    }

    void PressAt(Vector2 p)
    {
        for (int i = 0; i < spots.Count; i++)
            if (!pressed[i] && Mathf.Abs(spots[i].x - p.x) <= IronSize / 2 + 0.03f && Mathf.Abs(spots[i].y - p.y) <= IronSize / 2 + 0.03f)
                pressed[i] = true;
    }

    /// Each crease fades with the share of the fabric around it that's been pressed.
    void Refresh()
    {
        foreach (var (line, a, b) in creases)
        {
            int near = 0, done = 0;
            for (int i = 0; i < spots.Count; i++)
                if (PieceShape.DistanceToSegment(spots[i], a, b) < 0.1f) { near++; if (pressed[i]) done++; }
            float left = near > 0 ? 1f - done / (float)near : 0f;
            var c = Color.Lerp(area.fabricColor, Color.black, 0.3f);
            c.a = left;
            line.startColor = line.endColor = c;
        }
    }

    public override void Undo() { }   // there's no un-pressing

    public override float SaveWork()
    {
        int done = 0;
        foreach (bool b in pressed) if (b) done++;
        State.progress = done / (float)pressed.Length;
        return State.progress;
    }
}
