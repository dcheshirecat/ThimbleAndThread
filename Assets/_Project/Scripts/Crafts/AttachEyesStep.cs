using UnityEngine;

/// Recipe step: put the safety eyes in. Tap where each eye goes; the faint marks come from the
/// pattern (squares in its second colour). Closer to the marks is better.
public class AttachEyesStep : PieceStep
{
    public const float EyeSize = 0.05f;
    public static readonly Color EyeColor = new Color(0.08f, 0.08f, 0.1f);
    const float Perfect = 0.02f, Miss = 0.12f;   // distance for a perfect eye, and for no credit

    readonly System.Collections.Generic.List<GameObject> eyes = new System.Collections.Generic.List<GameObject>();

    protected override void Build()
    {
        Piece(Shape.Outline, area.fabricColor, 0);
        Line(Shape.Seam, Color.Lerp(area.fabricColor, Color.black, 0.35f), 0.006f, 1);
        foreach (var m in Shape.Eyes) Square(m, EyeSize, new Color(0, 0, 0, 0.18f), 1);
        foreach (var p in State.points) eyes.Add(Square(p, EyeSize, EyeColor, 2).gameObject);
    }

    protected override void OnPress(Vector2 p)
    {
        if (State.points.Count >= Shape.Eyes.Count || !PieceShape.Inside(Shape.Outline, p)) return;
        State.points.Add(p);
        eyes.Add(Square(p, EyeSize, EyeColor, 2).gameObject);
    }

    public override void Undo()
    {
        if (State.points.Count == 0) return;
        State.points.RemoveAt(State.points.Count - 1);
        Destroy(eyes[eyes.Count - 1]);
        eyes.RemoveAt(eyes.Count - 1);
    }

    /// Each mark scores by how close the nearest eye is to it.
    public override float SaveWork()
    {
        State.progress = Mathf.Clamp01(State.points.Count / (float)Shape.Eyes.Count);
        float sum = 0f;
        foreach (var m in Shape.Eyes)
        {
            float best = float.MaxValue;
            foreach (var e in State.points) best = Mathf.Min(best, Vector2.Distance(m, e));
            sum += 1f - Mathf.InverseLerp(Perfect, Miss, best);
        }
        return sum / Shape.Eyes.Count;
    }
}
