using System.Collections.Generic;
using UnityEngine;

/// Embroidery recipe step: French knots (e.g. an eye). Put your finger on the mark, twirl round it
/// to wrap the thread twice, then let go to pull the needle through. Closer to the mark is better.
public class FrenchKnotsStep : PlacementStep
{
    protected override bool SewnOn => true;

    const float WrapsNeeded = 2f;       // turns round the press point

    Vector2 pressAt;
    float turned, lastAngle;
    bool twirling;

    protected override float ShapePadding => 0.25f;
    protected override List<Vector2> Marks => Shape.Eyes;
    protected override Color MarkColor => new Color(1, 1, 1, 0.45f);

    protected override void DrawPiece() => DrawHoop(outlined: true, filled: true);

    protected override GameObject DrawPlaced(Vector2 p) => Dot(p, 0.045f, Floss(1), 6, shine: true);

    protected override void OnPress(Vector2 p)
    {
        pressAt = p;
        turned = 0f;
        twirling = false;
    }

    protected override void OnDrag(Vector2 p)
    {
        var d = p - pressAt;
        if (d.magnitude < 0.01f) return;
        float angle = Mathf.Atan2(d.y, d.x);
        if (twirling) turned += Mathf.Abs(Mathf.DeltaAngle(lastAngle * Mathf.Rad2Deg, angle * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
        lastAngle = angle;
        twirling = true;
    }

    protected override void OnRelease(Vector2 p)
    {
        if (turned >= WrapsNeeded * Mathf.PI * 2f) Place(pressAt);
        twirling = false;
    }
}
