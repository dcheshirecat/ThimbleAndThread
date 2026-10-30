using System.Collections.Generic;
using UnityEngine;

/// Embroidery recipe step: backstitch along the printed outline. How closely the needle follows
/// the line sets the quality.
public class BackstitchStep : TracingStep
{
    protected override float ShapePadding => 0.25f;
    protected override bool ClosedGuide => true;
    protected override Color ThreadColor => Color.Lerp(Floss(0), Color.black, 0.3f);
    protected override Color GuideColor => new Color(0.55f, 0.6f, 0.75f, 0.9f);   // the printed line

    protected override void DrawPiece() => DrawHoop(outlined: false, filled: false);

    protected override List<Vector2> Guide() => Shape.Outline;
}
