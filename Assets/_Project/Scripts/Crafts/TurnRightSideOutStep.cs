using UnityEngine;

/// Recipe step: turn the sewn piece right side out through the gap. Drag from inside the piece out
/// through the opening; each pull turns more of it, and the fabric's right side shows.
public class TurnRightSideOutStep : PieceStep
{
    const int PullsNeeded = 4;
    const float OpeningReach = 0.1f;    // how close to the opening a pull must pass

    MeshRenderer piece;
    bool pullingFromInside;
    float closestToOpening;

    Color InsideOut => Color.Lerp(area.fabricColor, new Color(0.55f, 0.5f, 0.5f), 0.6f);
    int Pulls => State.points.Count;

    protected override void Build()
    {
        piece = Piece(Shape.Outline, InsideOut, 0);
        Line(Shape.Seam, Color.Lerp(area.fabricColor, Color.black, 0.35f), 0.006f, 1);
        Dashes(PieceShape.Resample(Shape.Opening, false, 9), Color.white, 0.012f, 2);   // the gap
        Show();
    }

    protected override void OnPress(Vector2 p)
    {
        pullingFromInside = PieceShape.Inside(Shape.Outline, p);
        closestToOpening = PieceShape.DistanceToLine(Shape.Opening, p);
    }

    protected override void OnDrag(Vector2 p) => closestToOpening = Mathf.Min(closestToOpening, PieceShape.DistanceToLine(Shape.Opening, p));

    protected override void OnRelease(Vector2 p)
    {
        if (!pullingFromInside || Pulls >= PullsNeeded || PieceShape.Inside(Shape.Outline, p) || closestToOpening > OpeningReach) return;
        State.points.Add(p);
        Show();
    }

    public override void Undo()
    {
        if (Pulls == 0) return;
        State.points.RemoveAt(Pulls - 1);
        Show();
    }

    void Show() => Recolor(piece, Color.Lerp(InsideOut, area.fabricColor, Pulls / (float)PullsNeeded));

    public override float SaveWork()
    {
        State.progress = Mathf.Clamp01(Pulls / (float)PullsNeeded);
        return State.progress;
    }
}
