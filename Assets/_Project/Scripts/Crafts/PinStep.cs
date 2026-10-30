using System.Collections.Generic;
using UnityEngine;

/// Recipe step: pin the two pieces together. Tap each marked spot around the edge to put a pin in.
public class PinStep : PieceStep
{
    const int PinCount = 10;
    const float Reach = 0.07f;          // how close a tap must be to a mark

    List<Vector2> marks;
    readonly List<GameObject> pins = new List<GameObject>();
    readonly List<SpriteRenderer> markViews = new List<SpriteRenderer>();

    protected override void Build()
    {
        Piece(Shape.Outline, Color.Lerp(area.fabricColor, Color.black, 0.2f), 0, new Vector2(0.012f, -0.012f));
        Piece(Shape.Outline, area.fabricColor, 0);
        marks = PieceShape.Resample(Shape.Outline, true, PinCount + 1);
        marks.RemoveAt(marks.Count - 1);
        foreach (var m in marks) markViews.Add(Square(m, 0.03f, new Color(1, 1, 1, 0.9f), 1));
        foreach (var p in State.points) AddPin(p);
    }

    protected override void OnPress(Vector2 p)
    {
        int best = -1;
        for (int i = 0; i < marks.Count; i++)
            if (!State.points.Exists(q => Same(q, marks[i])) && Vector2.Distance(p, marks[i]) <= Reach && (best < 0 || Vector2.Distance(p, marks[i]) < Vector2.Distance(p, marks[best])))
                best = i;
        if (best < 0) return;
        State.points.Add(marks[best]);
        AddPin(marks[best]);
    }

    void AddPin(Vector2 at)
    {
        var pin = new GameObject("Pin");
        pin.transform.SetParent(transform, false);
        Line(new[] { at, at + new Vector2(0.03f, 0.06f) }, new Color(0.75f, 0.75f, 0.8f), 0.007f, 3).transform.SetParent(pin.transform, true);
        Square(at + new Vector2(0.03f, 0.06f), 0.025f, new Color(0.85f, 0.2f, 0.3f), 4).transform.SetParent(pin.transform, true);
        pins.Add(pin);
        int i = marks.FindIndex(m => Same(m, at));
        if (i >= 0) markViews[i].enabled = false;
    }

    static bool Same(Vector2 a, Vector2 b) => (a - b).sqrMagnitude < 1e-6f;   // saved points round-trip through JSON

    public override void Undo()
    {
        if (State.points.Count == 0) return;
        var last = State.points[State.points.Count - 1];
        State.points.RemoveAt(State.points.Count - 1);
        Destroy(pins[pins.Count - 1]);
        pins.RemoveAt(pins.Count - 1);
        int i = marks.FindIndex(m => Same(m, last));
        if (i >= 0) markViews[i].enabled = true;
    }

    public override float SaveWork()
    {
        State.progress = Mathf.Clamp01(State.points.Count / (float)marks.Count);
        return State.progress;
    }
}
